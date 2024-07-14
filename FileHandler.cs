namespace uwap.WebFramework.Plugins;

public partial class DisplayPlugin : Plugin
{
	public override byte[]? GetFile(string relPath, string pathPrefix, string domain)
		=> relPath switch
		{
			"/displays.js" => (byte[]?)PluginFiles_ResourceManager.GetObject("File0"),
			"/files.js" => (byte[]?)PluginFiles_ResourceManager.GetObject("File1"),
			"/icon.ico" => (byte[]?)PluginFiles_ResourceManager.GetObject("File2"),
			"/icon.png" => (byte[]?)PluginFiles_ResourceManager.GetObject("File3"),
			"/icon.svg" => (byte[]?)PluginFiles_ResourceManager.GetObject("File4"),
			"/manifest.json" => System.Text.Encoding.UTF8.GetBytes($"{{\r\n  \"name\": \"Manage display\",\r\n  \"short_name\": \"Manage display\",\r\n  \"start_url\": \"{(pathPrefix == "" ? "/" : pathPrefix)}\",\r\n  \"display\": \"minimal-ui\",\r\n  \"background_color\": \"#000000\",\r\n  \"theme_color\": \"#202024\",\r\n  \"orientation\": \"portrait-primary\",\r\n  \"icons\": [\r\n    {{\r\n      \"src\": \"{pathPrefix}/icon.svg\",\r\n      \"type\": \"image/svg+xml\",\r\n      \"sizes\": \"any\"\r\n    }},\r\n    {{\r\n      \"src\": \"{pathPrefix}/icon.png\",\r\n      \"type\": \"image/png\",\r\n      \"sizes\": \"512x512\"\r\n    }},\r\n    {{\r\n      \"src\": \"{pathPrefix}/icon.ico\",\r\n      \"type\": \"image/x-icon\",\r\n      \"sizes\": \"16x16 24x24 32x32 48x48 64x64 72x72 96x96 128x128 256x256\"\r\n    }}\r\n  ],\r\n  \"launch_handler\": {{\r\n    \"client_mode\": \"navigate-new\"\r\n  }},\r\n  \"related_applications\": [\r\n    {{\r\n      \"platform\": \"webapp\",\r\n      \"url\": \"{pathPrefix}/manifest.json\"\r\n    }}\r\n  ],\r\n  \"offline_enabled\": false,\r\n  \"omnibox\": {{\r\n    \"keyword\": \"display\"\r\n  }},\r\n  \"version\": \"1.0.0\"\r\n}}\r\n"),
			"/refresh.js" => (byte[]?)PluginFiles_ResourceManager.GetObject("File5"),
			"/views.js" => (byte[]?)PluginFiles_ResourceManager.GetObject("File6"),
			"/displays/edit.js" => (byte[]?)PluginFiles_ResourceManager.GetObject("File7"),
			"/templates/elements.js" => (byte[]?)PluginFiles_ResourceManager.GetObject("File8"),
			"/templates/views.js" => (byte[]?)PluginFiles_ResourceManager.GetObject("File9"),
			"/templates/elements/edit.js" => (byte[]?)PluginFiles_ResourceManager.GetObject("File10"),
			"/templates/views/edit.js" => (byte[]?)PluginFiles_ResourceManager.GetObject("File11"),
			"/views/edit.js" => (byte[]?)PluginFiles_ResourceManager.GetObject("File12"),
			_ => null
		};
	
	public override string? GetFileVersion(string relPath)
		=> relPath switch
		{
			"/displays.js" => "1720704075494",
			"/files.js" => "1720699745064",
			"/icon.ico" => "1719681702453",
			"/icon.png" => "1719681535559",
			"/icon.svg" => "1719681536839",
			"/manifest.json" => "1719681801174",
			"/refresh.js" => "1720434861744",
			"/views.js" => "1720615925955",
			"/displays/edit.js" => "1720704540592",
			"/templates/elements.js" => "1720615925958",
			"/templates/views.js" => "1720615925958",
			"/templates/elements/edit.js" => "1720615937288",
			"/templates/views/edit.js" => "1720615937298",
			"/views/edit.js" => "1720697889516",
			_ => null
		};
	
	private static readonly System.Resources.ResourceManager PluginFiles_ResourceManager = new("DisplayPlugin.Properties.PluginFiles", typeof(DisplayPlugin).Assembly);
}