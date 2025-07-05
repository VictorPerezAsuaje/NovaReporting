using NovaPdf.Reporting.Core;

namespace NovaPdf.Reporting.WebClient.Reports.Sales;

public class SalesSummaryDS : IReportDataSet
{
    public List<SalesSummaryItem> Data { get; set; } = [];
    public decimal SumTotalSales => Data.Sum(x => x.TotalSales);
    public decimal SumTotalReturns => Data.Sum(x => x.TotalReturns);
    public decimal SumNetSales => Data.Sum(x => x.NetSales);
    public decimal SumNewCustomers => Data.Sum(x => x.NewCustomers);
}

public class SalesSummaryItem
{
    public string Month { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalReturns { get; set; }
    public int NewCustomers { get; set; }
    public decimal NetSales => TotalSales - TotalReturns;
}