
using LocalShareCommunication;
using LocalShareCommunication.Client;
using LocalShareCommunication.Server;

namespace LocalShareApplication.Misc;

public static class CommunicationManager
{

    private static List<Action> clientStartHandlers = new List<Action>();
    private static bool pathInitialized = false;
    private static bool maxBytesPerPacketInitialized = false;

    public static LocalShareClient? Client
    {
        get 
        {
            if (_client == null && SettingsManager.ListenForNewFiles)
            {
                InitPath();
                InitMaxBytesPerPacket();
                _client = new LocalShareClient(SettingsManager.Port, SettingsManager.CallbackPort);
                _client.Start();
                foreach(Action handler in clientStartHandlers)
                {
                    handler.Invoke();
                }
            }
            return _client;
        }
    }
    
    private static LocalShareClient? _client;

    public static LocalShareServer Server
    {
        get
        {
            if (_server == null)
            {
                InitPath();
                InitMaxBytesPerPacket();
                _server = new LocalShareServer(SettingsManager.Port, SettingsManager.CallbackPort);
                _server.Start();
            }
            return _server;
        }
    }

    private static LocalShareServer? _server;

    public static void StopAll()
    {
        StopServer();
        StopClient();
    }

    public static void StopClient()
    {
        if (_client != null)
        {
            _client.Stop();
            _client = null;
        }
    }

    public static void StopServer()
    {
        if (_server != null)
        {
            _server.Stop();
            _server = null;
        }
    }

    public static void AddClientStartHandler(Action action)
    {
        clientStartHandlers.Add(action);
        if (Client != null)
        {
            action.Invoke();
        }
    }

    private static void InitPath()
    {
        if (pathInitialized)
        {
            return;
        }
        pathInitialized = true;
#if WINDOWS
        Shared.FilesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LocalShare/files") + "/";
#endif
#if ANDROID
        Shared.FilesPath = Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath, "LocalShare/");
#endif

    }

    private static void InitMaxBytesPerPacket()
    {
        if (maxBytesPerPacketInitialized)
        {
            return;
        }
        maxBytesPerPacketInitialized = true;
        Shared.MaxDataSize = SettingsManager.MaxBytesPerPacket;
    }

}
