using System.Runtime.Serialization;
using uwap.Database;

namespace uwap.WebFramework.Plugins;

public partial class DisplayPlugin : Plugin
{
    [DataContract]
    private class FileData(DateTime modifiedUTC) : ITableValue
    {
        [DataMember]
        public DateTime ModifiedUTC = modifiedUTC;
    }
}