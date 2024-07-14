using System.Runtime.Serialization;
using uwap.Database;

namespace uwap.WebFramework.Plugins;

public partial class DisplayPlugin : Plugin
{
    [DataContract]
    private class ViewTemplate(string name, string beforeElements, string beforeEachElement, string afterElements) : ITableValue
    {
        [DataMember]
        public string Name = name;

        [DataMember]
        public string BeforeElements = beforeElements;

        [DataMember]
        public string BeforeEachElement = beforeEachElement;

        [DataMember]
        public string AfterElements = afterElements;
    }
}