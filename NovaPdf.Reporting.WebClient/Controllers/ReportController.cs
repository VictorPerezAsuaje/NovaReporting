using Microsoft.AspNetCore.Mvc;
using NovaPdf.Reporting.Razor;
using NovaPdf.Reporting.WebClient.Reports.Sales;
using System.Diagnostics;

namespace NovaPdf.Reporting.WebClient.Controllers;

[Route("")]
public class ReportController : Controller
{
    IViewRenderer _renderer;

    public ReportController(IViewRenderer renderer)
    {
        _renderer = renderer;
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
            x.MarginTop = 150;
        });

        if(result.IsFailure)
        {
            TempData["Error"] = result.Error;
            return View("Index");
        }

        return File(result.Value, "application/pdf", "sample.pdf");
    }
}
