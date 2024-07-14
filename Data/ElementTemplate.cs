using System.Runtime.Serialization;
using uwap.Database;

namespace uwap.WebFramework.Plugins;

public partial class DisplayPlugin : Plugin
{
    [DataContract]
    private class ElementTemplate(string name, string code, List<ElementTemplateComponent> components) : ITableValue
    {
        [DataMember]
        public string Name = name;

        [DataMember]
        public string Code = code;

        [DataMember]
        public List<ElementTemplateComponent> Components = components;
    }
}