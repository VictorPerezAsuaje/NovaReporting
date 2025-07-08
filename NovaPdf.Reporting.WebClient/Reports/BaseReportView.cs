using Microsoft.AspNetCore.Mvc.Razor;
using NovaPdf.Reporting.Core;
using System.Reflection;

namespace NovaPdf.Reporting.WebClient.Reports;

public abstract class BaseReportView<TModel> : RazorPage<TModel> where TModel : NovaReport
{
    public T Parameter<T>(string name)
    {
        return Model.GetParameterValue<T>(name);
    }

    public T DataSet<T>() where T : class, IReportDataSet
    {
        return Model.GetDataSet<T>();
    }
}