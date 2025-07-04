using Microsoft.Playwright;

namespace NovaPdf.Reporting.Core;

public interface INovaReport
{
    bool IsPreview { get; set; }
    string Name { get; }
    ReportLayout Layout { get; }
    Dictionary<string, IReportParameter> Parameters { get; }
    Dictionary<string, IReportDataSet> DataSets { get; }
    void Init();
    Result Validate();

    void AddDataSet<T>(T dataset) where T : class, IReportDataSet;
    void AddParameter<T>(T parameter) where T : class, IReportParameter;
    T GetParameter<T>(string name) where T : class, IReportParameter;
    T GetDataSet<T>(string name) where T : class, IReportDataSet;
    
    Task<byte[]> GeneratePdf(Action<NovaReportRenderingData> options);
}

public abstract class NovaReport : INovaReport
{
    public bool IsPreview { get; set; } = false;
    public abstract string Name { get; }
    public ReportLayout Layout { get; protected set; } = ReportLayout.Vertical;
    public Dictionary<string, IReportParameter> Parameters { get; protected set; } = [];
    public Dictionary<string, IReportDataSet> DataSets { get; protected set; } = [];

    protected NovaReport() { }
    public abstract void Init();
    public abstract Result Validate();
    
    public void AddParameter<T>(T parameter) where T : class, IReportParameter
    {
        if (Parameters.ContainsKey(parameter.Name))
            throw new Exception($"A parameter with the name '{parameter.Name}' already exists for the Report '{Name}'");

        Parameters.Add(parameter.Name, parameter);
    }

    public T GetParameter<T>(string name) where T : class, IReportParameter
    {
        if (Parameters.ContainsKey(name))
            throw new Exception($"A parameter with the name '{name}' does not exist for the Report '{Name}'");

        return (T)Parameters[name];
    }

    public void AddDataSet<T>(T dataset) where T : class, IReportDataSet
    {
        if (DataSets.ContainsKey(typeof(T).Name))
            throw new Exception($"A dataset with the name '{typeof(T).Name}' already exists for the Report '{Name}'");

        DataSets.Add(typeof(T).Name, dataset);
    }

    public T GetDataSet<T>(string name) where T : class, IReportDataSet
    {
        if (DataSets.ContainsKey(name))
            throw new Exception($"A dataset with the name '{name}' does not exist for the Report '{Name}'");

        return (T)DataSets[name];
    }

    public async Task<byte[]> GeneratePdf(Action<NovaReportRenderingData> options)
    {
        NovaReportRenderingData data = new();
        options(data);

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var page = await browser.NewPageAsync();

        await page.EmulateMediaAsync(new PageEmulateMediaOptions { Media = Media.Print });
        await page.SetContentAsync(data.Html, new PageSetContentOptions { WaitUntil = WaitUntilState.Load });

        return await page.PdfAsync(new PagePdfOptions
        {
            Format = ReportFormat.A4,
            Landscape = Layout == ReportLayout.Horizontal,
            DisplayHeaderFooter = true,
            HeaderTemplate = data.Header,
            FooterTemplate = data.Footer,
            Margin = new Margin 
            { 
                Top = $"{data.MarginTop}px",
                Bottom = $"{data.MarginBottom}px",
                Right = $"{data.MarginRight}px",
                Left = $"{data.MarginLeft}px"
            }
        });
    }
}
