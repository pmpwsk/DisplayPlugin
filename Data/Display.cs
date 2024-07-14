using System.Runtime.Serialization;
using uwap.Database;

namespace uwap.WebFramework.Plugins;

public partial class DisplayPlugin : Plugin
{
    [DataContract]
    private class Display(string name, string? viewId) : ITableValue
    {
        [DataMember]
        public string Name = name;

        [DataMember]
        public string? ViewId = viewId;
    }
}