using System.Runtime.Serialization;
using uwap.Database;

namespace uwap.WebFramework.Plugins;

public partial class DisplayPlugin : Plugin
{
    [DataContract]
    private class Element(string templateId, List<string> values) : ITableValue
    {
        [DataMember]
        public string TemplateId = templateId;

        [DataMember]
        public List<string> Values = values;
    }
}