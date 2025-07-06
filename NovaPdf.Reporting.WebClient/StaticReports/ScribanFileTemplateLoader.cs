using Scriban.Runtime;
using Scriban;
using Scriban.Parsing;

public class ScribanFileTemplateLoader : ITemplateLoader
{
    private readonly string _basePath;

    public ScribanFileTemplateLoader(string basePath)
    {
        _basePath = basePath;
    }

    public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
    {
        return Path.Combine(_basePath, $"{templateName}.html");
    }

    public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
    {
        return File.ReadAllText(templatePath);
    }

    public ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath)
    {
        throw new NotImplementedException();
    }
}