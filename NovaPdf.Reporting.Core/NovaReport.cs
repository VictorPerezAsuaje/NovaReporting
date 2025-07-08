using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;

namespace NovaPdf.Reporting.Core;

public interface INovaReport
{
    bool IsPreview { get; set; }
    string Name { get; }
    ReportLayout Layout { get; }
    ReportFormat Format { get; }
    Dictionary<string, ReportParameter> Parameters { get; }
    Dictionary<string, IReportDataSet> DataSets { get; }
    void Init();
    Result Validate();

    void AddDataSet<T>(T dataset) where T : class, IReportDataSet;
    void AddParameter(ReportParameter parameter);
    T GetParameterValue<T>(string name);
    T GetDataSet<T>() where T : class, IReportDataSet;
    
    Task<Result<byte[]>> GeneratePdfAsync(Action<NovaReportRenderingData> options);
}

public abstract class NovaReport : INovaReport
{
    public bool IsPreview { get; set; } = false;
    public abstract string Name { get; }
    public ReportLayout Layout { get; protected set; } = ReportLayout.Vertical;
    public ReportFormat Format { get; protected set; } = ReportFormat.A4;
    public Dictionary<string, ReportParameter> Parameters { get; protected set; } = [];
    public Dictionary<string, IReportDataSet> DataSets { get; protected set; } = [];
    protected IServiceProvider _provider { get; set; }

    public NovaReport()
    {
        Init();
    }
    public NovaReport(IServiceProvider provider) {
        _provider = provider;
        Init();
    }
    public virtual void Init() { }
    public virtual Result Validate() => Result.Ok();

    protected T GetService<T>() where T : notnull
        => _provider.GetRequiredService<T>();

    public void AddParameter(ReportParameter parameter)
    {
        if (Parameters.ContainsKey(parameter.Name))
            throw new Exception($"A parameter with the name '{parameter.Name}' already exists for the Report '{Name}'");

        Parameters.Add(parameter.Name, parameter);
    }

    public T GetParameterValue<T>(string name)
    {
        if (!Parameters.ContainsKey(name))
            throw new Exception($"A parameter with the name '{name}' does not exist for the Report '{Name}'");

        return (T)Parameters[name].Value;
    }

    public void AddDataSet<T>(T dataset) where T : class, IReportDataSet
    {
        if (DataSets.ContainsKey(typeof(T).Name))
            throw new Exception($"A dataset with the name '{typeof(T).Name}' already exists for the Report '{Name}'");

        DataSets.Add(typeof(T).Name, dataset);
    }

    public T GetDataSet<T>() where T : class, IReportDataSet
    {
        if (!DataSets.ContainsKey(typeof(T).Name))
            throw new Exception($"A dataset with the name '{typeof(T).Name}' does not exist for the Report '{Name}'");

        return (T)DataSets[typeof(T).Name];
    }

    public async Task<Result<byte[]>> GeneratePdfAsync(Action<NovaReportRenderingData> options)
    {
        Result validationResult = Validate();
        if (validationResult.IsFailure)
            return Result.Fail<byte[]>(validationResult.Error);

        NovaReportRenderingData data = new();
        options(data);

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var page = await browser.NewPageAsync();

        await page.EmulateMediaAsync(new PageEmulateMediaOptions { Media = Media.Print });
        await page.SetContentAsync(data.Html, new PageSetContentOptions { WaitUntil = WaitUntilState.NetworkIdle });

        byte[] pdf = await page.PdfAsync(new PagePdfOptions
        {
            Format = Format,
            Landscape = Layout == ReportLayout.Horizontal,
            DisplayHeaderFooter = true,
            HeaderTemplate = data.Header,
            FooterTemplate = data.Footer,
            PrintBackground = true,
            Margin = new Margin 
            { 
                Top = $"{data.MarginTop}px",
                Bottom = $"{data.MarginBottom}px",
                Right = $"{data.MarginRight}px",
                Left = $"{data.MarginLeft}px"
            }
        });

        return Result.Ok(pdf);
    }
}
