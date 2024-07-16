using System.Web;
using uwap.WebFramework.Elements;

namespace uwap.WebFramework.Plugins;

public partial class DisplayPlugin : Plugin
{
    public async Task HandleFiles(Request req)
    {
        switch (req.Path)
        {
            // FILES
            case "/files":
            { CreatePage(req, "Files", out var page, out var e);
                page.Scripts.Add(Presets.SendRequestScript);
                page.Scripts.Add(new Script("files.js"));
                page.Navigation.Add(new Button("Back", ".", "right"));
                e.Add(new HeadingElement("Files"));
                e.Add(new ContainerElement("Add/change", new FileSelector("files", true)) { Button = new ButtonJS("Upload", "Upload()", "green", id: "upload") });
                page.AddError();
                bool foundAny = false;
                foreach (var kv in Files.OrderBy(x => Parsers.FromBase64PathSafe(x.Key)))
                {
                    foundAny = true;
                    string name = Parsers.FromBase64PathSafe(kv.Key);
                    e.Add(new ContainerElement(name, $"{kv.Value.ModifiedUTC.ToLongDateString()} UTC") { Buttons =
                    [
                        new Button("View", $"files/{HttpUtility.UrlEncode(name)}", newTab: true),
                        new ButtonJS("Delete", $"Delete('{kv.Key}')", "red", id: $"delete-{kv.Key}")
                    ]});
                }
                if (!foundAny)
                    e.Add(new ContainerElement("No files!", "", "red"));
            } break;

            case "/files/upload":
            { POST(req);
                Directory.CreateDirectory("../DisplayPlugin.Files");
                req.BodySizeLimit = null;
                if ((!req.IsForm) || req.Files.Count == 0)
                    throw new BadRequestSignal();
                foreach (var file in req.Files)
                {
                    string key = Parsers.ToBase64PathSafe(file.FileName);
                    file.Download($"../DisplayPlugin.Files/{key}", long.MaxValue);
                    if (Files.TryGetValue(key, out var data))
                    {
                        data.Lock();
                        data.ModifiedUTC = DateTime.UtcNow;
                        data.UnlockSave();
                    }
                    else Files[key] = new(DateTime.UtcNow);

                    foreach (var kv in Views)
                        if (kv.Value.Elements.Any(x => x.Values.Contains(file.FileName)))
                            await NotifyViewSubscribersForView(kv.Key);
                }
            } break;

            case "/files/delete":
            { POST(req);
                if (!req.Query.TryGetValue("key", out var key))
                    throw new BadRequestSignal();
                if (Files.Delete(key))
                    File.Delete($"../DisplayPlugin.Files/{key}");
            } break;
            



            // 404
            default:
            {
                string name = HttpUtility.UrlDecode(req.Path[7..]);
                string key = Parsers.ToBase64PathSafe(name);
                if (Files.TryGetValue(key, out var data))
                {
                    if (name.SplitAtLast('.', out _, out var extension))
                    {
                        req.Context.Response.ContentType = Server.Config.MimeTypes.TryGetValue('.' + extension, out var contentType) ? contentType : null;
                        if (Server.Config.BrowserCacheMaxAge.TryGetValue('.' + extension, out int maxAge))
                        {
                            if (maxAge == 0)
                                req.Context.Response.Headers.CacheControl = "no-cache, private";
                            else
                            {
                                string timestamp = data.ModifiedUTC.Ticks.ToString();
                                req.Context.Response.Headers.CacheControl = "public, max-age=0";
                                if (req.Context.Request.Headers.TryGetValue("If-None-Match", out var oldTag) && oldTag == timestamp)
                                    throw new HttpStatusSignal(304);
                                else req.Context.Response.Headers.ETag = timestamp;
                            }
                        }
                    }
                    await req.WriteFile($"../DisplayPlugin.Files/{key}");
                    break;
                }
                req.CreatePage("Error");
                req.Status = 404;
            } break;
        }
    }
}