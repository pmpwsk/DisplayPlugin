using System.Runtime.Serialization;
using uwap.Database;

namespace uwap.WebFramework.Plugins;

public partial class DisplayPlugin : Plugin
{
    [DataContract]
    private class View(string name, string? templateId, List<Element> elements) : ITableValue
    {
        [DataMember]
        public string Name = name;

        [DataMember]
        public string? TemplateId = templateId;

        [DataMember]
        public List<Element> Elements = elements;
    }
}