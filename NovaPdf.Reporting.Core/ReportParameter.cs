namespace NovaPdf.Reporting.Core;

public interface IReportParameter
{
    string Name { get; }
    object Value { get; }
}

public record ReportParameter<T>(string Name, T Value) : IReportParameter
{
    object IReportParameter.Value => Value;
}