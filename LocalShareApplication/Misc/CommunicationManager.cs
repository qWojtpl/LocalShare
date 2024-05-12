
using LocalShareCommunication;

namespace LocalShareApplication.Misc;

public static class CommunicationManager
{

    private static List<Action> clientStartHandlers = new List<Action>();

    public static LocalShareClient Client
    {
        get 
        {
            if (_client == null && SettingsManager.ListenForNewFiles)
            {
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

}
