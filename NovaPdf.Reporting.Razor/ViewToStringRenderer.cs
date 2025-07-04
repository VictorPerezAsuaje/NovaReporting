using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace NovaPdf.Reporting.Razor;


public interface IViewRenderer
{
    Task<string> RenderViewToStringAsync(ControllerContext context, string viewName, object model);
}

public class ViewToStringRenderer : IViewRenderer
{
    private readonly IRazorViewEngine _viewEngine;
    private readonly ITempDataProvider _tempDataProvider;

    public ViewToStringRenderer(IRazorViewEngine viewEngine, ITempDataProvider tempDataProvider)
    {
        _viewEngine = viewEngine;
        _tempDataProvider = tempDataProvider;
    }

    public async Task<string> RenderViewToStringAsync(ControllerContext context, string viewName, object model)
    {
        var viewResult = _viewEngine.FindView(context, viewName, isMainPage: false);

        if (!viewResult.Success)
            throw new InvalidOperationException($"View '{viewName}' not found.");

        var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        {
            Model = model
        };

        using var sw = new StringWriter();
        var viewContext = new ViewContext(
            actionContext: context,
            view: viewResult.View,
            viewData: viewDictionary,
            tempData: new TempDataDictionary(context.HttpContext, _tempDataProvider),
            writer: sw,
            htmlHelperOptions: new HtmlHelperOptions()
        );

        await viewResult.View.RenderAsync(viewContext);
        return sw.ToString();
    }
}
