using Microsoft.AspNetCore.Mvc;
using NovaPdf.Reporting.Razor;
using NovaPdf.Reporting.WebClient.Reports.Sales;
using System.Diagnostics;

namespace NovaPdf.Reporting.WebClient.Controllers;

[Route("")]
public class ReportController : Controller
{
    IViewRenderer _renderer;
    IWebHostEnvironment _env;

    public ReportController(IViewRenderer renderer, IWebHostEnvironment env)
    {
        _renderer = renderer;
        _env = env;
    }

    [HttpGet]
    [Route("")]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    [Route("sales")]
    public async Task<IActionResult> PreviewSales()
    {
        SalesNovaReport report = new SalesNovaReport(HttpContext.RequestServices);
        report.IsPreview = true;
        return View("Sales/Report", report);
    }

    [HttpPost]
    [Route("sales")]
    public async Task<IActionResult> DownloadSales() 
    {
        SalesNovaReport report = new SalesNovaReport(HttpContext.RequestServices);

        string html = await _renderer.RenderViewToStringAsync(ControllerContext, "Sales/Report", report);
        string headerHtml = await _renderer.RenderViewToStringAsync(ControllerContext, "Sales/Header", report);

        var result = await report.GeneratePdfAsync(x =>
        {
            x.Header = headerHtml;
            x.Html = html;
            x.MarginTop = 175;
        });

        if(result.IsFailure)
        {
            TempData["Error"] = result.Error;
            return View("Index");
        }

        return File(result.Value, "application/pdf", "sample.pdf");
    }

    [HttpPost]
    [Route("static-sales")]
    public async Task<IActionResult> DownloadStaticSales()
    {
        SalesNovaReport salesReport = new SalesNovaReport(HttpContext.RequestServices);
        
        string currentDir = Directory.GetCurrentDirectory();
        string reportFolder = Path.Combine(currentDir, "StaticReports", salesReport.Name);

        string header = System.IO.File.ReadAllText(Path.Combine(reportFolder, "header.html"));

        foreach (var param in salesReport.Parameters)
            header = header.Replace($"{{{{ {param.Key} }}}}", $"{param.Value.Value}");

        string salesSumaryRowsTemplate = "";
        string saleRowTemplate = System.IO.File.ReadAllText(Path.Combine(reportFolder, "sale-row.html"));

        decimal lastTotalSales = 0;
        decimal lastReturns = 0;
        decimal lastNewCustomers = 0;
        SalesSummaryDS salesDS = salesReport.GetDataSet<SalesSummaryDS>();

        foreach (var item in salesDS.Data)
        {
            string saleRow = saleRowTemplate;
            bool upSales = item.TotalSales > lastTotalSales;
            bool upReturns = item.TotalReturns > lastReturns;
            bool upNewCustomers = item.NewCustomers > lastNewCustomers;
            lastTotalSales = item.TotalSales;
            lastReturns = item.NetSales;
            lastNewCustomers = item.NewCustomers;

            saleRow = saleRow.Replace("{{ Month }}", $"{item.Month}");
            saleRow = saleRow.Replace("{{ TotalSales }}", $"{item.TotalSales}");
            saleRow = saleRow.Replace("{{ TotalSalesIconClass }}", upSales ? "fa-caret-up text-success" : "fa-caret-down text-danger");
            saleRow = saleRow.Replace("{{ TotalReturns }}", $"{item.TotalReturns}");
            saleRow = saleRow.Replace("{{ TotalReturnsIconClass }}", upReturns ? "fa-caret-up text-success" : "fa-caret-down text-danger");
            saleRow = saleRow.Replace("{{ NetSales }}", $"{item.NetSales}");
            saleRow = saleRow.Replace("{{ NewCustomers }}", $"{item.NewCustomers}");
            saleRow = saleRow.Replace("{{ NewCustomersIconClass }}", upNewCustomers ? "fa-caret-up text-success" : "fa-caret-down text-danger");
            salesSumaryRowsTemplate += saleRow;
        }

        string totaSaleRowTemplate = System.IO.File.ReadAllText(Path.Combine(reportFolder, "total-sale-row.html"));
        totaSaleRowTemplate = totaSaleRowTemplate.Replace("{{ SalesTargetIconClass }}", $"{(salesDS.SumTotalSales < salesReport.GetParameter<decimal>("SalesTarget") ? "bg-danger" : "bg-success")}");
        totaSaleRowTemplate = totaSaleRowTemplate.Replace("{{ SumTotalSales }}", $"{salesDS.SumTotalSales.ToString("N2")}");
        totaSaleRowTemplate = totaSaleRowTemplate.Replace("{{ SalesTarget }}", $"{salesReport.GetParameter<decimal>("SalesTarget").ToString("N2")}");
        totaSaleRowTemplate = totaSaleRowTemplate.Replace("{{ SumTotalReturns }}", $"{salesDS.SumTotalReturns.ToString("N2")}");
        totaSaleRowTemplate = totaSaleRowTemplate.Replace("{{ SumNetSales }}", $"{salesDS.SumNetSales.ToString("N2")}");
        totaSaleRowTemplate = totaSaleRowTemplate.Replace("{{ SumNewCustomers }}", $"{salesDS.SumNewCustomers}");
        salesSumaryRowsTemplate += totaSaleRowTemplate;

        string productPerformanceRowsTemplate = "";
        string productRowTemplate = System.IO.File.ReadAllText(Path.Combine(reportFolder, "product-row.html"));
        ProductPerformanceDS productDS = salesReport.GetDataSet<ProductPerformanceDS>();
        foreach (var item in productDS.Data)
        {
            string productRow = productRowTemplate;
            bool isTopPerforming = item.ProductId == productDS.TopPerformingProductId;

            productRow = productRow.Replace("{{ ProductId }}", $"{item.ProductId}");
            productRow = productRow.Replace("{{ TopPerformingIcon }}", isTopPerforming ? $"<i class=\"ms-2 fa-solid fa-star text-warning\"></i>" : "");

            productRow = productRow.Replace("{{ ProductName }}", $"{item.ProductName}");
            productRow = productRow.Replace("{{ UnitsSold }}", $"{item.UnitsSold}");
            productRow = productRow.Replace("{{ Revenue }}", $"{item.Revenue}");
            productRow = productRow.Replace("{{ AvgPrice }}", $"{item.AvgPrice}");
            productRow = productRow.Replace("{{ PercentageReturnRate }}", $"{item.PercentageReturnRate}");
            productPerformanceRowsTemplate += productRow;
        }

        string report = System.IO.File.ReadAllText(Path.Combine(reportFolder, "report.html"));

        report = report.Replace("{{ SalesSummaryRows }}", salesSumaryRowsTemplate);
        report = report.Replace("{{ ProductPerformanceRows }}", productPerformanceRowsTemplate);

        foreach (var param in salesReport.Parameters)
            report = report.Replace($"{{{{ {param.Key} }}}}", $"{param.Value.Value}");

        var result = await salesReport.GeneratePdfAsync(x =>
        {
            x.Header = header;
            x.Html = report;
            x.MarginTop = 175;
        });

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
            return View("Index");
        }

        return File(result.Value, "application/pdf", "sample.pdf");
    }
}
