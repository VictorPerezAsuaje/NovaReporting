namespace NovaPdf.Reporting.Core;

public interface IReportParameter
{
    string Name { get; }
    object Value { get; }
}

public record ReportParameter<T>(string Name, T Value) : IReportParameter where T : class
{
    object IReportParameter.Value => Value;
}
