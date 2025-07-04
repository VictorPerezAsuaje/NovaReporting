using NovaPdf.Reporting.Core;

namespace NovaPdf.Reporting.WebClient.Reports.Sales;

public class SalesNovaReport : NovaReport
{
    public override string Name => "Sales";
    public SalesNovaReport(IServiceProvider provider) : base(provider)
    {
    }

    public override void Init()
    {
        var env = GetService<IWebHostEnvironment>();
        string logoPath = Path.Combine(env.WebRootPath, "img", "logo.svg");

        if (!File.Exists(logoPath))
            throw new Exception($"The path '{logoPath}' does not contain a file.");

        AddParameter(new ReportParameter<string>("Logo", Convert.ToBase64String(File.ReadAllBytes(logoPath))));
        AddParameter(new ReportParameter<string>("ReportTitle", "Q2 2025 Sales Performance"));
        AddParameter(new ReportParameter<string>("CompanyName", "Nova Reporting"));
        AddParameter(new ReportParameter<DateRange>("DateRange", new DateRange(new DateTime(2025, 1, 1), new DateTime(2025, 6, 25))));
        AddParameter(new ReportParameter<decimal>("SalesTarget", 1500000m));

        var salesSummary = new SalesSummaryDS
        {
            Data = new List<SalesSummaryItem>
            {
                new SalesSummaryItem { Month = "April", TotalSales = 450000m, TotalReturns = 12000m, NewCustomers = 45 },
                new SalesSummaryItem { Month = "May", TotalSales = 520000m, TotalReturns = 8500m, NewCustomers = 52 },
                new SalesSummaryItem { Month = "June", TotalSales = 500000m, TotalReturns = 9500m, NewCustomers = 63 }
            }
        };

        AddDataSet(salesSummary);

        var productPerformance = new ProductPerformanceDS
        {
            Data = new List<ProductPerformanceItem>
            {
                new ProductPerformanceItem { ProductId = "P1001", ProductName = "Widget Pro", UnitsSold = 1250, Revenue = 312500m, ReturnRate = 0.02m },
                new ProductPerformanceItem { ProductId = "P1002", ProductName = "Gadget Lite", UnitsSold = 980, Revenue = 196000m, ReturnRate = 0.015m },
                new ProductPerformanceItem { ProductId = "P1003", ProductName = "Super Tool", UnitsSold = 750, Revenue = 375000m, ReturnRate = 0.008m },
                new ProductPerformanceItem { ProductId = "P1004", ProductName = "Basic Kit", UnitsSold = 1560, Revenue = 234000m, ReturnRate = 0.012m }
            }
        };

        AddDataSet(productPerformance);
    }
}

public class DateRange
{
    public DateTime Start { get; }
    public DateTime End { get; }

    public DateRange(DateTime start, DateTime end)
    {
        Start = start;
        End = end;
    }
}