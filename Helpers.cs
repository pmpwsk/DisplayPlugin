using System.Text;
using uwap.WebFramework.Elements;

namespace uwap.WebFramework.Plugins;

public partial class DisplayPlugin : Plugin
{
    private static void CreatePage(Request req, string title, out Page page, out List<IPageElement> e)
    {
        req.ForceGET();
        req.CreatePage(title, out page, out e);
        ForcePermission(req, true);
        page.Favicon = $"{req.PluginPathPrefix}/icon.ico";
        page.Head.Add($"<link rel=\"manifest\" href=\"{req.PluginPathPrefix}/manifest.json\" />");

        if (req.Path == "/")
            return;

        page.Sidebar.Add(new ButtonElement("Menu:", null, $"{req.PluginPathPrefix}/"));
        page.Sidebar.Add(new ButtonElement(null, "Views", $"{req.PluginPathPrefix}/views"));
        page.Sidebar.Add(new ButtonElement(null, "Displays", $"{req.PluginPathPrefix}/displays"));
        page.Sidebar.Add(new ButtonElement(null, "Files", $"{req.PluginPathPrefix}/files"));
        page.Sidebar.Add(new ButtonElement(null, "Templates", $"{req.PluginPathPrefix}/templates"));

        foreach (IPageElement element in page.Sidebar)
            if (element is ButtonElement button && button.Title == null && req.Context.ProtoHostPath().StartsWith(button.Link))
                button.Class = "green";
    }

    private static void POST(Request req)
    {
        req.ForcePOST();
        ForcePermission(req, false);
    }

    private static void ForcePermission(Request req, bool redirect)
    {
        req.ForceLogin(redirect);
        if (req.User.AccessLevel < 50)
            throw new ForbiddenSignal();
    }
}