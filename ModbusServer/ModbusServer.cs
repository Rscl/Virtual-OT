using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace ModbusServer
{
    public class ModbusServer
    {
        private readonly int _port;
        private TcpListener _listener;
        private bool _isRunning;

        public delegate void PacketReceivedHandler(ModbusPacket packet, NetworkStream stream);
        public event PacketReceivedHandler? PacketReceived; // Declare the event as nullable

        public ModbusServer(int port)
        {
            _port = port;
            _listener = new TcpListener(IPAddress.Any, _port);
        }

        public void Start()
        {
            _listener.Start();
            _isRunning = true;
            new Thread(Listen).Start();
        }

        public void Stop()
        {
            _isRunning = false;
            Thread.Sleep(500);
            _listener.Stop();
            
        }

        private void Listen()
        {
            while (_isRunning)
            {
                try
                {
                    if (_listener.Pending())
                    {
                        var client = _listener.AcceptTcpClient();
                        // Display the remote party IP address
                        var remoteEndpoint = client.Client.RemoteEndPoint as IPEndPoint;
                        Console.WriteLine($"New connection from {remoteEndpoint?.Address.ToString()}");
                        new Thread(() => HandleClient(client)).Start();
                    }
                    else
                    {
                        Thread.Sleep(100); // Add a small delay to reduce CPU usage
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private void HandleClient(TcpClient client)
        {
            var networkStream = client.GetStream();
            try
            {
                while (_isRunning)
                {
                    if (networkStream.DataAvailable)
                    {
                        byte[] buffer = new byte[256];
                        int bytesRead = networkStream.Read(buffer, 0, buffer.Length);
                        if (bytesRead > 0)
                        {
                            byte[] data = buffer.Take(bytesRead).ToArray();
                            ModbusPacket request = ModbusPacket.FromByteArray(data);
                            PacketReceived?.Invoke(request, networkStream);
                        }
                    }
                    else
                    {
                        Thread.Sleep(100); // Add a small delay to reduce CPU usage
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                networkStream.Close();
                client.Close();
            }
        }
    }
}
