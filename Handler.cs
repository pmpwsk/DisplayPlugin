namespace uwap.WebFramework.Plugins;

public partial class DisplayPlugin : Plugin
{
    public override async Task Handle(Request req)
    {
        switch (Parsers.GetFirstSegment(req.Path, out _))
        {
            case "displays":
                await HandleDisplays(req);
                break;
            case "files":
                await HandleFiles(req);
                break;
            case "templates":
                await HandleTemplates(req);
                break;
            case "views":
                await HandleViews(req);
                break;
            default:
                await HandleOther(req);
                break;
        }
    }
}