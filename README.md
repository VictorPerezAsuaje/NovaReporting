# NovaReporting

NovaReporting began as a proof of concept on my journey to move away from outdated, heavy, not cross-platform and/or often costly reporting systems like Crystal Reports or RDLs. It aims to be lightweight, cross-platform, and developer-friendly.

> 🚀 NovaReporting uses HTML + CSS to generate PDF reports — making it intuitive, flexible, and perfect for fast iterations and cross-platform environments.

Feel free to use this project as a foundation or inspiration for your own reporting solution.

![Sample Report PDF](sample-report-pdf.jpeg)

## ✨ Why NovaReporting?

The core idea is to simplify report generation by using technologies most developers already know: HTML and CSS.

**Key Benefits:**

* ✅ Easy to learn – Uses standard HTML/CSS for report design.
* 🔁 Live previews – See changes immediately during development.
* 🧪 Hot reload support – Works seamlessly with Visual Studio / VS Code.
* 🎨 Style freely – Use CSS frameworks and icon libraries like Bootstrap or Font Awesome (used for the sample pdfs shown here).
* 📄 Cross-platform – Ideal for modern .NET Core web applications.

![Sample Report Preview](sample-report-preview.jpeg)

> ⚠️ Note: Most JavaScript libraries may not function as expected since scripts are often executed after DOMContentLoaded.

### 🚀 Getting Started

If you're building a .NET Core Web Application (MVC, API, Blazor, etc.), NovaReporting integrates effortlessly. You can use Razor views or any server-rendered HTML as your report source. 

You can see an example of using Razor here: [Razor Example](#razor-example)

If your project is not a .NET Web-based Application, You can also generate reports using static HTML templates, with or without a templating engine. NovaReporting just needs HTML — you're free to choose how you generate it.

You can see an example of using static HTML here: [Static HTML](#static-html-example)

And of course you can choose another HTML Template Renderer you want, in fact I've also added an example using Scriban as well, so that you are not bound to a Web Application nor have to abandon all the comodities that the Razor Engine provides.

You can see an example of using static HTML here: [Platform-Agnostic HTML Template Renderer Example (Scriban)](#platform-agnostic-html-template-renderer-example)

The following scenarios are simple ones, with as little abstractions as possible for it to show as much of the important code as possible. Feel free to create the abstractions you need, group, extract or refactor it... The sky is the limit 🚀

### Razor Example

**Razor View:**
```

@using NovaPdf.Reporting.WebClient.Reports.Sales
@inherits BaseReportView<SalesNovaReport>

@{
    Layout = "_ReportLayout";
    SalesSummaryDS salesDS = DataSet<SalesSummaryDS>();
}

<h5>Sales Summary</h5>
<table class="table mt-4" style="font-size:0.9rem;">
    <thead>
        <tr>
            <th>Month</th>
            <th class="text-end">Total Sales</th>
            <th class="text-end">Returns</th>
            <th class="text-end">Net Sales</th>
            <th class="text-end">New Customers</th>
        </tr>
    </thead>
    <tbody>
        @{
            decimal lastTotalSales = 0;
            decimal lastReturns = 0;
            decimal lastNewCustomers = 0;
        }
        @foreach (var item in salesDS.Data)
        {
            bool upSales = item.TotalSales > lastTotalSales;
            bool upReturns = item.TotalReturns > lastReturns;
            bool upNewCustomers = item.NewCustomers > lastNewCustomers;
            lastTotalSales = item.TotalSales;
            lastReturns = item.NetSales;
            lastNewCustomers = item.NewCustomers;

            <tr>
                <td>@item.Month</td>
                <td class="text-end">
                    @item.TotalSales.ToString("N2")€
                    <i class="ms-2 fa-solid @(upSales ? "fa-caret-up text-success" : "fa-caret-down text-danger")"></i>
                </td>
                <td class="text-end">
                    @item.TotalReturns.ToString("N2")€
                    <i class="ms-2 fa-solid @(upReturns ? "fa-caret-up text-success" : "fa-caret-down text-danger")"></i>
                </td>
                <td class="text-end">@item.NetSales.ToString("N2")€</td>
                <td class="text-end">
                    @item.NewCustomers
                    <i class="ms-2 fa-solid @(upNewCustomers ? "fa-caret-up text-success" : "fa-caret-down text-danger")"></i>
                </td>
            </tr>
        }

        <tr>
            <td class="text-end fw-bold">Total Q2</td>
            <td class="text-end fw-bold @(salesDS.SumTotalSales < Parameter<decimal>("SalesTarget") ? "bg-danger" : "bg-success")" style="--bs-bg-opacity: .2;">
                @salesDS.SumTotalSales.ToString("N2")€ / @(Parameter<decimal>("SalesTarget").ToString("N2"))€
            </td>
            <td class="text-end">
                @salesDS.SumTotalReturns.ToString("N2")€
            </td>
            <td class="text-end">@salesDS.SumNetSales.ToString("N2")€</td>
            <td class="text-end">
                @salesDS.SumNewCustomers
            </td>
        </tr>
    </tbody>
</table>
```

**Controller Endpoint:**

```

[HttpPost]
[Route("sales")]
public async Task<IActionResult> DownloadSales() 
{
    SalesNovaReport report = new SalesNovaReport(HttpContext.RequestServices);

    string html = await _renderer.RenderViewToStringAsync(ControllerContext, "Sales/Report", report);

    var result = await report.GeneratePdfAsync(x =>
    {
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

```

### Static HTML Example

**HTML Template:**

```
<!--- ### layout.html ### --->

<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.7/dist/css/bootstrap.min.css" rel="stylesheet" integrity="sha384-LN+7fdVzj6u52u30Kp6M/trliBMCMKTyK833zpbD+pXdCLuTusPj697FH4R/5mcr" crossorigin="anonymous">
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.7.2/css/all.min.css" rel="stylesheet">
</head>
<body class="p-4">
    <h5>Sales Summary</h5>
    <table class="table mt-4" style="font-size:0.9rem;">
        <thead>
            <tr>
                <th>Month</th>
                <th class="text-end">Total Sales</th>
                <th class="text-end">Returns</th>
                <th class="text-end">Net Sales</th>
                <th class="text-end">New Customers</th>
            </tr>
        </thead>
        <tbody>
            {{ SalesSummaryRows }}
        </tbody>
    </table>
</body>
</html>

<!--- ### sale-row.html ### --->

<tr>
    <td>{{ Month }}</td>
    <td class="text-end">
        {{ TotalSales }} €
        <i class="ms-2 fa-solid {{ TotalSalesIconClass }}"></i>
    </td>
    <td class="text-end">
        {{ TotalReturns }} €
        <i class="ms-2 fa-solid {{ TotalReturnsIconClass }}"></i>
    </td>
    <td class="text-end">{{ NetSales }} €</td>
    <td class="text-end">
        {{ NewCustomers }}
        <i class="ms-2 fa-solid {{ NewCustomersIconClass }}"></i>
    </td>
</tr>
```


**Controller Endpoint:**

```
[HttpPost]
[Route("static-sales")]
public async Task<IActionResult> DownloadStaticSales()
{
    SalesNovaReport salesReport = new SalesNovaReport(HttpContext.RequestServices);
        
    string currentDir = Directory.GetCurrentDirectory();
    string reportFolder = Path.Combine(currentDir, "StaticReports", salesReport.Name);

    string report = System.IO.File.ReadAllText(Path.Combine(reportFolder, "report.html"));

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

    report = report.Replace("{{ SalesSummaryRows }}", salesSumaryRowsTemplate);

    foreach (var param in salesReport.Parameters)
        report = report.Replace($"{{{{ {param.Key} }}}}", $"{param.Value.Value}");

    var result = await salesReport.GeneratePdfAsync(x =>
    {
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
```

### Platform-Agnostic HTML Template Renderer Example

For this example I opted for Scriban as my Template renderer, but you can choose whichever fits your needs. You can check out their amazing library here: [Scriban](https://github.com/scriban/scriban/blob/master/doc/language.md).

Scriban allows me to not depend heavily on the Razor Rendering Engine, so this approach makes it cross-platform (Web, Desktop, Mobile, etc.) whilst harnessing all the power of C# and a more extensible and extensive syntax than what you would have to do in the Static HTML Example.


**HTML Template:**

```
<h5>Sales Summary</h5>
<table class="table mt-4" style="font-size:0.9rem;">
    <thead>
        <tr>
            <th>Month</th>
            <th class="text-end">Total Sales</th>
            <th class="text-end">Returns</th>
            <th class="text-end">Net Sales</th>
            <th class="text-end">New Customers</th>
        </tr>
    </thead>
    <tbody>
        {{-
          # last values for comparisons
          last_total_sales = 0
          last_returns = 0
          last_new_customers = 0
        -}}

        {{ for item in datasets.SalesSummaryDS.data }}
        {{-
        up_sales = item.total_sales > last_total_sales
        up_returns = item.total_returns > last_returns
        up_new_customers = item.new_customers > last_new_customers
        last_total_sales = item.total_sales
        last_returns = item.total_returns
        last_new_customers = item.new_customers
        -}}

        <tr>
            <td>{{ item.month }}</td>
            <td class="text-end">
                {{ item.total_sales | string_format "N2" }}€
                <i class="ms-2 fa-solid {{ up_sales ? "fa-caret-up text-success" : "fa-caret-down text-danger" }}"></i>
            </td>
            <td class="text-end">
                {{ item.total_returns | string_format "N2" }}€
                <i class="ms-2 fa-solid {{ up_returns ? "fa-caret-up text-success" : "fa-caret-down text-danger" }}"></i>
            </td>
            <td class="text-end">
                {{ item.total_sales - item.total_returns | string_format "N2" }}€
            </td>
            <td class="text-end">
                {{ item.new_customers }}
                <i class="ms-2 fa-solid {{ up_new_customers ? "fa-caret-up text-success" : "fa-caret-down text-danger" }}"></i>
            </td>
        </tr>
        {{ end }}

        <tr>
            <td class="text-end fw-bold">Total Q2</td>
            <td class="text-end fw-bold {{ datasets.SalesSummaryDS.sum_total_sales < parameters.SalesTarget ? "bg-danger" : "bg-success" }}" style="--bs-bg-opacity: .2;">
                {{ datasets.SalesSummaryDS.sum_total_sales | string_format "N2" }}€ / {{ parameters.SalesTarget | string_format "N2" }}€
            </td>
            <td class="text-end">{{ datasets.SalesSummaryDS.sum_total_returns | string_format "N2" }}€</td>
            <td class="text-end">{{ datasets.SalesSummaryDS.sum_net_sales | string_format "N2" }}€</td>
            <td class="text-end">{{ datasets.SalesSummaryDS.sum_new_customers }}</td>
        </tr>
    </tbody>
</table>
```


**Controller Endpoint:**

```
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
```


### 🔭 Roadmap

Here are the planned features and improvements.

**Razor**

[ X ] Templating scenarios

**Static HTML**

[ X ] Basic Templating scenarios

**Cross-platform**

[ X ] Templating scenarios

### 🙏 Acknowledgements

NovaReporting is powered by:
* [Microsoft Playwright for .NET](https://github.com/microsoft/playwright-dotnet): Core component for HTML to PDF rendering.
