using uwap.Database;

namespace uwap.WebFramework.Plugins;

public partial class DisplayPlugin : Plugin
{
    private readonly Table<Display> Displays = Table<Display>.Import("DisplayPlugin.Displays");

    private readonly Table<View> Views = Table<View>.Import("DisplayPlugin.Views");

    private readonly Table<ViewTemplate> ViewTemplates = Table<ViewTemplate>.Import("DisplayPlugin.ViewTemplates");

    private readonly Table<ElementTemplate> ElementTemplates = Table<ElementTemplate>.Import("DisplayPlugin.ElementTemplates");

    private readonly FileDataTable Files = FileDataTable.Import("DisplayPlugin.Files");

    private class FileDataTable : Table<FileData>
    {
        private FileDataTable(string name) : base(name) { }

        protected static new FileDataTable Create(string name)
        {
            if (!name.All(Tables.KeyChars.Contains))
                throw new Exception($"This name contains characters that are not part of Tables.KeyChars ({Tables.KeyChars}).");
            if (Directory.Exists("../Database/" + name))
                throw new Exception("A table with this name already exists, try importing it instead.");
            Directory.CreateDirectory("../Database/" + name);
            FileDataTable table = new(name);
            Tables.Dictionary[name] = table;
            return table;
        }

        public static new FileDataTable Import(string name, bool skipBroken = false)
        {
            if (Tables.Dictionary.TryGetValue(name, out ITable? table))
                return (FileDataTable)table;
            if (!name.All(Tables.KeyChars.Contains))
                throw new Exception($"This name contains characters that are not part of Tables.KeyChars ({Tables.KeyChars}).");
            if (!Directory.Exists("../Database/" + name))
                return Create(name);

            if (Directory.Exists("../Database/Buffer/" + name) && Directory.GetFiles("../Database/Buffer/" + name, "*.json", SearchOption.AllDirectories).Length > 0)
                Console.WriteLine($"The database buffer of table '{name}' contains an entry because a database operation was interrupted. Please manually merge the files and delete the file from the buffer.");

            FileDataTable result = new(name);
            result.Reload(skipBroken);
            Tables.Dictionary[name] = result;
            return result;
        }

        protected override IEnumerable<string> EnumerateDirectoriesToClear()
        {
            yield return "../DisplayPlugin.Files";
        }

        protected override IEnumerable<string> EnumerateOtherFiles(TableEntry<FileData> entry)
        {
            yield return $"../DisplayPlugin.Files/{entry.Key}";
        }
    }
}