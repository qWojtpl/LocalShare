
using LocalShareCommunication;

namespace LocalShareApplication.Misc;

public static class CommunicationManager
{

    public static LocalShareClient Client
    {
        get 
        {
            if (_client == null)
            {
                _client = new LocalShareClient(SettingsManager.Port, SettingsManager.CallbackPort);
                _client.Start();
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
        if (_server != null) 
        { 
            _server.Stop();
        }
        if (_client != null)
        {
            _client.Stop();
        }
    }


}
