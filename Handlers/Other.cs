using System.Web;
using Microsoft.AspNetCore.Http;
using uwap.WebFramework.Elements;

namespace uwap.WebFramework.Plugins;

public partial class DisplayPlugin : Plugin
{
    public async Task HandleOther(Request req)
    {
        switch (req.Path)
        {
            // MENU
            case "/":
            { CreatePage(req, "Manage displays", out var page, out var e);
                e.Add(new HeadingElement("Manage displays"));
                e.Add(new ButtonElement("Views", null, $"{req.PluginPathPrefix}/views"));
                e.Add(new ButtonElement("Displays", null, $"{req.PluginPathPrefix}/displays"));
                e.Add(new ButtonElement("Files", null, $"{req.PluginPathPrefix}/files"));
                e.Add(new ButtonElement("Templates", null, $"{req.PluginPathPrefix}/templates"));
            } break;
            



            // VIEWS
            case "/views":
            { CreatePage(req, "Views", out var page, out var e);
                page.Navigation.Add(new Button("Back", ".", "right"));
                e.Add(new HeadingElement("Views"));
            } break;




            // SHOW DISPLAY/VIEW
            case "/show":
            { req.ForceGET();
                View? view;
                if (req.Query.TryGetValue("display", out var displayId))
                {
                    if (!Displays.TryGetValue(displayId, out var display))
                        throw new NotFoundSignal();
                    if (display.ViewId == null)
                    {
                        req.Context.Response.ContentType = "text/html;charset=utf-8";
                        await req.Write("<!DOCTYPE html>\n<html>\n<head>\n\t<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\n\t<meta charset=\"utf-8\">\n</head>\n<body>\n\t<h1>No view was assigned!</h1>\n\t<script src=\"refresh.js\"></script>\n</body>\n</html>");
                        break;
                    }
                    if (!Views.TryGetValue(display.ViewId, out view))
                        throw new NotFoundSignal();
                }
                else if (req.Query.TryGetValue("view", out var viewId))
                {
                    if (!Views.TryGetValue(viewId, out view))
                        throw new NotFoundSignal();
                }
                else throw new BadRequestSignal();
                ViewTemplate? viewTemplate;
                if (view.TemplateId == null)
                    viewTemplate = new("No template", "<!DOCTYPE html>\n<html>\n<head>\n\t<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\n\t<meta charset=\"utf-8\">\n</head>\n<body>", "\n\t", "\n\t<script src=\"refresh.js\"></script>\n</body>\n</html>");
                else if (!ViewTemplates.TryGetValue(view.TemplateId, out viewTemplate))
                    throw new NotFoundSignal();
                req.Context.Response.ContentType = "text/html;charset=utf-8";
                await req.Write(viewTemplate.BeforeElements);
                foreach (var element in view.Elements)
                {
                    if (!ElementTemplates.TryGetValue(element.TemplateId, out var elementTemplate))
                        continue;
                    string code = elementTemplate.Code;
                    int counter = 0;
                    foreach (var (component, componentValue) in elementTemplate.Components.Zip(element.Values))
                    {
                        string val = component.SupportedFileExtensions != null ? $"{req.PluginPathPrefix}/files/{HttpUtility.UrlEncode(Parsers.FromBase64PathSafe(componentValue))}" : componentValue;
                        code = code.Replace($"[VALUE_{counter}]", val)
                            .Replace($"<VALUE_{counter}>", val.HtmlSafe())
                            .Replace($"\"VALUE_{counter}\"", val.HtmlValueSafe());
                        counter++;
                    }
                    await req.Write(viewTemplate.BeforeEachElement + code);
                }
                await req.Write(viewTemplate.AfterElements);
            } break;




            // REFRESH EVENT
            case "/refresh-event":
            { req.ForceGET();
                if (req.Query.TryGetValue("display", out var displayId))
                {
                    if (!(Displays.TryGetValue(displayId, out var display) && (display.ViewId == null || Views.ContainsKey(display.ViewId))))
                        throw new NotFoundSignal();

                    lock (DisplaySubscribers)
                    {
                        if (!DisplaySubscribers.TryGetValue(displayId, out var displaySet))
                            DisplaySubscribers[displayId] = displaySet = [];
                        displaySet.Add(req);
                    }

                    if (display.ViewId != null)
                        lock (ViewSubscribers)
                        {
                            if (!ViewSubscribers.TryGetValue(display.ViewId, out var viewSet))
                                ViewSubscribers[display.ViewId] = viewSet = [];
                            viewSet.Add(req);
                        }
                }
                else if (req.Query.TryGetValue("view", out var viewId))
                {
                    if (!Views.ContainsKey(viewId))
                        throw new NotFoundSignal();

                    lock(ViewSubscribers)
                    {
                        if (!ViewSubscribers.TryGetValue(viewId, out var viewSet))
                            ViewSubscribers[viewId] = viewSet = [];
                        viewSet.Add(req);
                    }
                }
                else throw new BadRequestSignal();
                req.KeepEventAliveCancelled += Unsubscribe;
                await req.KeepEventAlive();
            } break;
            



            // 404
            default:
                req.CreatePage("Error");
                req.Status = 404;
                break;
        }
    }
}