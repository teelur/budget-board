// Based on output by ML.NET Model Builder.
using BudgetBoard.Database.Data;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Models;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
using Microsoft.ML.Transforms.Text;

namespace BudgetBoard.Service.Helpers;

/// <summary>
/// Constructor
/// </summary>
/// <param name="mlNetModel">Stream containing the categorizer ML model</param>
public class AutomaticTransactionCategorizer
{
    /// <summary>
    /// model input class for BayesianTransactionCategorizer.
    /// </summary>
    #region model input class
    public class ModelInput
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

    #endregion

    /// <summary>
    /// model output class for BayesianTransactionCategorizer.
    /// </summary>
    #region model output class
    public class ModelOutput
    {
        [ColumnName(@"Features")]
        public float[]? Features { get; set; }

        [ColumnName(@"PredictedLabel")]
        public string? PredictedLabel { get; set; }

        [ColumnName(@"Score")]
        public float[]? Score { get; set; }

    }

    #endregion

    private PredictionEngine<ModelInput, ModelOutput> _predictEngine;

    public AutomaticTransactionCategorizer(byte[] mlNetModel)
    {
        _predictEngine = CreatePredictEngine(mlNetModel);
    }

    private static PredictionEngine<ModelInput, ModelOutput> CreatePredictEngine(byte[] mlNetModel)
    {
        var mlContext = new MLContext();
        ITransformer mlModel = mlContext.Model.Load(new MemoryStream(mlNetModel), out var _);
        return mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);
    }

    /// <summary>
    /// Use this method to predict on <see cref="ModelInput"/>.
    /// </summary>
    /// <param name="input">Transaction to categorize.</param>
    /// <returns>String with the predicted category</returns>
    public string Predict(Transaction input)
    {
        ModelInput modelInput = new ModelInput
        {
            MerchantName = input.MerchantName!,
            Account = input.Account!.Name,
            Amount = (float)input.Amount
        };

        return _predictEngine.Predict(modelInput).PredictedLabel!;
    }

    /// <summary>
    /// Train a new model with the provided dataset.
    /// </summary>
    /// <param name="trainingTransactions">List of transactions to use for training</param>
    /// <returns>byte array containing the compressed ML model</returns>
    public static byte[] Train(IEnumerable<Transaction> trainingTransactions)
    {
        var mlContext = new MLContext();

        // Map transactions into inputs
        IEnumerable<ModelInput> trainingInput = trainingTransactions.Select(
            t => new ModelInput
            {
                MerchantName = t.MerchantName!,
                Amount = (float)t.Amount,
                Account = t.Account!.Name,
                Category = (t.Subcategory is null || t.Subcategory.Equals(string.Empty)) ? t.Category! : t.Subcategory
            }
        );

        // Load input to create a new ML model
        var trainData = mlContext.Data.LoadFromEnumerable<ModelInput>(trainingInput);
        var pipeline = BuildPipeline(mlContext);
        var mlModel = pipeline.Fit(trainData);

        // Generate the model save data
        MemoryStream memoryStream = new MemoryStream();
        mlContext.Model.Save(mlModel, trainData.Schema, memoryStream);

        return memoryStream.ToArray();
    }

    /// <summary>
    /// build the pipeline that is used from model builder. Use this function to retrain model.
    /// </summary>
    /// <param name="mlContext"></param>
    /// <returns></returns>
    private static IEstimator<ITransformer> BuildPipeline(MLContext mlContext)
    {
        // Data process configuration with pipeline data transformations
        var pipeline =
            // Accounts are categorized because there's no ordering
            mlContext.Transforms.Categorical.OneHotEncoding(@"account", @"account", outputKind: OneHotEncodingEstimator.OutputKind.Indicator)
            // Normalize merchant name before tokenizing
            .Append(mlContext.Transforms.Text.NormalizeText(inputColumnName: @"merchant_name", outputColumnName: @"merchant_name_normalized", caseMode: TextNormalizingEstimator.CaseMode.Lower, keepNumbers: true))
            // Tokenize (or featurize) merchant name
            .Append(mlContext.Transforms.Text.FeaturizeText(inputColumnName: @"merchant_name_normalized", outputColumnName: @"merchant_name_features"))
            // Normalize amount into bins
            .Append(mlContext.Transforms.NormalizeBinning(inputColumnName: @"amount", outputColumnName: @"amount_bins"))
            // Concatenate the account values, merchant name tokens and amount bins
            .Append(mlContext.Transforms.Concatenate(@"Features", [@"account", @"amount_bins", @"merchant_name_features"]))
            // Map the given categories to the model keys
            .Append(mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: @"category", inputColumnName: @"category", addKeyValueAnnotationsAsText: false))
            // Instantiate a trainer
            .Append(mlContext.MulticlassClassification.Trainers.SdcaNonCalibrated(labelColumnName: @"category", featureColumnName: @"Features"))
            // Convert the keys to the predicted label values
            .Append(mlContext.Transforms.Conversion.MapKeyToValue(outputColumnName:@"PredictedLabel",inputColumnName:@"PredictedLabel"));

        return pipeline;
    }

    internal static AutomaticTransactionCategorizer? CreateAutoCategorizer(
        UserDataContext userDataContext,
        ApplicationUser userData
    )
    {
        AutomaticTransactionCategorizer? autoCategorizer = null;

        // First we check that the user has autoCategorizer on, and has a stored model.
        if (
            userData.UserSettings is not null &&
            userData.UserSettings.EnableAutoCategorizer &&
            userData.UserSettings.AutoCategorizerModelOID is not null
        )
        {
            // Load the Large Object from the database
            var autoCategorizerTrainingModel = userDataContext.AutoCategorizerTrainingModel.Value;
            if (autoCategorizerTrainingModel is not null)
            {
                autoCategorizer = new AutomaticTransactionCategorizer(autoCategorizerTrainingModel);
            }
        }

        return autoCategorizer;
    }
}
