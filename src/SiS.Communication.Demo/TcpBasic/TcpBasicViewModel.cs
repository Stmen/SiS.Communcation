﻿using SiS.Communication.Spliter;
using SiS.Communication.Tcp;
using System.Linq;

namespace SiS.Communication.Demo
{
    public class TcpBasicViewModel : PageBaseViewModel
    {
        #region Constructor

        public TcpBasicViewModel(string title) : base(title)
        {

        }
        public TcpBasicViewModel() : this("Tcp Basic")
        {

        }

        #endregion

        private ServerBaseViewModel _serverVM = new TcpBasicServerViewModel();
        public override ServerBaseViewModel ServerVM
        {
            get { return _serverVM; }
        }

        private ClientBaseViewModel _clientVM = new TcpBasicClientViewModel();
        public override ClientBaseViewModel ClientVM
        {
            get { return _clientVM; }
        }
    }

    public class TcpBasicServerViewModel : ServerBaseViewModel
    {
        #region Constructor

        public TcpBasicServerViewModel()
        {
            //1.the default spliter
            //IPacketSpliter spliter = new SimplePacketSpliter();

            //2.packet with specific header which can prevent illegal network connection
            IPacketSpliter headerSpliter = new HeaderPacketSpliter(0xFFFF1111);

            //3.packet with length ahead in network byte order
            IPacketSpliter simpleSpliter = new SimplePacketSpliter(true);

            //4.packet end with specific mark
            IPacketSpliter endMarkSpliter = new EndMarkPacketSpliter(false, (byte)'\x04');

            //5.packet that do not need to be split
            IPacketSpliter rawSpliter = RawPacketSpliter.Default;

            //_tcpServer = new TcpServer();
            _tcpServer = new TcpServer(simpleSpliter);
            _tcpServer.ClientStatusChanged += OnTcpServer_ClientStatusChanged;
            _tcpServer.MessageReceived += OnTcpServer_MessageReceived;

            ServerSendText = "I am server";
        }

        #endregion

        #region Private Members

        protected TcpServer _tcpServer;
        protected const ushort _serverPort = 9999;
        protected long _serverClientID = -1;

        #endregion

        #region Server Event Handlers

        protected virtual void OnTcpServer_ClientStatusChanged(object sender, ClientStatusChangedEventArgs args)
        {
            if (args.Status == ClientStatus.Connected)
            {
                _serverClientID = args.ClientID;
            }
            else
            {
                _serverClientID = -1;
            }
        }
        protected virtual void OnTcpServer_MessageReceived(object sender, TcpRawMessageReceivedEventArgs args)
        {
            string text = _tcpServer.TextEncoding.GetString(args.Message.MessageRawData.ToArray());
            ServerRecvText += text + "\r\n";
        }

        #endregion

        #region Server Operations

        public override void ServerSend()
        {
            if (_serverClientID > 0)
            {
                if (!string.IsNullOrWhiteSpace(ServerSendText))
                {
                    _tcpServer.SendText(_serverClientID, ServerSendText.Trim());
                }
            }
        }

        public override void StartServer()
        {
            if (CanStartServer)
            {
                _tcpServer.Start(_serverPort);
                CanStartServer = false;
            }
        }

        public override void StopServer()
        {
            _tcpServer.Stop();
            CanStartServer = true;
        }

        #endregion

    }

    public class TcpBasicClientViewModel : ClientBaseViewModel
    {
        #region Constructor

        public TcpBasicClientViewModel()
        {
            //1.the default spliter
            //IPacketSpliter spliter = new SimplePacketSpliter();

            //2.packet with specific header which can prevent illegal network connection
            IPacketSpliter headerSpliter = new HeaderPacketSpliter(0xFFFF1111);

            //3.packet with length ahead in network byte order
            IPacketSpliter simpleSpliter = new SimplePacketSpliter(true);

            //4.packet end with specific mark
            IPacketSpliter endMarkSpliter = new EndMarkPacketSpliter(false, (byte)'\x04');

            //5.packet that do not need to be split
            IPacketSpliter rawSpliter = RawPacketSpliter.Default;

            //_tcpClient = new TcpClientEx(false);
            _tcpClient = new TcpClientEx(false, simpleSpliter);
            _tcpClient.ClientStatusChanged += OnTcpCient_ClientStatusChanged;
            _tcpClient.MessageReceived += OnTcpClient_PacketReceived;

            ClientSendText = "I am client";
        }

        #endregion

        #region Private Members
        protected TcpClientEx _tcpClient;
        private const ushort _serverPort = 9999;
        #endregion


        #region Client Event Handlers

        protected virtual void OnTcpCient_ClientStatusChanged(object sender, ClientStatusChangedEventArgs args)
        {
            ClientStatus = args.Status;
            if (args.Status == ClientStatus.Closed)
            {
                CanConnect = true;
            }
        }

        protected virtual void OnTcpClient_PacketReceived(object sender, TcpRawMessageReceivedEventArgs args)
        {
            string text = _tcpClient.TextEncoding.GetString(args.Message.MessageRawData.ToArray());
            ClientRecvText += text + "\r\n";
        }

        #endregion

        #region Client Operations

        public override void ClientSend()
        {
            if (!string.IsNullOrWhiteSpace(ClientSendText))
            {
                _tcpClient.SendText(ClientSendText.Trim());
            }
        }

        public override void ConnectToServer()
        {
            if (CanConnect)
            {
                CanConnect = false;
                _tcpClient.AutoReconnect = EnableAutoReconnect;
                _tcpClient.ConnectAsync("127.0.0.1", _serverPort, (isConnected) =>
                {
                    CanConnect = !isConnected;
                    //_tcpClient.SendText("1234567890");
                });
            }
        }

        public override void Disconnect()
        {
            _tcpClient.Close();
        }

        #endregion
    }
}
