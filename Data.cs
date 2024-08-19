namespace uwap.WebFramework.Plugins;

public partial class DisplayPlugin : Plugin
{
    /// <summary>
    /// Whether to send sandbox headers with /show or not.<br/>
    /// Default: false
    /// </summary>
    public bool SandboxViewer = false;

    private Dictionary<string, HashSet<Request>> DisplaySubscribers = [];

    private Dictionary<string, HashSet<Request>> ViewSubscribers = [];

    private async Task NotifyViewSubscribersForDisplay(string displayId)
    {
        if (DisplaySubscribers.TryGetValue(displayId, out var displaySet))
            foreach (var req in displaySet)
                await req.EventMessage("refresh");
    }

    private async Task NotifyViewSubscribersForView(string viewId)
    {
        if (ViewSubscribers.TryGetValue(viewId, out var viewSet))
            foreach (var req in viewSet)
                await req.EventMessage("refresh");
    }

    private async Task NotifyViewSubscribersForViewTemplate(string viewTemplateId)
    {
        foreach (var kv in Views)
            if (kv.Value.TemplateId == viewTemplateId)
                await NotifyViewSubscribersForView(kv.Key);
    }

    private async Task NotifyViewSubscribersForElementTemplate(string elementTemplateId)
    {
        foreach (var kv in Views)
            if (kv.Value.Elements.Any(x => x.TemplateId == elementTemplateId))
                await NotifyViewSubscribersForView(kv.Key);
    }

    private Task Unsubscribe(Request req)
    {
        if (req.Query.TryGetValue("display", out var displayId))
        {
            lock (DisplaySubscribers)
                if (DisplaySubscribers.TryGetValue(displayId, out var displaySet))
                    if (displaySet.Remove(req) && displaySet.Count == 0)
                        DisplaySubscribers.Remove(displayId);

            if (Displays.TryGetValue(displayId, out var display) && display.ViewId != null)
                lock (ViewSubscribers)
                    if (ViewSubscribers.TryGetValue(display.ViewId, out var viewSet))
                        if (viewSet.Remove(req) && viewSet.Count == 0)
                            ViewSubscribers.Remove(display.ViewId);
        }

        if (req.Query.TryGetValue("view", out var viewId))
            lock(ViewSubscribers)
                if (ViewSubscribers.TryGetValue(viewId, out var viewSet))
                    if (viewSet.Remove(req) && viewSet.Count == 0)
                        ViewSubscribers.Remove(viewId);

        return Task.CompletedTask;
    }
}