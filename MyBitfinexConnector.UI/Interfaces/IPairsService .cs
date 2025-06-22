namespace MyBitfinexConnector.UI.Interfaces;

public interface IPairsService
{
    Task<IEnumerable<string>> GetPairsAsync();
}