namespace TestConnector.Bitfinex.Models;

public class SubscriptionInfo
{
    public int ChanId { get; set; }
    public string Channel { get; set; }
    public string Pair { get; set; }
    public string CandlesPeriod { get; set; }
}