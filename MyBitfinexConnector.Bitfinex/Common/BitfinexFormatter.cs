namespace TestConnector.Bitfinex.Common;

internal static class BitfinexFormatter
{
    public static string FormatTradePair(this string tradePair)
    {
        return $"t{tradePair}";
    }
}