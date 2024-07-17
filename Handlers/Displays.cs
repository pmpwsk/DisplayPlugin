using uwap.WebFramework.Elements;

namespace uwap.WebFramework.Plugins;

public partial class DisplayPlugin : Plugin
{
    public async Task HandleDisplays(Request req)
    {
        switch (req.Path)
        {
            // DISPLAYS
            case "/displays":
            { CreatePage(req, "Displays", out var page, out var e);
                page.Navigation.Add(new Button("Back", ".", "right"));
                e.Add(new HeadingElement("Displays"));
                page.Scripts.Add(Presets.SendRequestScript);
                page.Scripts.Add(new Script("displays.js"));
                e.Add(new ContainerElement("New:", [ new TextBox("Enter a name...", null, "name", onEnter: "Create()")] ) { Button = new ButtonJS("Create", "Create()", "green")});
                page.AddError();
                bool foundAny = false;
                foreach (var kv in Displays.OrderBy(x => x.Value.Name))
                {
                    foundAny = true;
                    e.Add(new ButtonElement(kv.Value.Name, null, $"displays/edit?id={kv.Key}"));
                }
                if (!foundAny)
                    e.Add(new ContainerElement("No displays!", "", "red"));
            } break;

            case "/displays/create":
            { POST(req);
                if (!req.Query.TryGetValue("name", out var name))
                   throw new BadRequestSignal();
                if (Displays.Any(x => x.Value.Name == name))
                    throw new HttpStatusSignal(302);
                string id;
                do id = Parsers.RandomString(6);
                while (Displays.ContainsKey(id));
                Displays[id] = new(name, null);
                await req.Write(id);
            } break;
            



            // EDIT DISPLAY
            case "/displays/edit":
            { CreatePage(req, "Edit display", out var page, out var e);
                if (!req.Query.TryGetValue("id", out var id))
                    throw new BadRequestSignal();
                if (!Displays.TryGetValue(id, out var display))
                    throw new NotFoundSignal();
                page.Scripts.Add(Presets.SendRequestScript);
                page.Scripts.Add(new Script("edit.js"));
                page.Navigation.Add(new Button("Back", "../displays", "right"));
                e.Add(new LargeContainerElement("Edit display", new TextBox("Enter a name...", display.Name, "name", onEnter: "Rename()")) { Buttons =
                [
                    new ButtonJS("Refresh", "Refresh()", id: "refresh"),
                    new ButtonJS("Delete", "Delete()", "red", id: "delete")
                ]});
                page.AddError();
                e.Add(new ButtonElement("Show", null, $"../show?display={id}", "green", newTab: true));
                e.Add(new ContainerElement("View", new Selector("view", [new SelectorItem("Select view...", "default", display.ViewId == null), ..Views.OrderBy(x => x.Value.Name).Select(x => new SelectorItem(x.Value.Name, x.Key, display.ViewId == x.Key))]) {OnChange="SetView()"}));
            } break;

            case "/displays/edit/delete":
            { POST(req);
                if (!req.Query.TryGetValue("id", out var id))
                    throw new BadRequestSignal();
                if (!Displays.ContainsKey(id))
                    throw new NotFoundSignal();
                Displays.Delete(id);
                lock (DisplaySubscribers)
                    DisplaySubscribers.Remove(id);
            } break;

            case "/displays/edit/rename":
            { POST(req);
                if (!(req.Query.TryGetValue("id", out var id) && req.Query.TryGetValue("name", out var name)))
                    throw new BadRequestSignal();
                if (!Displays.TryGetValue(id, out var display))
                    throw new NotFoundSignal();
                if (display.Name != name)
                {
                    if (Displays.Any(x => x.Value.Name == name))
                        throw new HttpStatusSignal(302);
                    display.Lock();
                    display.Name = name;
                    display.UnlockSave();
                }
            } break;

            case "/displays/edit/refresh":
            { POST(req);
                if (!req.Query.TryGetValue("id", out var id))
                    throw new BadRequestSignal();
                await NotifyViewSubscribersForDisplay(id);
            } break;

            case "/displays/edit/set-view":
            { POST(req);
                if (!(req.Query.TryGetValue("id", out var id) && req.Query.TryGetValue("view", out var viewId)))
                    throw new BadRequestSignal();
                if (viewId == "default")
                    viewId = null;
                else if (!Views.ContainsKey(viewId))
                    throw new NotFoundSignal();
                if (!Displays.TryGetValue(id, out var display))
                    throw new NotFoundSignal();
                if (display.ViewId != null)
                    lock (ViewSubscribers)
                        if (ViewSubscribers.TryGetValue(display.ViewId, out var viewSet))
                        {
                            lock (DisplaySubscribers)
                                if (DisplaySubscribers.TryGetValue(id, out var displaySet))
                                    foreach (var r in displaySet)
                                        viewSet.Remove(r);
                            if (viewSet.Count == 0)
                                ViewSubscribers.Remove(display.ViewId);
                        }
                display.Lock();
                display.ViewId = viewId;
                display.UnlockSave();
                await NotifyViewSubscribersForDisplay(id);
            } break;
            



            // 404
            default:
                req.CreatePage("Error");
                req.Status = 404;
                break;
        }
    }
}