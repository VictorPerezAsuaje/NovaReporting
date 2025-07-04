namespace NovaPdf.Reporting.Core;

public class ReportFormat
{
    public string Name { get; }
    public int Width { get; }
    public int Height { get; }

    ReportFormat(string name, int width, int height)
    {
        Name = name;
        Width = width;
        Height = height;
    }

    public static ReportFormat A4 = new("A4", 210, 297);

    public override string ToString() => Name;

    public static implicit operator string(ReportFormat d) => d.Name;
}
