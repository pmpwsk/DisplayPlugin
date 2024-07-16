using System.Web;
using uwap.WebFramework.Elements;

namespace uwap.WebFramework.Plugins;

public partial class DisplayPlugin : Plugin
{
    public async Task HandleViews(Request req)
    {
        switch (req.Path)
        {
            // VIEWS
            case "/views":
            { CreatePage(req, "Views", out var page, out var e);
                page.Navigation.Add(new Button("Back", ".", "right"));
                page.Scripts.Add(Presets.SendRequestScript);
                page.Scripts.Add(new Script("views.js"));
                e.Add(new HeadingElement("Views"));
                e.Add(new ContainerElement("New:", [ new TextBox("Enter a name...", null, "name", onEnter: "Create()")] ) { Button = new ButtonJS("Create", "Create()", "green")});
                page.AddError();
                bool foundAny = false;
                foreach (var kv in Views.OrderBy(x => x.Value.Name))
                {
                    foundAny = true;
                    e.Add(new ButtonElement(kv.Value.Name, null, $"views/edit?id={kv.Key}"));
                }
                if (!foundAny)
                    e.Add(new ContainerElement("No views!", "", "red"));
            } break;

            case "/views/create":
            { POST(req);
                if (!req.Query.TryGetValue("name", out var name))
                   throw new BadRequestSignal();
                if (Views.Any(x => x.Value.Name == name))
                    throw new HttpStatusSignal(302);
                string id;
                do id = Parsers.RandomString(6);
                while (Views.ContainsKey(id));
                Views[id] = new(name, null, []);
                await req.Write(id);
            } break;




            // EDIT VIEW
            case "/views/edit":
            { CreatePage(req, "Edit view", out var page, out var e);
                page.Navigation.Add(new Button("Back", "../views", "right"));
                if (!req.Query.TryGetValue("id", out var id))
                    throw new BadRequestSignal();
                if (!Views.TryGetValue(id, out var view))
                    throw new NotFoundSignal();
                page.Scripts.Add(Presets.SendRequestScript);
                page.Scripts.Add(new Script("edit.js"));
                e.Add(new LargeContainerElement("Edit view",
                [
                    new TextBox("Enter a name...", view.Name, "name", onEnter: "Rename()"),
                    new Selector("template", [new SelectorItem("Default", "default", view.TemplateId == null), ..ViewTemplates.OrderBy(x => x.Value.Name).Select(x => new SelectorItem(x.Value.Name, x.Key, view.TemplateId == x.Key))]) {OnChange="SetTemplate()"}
                ]) { Buttons =
                [
                    new Button("Show", $"../show?view={id}", newTab: true),
                    new ButtonJS("Delete", "Delete()", "red", id: "delete")
                ]});
                page.AddError();
                int counter = 0;
                foreach (var element in view.Elements)
                {
                    if (!ElementTemplates.TryGetValue(element.TemplateId, out var elementTemplate))
                    {
                        view.Lock();
                        view.Elements.Remove(element);
                        view.UnlockSave();
                        continue;
                    }
                    e.Add(new ContainerElement(null, new Selector($"add-{counter}", [new SelectorItem("Add", "default", true), ..ElementTemplates.OrderBy(x => x.Value.Name).Select(x => new SelectorItem(x.Value.Name, x.Key))]) {OnChange=$"ElementAdd('{counter}')"}));
                    var elementElement = new ContainerElement(elementTemplate.Name);
                    e.Add(elementElement);
                    int componentCounter = 0;
                    foreach ((var component, var value) in elementTemplate.Components.Zip(element.Values))
                    {
                        elementElement.Contents.Add(new Paragraph($"{component.Name}:"));
                        elementElement.Contents.Add(component.SupportedFileExtensions == null
                            ? new TextBox("Enter something...", value, $"value-{counter}-{componentCounter}", onInput: $"ElementChanged('{counter}')")
                            : new Selector($"value-{counter}-{componentCounter}", [new SelectorItem("Select file...", "null", value == null), ..Files.Where(x => Parsers.FromBase64PathSafe(x.Key).SplitAtLast('.', out _, out var extension) && component.SupportedFileExtensions.Contains(extension)).OrderBy(x => Parsers.FromBase64PathSafe(x.Key)).Select(x => new SelectorItem(Parsers.FromBase64PathSafe(x.Key), x.Key, x.Key == value))]) {OnChange = $"ElementChanged('{counter}')"});
                        componentCounter++;
                    }
                    if (elementTemplate.Components.Count > 0)
                        elementElement.Buttons.Add(new ButtonJS("Saved!", $"ElementSave('{counter}', '{componentCounter}')", id: $"save-{counter}"));
                    elementElement.Buttons.Add(new ButtonJS("Delete", $"ElementDelete('{counter}')", "red", id: $"delete-{counter}"));
                    counter++;
                }
                e.Add(new ContainerElement(null, new Selector($"add-{counter}", [new SelectorItem("Add", "default", true), ..ElementTemplates.OrderBy(x => x.Value.Name).Select(x => new SelectorItem(x.Value.Name, x.Key))]) {OnChange=$"ElementAdd('{counter}')"}));
                page.Scripts.Add(new CustomScript($"for (var i = 0; i <= {counter}; i++)\n\tdocument.getElementById(`add-${{i}}`).value = \"default\";"));
            } break;

            case "/views/edit/delete":
            { POST(req);
                if (!req.Query.TryGetValue("id", out var id))
                    throw new BadRequestSignal();
                if (!Views.ContainsKey(id))
                    throw new NotFoundSignal();
                Views.Delete(id);
                await NotifyViewSubscribersForView(id);
            } break;

            case "/views/edit/rename":
            { POST(req);
                if (!(req.Query.TryGetValue("id", out var id) && req.Query.TryGetValue("name", out var name)))
                    throw new BadRequestSignal();
                if (!Views.TryGetValue(id, out var view))
                    throw new NotFoundSignal();
                if (view.Name != name)
                {
                    if (Views.Any(x => x.Value.Name == name))
                        throw new HttpStatusSignal(302);
                    view.Lock();
                    view.Name = name;
                    view.UnlockSave();
                }
            } break;

            case "/views/edit/set-template":
            { POST(req);
                if (!(req.Query.TryGetValue("id", out var id) && req.Query.TryGetValue("template", out var templateId)))
                    throw new BadRequestSignal();
                if (templateId == "default")
                    templateId = null;
                if (!(Views.TryGetValue(id, out var view) && (templateId == null || ViewTemplates.ContainsKey(templateId))))
                    throw new NotFoundSignal();
                view.Lock();
                view.TemplateId = templateId;
                view.UnlockSave();
                await NotifyViewSubscribersForView(id);
            } break;

            case "/views/edit/add-element":
            { POST(req);
                if (!(req.Query.TryGetValue("id", out var id) && req.Query.TryGetValue("index", out ushort index) && req.Query.TryGetValue("template", out var elementTemplateId)))
                    throw new BadRequestSignal();
                if (!(Views.TryGetValue(id, out var view) && ElementTemplates.TryGetValue(elementTemplateId, out var elementTemplate)))
                    throw new NotFoundSignal();
                if (index > view.Elements.Count)
                    throw new BadRequestSignal();
                view.Lock();
                view.Elements.Insert(index, new(elementTemplateId, elementTemplate.Components.Select(x => x.SupportedFileExtensions == null ? "" : "null").ToList()));
                view.UnlockSave();
                await NotifyViewSubscribersForView(id);
            } break;

            case "/views/edit/delete-element":
            { POST(req);
                if (!(req.Query.TryGetValue("id", out var id) && req.Query.TryGetValue("index", out ushort index)))
                    throw new BadRequestSignal();
                if (!Views.TryGetValue(id, out var view))
                    throw new NotFoundSignal();
                if (index > view.Elements.Count-1)
                    throw new BadRequestSignal();
                view.Lock();
                view.Elements.RemoveAt(index);
                view.UnlockSave();
                await NotifyViewSubscribersForView(id);
            } break;

            case "/views/edit/save-element":
            { POST(req);
                if (!(req.Query.TryGetValue("id", out var id) && req.Query.TryGetValue("index", out ushort index) && req.Query.TryGetValue("values", out var valuesEnc)))
                    throw new BadRequestSignal();
                if (!Views.TryGetValue(id, out var view))
                    throw new NotFoundSignal();
                if (index > view.Elements.Count-1)
                    throw new BadRequestSignal();
                List<string> values = valuesEnc.Split('&').Select(x => HttpUtility.UrlDecode(x)).ToList();
                if (view.Elements[index].Values.Count != values.Count)
                    throw new BadRequestSignal();
                view.Lock();
                view.Elements[index].Values = values;
                view.UnlockSave();
                await NotifyViewSubscribersForView(id);
            } break;
            



            // 404
            default:
                req.CreatePage("Error");
                req.Status = 404;
                break;
        }
    }
}