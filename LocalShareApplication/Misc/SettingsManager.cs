namespace LocalShareApplication.Misc;

public static class SettingsManager
{

    public static int Port 
    {
        get {
            return GetSetting("port", Default.Port);
        }
        set {
            SetSetting("port", value);
        }
    }

    public static int CallbackPort
    {
        get
        {
            return GetSetting("callbackPort", Default.CallbackPort);
        }
        set
        {
            SetSetting("callbackPort", value);
        }
    }

    public class Default
    {
        public static int Port
        {
            get => 2780;
        }

        public static int CallbackPort
        {
            get => 2781;
        }
    }

    private static int GetSetting(string key, int defaultValue)
    {
        return Preferences.Default.Get(key, defaultValue);
    }

    private static void SetSetting(string key, int value)
    {
        Preferences.Default.Set(key, value);
    }

    private static bool GetSetting(string key, bool defaultValue)
    {
        return Preferences.Default.Get(key, defaultValue);
    }

    private static void SetSetting(string key, bool value)
    {
        Preferences.Default.Set(key, value);
    }

}
