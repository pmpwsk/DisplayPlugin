using System.Runtime.Serialization;
using uwap.Database;

namespace uwap.WebFramework.Plugins;

public partial class DisplayPlugin : Plugin
{
    [DataContract]
    private class ElementTemplateComponent(string name, HashSet<string>? supportedFileExtensions) : ITableValue
    {
        [DataMember]
        public string Name = name;

        [DataMember]
        public HashSet<string>? SupportedFileExtensions = supportedFileExtensions;
    }
}