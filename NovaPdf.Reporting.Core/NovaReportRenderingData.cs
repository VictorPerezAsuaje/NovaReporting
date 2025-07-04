namespace NovaPdf.Reporting.Core;

public class NovaReportRenderingData
{
    public string Html { get; set; }
    public string? Header { get; set; } = "<span></span>";
    public string? Footer { get; set; } = "<span></span>";
    public int MarginTop { get; set; } = 80;
    public int MarginBottom { get; set; } = 80;
    public int MarginLeft { get; set; } = 80;
    public int MarginRight { get; set; } = 80;
}
