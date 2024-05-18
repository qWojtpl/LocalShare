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

    public static int MaxBytesPerPacket
    {
        get
        {
            return GetSetting("maxBytesPerPacket", Default.MaxBytesPerPacket);
        }
        set
        {
            SetSetting("maxBytesPerPacket", value);
        }
    }

    public static string Language
    {
        get
        {
            return GetSetting("language", Default.Language);
        }
        set
        {
            SetSetting("language", value);
        }
    }

    public static bool ListenForNewFiles
    {
        get
        {
            return GetSetting("listenForNewFiles", Default.ListenForNewFiles);
        }
        set
        {
            SetSetting("listenForNewFiles", value);
        }
    }

    public static string History
    {
        get
        {
            return GetSetting("history", Default.History);
        }
        set
        {
            SetSetting("history", value);
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

        public static int MaxBytesPerPacket
        {
            get => 1447;
        }

        public static string Language
        {
            get => "en";
        }

        public static bool ListenForNewFiles
        {
            get => true;
        }

        public static string History
        {
            get => "";
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

    private static string GetSetting(string key, string defaultValue)
    {
        return Preferences.Default.Get(key, defaultValue);
    }

    private static void SetSetting(string key, string value)
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
