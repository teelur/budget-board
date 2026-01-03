namespace BudgetBoard.Service.Interfaces;

/// <summary>
/// Service for syncing data with SimpleFIN.
/// </summary>
public interface ISimpleFinService : ISyncProvider
{
    /// <summary>
    /// Configure the access token for SimpleFIN.
    /// </summary>
    /// <param name="userGuid">
    /// The unique identifier of the user whose access token is being configured.
    /// </param>
    /// <param name="setupToken">
    /// The setup token for the user.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    Task ConfigureAccessTokenAsync(Guid userGuid, string setupToken);

    /// <summary>
    /// Removes the access token for SimpleFIN.
    /// </summary>
    /// <param name="userGuid">The unique identifier of the user whose access token is being removed.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RemoveAccessTokenAsync(Guid userGuid);
}
