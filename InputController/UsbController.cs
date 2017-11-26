using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InputController
{
    public class UsbController
    {
        public const string PACKET_FOUND = "FIND_VUSB_DEVICE";
        public const short ServicePort = 19508;
        FindService _findService;
        Socket _controlSocket;

        bool _activeFindService = false;
        KeyboardDevice _keyboard;
        MouseDevice _mouse;

        public event EventHandler<IPEndPoint> Connected;

        public bool IsConnected
        {
            get { return _controlSocket != null; }
        }

        public UsbController()
        {
            _keyboard = new KeyboardDevice();
            _mouse = new MouseDevice();

            _findService = new FindService(this);
            _findService.Found += _findService_Found;
        }

        public void ActivateFindService()
        {
            _activeFindService = true;
            _findService.Run();
        }

        internal void Connect(string ipAddress, short port)
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            ConnectToServer(ep);
        }

        private void _findService_Found(object sender, FindEndPointArgs e)
        {
            IPEndPoint remote = new IPEndPoint(e.Remote.Address, ServicePort + 1);
            ConnectToServer(remote);
        }

        protected virtual void OnConnected(IPEndPoint remote)
        {
            Connected?.Invoke(this, remote);
        }

        void ConnectToServer(IPEndPoint remote)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                socket.Connect(remote);
                _controlSocket = socket;
                OnConnected(remote);
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.ConnectionRefused)
                {
                    Console.WriteLine("NO listening server: " + remote);
                }
            }
        }

        void Send(byte[] buffer)
        {
            if (_controlSocket == null)
            {
                Console.WriteLine("Socket not connected");
                return;
            }

            SocketAsyncEventArgs arg = new SocketAsyncEventArgs();

            arg.SetBuffer(buffer, 0, buffer.Length);
            arg.Completed += ControlSend_Completed;

            bool isAsync = _controlSocket.SendAsync(arg);
            if (isAsync == false)
            {
                ControlSend_Completed(this, arg);
            }
        }

        internal void MoveText(string txt)
        {
            byte[] buffer = _mouse.ConvertToMouseCommand(txt);
            Send(buffer);
        }

        public void SendText(string txt)
        {
            byte[] buffer = _keyboard.ConvertToKeyboardCommand(txt);
            Send(buffer);
        }

        public void SendChar(ConsoleKeyInfo key)
        {
            byte[] buffer = _keyboard.ConvertToKeyboardCommand(key);
            Send(buffer);
        }

        public void SendKeyRawBuffer(byte[] buffer)
        {
            Send(buffer);
        }

        void CloseSocket()
        {
            Console.WriteLine("Socket is disconnected");
            _controlSocket.Close();
            _controlSocket = null;

            if (_activeFindService == true)
            {
                ActivateFindService();
            }
        }

        private void ControlSend_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred == 0)
            {
                if (e.SocketError != SocketError.Success)
                {
                    CloseSocket();
                }
            }
        }

        class FindEndPointArgs : EventArgs
        {
            IPEndPoint _remote;
            public IPEndPoint Remote
            {
                get { return _remote; }
            }

            public FindEndPointArgs(IPEndPoint remote)
            {
                _remote = remote;
            }
        }

        class FindService
        {
            Thread _findServiceThread;
            Socket _findSocket;
            UsbController _usbController;

            public event EventHandler<FindEndPointArgs> Found;

            public FindService(UsbController usbController)
            {
                _usbController = usbController;
            }

            public void Run()
            {
                _findSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                _findSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);

                _findServiceThread = new Thread(findServiceFunc);
                _findServiceThread.IsBackground = true;
                _findServiceThread.Start();
            }

            protected virtual void OnFound(IPEndPoint endPoint)
            {
                Found?.Invoke(this, new FindEndPointArgs(endPoint));
            }

            private void findServiceFunc()
            {
                _findSocket.SendTimeout = 5 * 1_000;
                _findSocket.ReceiveTimeout = 2 * 1_000;
                byte[] findPacketBuffer = Encoding.UTF8.GetBytes(PACKET_FOUND);
                byte[] recvBuffer = new byte[1024];

                IPAddress[] broadcastAddresses = GetDirectedBroadcastAddresses();

                Console.WriteLine("Networks: ");
                foreach (IPAddress item in broadcastAddresses)
                {
                    Console.WriteLine("\t" +item);
                }

                int order = 0;

                while (true)
                {
                    int index = (order ++) % broadcastAddresses.Length;
                    if (order == broadcastAddresses.Length)
                    {
                        order = 0;
                    }

                    if (_usbController.IsConnected == true)
                    {
                        Thread.Sleep(5 * 1000);
                        continue;
                    }

                    IPAddress broadcastAddress = broadcastAddresses[index];
                    Console.Write("Searching on " + broadcastAddress);
                    _findSocket.SendTo(findPacketBuffer, new IPEndPoint(broadcastAddress, ServicePort));

                    EndPoint ep = new IPEndPoint(IPAddress.Any, 0);

                    try
                    {
                        Array.Clear(recvBuffer, 0, recvBuffer.Length);

                        _findSocket.ReceiveFrom(recvBuffer, ref ep);
                        IPEndPoint connected = ep as IPEndPoint;

                        OnFound(connected);
                        Console.WriteLine(": Found");
                    }
                    catch (SocketException e)
                    {
                        if (e.SocketErrorCode == SocketError.TimedOut)
                        {
                            Console.WriteLine(": NOT Found");
                            continue;
                        }

                        break;
                    }
                }

                _findSocket.Close();
            }

            private IPAddress[] GetDirectedBroadcastAddresses()
            {
                List<IPAddress> list = new List<IPAddress>();

                foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (item.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    {
                        continue;
                    }

                    if (item.OperationalStatus != OperationalStatus.Up)
                    {
                        continue;
                    }

                    UnicastIPAddressInformationCollection unicasts = item.GetIPProperties().UnicastAddresses;

                    foreach (UnicastIPAddressInformation unicast in unicasts)
                    {
                        IPAddress ipAddress = unicast.Address;

                        if (ipAddress.AddressFamily != AddressFamily.InterNetwork)
                        {
                            continue;
                        }

                        // http://blogs.msdn.com/b/knom/archive/2008/12/31/ip-address-calculations-with-c-subnetmasks-networks.aspx
                        byte[] addressBytes = ipAddress.GetAddressBytes();
                        byte[] subnetBytes = unicast.IPv4Mask.GetAddressBytes();

                        if (addressBytes.Length != subnetBytes.Length)
                        {
                            continue;
                        }

                        byte[] broadcastAddress = new byte[addressBytes.Length];
                        for (int i = 0; i < broadcastAddress.Length; i++)
                        {
                            broadcastAddress[i] = (byte)(addressBytes[i] | (subnetBytes[i] ^ 255));
                        }

                        list.Add(new IPAddress(broadcastAddress));
                    }
                }

                return list.ToArray();
            }
        }
    }
}
