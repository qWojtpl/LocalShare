
using System.Text.Json;

namespace LocalShareApplication.Misc;

public static class LanguageManager
{

    private static bool loaded = false;
    private static Dictionary<string, string> language = new();

    public static void SwitchLanguage()
    {
        string currentLanguage = SettingsManager.Language;
        if (currentLanguage.Equals("en"))
        {
            SettingsManager.Language = "pl";
        } else
        {
            SettingsManager.Language = "en";
        }
        loaded = false;
    }

    private static void LoadLanguage()
    {
        using var stream = FileSystem.OpenAppPackageFileAsync(SettingsManager.Language + ".json");
        using var reader = new StreamReader(stream.Result);

        var contents = reader.ReadToEnd();
        var result = JsonSerializer.Deserialize<Dictionary<string, string>>(contents);

        if (result != null)
        {
            language = result;
            loaded = true;
        }

    }

    public static string Get(string key)
    {
        if(!loaded)
        {
            LoadLanguage();
        }
        return language[key];
    }

}
