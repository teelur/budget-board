// Based on output by ML.NET Model Builder.
using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
using Microsoft.ML.Transforms.Text;

namespace BudgetBoard.Service.Helpers;

/// <summary>
/// Helper class for automatic transaction categorization.
/// This class is responsible for training a ML.NET model to categorize transactions based on their merchant name, amount and account, and for using the trained model to predict categories for new transactions.
/// </summary>
public class AutomaticTransactionCategorizerHelper(byte[] mlNetModel)
{
    private readonly PredictionEngine<ModelInput, ModelOutput> _predictEngine = CreatePredictEngine(
        mlNetModel
    );

    /// <summary>
    /// Train a new model with the provided dataset.
    /// </summary>
    /// <param name="trainingTransactions">List of transactions to use for training</param>
    /// <returns>Byte array containing the compressed ML model</returns>
    public static byte[] Train(IEnumerable<Transaction> trainingTransactions)
    {
        var mlContext = new MLContext();

        IEnumerable<ModelInput> trainingInput = trainingTransactions.Select(t => new ModelInput
        {
            MerchantName = t.MerchantName ?? string.Empty,
            Amount = (float)t.Amount,
            Account = t.Account!.Name,
            Category =
                (t.Subcategory is null || t.Subcategory.Equals(string.Empty))
                    ? t.Category ?? string.Empty
                    : t.Subcategory,
        });

        var trainData = mlContext.Data.LoadFromEnumerable(trainingInput);
        var pipeline = BuildPipeline(mlContext);
        var mlModel = pipeline.Fit(trainData);

        using MemoryStream memoryStream = new();
        mlContext.Model.Save(mlModel, trainData.Schema, memoryStream);

        return memoryStream.ToArray();
    }

    /// <summary>
    /// Use this method to predict on <see cref="ModelInput"/>.
    /// </summary>
    /// <param name="transaction">Transaction to categorize.</param>
    /// <returns>String with the predicted category</returns>
    public string PredictCategory(Transaction transaction)
    {
        ModelInput modelInput = new()
        {
            MerchantName = transaction.MerchantName!,
            Account = transaction.Account!.Name,
            Amount = (float)transaction.Amount,
        };

        return _predictEngine.Predict(modelInput).PredictedLabel!;
    }

    internal static async Task<AutomaticTransactionCategorizerHelper?> CreateAutoCategorizerAsync(
        UserDataContext userDataContext,
        ApplicationUser userData
    )
    {
        AutomaticTransactionCategorizerHelper? autoCategorizer = null;

        // First we check that the user has autoCategorizer on, and has a stored model.
        if (
            userData.UserSettings is not null
            && userData.UserSettings.EnableAutoCategorizer
            && userData.UserSettings.AutoCategorizerModelOID is not null
        )
        {
            // Load the Large Object from the database
            var autoCategorizerTrainingModel = await userDataContext.ReadLargeObjectAsync(
                (uint)userData.UserSettings.AutoCategorizerModelOID
            );
            if (autoCategorizerTrainingModel is not null && autoCategorizerTrainingModel.Length > 0)
            {
                autoCategorizer = new AutomaticTransactionCategorizerHelper(
                    autoCategorizerTrainingModel
                );
            }
        }

        return autoCategorizer;
    }

    private static PredictionEngine<ModelInput, ModelOutput> CreatePredictEngine(byte[] mlNetModel)
    {
        var mlContext = new MLContext();
        using var modelStream = new MemoryStream(mlNetModel);
        ITransformer mlModel = mlContext.Model.Load(modelStream, out var _);
        return mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);
    }

    private static EstimatorChain<KeyToValueMappingTransformer> BuildPipeline(MLContext mlContext)
    {
        // Data process configuration with pipeline data transformations
        var pipeline =
            // Accounts are categorized because there's no ordering
            mlContext
                .Transforms.Categorical.OneHotEncoding(
                    @"account",
                    @"account",
                    outputKind: OneHotEncodingEstimator.OutputKind.Indicator
                )
                // Normalize merchant name before tokenizing
                .Append(
                    mlContext.Transforms.Text.NormalizeText(
                        inputColumnName: @"merchant_name",
                        outputColumnName: @"merchant_name_normalized",
                        caseMode: TextNormalizingEstimator.CaseMode.Lower,
                        keepNumbers: true
                    )
                )
                // Tokenize (or featurize) merchant name
                .Append(
                    mlContext.Transforms.Text.FeaturizeText(
                        inputColumnName: @"merchant_name_normalized",
                        outputColumnName: @"merchant_name_features"
                    )
                )
                // Normalize amount into bins
                .Append(
                    mlContext.Transforms.NormalizeBinning(
                        inputColumnName: @"amount",
                        outputColumnName: @"amount_bins"
                    )
                )
                // Concatenate the account values, merchant name tokens and amount bins
                .Append(
                    mlContext.Transforms.Concatenate(
                        @"Features",
                        [@"account", @"amount_bins", @"merchant_name_features"]
                    )
                )
                // Map the given categories to the model keys
                .Append(
                    mlContext.Transforms.Conversion.MapValueToKey(
                        outputColumnName: @"category",
                        inputColumnName: @"category",
                        addKeyValueAnnotationsAsText: false
                    )
                )
                // Instantiate a trainer
                .Append(
                    mlContext.MulticlassClassification.Trainers.SdcaNonCalibrated(
                        labelColumnName: @"category",
                        featureColumnName: @"Features"
                    )
                )
                // Convert the keys to the predicted label values
                .Append(
                    mlContext.Transforms.Conversion.MapKeyToValue(
                        outputColumnName: @"PredictedLabel",
                        inputColumnName: @"PredictedLabel"
                    )
                );

        return pipeline;
    }

    private class ModelInput
    {
        [LoadColumn(0)]
        [ColumnName(@"amount")]
        public required float Amount { get; set; }

        [LoadColumn(1)]
        [ColumnName(@"merchant_name")]
        public required string MerchantName { get; set; }

        [LoadColumn(2)]
        [ColumnName(@"account")]
        public required string Account { get; set; }

        [LoadColumn(3)]
        [ColumnName(@"category")]
        public string? Category { get; set; }
    }

    private class ModelOutput
    {
        [ColumnName(@"Features")]
        public float[]? Features { get; set; }

        [ColumnName(@"PredictedLabel")]
        public string? PredictedLabel { get; set; }

        [ColumnName(@"Score")]
        public float[]? Score { get; set; }
    }
}
