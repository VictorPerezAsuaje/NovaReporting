using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace NovaPdf.Reporting.WebClient.Controllers;

public class ReportController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

}
