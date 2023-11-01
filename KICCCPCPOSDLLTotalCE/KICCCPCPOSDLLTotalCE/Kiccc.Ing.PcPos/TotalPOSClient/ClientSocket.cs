using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Kiccc.Ing.PcPos.TotalPOSClient
{
    internal class ClientSocket : IDisposable
    {
        private const byte Bot = 2;

        private const byte Eot = 4;

        private readonly static ManualResetEvent ConnectDone;

        private readonly static ManualResetEvent SendDone;

        private readonly static ManualResetEvent ReceiveDone;

        private byte[] Buffer
        {
            get;
            set;
        }

        private System.Text.Encoding Encoding
        {
            get;
            set;
        }

        private bool IsDisposed
        {
            get;
            set;
        }

        public int Port
        {
            get;
            private set;
        }

        private StringBuilder Sb { get; set; }

        public IPAddress ServeripAddress
        {
            get;
            private set;
        }

        public System.Net.Sockets.Socket Socket
        {
            get;
            set;
        }

        public SocketState State
        {
            get;
            set;
        }

        static ClientSocket()
        {
            ClientSocket.ConnectDone = new ManualResetEvent(false);
            ClientSocket.SendDone = new ManualResetEvent(false);
            ClientSocket.ReceiveDone = new ManualResetEvent(false);
        }

        public ClientSocket(IPAddress serveripAddress, int port)
            : this(serveripAddress, port, null, 256)
        {
        }

        public ClientSocket(IPAddress serveripAddress, int port, System.Text.Encoding encoding)
            : this(serveripAddress, port, encoding, 256)
        {
        }

        public ClientSocket(IPAddress serveripAddress, int port, int buffersize)
            : this(serveripAddress, port, null, buffersize)
        {
        }

        public ClientSocket(IPAddress serveripAddress, int port, System.Text.Encoding encoding, int buffersize)
        {
            this.Sb = new StringBuilder();
            this.ServeripAddress = serveripAddress;
            this.Encoding = encoding ?? System.Text.Encoding.Unicode;
            this.Buffer = new byte[buffersize];
            this.Port = port;
            this.IsDisposed = false;
        }

        private void CheckObjectDisposalState()
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                this.State = SocketState.Connected;
                this.Socket = (System.Net.Sockets.Socket)ar.AsyncState;
                this.Socket.EndConnect(ar);
                ClientSocket.ConnectDone.Set();
                this.OnConnectionEstablished();
            }
            catch (SocketException socketException)
            {
                SocketException ex = socketException;
                if (ex.ErrorCode == 10054)
                {
                    this.OnConnectionClosed();
                }
                this.OnExecptionOccured(ex);
            }
            catch (Exception exception)
            {
                this.OnExecptionOccured(exception);
            }
        }

        public void Dispose()
        {
            if (!this.IsDisposed)
            {
                System.Net.Sockets.Socket socket = this.Socket;
                if (socket != null)
                {
                    //socket.Disconnect(true);
                    socket.Close();
                }
                else
                {
                }
                System.Net.Sockets.Socket socket1 = this.Socket;
                if (socket1 != null)
                {
                    socket1.Close();
                }
                else
                {
                }
                this.IsDisposed = true;
            }
        }

        protected virtual void OnConnectionClosed()
        {
            ConnectionClosedEventHandler connectionClosedEventHandler = this.ConnectionClosed;
            if (connectionClosedEventHandler != null)
            {
                connectionClosedEventHandler();
            }
            else
            {
            }
        }

        protected virtual void OnConnectionEstablished()
        {
            ConnectedEventHandler connectedEventHandler = this.ConnectionEstablished;
            if (connectedEventHandler != null)
            {
                connectedEventHandler();
            }
            else
            {
            }
        }

        protected virtual void OnDataReceived(string data)
        {
            DataReceivedEventHandler dataReceivedEventHandler = this.DataReceived;
            if (dataReceivedEventHandler != null)
            {
                dataReceivedEventHandler(data);
            }
            else
            {
            }
        }

        protected virtual void OnDataSent(int datalenght)
        {
            DataSentEventHandler dataSentEventHandler = this.DataSent;
            if (dataSentEventHandler != null)
            {
                dataSentEventHandler(datalenght);
            }
            else
            {
            }
        }

        protected virtual void OnExecptionOccured(Exception ex)
        {
            ExecptionOccuredEventHandler execptionOccuredEventHandler = this.ExecptionOccured;
            if (execptionOccuredEventHandler != null)
            {
                execptionOccuredEventHandler(ex);
            }
            else
            {
            }
        }

        protected virtual void OnStateChanged(string str)
        {
            StateChangedEventHandler stateChangedEventHandler = this.StateChanged;
            if (stateChangedEventHandler != null)
            {
                stateChangedEventHandler(str);
            }
            else
            {
            }
        }

        private void Receive()
        {
            try
            {
                this.State = SocketState.Reading;
                this.Socket.BeginReceive(this.Buffer, 0, (int)this.Buffer.Length, SocketFlags.None, new AsyncCallback(this.ReceiveCallback), this.Socket);
            }
            catch (SocketException socketException)
            {
                SocketException ex = socketException;
                if (ex.ErrorCode == 10054)
                {
                    this.OnConnectionClosed();
                }
                this.OnExecptionOccured(ex);
            }
            catch (Exception exception)
            {
                this.OnExecptionOccured(exception);
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                this.Sb = new StringBuilder();
                this.Socket.EndReceive(ar);
                bool botDetected = false;
                bool eotDetected = false;
                byte[] buffer = this.Buffer;
                for (int i = 0; i < (int)buffer.Length; i++)
                {
                    byte byt = buffer[i];
                    if ((byt == 2 ? true : botDetected))
                    {
                        if ((byt != 2 ? false : !botDetected))
                        {
                            botDetected = true;
                        }
                        else if (byt != 4)
                        {
                            this.Sb.Append(this.Encoding.GetString(new byte[] { byt }, 0, 1));
                        }
                        else
                        {
                            eotDetected = true;
                            break;
                        }
                    }
                }
                if (!(!botDetected | !eotDetected))
                {
                    this.OnDataReceived(this.Sb.ToString());
                }
            }
            catch (SocketException socketException)
            {
                SocketException ex = socketException;
                if (ex.ErrorCode == 10054)
                {
                    this.OnConnectionClosed();
                }
                this.OnExecptionOccured(ex);
            }
            catch (Exception exception)
            {
                this.OnExecptionOccured(exception);
            }
        }

        public void Send(string data, ref System.Net.Sockets.Socket OldSocket)
        {
            this.CheckObjectDisposalState();
            this.State = SocketState.Sending;
            try
            {
                byte[] tmpBuffer = this.Encoding.GetBytes(data);
                byte[] messageBuffer = new byte[(int)tmpBuffer.Length + 2];
                tmpBuffer.CopyTo(messageBuffer, 1);
                messageBuffer[0] = 2;
                messageBuffer[(int)messageBuffer.Length - 1] = 4;
                this.Socket.BeginSend(messageBuffer, 0, (int)messageBuffer.Length, SocketFlags.None, new AsyncCallback(this.SendCallback), this.Socket);
            }
            catch (SocketException socketException)
            {
                SocketException ex = socketException;
                if (ex.ErrorCode == 10054)
                {
                    this.OnConnectionClosed();
                }
                this.OnExecptionOccured(ex);
            }
            catch (Exception exception)
            {
                this.OnExecptionOccured(exception);
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                this.OnDataSent(this.Socket.EndSend(ar));
                ClientSocket.SendDone.Set();
            }
            catch (SocketException socketException)
            {
                SocketException ex = socketException;
                if (ex.ErrorCode == 10054)
                {
                    this.OnConnectionClosed();
                }
                this.OnExecptionOccured(ex);
            }
            catch (Exception exception)
            {
                this.OnExecptionOccured(exception);
            }
        }

        public void Start()
        {
            this.CheckObjectDisposalState();
            try
            {
                IPEndPoint remoteEp = new IPEndPoint(this.ServeripAddress, this.Port);
                this.Socket = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    //ReceiveTimeout = 200000,
                    //SendTimeout = 200000
                };
                this.Socket.Connect(remoteEp);
                this.Receive();
            }
            catch (SocketException socketException)
            {
                SocketException ex = socketException;
                if (ex.ErrorCode == 10054)
                {
                    this.OnConnectionClosed();
                }
                this.OnExecptionOccured(ex);
            }
            catch (Exception exception)
            {
                this.OnExecptionOccured(exception);
            }
        }

        public event ConnectionClosedEventHandler ConnectionClosed;

        public event ConnectedEventHandler ConnectionEstablished;

        public event DataReceivedEventHandler DataReceived;

        public event DataSentEventHandler DataSent;

        public event ExecptionOccuredEventHandler ExecptionOccured;

        public event StateChangedEventHandler StateChanged;
    }
}