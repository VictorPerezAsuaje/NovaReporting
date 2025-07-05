using NovaPdf.Reporting.Core;

namespace NovaPdf.Reporting.WebClient.Reports.Sales;

public class ProductPerformanceDS : IReportDataSet
{
    public List<ProductPerformanceItem> Data { get; set; } = [];
    public string TopPerformingProductId => Data.MaxBy(x => x.Revenue)?.ProductId ?? "";
}


public class ProductPerformanceItem
{
    public string ProductId { get; set; }
    public string ProductName { get; set; }
    public int UnitsSold { get; set; }
    public decimal Revenue { get; set; }
    public decimal ReturnRate { get; set; }
    public decimal PercentageReturnRate => ReturnRate * 100;
    public decimal AvgPrice => UnitsSold > 0 ? Revenue / UnitsSold : 0;
}
