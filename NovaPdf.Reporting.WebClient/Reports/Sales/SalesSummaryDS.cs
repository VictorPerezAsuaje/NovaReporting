using NovaPdf.Reporting.Core;

namespace NovaPdf.Reporting.WebClient.Reports.Sales;

public class SalesSummaryDS : IReportDataSet
{
    public List<SalesSummaryItem> Data { get; set; }
}

public class SalesSummaryItem
{
    public string Month { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalReturns { get; set; }
    public int NewCustomers { get; set; }
    public decimal NetSales => TotalSales - TotalReturns;
}