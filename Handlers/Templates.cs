using uwap.WebFramework.Elements;

namespace uwap.WebFramework.Plugins;

public partial class DisplayPlugin : Plugin
{
    public async Task HandleTemplates(Request req)
    {
        switch (req.Path)
        {
            // TEMPLATES
            case "/templates":
            { CreatePage(req, "Templates", out var page, out var e);
                page.Navigation.Add(new Button("Back", ".", "right"));
                e.Add(new HeadingElement("Templates"));
                e.Add(new ButtonElement("View templates", null, "templates/views"));
                e.Add(new ButtonElement("Element templates", null, "templates/elements"));
            } break;
            



            // VIEW TEMPLATES
            case "/templates/views":
            { CreatePage(req, "View templates", out var page, out var e);
                page.Navigation.Add(new Button("Back", "../templates", "right"));
                page.Scripts.Add(Presets.SendRequestScript);
                page.Scripts.Add(new Script("views.js"));
                e.Add(new HeadingElement("View templates"));
                e.Add(new ContainerElement("New:", [ new TextBox("Enter a name...", null, "name", onEnter: "Create()")] ) { Button = new ButtonJS("Create", "Create()", "green")});
                page.AddError();
                bool foundAny = false;
                foreach (var kv in ViewTemplates.OrderBy(x => x.Value.Name))
                {
                    foundAny = true;
                    e.Add(new ButtonElement(kv.Value.Name, null, $"views/edit?id={kv.Key}"));
                }
                if (!foundAny)
                    e.Add(new ContainerElement("No view templates!", "", "red"));
            } break;

            case "/templates/views/create":
            { POST(req);
                if (!req.Query.TryGetValue("name", out var name))
                   throw new BadRequestSignal();
                if (ViewTemplates.Any(x => x.Value.Name == name))
                    throw new HttpStatusSignal(302);
                string id;
                do id = Parsers.RandomString(6);
                while (ViewTemplates.ContainsKey(id));
                ViewTemplates[id] = new(name, "<!DOCTYPE html>\n<html>\n<head>\n\t<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\n\t<meta charset=\"utf-8\">\n</head>\n<body>", "\n\t", "\n\t<script src=\"refresh.js\"></script>\n</body>\n</html>");
                await req.Write(id);
            } break;
            



            // EDIT VIEW TEMPLATE
            case "/templates/views/edit":
            { CreatePage(req, "Edit view template", out var page, out var e);
                if (!req.Query.TryGetValue("id", out var id))
                    throw new BadRequestSignal();
                if (!ViewTemplates.TryGetValue(id, out var viewTemplate))
                    throw new NotFoundSignal();
                page.Scripts.Add(Presets.SendRequestScript);
                page.Scripts.Add(new Script("edit.js"));
                page.Navigation.Add(new Button("Back", "../views", "right"));
                e.Add(new LargeContainerElement("Edit view template", new TextBox("Enter a name...", viewTemplate.Name, "name", onEnter: "Rename()")) { Button = new ButtonJS("Delete", "Delete()", "red", id: "delete")});
                page.AddError();
                e.Add(new ContainerElement(null,
                [
                    new Heading("Before elements:"),
                    new TextArea("Enter HTML code...", viewTemplate.BeforeElements, "before-elements", 8, onInput: "Changed()"),
                    new Heading("Before each element:"),
                    new TextArea("Enter HTML code...", viewTemplate.BeforeEachElement, "before-each-element", 3, onInput: "Changed()"),
                    new Heading("After elements:"),
                    new TextArea("Enter HTML code...", viewTemplate.AfterElements, "after-elements", 8, onInput: "Changed()")
                ]) { Button = new ButtonJS("Saved!", "Save()", id: "save")});
            } break;

            case "/templates/views/edit/delete":
            { POST(req);
                if (!req.Query.TryGetValue("id", out var id))
                    throw new BadRequestSignal();
                if (!ViewTemplates.ContainsKey(id))
                    throw new NotFoundSignal();
                foreach (var kv in Views)
                    if (kv.Value.TemplateId == id)
                    {
                        kv.Value.Lock();
                        kv.Value.TemplateId = null;
                        kv.Value.UnlockSave();
                    }
                ViewTemplates.Delete(id);
                await NotifyViewSubscribersForViewTemplate(id);
            } break;

            case "/templates/views/edit/rename":
            { POST(req);
                if (!(req.Query.TryGetValue("id", out var id) && req.Query.TryGetValue("name", out var name)))
                    throw new BadRequestSignal();
                if (!ViewTemplates.TryGetValue(id, out var viewTemplate))
                    throw new NotFoundSignal();
                if (viewTemplate.Name != name)
                {
                    if (ViewTemplates.Any(x => x.Value.Name == name))
                        throw new HttpStatusSignal(302);
                    viewTemplate.Lock();
                    viewTemplate.Name = name;
                    viewTemplate.UnlockSave();
                }
            } break;

            case "/templates/views/edit/save":
            { POST(req);
                if (!(req.Query.TryGetValue("id", out var id)
                    && req.Query.TryGetValue("before-elements", out var beforeElements)
                    && req.Query.TryGetValue("before-each-element", out var beforeEachElement)
                    && req.Query.TryGetValue("after-elements", out var afterElements)))
                    throw new BadRequestSignal();
                if (!ViewTemplates.TryGetValue(id, out var viewTemplate))
                    throw new NotFoundSignal();
                viewTemplate.Lock();
                viewTemplate.BeforeElements = beforeElements;
                viewTemplate.BeforeEachElement = beforeEachElement;
                viewTemplate.AfterElements = afterElements;
                viewTemplate.UnlockSave();
                await NotifyViewSubscribersForViewTemplate(id);
            } break;
            



            // ELEMENT TEMPLATES
            case "/templates/elements":
            { CreatePage(req, "Element templates", out var page, out var e);
                page.Navigation.Add(new Button("Back", "../templates", "right"));
                e.Add(new HeadingElement("Element templates"));
                page.Scripts.Add(Presets.SendRequestScript);
                page.Scripts.Add(new Script("elements.js"));
                e.Add(new ContainerElement("New:", [ new TextBox("Enter a name...", null, "name", onEnter: "Create()")] ) { Button = new ButtonJS("Create", "Create()", "green")});
                page.AddError();
                bool foundAny = false;
                foreach (var kv in ElementTemplates.OrderBy(x => x.Value.Name))
                {
                    foundAny = true;
                    e.Add(new ButtonElement(kv.Value.Name, null, $"elements/edit?id={kv.Key}"));
                }
                if (!foundAny)
                    e.Add(new ContainerElement("No element templates!", "", "red"));
            } break;

            case "/templates/elements/create":
            { POST(req);
                if (!req.Query.TryGetValue("name", out var name))
                   throw new BadRequestSignal();
                if (ElementTemplates.Any(x => x.Value.Name == name))
                    throw new HttpStatusSignal(302);
                string id;
                do id = Parsers.RandomString(6);
                while (ElementTemplates.ContainsKey(id));
                ElementTemplates[id] = new(name, "", []);
                await req.Write(id);
            } break;
            



            // EDIT ELEMENT TEMPLATE
            case "/templates/elements/edit":
            { CreatePage(req, "Edit element template", out var page, out var e);
                if (!req.Query.TryGetValue("id", out var id))
                    throw new BadRequestSignal();
                if (!ElementTemplates.TryGetValue(id, out var elementTemplate))
                    throw new NotFoundSignal();
                page.Scripts.Add(Presets.SendRequestScript);
                page.Scripts.Add(new Script("edit.js"));
                page.Navigation.Add(new Button("Back", "../elements", "right"));
                e.Add(new LargeContainerElement("Edit element template", new TextBox("Enter a name...", elementTemplate.Name, "name", onEnter: "Rename()")) { Button = new ButtonJS("Delete", "Delete()", "red", id: "delete")});
                page.AddError();
                e.Add(new ContainerElement("HTML code", new TextArea("Enter HTML code...", elementTemplate.Code, "code", 5, onInput: "CodeChanged()")) { Button = new ButtonJS("Saved!", "Save()", id: "save")});
                int counter = 0;
                foreach (var component in elementTemplate.Components)
                {
                    e.Add(new ContainerElement(null, new Selector($"add-{counter}", new SelectorItem("Add", "default", true), new("Text", "text"), new("File", "file")) {OnChange=$"ComponentAdd('{counter}')"}));
                    e.Add(new ContainerElement($"[VALUE_{counter}] ({(component.SupportedFileExtensions == null ? "Text" : "File")})",
                    [
                        new TextBox("Enter a name...", component.Name, $"name-{counter}", onEnter: $"ComponentSave('{counter}')", onInput: $"ComponentChanged('{counter}')"),
                        ..(IEnumerable<IContent>)(component.SupportedFileExtensions == null ? [] : [new TextBox("Enter file extensions (no dots)...", string.Join(", ", component.SupportedFileExtensions), $"types-{counter}", onEnter: $"ComponentSave('{counter}')", onInput: $"ComponentChanged('{counter}')")])
                    ]) { Buttons =
                    [
                        new ButtonJS("Saved!", $"ComponentSave('{counter}')", id: $"save-{counter}"),
                        new ButtonJS("Delete", $"ComponentDelete('{counter}')", "red", id: $"delete-{counter}")
                    ]});
                    counter++;
                }
                e.Add(new ContainerElement(null, new Selector($"add-{counter}", new SelectorItem("Add", "default", true), new("Text", "text"), new("File", "file")) {OnChange=$"ComponentAdd('{counter}')"}));
                page.Scripts.Add(new CustomScript($"for (var i = 0; i <= {counter}; i++)\n\tdocument.getElementById(`add-${{i}}`).value = \"default\";"));
            } break;

            case "/templates/elements/edit/save":
            { POST(req);
                if (!(req.Query.TryGetValue("id", out var id) && req.Query.TryGetValue("code", out var code)))
                    throw new BadRequestSignal();
                if (!ElementTemplates.TryGetValue(id, out var elementTemplate))
                    throw new NotFoundSignal();
                elementTemplate.Lock();
                elementTemplate.Code = code;
                elementTemplate.UnlockSave();
                await NotifyViewSubscribersForElementTemplate(id);
            } break;

            case "/templates/elements/edit/delete":
            { POST(req);
                if (!req.Query.TryGetValue("id", out var id))
                    throw new BadRequestSignal();
                if (!ElementTemplates.ContainsKey(id))
                    throw new NotFoundSignal();
                foreach (var kv in Views)
                {
                    kv.Value.Lock();
                    var newElements = kv.Value.Elements.Where(x => x.TemplateId != id).ToList();
                    if (kv.Value.Elements.Count != newElements.Count)
                    {
                        kv.Value.Elements = newElements;
                        kv.Value.UnlockSave();
                    }
                    else kv.Value.UnlockIgnore();
                }
                ElementTemplates.Delete(id);
                await NotifyViewSubscribersForElementTemplate(id);
            } break;

            case "/templates/elements/edit/rename":
            { POST(req);
                if (!(req.Query.TryGetValue("id", out var id) && req.Query.TryGetValue("name", out var name)))
                    throw new BadRequestSignal();
                if (!ElementTemplates.TryGetValue(id, out var elementTemplate))
                    throw new NotFoundSignal();
                if (elementTemplate.Name != name)
                {
                    if (ElementTemplates.Any(x => x.Value.Name == name))
                        throw new HttpStatusSignal(302);
                    elementTemplate.Lock();
                    elementTemplate.Name = name;
                    elementTemplate.UnlockSave();
                }
            } break;

            case "/templates/elements/edit/add-component":
            { POST(req);
                if (!(req.Query.TryGetValue("id", out var id) && req.Query.TryGetValue("index", out ushort index) && req.Query.TryGetValue("type", out var type)))
                    throw new BadRequestSignal();
                if (!ElementTemplates.TryGetValue(id, out var elementTemplate))
                    throw new NotFoundSignal();
                if (index > elementTemplate.Components.Count)
                    throw new BadRequestSignal();
                ElementTemplateComponent component = new("Value", type switch
                {
                    "text" => null,
                    "file" => [],
                    _ => throw new BadRequestSignal()
                });
                foreach (var kv in Views)
                    foreach (var element in kv.Value.Elements)
                        if (element.TemplateId == id)
                        {
                            kv.Value.Lock();
                            element.Values.Insert(index, "");
                            kv.Value.UnlockSave();
                        }
                elementTemplate.Lock();
                elementTemplate.Components.Insert(index, component);
                elementTemplate.UnlockSave();
                await NotifyViewSubscribersForElementTemplate(id);
            } break;

            case "/templates/elements/edit/delete-component":
            { POST(req);
                if (!(req.Query.TryGetValue("id", out var id) && req.Query.TryGetValue("index", out ushort index)))
                    throw new BadRequestSignal();
                if (!ElementTemplates.TryGetValue(id, out var elementTemplate))
                    throw new NotFoundSignal();
                if (index > elementTemplate.Components.Count-1)
                    throw new BadRequestSignal();
                foreach (var kv in Views)
                    foreach (var element in kv.Value.Elements)
                        if (element.TemplateId == id)
                        {
                            kv.Value.Lock();
                            element.Values.RemoveAt(index);
                            kv.Value.UnlockSave();
                        }
                elementTemplate.Lock();
                elementTemplate.Components.RemoveAt(index);
                elementTemplate.UnlockSave();
                await NotifyViewSubscribersForElementTemplate(id);
            } break;

            case "/templates/elements/edit/save-component":
            { POST(req);
                if (!(req.Query.TryGetValue("id", out var id) && req.Query.TryGetValue("index", out ushort index) && req.Query.TryGetValue("name", out var name)))
                    throw new BadRequestSignal();
                if (!ElementTemplates.TryGetValue(id, out var elementTemplate))
                    throw new NotFoundSignal();
                if (index > elementTemplate.Components.Count-1)
                    throw new BadRequestSignal();
                var component = elementTemplate.Components[index];
                HashSet<string>? supportedFileExtensions;
                if (component.SupportedFileExtensions != null)
                    if (req.Query.TryGetValue("types", out var types))
                        supportedFileExtensions = [..types.Split(',', ' ', ';').Where(x => x != "")];
                    else throw new BadRequestSignal();
                else supportedFileExtensions = null;
                elementTemplate.Lock();
                component.Name = name;
                component.SupportedFileExtensions = supportedFileExtensions;
                elementTemplate.UnlockSave();
                await NotifyViewSubscribersForElementTemplate(id);
            } break;
            



            // 404
            default:
                req.CreatePage("Error");
                req.Status = 404;
                break;
        }
    }
}