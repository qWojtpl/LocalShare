
using LocalShareCommunication.Misc;
using System.Net;
using System.Net.Sockets;
#if ANDROID
using Android.App;
using Android.Net.Wifi;
#endif

namespace LocalShareCommunication.UdpService;

public class UdpCallbacker : IDisposable
{

    private bool _disposed = false;
    private UdpClient _listener;
    private UdpClient _sender;
    public int ReceivePort { get; }
    public int SendPort { get; }
    private Action<IPAddress> _action;
    
    public UdpCallbacker(int receivePort, int sendPort)
    {
        ReceivePort = receivePort;
        SendPort = sendPort;
        _listener = new UdpClient(receivePort);
        _sender = new UdpClient();
        _sender.EnableBroadcast = true;
    }

    public void Start()
    {
        if (_disposed)
        {
            throw new Exception("You can't start the same UdpCallbacker object after stopping it.");
        }
        new Thread(() =>
        {
            while (!_disposed)
            {
                try
                {
                    IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, ReceivePort);
                    byte[] responseData = _listener.Receive(ref remoteEP);

                    if (responseData.Length == 0)
                    {
                        continue;
                    }

                    if (EncodingManager.GetText(responseData).Equals("SCAN"))
                    {
                        SendMyAddress();
                    } else
                    {
                        _action.Invoke(new IPAddress(responseData));
                    }

                } catch(Exception)
                {
                    Console.WriteLine();
                }
            }
        }).Start();
    }

    public void Stop()
    {
        Dispose();
    }

    public void SetAction(Action<IPAddress> action)
    {
        _action = action;
    }

    public void SendScan()
    {
        SendData(EncodingManager.GetBytes("SCAN"));
    }

    public void SendMyAddress()
    {
        IPHostEntry entry = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress ipAddress = IPAddress.Loopback;
#if WINDOWS
        foreach (IPAddress ip in entry.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                ipAddress = ip;
                break;
            }
        }
#endif
#if ANDROID
        WifiManager wifiManager = (WifiManager)Android.App.Application.Context.GetSystemService(Service.WifiService);
        int ipaddress = wifiManager.ConnectionInfo.IpAddress;
        ipAddress = new IPAddress(ipaddress);  
#endif
        SendAddress(ipAddress);
    }

    private void SendAddress(IPAddress ip)
    {
        SendData(ip.GetAddressBytes());
    }

    private void SendData(byte[] bytes)
    {
        Console.WriteLine("sending " + EncodingManager.GetText(bytes) + " to " + SendPort);
        _sender.SendAsync(bytes, bytes.Length, new IPEndPoint(IPAddress.Broadcast, SendPort));
    }

    public void Dispose()
    {
        _disposed = true;
        _listener.Close();
        _sender.Close();
    }
}
