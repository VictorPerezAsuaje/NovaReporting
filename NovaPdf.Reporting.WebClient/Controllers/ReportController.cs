using Microsoft.AspNetCore.Mvc;
using NovaPdf.Reporting.Core;
using NovaPdf.Reporting.Razor;
using NovaPdf.Reporting.WebClient.Reports.Sales;
using Scriban.Runtime;
using Scriban;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Playwright;

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

    #region RAZOR_SALES

    [HttpGet]
    [Route("previews/sales")]
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

        return File(result.Value, "application/pdf", "razor-sales.pdf");
    }

    #endregion RAZOR_SALES

    #region STATIC_SALES 

    [HttpGet]
    [Route("previews/static-sales")]
    public async Task<IActionResult> PreviewStaticSales()
    {
        SalesNovaReport salesReport = new SalesNovaReport(HttpContext.RequestServices);
        string currentDir = Directory.GetCurrentDirectory();
        string reportFolder = Path.Combine(currentDir, "StaticReports", salesReport.Name);
        string layout = System.IO.File.ReadAllText(Path.Combine(reportFolder, "layout.html"));

        layout = layout.Replace("{{ BODY_CLASSES }}", "d-flex flex-column align-items-center justify-content-center min-vh-100");
        layout = layout.Replace("{{ BODY_STYLES }}", "background-color:#f1f1f1;");
        layout = layout.Replace("{{ CONTAINER_CLASSES }}", "pt-4 bg-white");
        layout = layout.Replace("{{ CONTAINER_STYLES }}", $"width: {(salesReport.Layout == ReportLayout.Vertical ? salesReport.Format.Width : salesReport.Format.Height)}mm; min-height: 95vh;");
        layout = layout.Replace("{{ REPORT_CONTAINER_CLASSES }}", "p-4");

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
        totaSaleRowTemplate = totaSaleRowTemplate.Replace("{{ SalesTargetIconClass }}", $"{(salesDS.SumTotalSales < salesReport.GetParameterValue<decimal>("SalesTarget") ? "bg-danger" : "bg-success")}");
        totaSaleRowTemplate = totaSaleRowTemplate.Replace("{{ SumTotalSales }}", $"{salesDS.SumTotalSales.ToString("N2")}");
        totaSaleRowTemplate = totaSaleRowTemplate.Replace("{{ SalesTarget }}", $"{salesReport.GetParameterValue<decimal>("SalesTarget").ToString("N2")}");
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

        layout = layout.Replace("{{ HEADER }}", header);
        layout = layout.Replace("{{ REPORT }}", report);

        return Content(layout, "text/html");
    }

    [HttpPost]
    [Route("static-sales")]
    public async Task<IActionResult> DownloadStaticSales()
    {
        SalesNovaReport salesReport = new SalesNovaReport(HttpContext.RequestServices);
        string currentDir = Directory.GetCurrentDirectory();
        string reportFolder = Path.Combine(currentDir, "StaticReports", salesReport.Name);
        string layout = System.IO.File.ReadAllText(Path.Combine(reportFolder, "layout.html"));

        layout = layout.Replace("{{ BODY_CLASSES }}", "p-4");
        layout = layout.Replace("{{ BODY_STYLES }}", "");
        layout = layout.Replace("{{ CONTAINER_CLASSES }}", "");
        layout = layout.Replace("{{ CONTAINER_STYLES }}", "");
        layout = layout.Replace("{{ REPORT_CONTAINER_CLASSES }}", "");

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
        totaSaleRowTemplate = totaSaleRowTemplate.Replace("{{ SalesTargetIconClass }}", $"{(salesDS.SumTotalSales < salesReport.GetParameterValue<decimal>("SalesTarget") ? "bg-danger" : "bg-success")}");
        totaSaleRowTemplate = totaSaleRowTemplate.Replace("{{ SumTotalSales }}", $"{salesDS.SumTotalSales.ToString("N2")}");
        totaSaleRowTemplate = totaSaleRowTemplate.Replace("{{ SalesTarget }}", $"{salesReport.GetParameterValue<decimal>("SalesTarget").ToString("N2")}");
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

        layout = layout.Replace("{{ HEADER }}", "");
        layout = layout.Replace("{{ REPORT }}", report);

        var result = await salesReport.GeneratePdfAsync(x =>
        {
            x.Header = header;
            x.Html = layout;
            x.MarginTop = 175;
        });

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
            return View("Index");
        }

        return File(result.Value, "application/pdf", "static-html-sample.pdf");
    }

    #endregion STATIC_SALES

    #region SCRIBAN_SALES

    [HttpGet]
    [Route("previews/scriban-sales")]
    public async Task<IActionResult> PreviewScribanSales()
    {
        // Syntax Docs: https://github.com/scriban/scriban/blob/master/doc/language.md
        SalesNovaReport salesReport = new SalesNovaReport(HttpContext.RequestServices);
        salesReport.IsPreview = true;

        var templateContext = new TemplateContext();

        var helperFunctions = new ScriptObject();
        helperFunctions.Import("date_format", new Func<DateTime, string, string>((dt, format) => dt.ToString(format)));
        helperFunctions.Import("string_format", new Func<int, string, string>((dt, format) => dt.ToString(format)));
        helperFunctions.Import("string_format", new Func<decimal, string, string>((dt, format) => dt.ToString(format)));
        templateContext.PushGlobal(helperFunctions);

        var globals = new ScriptObject();

        // Transformed parameters into key-value pairs for simplicity
        globals["parameters"] = salesReport.Parameters.ToDictionary(p => p.Key, p => p.Value.Value);
        globals["datasets"] = salesReport.DataSets;
        globals["model"] = salesReport;
        templateContext.PushGlobal(globals);

        string currentDir = Directory.GetCurrentDirectory();
        string reportFolder = Path.Combine(currentDir, "StaticReports", "ScribanSales");
        templateContext.TemplateLoader = new ScribanFileTemplateLoader(reportFolder);

        string layout = System.IO.File.ReadAllText(Path.Combine(reportFolder, "layout.html"));
        var template = Template.Parse(layout);

        if (template.HasErrors)
        {
            var errors = string.Join("\n", template.Messages.Select(m => m.Message));
            return Content($"Template parsing errors:\n{errors}", "text/plain");
        }

        return Content(template.Render(templateContext), "text/html");
    }

    [HttpPost]
    [Route("scriban-sales")]
    public async Task<IActionResult> DownloadScribanSales()
    {
        // Syntax Docs: https://github.com/scriban/scriban/blob/master/doc/language.md
        SalesNovaReport salesReport = new SalesNovaReport(HttpContext.RequestServices);

        var templateContext = new TemplateContext();

        var helperFunctions = new ScriptObject();
        helperFunctions.Import("date_format", new Func<DateTime, string, string>((dt, format) => dt.ToString(format)));
        helperFunctions.Import("string_format", new Func<int, string, string>((dt, format) => dt.ToString(format)));
        helperFunctions.Import("string_format", new Func<decimal, string, string>((dt, format) => dt.ToString(format)));
        templateContext.PushGlobal(helperFunctions);

        var globals = new ScriptObject();

        // Transformed parameters into key-value pairs for simplicity
        globals["parameters"] = salesReport.Parameters.ToDictionary(p => p.Key, p => p.Value.Value);
        globals["datasets"] = salesReport.DataSets;
        globals["model"] = salesReport;
        templateContext.PushGlobal(globals);

        string currentDir = Directory.GetCurrentDirectory();
        string reportFolder = Path.Combine(currentDir, "StaticReports", "ScribanSales");
        templateContext.TemplateLoader = new ScribanFileTemplateLoader(reportFolder);

        string layout = System.IO.File.ReadAllText(Path.Combine(reportFolder, "layout.html"));
        string header = System.IO.File.ReadAllText(Path.Combine(reportFolder, "header.html"));

        var reportTemplate = Template.Parse(layout);
        if (reportTemplate.HasErrors)
        {
            TempData["Error"] = reportTemplate.Messages;
            return View("Index");
        }

        var headerTemplate = Template.Parse(header);
        if (headerTemplate.HasErrors)
        {
            TempData["Error"] = headerTemplate.Messages;
            return View("Index");
        }

        var renderedHeader = headerTemplate.Render(templateContext);
        var renderedReport = reportTemplate.Render(templateContext);
        var result = await salesReport.GeneratePdfAsync(x =>
        {
            x.Header = renderedHeader;
            x.Html = renderedReport;
            x.MarginTop = 175;
        });

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error;
            return View("Index");
        }

        return File(result.Value, "application/pdf", "scriban-sales.pdf");
    }

    #endregion SCRIBAN_SALES
}
