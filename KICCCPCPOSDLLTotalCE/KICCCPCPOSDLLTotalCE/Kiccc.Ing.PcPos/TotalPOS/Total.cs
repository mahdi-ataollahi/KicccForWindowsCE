using Kiccc.Ing.PcPos;
using Kiccc.Ing.PcPos.Core;
using Kiccc.Ing.PcPos.Logger;
using Kiccc.Ing.PcPos.TotalPOSClient;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using System.Xml;

namespace Kiccc.Ing.PcPos.TotalPOS
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    [Guid("DAE235E2-DC2A-4DD1-9EE1-096FCB0B7B55")]
    [ProgId("Kiccc.Ing.PcPos.TotalPOS.TotalPOS")]
    public class Total : IntractingCore, IDisposable, ITotal
    {
        private bool _timeOutReached;

        private System.Timers.Timer _timeOutTimer;

        private const string LogPhraseId = "12713803";

        private const byte Bot = 2;

        private const byte Eot = 4;

        private ManualResetEvent mre = new ManualResetEvent(false);

        private static Socket Oldsocket;

        private static ClientSocket Client;

        private const string ResponseTemplate = "{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}";

        private const string RegName = "Software\\KICCC_PcPos\\{0}{1}{2}{3}";

        public string AcceptorId
        {
            get
            {
                return this._acceptorId;
            }
            set
            {
                if (!Regex.IsMatch(value, "^[0-9]{12,15}$"))
                {
                    throw new Exception("Invalid Value For AcceptorId !!!");
                }
                this._acceptorId = value;
            }
        }

        public int BaudRate
        {
            get;
            set;
        }

        public string ComPort
        {
            get
            {
                return this._comPort;
            }
            set
            {
                if (!Regex.IsMatch(value.ToUpper(), "^COM[1-9]{1}[0-9]{0,1}$"))
                {
                    throw new Exception("Invalid Value For ComPort !!!");
                }
                this._comPort = value.ToUpper();
            }
        }

        public ConnetionTypes ConnetionType
        {
            get
            {
                return this._connetionType;
            }
            set
            {
                this._connetionType = value;
            }
        }

        public int DataBits
        {
            get;
            set;
        }

        public bool GenerateLog
        {
            get;
            set;
        }

        public string IpAddress
        {
            get;
            set;
        }

        public string LogPath
        {
            get;
            set;
        }

        public string LogPhrase
        {
            get;
            set;
        }

        public SerialPortParity Parity
        {
            get;
            set;
        }

        public int Port
        {
            get;
            set;
        }

        public string SerialNo
        {
            get
            {
                return this._serialNo;
            }
            set
            {
                this._serialNo = value;
            }
        }

        public SerialPortStopBit StopBit
        {
            get;
            set;
        }

        public string TerminalId
        {
            get
            {
                return this._terminalId;
            }
            set
            {
                if (!Regex.IsMatch(value, "^[0-9]{8}$"))
                {
                    throw new Exception("Invalid Value For TerminalId !!!");
                }
                this._terminalId = value;
            }
        }

        public int TimeOut
        {
            get
            {
                return this._timeOut;
            }
            set
            {
                this._timeOut = value * 1000;
            }
        }

        public Total()
        {
            this.GenerateLog = false;
            this._isDisposed = true;
            this.DoLogSetting();
            base.State = ReadyState.InitializeRequired;
        }

        private string BuildMessage(Total.MessageStructure msg)
        {
            msg.Serial = this.SerialNo;
            msg.TerminalId = this.TerminalId;
            msg.AcceptorId = this.AcceptorId;
            return msg.ToString();
        }

        public override string Communicate(Total.MessageStructure msg)
        {
            string str;
            str = (this.ConnetionType != ConnetionTypes.TCP ? this.SerialCommunicate(this.BuildMessage(msg)) : this.TcpCommunicate(this.BuildMessage(msg)));
            return str;
        }

        public override string Communicate(string msg)
        {
            string str;
            str = (this.ConnetionType != ConnetionTypes.TCP ? this.SerialCommunicate(msg) : this.TcpCommunicateEnc(msg));
            return str;
        }

        protected virtual void Dispose(bool dispose)
        {
            if (dispose)
            {
                this.Dispose(false);
                if ((this._currentOps == null ? false : this._currentOps.IsAlive))
                {
                    this._currentOps.Abort();
                }
                if (this.SerialPort != null)
                {
                    this.SerialPort.Dispose();
                }
                if (this._timeOutTimer != null)
                {
                    this._timeOutTimer.Stop();
                    this._timeOutTimer.Enabled = false;
                    this._timeOutTimer.Dispose();
                }
                this._isDisposed = true;
                base.State = ReadyState.Disposed;
            }
            else
            {
                FieldInfo fieldInfo = null;
                EventInfo eventInfo = null;
                if (this._timeOutTimer != null)
                {
                    fieldInfo = this._timeOutTimer.GetType().GetField("Elapsed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    eventInfo = this._timeOutTimer.GetType().GetEvent("Elapsed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (fieldInfo != null)
                    {
                        Delegate del = fieldInfo.GetValue(this._timeOutTimer) as Delegate;
                        if (del != null && eventInfo != null)
                        {
                            eventInfo.RemoveEventHandler(this._timeOutTimer, del);
                        }
                    }
                    this._timeOutTimer.Stop();
                    this._timeOutTimer.Enabled = false;
                    this._timeOutTimer.Dispose();
                }
                if (this.SerialPort != null)
                {
                    fieldInfo = this.SerialPort.GetType().GetField("DataReceived", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    eventInfo = this.SerialPort.GetType().GetEvent("DataReceived", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    this.SerialPort.Close();
                    if (fieldInfo != null)
                    {
                        Delegate del = fieldInfo.GetValue(this.SerialPort) as Delegate;
                        if (del != null && eventInfo != null)
                        {
                            eventInfo.RemoveEventHandler(this.SerialPort, del);
                        }
                    }
                }
                base.State = ReadyState.Ready;
            }
        }

        public void Dispose()
        {
            if (!this._isDisposed)
            {
                this.Dispose(true);
            }
        }

        internal void DoLogSetting()
        {
            string str;
            string str1;
            try
            {
                Version ver = Assembly.GetExecutingAssembly().GetName().Version;
                string regKey = string.Format("Software\\KICCC_PcPos\\{0}{1}{2}{3}", new object[] { ver.Build, ver.Major, ver.Minor, ver.Revision });
                RegistryKey key = Registry.CurrentUser.OpenSubKey(regKey);
                if (key == null)
                {
                    key = Registry.CurrentUser.CreateSubKey(regKey);
                    if (key != null)
                    {
                        key.SetValue("LogPhrase", "None", RegistryValueKind.String);
                        key.SetValue("LogPath", "", RegistryValueKind.String);
                        key.SetValue("LoggingState", 0, RegistryValueKind.DWord);
                    }
                    else
                    {
                        return;
                    }
                }
                else if ((key.GetValue("LoggingState") == null ? false : int.Parse(key.GetValue("LoggingState").ToString()) == 1))
                {
                    this.GenerateLog = true;
                    object value = key.GetValue("LogPhrase");
                    if (value != null)
                    {
                        str = value.ToString();
                    }
                    else
                    {
                        str = null;
                    }
                    this.LogPhrase = str;
                    object obj = key.GetValue("LogPath");
                    if (obj != null)
                    {
                        str1 = obj.ToString();
                    }
                    else
                    {
                        str1 = null;
                    }
                    this.LogPath = str1;
                }
                else
                {
                    return;
                }
            }
            catch (Exception exception)
            {
            }
        }

        public void InitiateService()
        {
        }

        public void InitiateService(string serialNo, string acceptorId, string terminalId, string ipAddress, int port, int timeOut = 200)
        {
            this.SerialNo = serialNo;
            this.AcceptorId = acceptorId;
            this.TerminalId = terminalId;
            this.TimeOut = timeOut;
            this.IpAddress = ipAddress;
            this.Port = port;
            base.State = ReadyState.Ready;
            this._isDisposed = false;
            this.ConnetionType = ConnetionTypes.TCP;
            this.DoLogSetting();
        }

        public void InitiateService(string serialNo, string acceptorId, string terminalId, string comPort, int baudRate, int dataBits, SerialPortStopBit stopBit, SerialPortParity parity, int timeOut = 200)
        {
            this.SerialNo = serialNo;
            this.AcceptorId = acceptorId;
            this.TerminalId = terminalId;
            this.ComPort = comPort;
            this.BaudRate = baudRate;
            this.DataBits = dataBits;
            this.StopBit = stopBit;
            this.Parity = parity;
            this.TimeOut = timeOut;
            this.SerialPort = new System.IO.Ports.SerialPort(this.ComPort, this.BaudRate, (System.IO.Ports.Parity)this.Parity, this.DataBits, (StopBits)this.StopBit)
            {
                Encoding = Encoding.UTF8
            };
            base.State = ReadyState.Ready;
            this._isDisposed = false;
            this.DoLogSetting();
            this.ConnetionType = ConnetionTypes.Serial;
        }

        public void InitiateService(string serialNo, string acceptorId, string terminalId, string comPort, int baudRate, int dataBits, int stopBit, int parity, int timeOut = 200)
        {
            this.InitiateService(serialNo, acceptorId, terminalId, comPort, baudRate, dataBits, (SerialPortStopBit)stopBit, (SerialPortParity)parity, timeOut);
        }

        public override void JustSend(string msg)
        {
            if (this.ConnetionType == ConnetionTypes.TCP)
            {
                this.TcpCommunicateEnc(msg);
            }
            else if (this.ConnetionType == ConnetionTypes.Serial)
            {
                this.SerialCommunicate(msg);
            }
        }

        internal static string MakeFackeResponse(FakeResponseType type)
        {
            return string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}", new object[] { "0".PadLeft(6, '0'), "0".PadLeft(12, '0'), (int)type, "0".PadLeft(12, '0'), "0".PadLeft(14, '0'), "0".PadLeft(12, '0'), "0".PadLeft(15, '0'), "0".PadLeft(8, '0'), "0".PadLeft(16, '0') });
        }

        internal static string MakeFackeResponse(string resp)
        {
            return string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}", new object[] { "0".PadLeft(6, '0'), "0".PadLeft(12, '0'), resp, "0".PadLeft(12, '0'), "0".PadLeft(14, '0'), "0".PadLeft(12, '0'), "0".PadLeft(15, '0'), "0".PadLeft(8, '0'), "0".PadLeft(16, '0') });
        }

        protected virtual new void OnResponseReceived(ResponseReceivedEventArgs e)
        {
            (new EventHelper()).RaiseAsync(this.ResponseReceived, new object[] { this, e });
        }

        internal static string ParseMessage(string receivedstr)
        {
            bool flag;
            bool flag1;
            if (string.IsNullOrEmpty(receivedstr))
            {
                flag = false;
            }
            else
            {
                flag = (receivedstr.Contains("|") ? false : Regex.IsMatch(receivedstr, "^[0-9]{1-20}$"));
            }
            if (flag)
            {
                receivedstr = Total.MakeFackeResponse((FakeResponseType)int.Parse(receivedstr));
            }
            if (string.IsNullOrEmpty(receivedstr))
            {
                flag1 = false;
            }
            else
            {
                flag1 = (!receivedstr.Contains("|") ? true : (int)receivedstr.Split(new char[] { '|' }).Length < 9);
            }
            if (flag1)
            {
                receivedstr = Total.MakeFackeResponse(receivedstr.Replace("|", "ISO"));
            }
            if (string.IsNullOrEmpty(receivedstr))
            {
                receivedstr = Total.MakeFackeResponse(FakeResponseType.UnknownError);
            }
            string[] tmpResponse = receivedstr.Split(new char[] { '|' });
            XmlDocument xmlDoc = new XmlDocument();
            PosResponse posResponse = new PosResponse()
            {
                RRN = tmpResponse[1],
                RespCode = tmpResponse[2],
                SerialNo = tmpResponse[3],
                TransactionDate = string.Format("{0}/{1}/{2}", tmpResponse[4].Substring(0, 4), tmpResponse[4].Substring(4, 2), tmpResponse[4].Substring(6, 2)),
                TransactionTime = string.Format("{0}:{1}:{2}", tmpResponse[4].Substring(8, 2), tmpResponse[4].Substring(10, 2), tmpResponse[4].Substring(12, 2)),
                Amount = tmpResponse[5],
                TerminalId = tmpResponse[7],
                TraceNo = tmpResponse[0],
                Pan = string.Format("{0}******{1}", tmpResponse[8].Substring(0, 6), tmpResponse[8].Substring(12, 4)),
                PaymentlId = ((int)tmpResponse.Length > 9 ? tmpResponse[9] : string.Empty),
                CRCResponse = ((int)tmpResponse.Length > 10 ? tmpResponse[10] : string.Empty)
            };
            return string.Concat(new string[] { "{\"ActionResult\":\"Success\",\"Description\":\"عملیات با موفقیت انجام شد\",\"Result\":{", string.Format("\"RRN\":\"{0}\"", posResponse.RRN), string.Format(",\"RespCode\":\"{0}\",", posResponse.RespCode), string.Format("\"SerialNo\":\"{0}\",", posResponse.SerialNo), string.Format("\"TransactionDate\":\"{0}\",", posResponse.TransactionDate), string.Format("\"TransactionTime\":\"{0}\",", posResponse.TransactionTime), string.Format("\"Amount\":\"{0}\",", posResponse.Amount), string.Format("\"TerminalId\":\"{0}\",", posResponse.TerminalId), string.Format("\"TraceNo\":\"{0}\",", posResponse.TraceNo), string.Format("\"Pan\":\"{0}\",", posResponse.Pan), string.Format("\"PaymentlId\":\"{0}\",", posResponse.PaymentlId), string.Format("\"CRCResponse\":\"{0}\"", posResponse.CRCResponse), "}}" });
        }

        public void ResetService()
        {
            this.Dispose();
        }

        private string SerialCommunicate(string message)
        {
            string str;
            bool flag;
            base.CheckObjectDisposeStatus();
            if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
            {
                Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Description, "Communication Begin ...", this.LogPath);
            }
            if (base.State != ReadyState.Ready)
            {
                throw new Exception("System resources occupancy\r\bInitiate Service");
            }
            base.State = ReadyState.Busy;
            this._timeOutTimer = new System.Timers.Timer()
            {
                Interval = (double)this.TimeOut
            };
            this._timeOutTimer.Elapsed += new ElapsedEventHandler((object sender, ElapsedEventArgs e) =>
            {
                if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                {
                    Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Description, "Timeout Reached . No Response !!!", this.LogPath);
                }
                this._timeOutReached = true;
            });
            try
            {
                string str1 = null;
                if (this.SerialPort.IsOpen)
                {
                    throw new Exception("SerialPort Already In Use");
                }
                this.SerialPort.Open();
                bool flag1 = false;
                this.SerialPort.DataReceived += new SerialDataReceivedEventHandler((object sender, SerialDataReceivedEventArgs e) =>
                {
                    if (!flag1)
                    {
                        flag1 = !flag1;
                        if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                        {
                            Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Description, "Responswe Received ...", this.LogPath);
                        }
                        List<byte> receiveBuffer = new List<byte>();
                        System.IO.Ports.SerialPort currentPort = sender as System.IO.Ports.SerialPort;
                        bool botDetected = false;
                        bool eotDetected = false;
                        while (true)
                        {
                            if ((currentPort == null ? true : (!currentPort.BaseStream.CanRead ? true : currentPort.BytesToRead == 0)))
                            {
                                break;
                            }
                            byte readed = (byte)currentPort.ReadByte();
                            if (botDetected)
                            {
                                if (readed == 4)
                                {
                                    eotDetected = true;
                                }
                                if (!eotDetected)
                                {
                                    receiveBuffer.Add(readed);
                                }
                                Thread.Sleep(5);
                            }
                            else if (readed == 2)
                            {
                                botDetected = true;
                            }
                        }
                        if (currentPort != null)
                        {
                            currentPort.BaseStream.Flush();
                        }
                        if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                        {
                            Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Description, "Parsing Message ...", this.LogPath);
                        }
                        str1 = Encoding.ASCII.GetString(receiveBuffer.ToArray());
                        if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                        {
                            Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Response, str1, this.LogPath);
                        }
                    }
                    else if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                    {
                        Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Description, "Abnormal Receive Detected", this.LogPath);
                    }
                });
                byte[] tmpBuffer = Encoding.UTF8.GetBytes(message);
                byte[] messageBuffer = new byte[(int)tmpBuffer.Length + 2];
                tmpBuffer.CopyTo(messageBuffer, 1);
                messageBuffer[0] = 2;
                messageBuffer[(int)messageBuffer.Length - 1] = 4;
                if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                {
                    Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Request, "Commanding ...", this.LogPath);
                }
                if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                {
                    Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Request, message, this.LogPath);
                }
                while (true)
                {
                    if ((!this.SerialPort.BaseStream.CanRead ? true : this.SerialPort.BytesToRead <= 0))
                    {
                        break;
                    }
                    this.SerialPort.ReadByte();
                }
                this.SerialPort.BaseStream.Flush();
                this.SerialPort.Write(messageBuffer, 0, (int)messageBuffer.Length);
                this.SerialPort.BaseStream.Flush();
                this._timeOutTimer.Enabled = true;
                this._timeOutTimer.Start();
                if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                {
                    Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Description, "Waiting For Response ...", this.LogPath);
                }
                do
                {
                    flag = (this._timeOutReached ? false : string.IsNullOrEmpty(str1));
                }
                while (flag);
                this.Dispose(false);
                str = Total.ParseMessage((this._timeOutReached ? Total.MakeFackeResponse(FakeResponseType.Timeout) : str1));
            }
            catch (Exception exception)
            {
                Exception ex = exception;
                if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                {
                    Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Exception, ex.Message, this.LogPath);
                }
                if (ex.InnerException != null)
                {
                    if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                    {
                        Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Exception, ex.InnerException.Message, this.LogPath);
                    }
                }
                if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                {
                    Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Description, "Disposing Resources In Level 0 ", this.LogPath);
                }
                this.Dispose(false);
                base.State = ReadyState.Fault;
                throw;
            }
            return str;
        }

        private string SerialCommunicateEnc(string message)
        {
            string str;
            bool flag;
            base.CheckObjectDisposeStatus();
            if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
            {
                Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Description, "Communication Begin ...", this.LogPath);
            }
            if (base.State != ReadyState.Ready)
            {
                throw new Exception("System resources occupancy\r\bInitiate Service");
            }
            base.State = ReadyState.Busy;
            this._timeOutTimer = new System.Timers.Timer()
            {
                Interval = (double)this.TimeOut
            };
            this._timeOutTimer.Elapsed += new ElapsedEventHandler((object sender, ElapsedEventArgs e) =>
            {
                if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                {
                    Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Description, "Timeout Reached . No Response !!!", this.LogPath);
                }
                this._timeOutReached = true;
            });
            try
            {
                string str1 = null;
                if (this.SerialPort.IsOpen)
                {
                    throw new Exception("SerialPort Already In Use");
                }
                this.SerialPort.Open();
                bool flag1 = false;
                this.SerialPort.DataReceived += new SerialDataReceivedEventHandler((object sender, SerialDataReceivedEventArgs e) =>
                {
                    if (!flag1)
                    {
                        flag1 = !flag1;
                        if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                        {
                            Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Description, "Responswe Received ...", this.LogPath);
                        }
                        List<byte> receiveBuffer = new List<byte>();
                        System.IO.Ports.SerialPort currentPort = sender as System.IO.Ports.SerialPort;
                        bool botDetected = false;
                        bool eotDetected = false;
                        while (true)
                        {
                            if ((currentPort == null ? true : (!currentPort.BaseStream.CanRead ? true : currentPort.BytesToRead == 0)))
                            {
                                break;
                            }
                            byte readed = (byte)currentPort.ReadByte();
                            if (botDetected)
                            {
                                if (readed == 4)
                                {
                                    eotDetected = true;
                                }
                                if (!eotDetected)
                                {
                                    receiveBuffer.Add(readed);
                                }
                                Thread.Sleep(5);
                            }
                            else if (readed == 2)
                            {
                                botDetected = true;
                            }
                        }
                        if (currentPort != null)
                        {
                            currentPort.BaseStream.Flush();
                        }
                        if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                        {
                            Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Description, "Parsing Message ...", this.LogPath);
                        }
                        str1 = Encoding.ASCII.GetString(receiveBuffer.ToArray());
                        if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                        {
                            Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Response, str1, this.LogPath);
                        }
                    }
                    else if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                    {
                        Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Description, "Abnormal Receive Detected", this.LogPath);
                    }
                });
                byte[] tmpBuffer = Encoding.UTF8.GetBytes(message);
                byte[] messageBuffer = new byte[(int)tmpBuffer.Length + 2];
                tmpBuffer.CopyTo(messageBuffer, 1);
                messageBuffer[0] = 2;
                messageBuffer[(int)messageBuffer.Length - 1] = 4;
                if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                {
                    Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Request, "Commanding ...", this.LogPath);
                }
                if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                {
                    Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Request, message, this.LogPath);
                }
                while (true)
                {
                    if ((!this.SerialPort.BaseStream.CanRead ? true : this.SerialPort.BytesToRead <= 0))
                    {
                        break;
                    }
                    this.SerialPort.ReadByte();
                }
                this.SerialPort.BaseStream.Flush();
                this.SerialPort.Write(messageBuffer, 0, (int)messageBuffer.Length);
                this.SerialPort.BaseStream.Flush();
                this._timeOutTimer.Enabled = true;
                this._timeOutTimer.Start();
                if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                {
                    Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Description, "Waiting For Response ...", this.LogPath);
                }
                do
                {
                    flag = (this._timeOutReached ? false : string.IsNullOrEmpty(str1));
                }
                while (flag);
                this.Dispose(false);
                str = str1;
            }
            catch (Exception exception)
            {
                Exception ex = exception;
                if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                {
                    Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Exception, ex.Message, this.LogPath);
                }
                if (ex.InnerException != null)
                {
                    if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                    {
                        Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Exception, ex.InnerException.Message, this.LogPath);
                    }
                }
                if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                {
                    Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Description, "Disposing Resources In Level 0 ", this.LogPath);
                }
                this.Dispose(false);
                base.State = ReadyState.Fault;
                throw;
            }
            return str;
        }

        private string TcpCommunicate(string message)
        {
            string str1;
            base.CheckObjectDisposeStatus();
            if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
            {
                Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Description, "Communication Begin ...", this.LogPath);
            }
            if (base.State != ReadyState.Ready)
            {
                throw new Exception("System resources occupancy\r\bInitiate Service");
            }
            base.State = ReadyState.Busy;
            this.mre.Reset();
            this._timeOutTimer = new System.Timers.Timer()
            {
                Interval = (double)this.TimeOut
            };
            this._timeOutTimer.Elapsed += new ElapsedEventHandler((object sender, ElapsedEventArgs e) =>
            {
                if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                {
                    Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Description, "Timeout Reached . No Response !!!", this.LogPath);
                }
                this._timeOutReached = true;
                this.mre.Set();
            });
            try
            {
                string str2 = null;
                ClientSocket clientSocket = new ClientSocket(IPAddress.Parse(this.IpAddress), this.Port, Encoding.UTF8, 1024);
                Total.Client = clientSocket;
                using (clientSocket)
                {
                    Total.Client.ConnectionEstablished += new ConnectedEventHandler(() =>
                    {
                        if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                        {
                            Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Request, "Connection Established ...", this.LogPath);
                        }
                    });
                    Total.Client.ConnectionClosed += new ConnectionClosedEventHandler(() =>
                    {
                        if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                        {
                            Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Request, "Connection Closed ...", this.LogPath);
                        }
                    });
                    Total.Client.DataReceived += new DataReceivedEventHandler((string data) =>
                    {
                        if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                        {
                            Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Description, "Responswe Received ...", this.LogPath);
                        }
                        str2 = data;
                        if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                        {
                            Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Response, str2, this.LogPath);
                        }
                        this.mre.Set();
                    });
                    Total.Client.DataSent += new DataSentEventHandler((int datalenght) =>
                    {
                        if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                        {
                            Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Description, "Data Sent ...", this.LogPath);
                        }
                    });
                    Total.Client.StateChanged += new StateChangedEventHandler((string str) =>
                    {
                        if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                        {
                            Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Description, str, this.LogPath);
                        }
                    });
                    if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                    {
                        Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Request, "Establishing Connection ...", this.LogPath);
                    }
                    this._timeOutTimer.Enabled = true;
                    this._timeOutTimer.Start();
                    Total.Client.Start();
                    if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                    {
                        Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Request, "Commanding ...", this.LogPath);
                    }
                    if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                    {
                        Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Request, message, this.LogPath);
                    }
                    Total.Oldsocket = null;
                    Total.Client.Send(message, ref Total.Oldsocket);
                    if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                    {
                        Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Description, "Waiting For Response ...", this.LogPath);
                    }
                    this.mre.WaitOne();
                    this.Dispose(false);
                }
                str1 = Total.ParseMessage((this._timeOutReached ? Total.MakeFackeResponse(FakeResponseType.Timeout) : str2));
            }
            catch (Exception exception)
            {
                Exception ex = exception;
                if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                {
                    Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Exception, ex.Message, this.LogPath);
                }
                if (ex.InnerException != null)
                {
                    if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                    {
                        Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Exception, ex.InnerException.Message, this.LogPath);
                    }
                }
                if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                {
                    Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Description, "Disposing Resources In Level 0 ", this.LogPath);
                }
                this.Dispose(false);
                base.State = ReadyState.Fault;
                throw;
            }
            return str1;
        }

        private string TcpCommunicateEnc(string message)
        {
            string str1;
            base.CheckObjectDisposeStatus();
            if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
            {
                Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Description, "Communication Begin ...", this.LogPath);
            }
            if (base.State != ReadyState.Ready)
            {
                throw new Exception("System resources occupancy\r\bInitiate Service");
            }
            base.State = ReadyState.Busy;
            this._timeOutTimer = new System.Timers.Timer()
            {
                Interval = (double)this.TimeOut
            };
            this._timeOutTimer.Elapsed += new ElapsedEventHandler((object sender, ElapsedEventArgs e) =>
            {
                if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                {
                    Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Description, "Timeout Reached . No Response !!!", this.LogPath);
                }
                this._timeOutReached = true;
            });
            try
            {
                string str2 = null;
                using (ClientSocket client = new ClientSocket(IPAddress.Parse(this.IpAddress), this.Port, Encoding.UTF8, 1024))
                {
                    client.ConnectionEstablished += new ConnectedEventHandler(() =>
                    {
                        if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                        {
                            Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Request, "Connection Established ...", this.LogPath);
                        }
                    });
                    client.ConnectionClosed += new ConnectionClosedEventHandler(() =>
                    {
                        if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                        {
                            Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Request, "Connection Closed ...", this.LogPath);
                        }
                    });
                    client.DataReceived += new DataReceivedEventHandler((string data) =>
                    {
                        if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                        {
                            Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Description, "Responswe Received ...", this.LogPath);
                        }
                        str2 = data;
                        if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                        {
                            Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Response, str2, this.LogPath);
                        }
                    });
                    client.DataSent += new DataSentEventHandler((int datalenght) =>
                    {
                        if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                        {
                            Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Description, "Data Sent ...", this.LogPath);
                        }
                    });
                    client.StateChanged += new StateChangedEventHandler((string str) =>
                    {
                        if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                        {
                            Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Description, str, this.LogPath);
                        }
                    });
                    if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                    {
                        Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Request, "Establishing Connection ...", this.LogPath);
                    }
                    this._timeOutTimer.Enabled = true;
                    this._timeOutTimer.Start();
                    client.Start();
                    if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                    {
                        Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Request, "Commanding ...", this.LogPath);
                    }
                    if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                    {
                        Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Request, message, this.LogPath);
                    }
                    client.Send(message, ref Total.Oldsocket);
                    if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                    {
                        Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Description, "Waiting For Response ...", this.LogPath);
                    }
                    while (true)
                    {
                        if ((this._timeOutReached ? true : !string.IsNullOrEmpty(str2)))
                        {
                            break;
                        }
                    }
                    this.Dispose(false);
                }
                str1 = str2;
            }
            catch (Exception exception)
            {
                Exception ex = exception;
                if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                {
                    Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Exception, ex.Message, this.LogPath);
                }
                if (ex.InnerException != null)
                {
                    if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                    {
                        Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Exception, ex.InnerException.Message, this.LogPath);
                    }
                }
                if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                {
                    Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Description, "Disposing Resources In Level 0 ", this.LogPath);
                }
                this.Dispose(false);
                base.State = ReadyState.Fault;
                throw;
            }
            return str1;
        }

        private void TcpCommunicateSend(string message)
        {
            base.CheckObjectDisposeStatus();
            if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
            {
                Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Description, "Communication Begin ...", this.LogPath);
            }
            try
            {
                if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                {
                    Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Description, "Data Sent ...", this.LogPath);
                }
                Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Request, "Commanding ...", this.LogPath);
                Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Request, message, this.LogPath);
                Total.Client.Send(message, ref Total.Oldsocket);
            }
            catch (Exception exception)
            {
                Exception ex = exception;
                if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                {
                    Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Exception, ex.Message, this.LogPath);
                }
                if (ex.InnerException != null)
                {
                    if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                    {
                        Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Exception, ex.InnerException.Message, this.LogPath);
                    }
                }
                if ((!this.GenerateLog ? false : this.LogPhrase.Equals("12713803")))
                {
                    Kiccc.Ing.PcPos.Logger.Logger.Log(Kiccc.Ing.PcPos.Logger.Logger.Type.Description, "Disposing Resources In Level 0 ", this.LogPath);
                }
                this.Dispose(false);
                base.State = ReadyState.Fault;
                throw;
            }
        }

        public void TerminateService()
        {
            this.Dispose();
        }

        public event ResponseReceivedEventHandler ResponseReceived;

        [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
        internal class MessageIndicator : Attribute
        {
            private string _default;

            public bool AppearIfNull
            {
                get;
                set;
            }

            public string Default
            {
                get
                {
                    return this._default;
                }
                set
                {
                    this._default = value;
                    this.GenerateDefaultValue = true;
                }
            }

            public bool DoTrim
            {
                get;
                set;
            }

            public bool GenerateDefaultValue
            {
                get;
                set;
            }

            public int MaxLenght
            {
                get;
                set;
            }

            public MessageType MessageType
            {
                get;
            }

            public bool MustHaveFixedLenght
            {
                get;
                set;
            }

            public bool MustHaveFixedLenghtIfEmpty
            {
                get;
                set;
            }

            public int Order
            {
                get;
                set;
            }

            public PaddingOption PaddingOption
            {
                get;
                set;
            }

            public MessageIndicator(MessageType messageType)
            {
                this.MessageType = messageType;
            }
        }

        public class MessageStructure
        {
            [MessageIndicator(MessageType.Sale, Order = 2, AppearIfNull = true, MaxLenght = 15, DoTrim = true, MustHaveFixedLenght = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.SaleWithPaymentId, Order = 2, AppearIfNull = true, MaxLenght = 15, DoTrim = true, MustHaveFixedLenght = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.VasDynamic, Order = 2, AppearIfNull = false, MaxLenght = 15, DoTrim = true, MustHaveFixedLenght = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.Multiplex, Order = 2, AppearIfNull = true, MaxLenght = 15, DoTrim = true, MustHaveFixedLenght = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.CreditRemaining, Order = 2, AppearIfNull = true, MaxLenght = 15, DoTrim = true, MustHaveFixedLenght = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.Special, Order = 2, AppearIfNull = true, MaxLenght = 15, DoTrim = true, MustHaveFixedLenght = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.MultiplexWithPaymentId, Order = 2, AppearIfNull = true, MaxLenght = 15, DoTrim = true, MustHaveFixedLenght = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.BillPayment, Order = 2, AppearIfNull = true, MaxLenght = 15, DoTrim = true, MustHaveFixedLenght = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.Extra, Order = 2, AppearIfNull = true, MaxLenght = 15, DoTrim = true, MustHaveFixedLenght = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.VasMultiplex, Order = 2, AppearIfNull = false, MaxLenght = 15, DoTrim = true, MustHaveFixedLenght = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.ESpecialPayment, Order = 2, AppearIfNull = true, MaxLenght = 15, DoTrim = true, MustHaveFixedLenght = false, PaddingOption = PaddingOption.None)]
            public string AcceptorId
            {
                get;
                set;
            }

            [MessageIndicator(MessageType.Sale, Order = 1, AppearIfNull = true, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.SaleWithPaymentId, Order = 1, AppearIfNull = true, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.VasDynamic, Order = 1, AppearIfNull = false, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.Multiplex, Order = 1, AppearIfNull = true, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.CreditRemaining, Order = 1, AppearIfNull = true, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.Special, Order = 1, AppearIfNull = true, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.MultiplexWithPaymentId, Order = 1, AppearIfNull = true, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.BillPayment, Order = 1, AppearIfNull = true, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.Extra, Order = 1, AppearIfNull = true, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.VasMultiplex, Order = 1, AppearIfNull = false, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.ESpecialPayment, Order = 1, AppearIfNull = true, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            public string AcceptorIdLenght
            {
                get
                {
                    string str;
                    string acceptorId = this.AcceptorId;
                    if (acceptorId != null)
                    {
                        str = acceptorId.Length.ToString();
                    }
                    else
                    {
                        str = null;
                    }
                    return str;
                }
            }

            [MessageIndicator(MessageType.Sale, Order = 5, AppearIfNull = true, DoTrim = true, MaxLenght = 12, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.SaleWithPaymentId, Order = 5, AppearIfNull = true, DoTrim = true, MaxLenght = 12, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.VasDynamic, Order = 5, AppearIfNull = false, DoTrim = true, MaxLenght = 12, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.Multiplex, Order = 5, AppearIfNull = true, DoTrim = true, MaxLenght = 12, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.CreditRemaining, Order = 5, AppearIfNull = true, DoTrim = true, MaxLenght = 12, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.Special, Order = 5, AppearIfNull = true, DoTrim = true, MaxLenght = 12, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.MultiplexWithPaymentId, Order = 5, AppearIfNull = true, DoTrim = true, MaxLenght = 12, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.BillPayment, Order = 5, AppearIfNull = true, DoTrim = true, MaxLenght = 12, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.Extra, Order = 5, AppearIfNull = true, DoTrim = true, MaxLenght = 12, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.VasMultiplex, Order = 5, AppearIfNull = false, DoTrim = true, MaxLenght = 12, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.ESpecialPayment, Order = 5, AppearIfNull = true, DoTrim = true, MaxLenght = 12, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            public string Amount
            {
                get;
                set;
            }

            [MessageIndicator(MessageType.Sale, Order = 13, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.SaleWithPaymentId, Order = 13, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.VasDynamic, Order = 13, AppearIfNull = false)]
            [MessageIndicator(MessageType.Multiplex, Order = 13, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.CreditRemaining, Order = 13, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.Special, Order = 13, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.MultiplexWithPaymentId, Order = 13, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.BillPayment, Order = 13, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.Extra, Order = 13, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.VasMultiplex, Order = 13, AppearIfNull = false, Default = "", DoTrim = true, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.ESpecialPayment, Order = 13, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            public string BankCode
            {
                get;
                set;
            }

            [MessageIndicator(MessageType.Sale, Order = 14, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 20, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.SaleWithPaymentId, Order = 14, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 20, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.VasDynamic, Order = 14, AppearIfNull = false)]
            [MessageIndicator(MessageType.Multiplex, Order = 14, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 20, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.CreditRemaining, Order = 14, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 20, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.Special, Order = 14, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 20, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.MultiplexWithPaymentId, Order = 14, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 20, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.BillPayment, Order = 14, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 20, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.Extra, Order = 14, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 20, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.VasMultiplex, Order = 14, AppearIfNull = false)]
            [MessageIndicator(MessageType.ESpecialPayment, Order = 14, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 20, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            public string BillId
            {
                get;
                set;
            }

            [MessageIndicator(MessageType.Sale, Order = 15, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 20, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.SaleWithPaymentId, Order = 15, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 20, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.VasDynamic, Order = 15, AppearIfNull = false)]
            [MessageIndicator(MessageType.Multiplex, Order = 15, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 20, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.CreditRemaining, Order = 15, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 20, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.Special, Order = 15, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 20, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.MultiplexWithPaymentId, Order = 15, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 20, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.BillPayment, Order = 15, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 20, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.Extra, Order = 15, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 20, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.VasMultiplex, Order = 15, AppearIfNull = false)]
            [MessageIndicator(MessageType.ESpecialPayment, Order = 15, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 20, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            public string BillPaymentId
            {
                get;
                set;
            }

            [MessageIndicator(MessageType.Sale, Order = 4, AppearIfNull = true, DoTrim = false, MaxLenght = 1, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.SaleWithPaymentId, Order = 4, AppearIfNull = true, DoTrim = false, MaxLenght = 1, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.VasDynamic, Order = 4, AppearIfNull = false, DoTrim = false, MaxLenght = 1, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.Multiplex, Order = 4, AppearIfNull = true, DoTrim = false, MaxLenght = 1, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.CreditRemaining, Order = 4, AppearIfNull = true, DoTrim = false, MaxLenght = 1, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.Special, Order = 4, AppearIfNull = true, DoTrim = false, MaxLenght = 1, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.MultiplexWithPaymentId, Order = 4, AppearIfNull = true, DoTrim = false, MaxLenght = 1, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.BillPayment, Order = 4, AppearIfNull = true, DoTrim = false, MaxLenght = 1, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.Extra, Order = 4, AppearIfNull = true, DoTrim = false, MaxLenght = 1, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.VasMultiplex, Order = 4, AppearIfNull = false, DoTrim = false, MaxLenght = 1, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.ESpecialPayment, Order = 4, AppearIfNull = true, DoTrim = false, MaxLenght = 1, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            public string Command
            {
                get;
                set;
            }

            [MessageIndicator(MessageType.Sale, Order = 17, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 99, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.SaleWithPaymentId, Order = 17, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 99, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.VasDynamic, Order = 17, AppearIfNull = false)]
            [MessageIndicator(MessageType.Multiplex, Order = 17, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 99, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.CreditRemaining, Order = 17, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 99, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.Special, Order = 17, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 99, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.MultiplexWithPaymentId, Order = 17, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 99, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.BillPayment, Order = 17, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 99, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.Extra, Order = 17, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 99, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.VasMultiplex, Order = 17, AppearIfNull = false)]
            [MessageIndicator(MessageType.ESpecialPayment, Order = 17, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 99, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            public string ExtraParam
            {
                get;
                set;
            }

            [MessageIndicator(MessageType.Sale, Order = 16, AppearIfNull = true, Default = "0", DoTrim = true, MaxLenght = 2, GenerateDefaultValue = true, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.SaleWithPaymentId, Order = 16, AppearIfNull = true, Default = "0", DoTrim = true, MaxLenght = 2, GenerateDefaultValue = true, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.VasDynamic, Order = 16, AppearIfNull = false)]
            [MessageIndicator(MessageType.Multiplex, Order = 16, AppearIfNull = true, Default = "0", DoTrim = true, MaxLenght = 2, GenerateDefaultValue = true, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.CreditRemaining, Order = 16, AppearIfNull = true, Default = "0", DoTrim = true, MaxLenght = 2, GenerateDefaultValue = true, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.Special, Order = 16, AppearIfNull = true, Default = "0", DoTrim = true, MaxLenght = 2, GenerateDefaultValue = true, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.MultiplexWithPaymentId, Order = 16, AppearIfNull = true, Default = "0", DoTrim = true, MaxLenght = 2, GenerateDefaultValue = true, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.BillPayment, Order = 16, AppearIfNull = true, Default = "0", DoTrim = true, MaxLenght = 2, GenerateDefaultValue = true, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.Extra, Order = 16, AppearIfNull = true, Default = "0", DoTrim = true, MaxLenght = 2, GenerateDefaultValue = true, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.VasMultiplex, Order = 16, AppearIfNull = false)]
            [MessageIndicator(MessageType.ESpecialPayment, Order = 16, AppearIfNull = true, Default = "0", DoTrim = true, MaxLenght = 2, GenerateDefaultValue = true, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            public string ExtraParamLenght
            {
                get;
                set;
            }

            [MessageIndicator(MessageType.Special, Order = 26, Default = "", AppearIfNull = false, MaxLenght = 2, DoTrim = true, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            public string Installments
            {
                get;
                set;
            }

            private MessageType MessageType
            {
                get;
                set;
            }

            [MessageIndicator(MessageType.Sale, Order = 12, AppearIfNull = true, Default = "0", DoTrim = true, MaxLenght = 0, MustHaveFixedLenght = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.SaleWithPaymentId, Order = 12, AppearIfNull = true, Default = "0", DoTrim = true, MaxLenght = 0, MustHaveFixedLenght = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.VasDynamic, Order = 12, AppearIfNull = false, Default = "", DoTrim = true, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.Multiplex, Order = 12, AppearIfNull = true, Default = "0", DoTrim = true, MaxLenght = 0, MustHaveFixedLenght = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.CreditRemaining, Order = 12, AppearIfNull = true, Default = "0", DoTrim = true, MaxLenght = 0, MustHaveFixedLenght = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.Special, Order = 12, AppearIfNull = true, Default = "0", DoTrim = true, MaxLenght = 0, MustHaveFixedLenght = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.MultiplexWithPaymentId, Order = 12, AppearIfNull = true, Default = "0", DoTrim = true, MaxLenght = 0, MustHaveFixedLenght = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.BillPayment, Order = 12, AppearIfNull = true, Default = "0", DoTrim = true, MaxLenght = 0, MustHaveFixedLenght = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.Extra, Order = 12, AppearIfNull = true, Default = "0", DoTrim = true, MaxLenght = 0, MustHaveFixedLenght = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.VasMultiplex, Order = 12, AppearIfNull = false, Default = "0", DoTrim = true, MaxLenght = 0, MustHaveFixedLenght = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.ESpecialPayment, Order = 12, AppearIfNull = true, Default = "0", DoTrim = true, MaxLenght = 0, MustHaveFixedLenght = false, PaddingOption = PaddingOption.None)]
            public string MultiPlexAmount
            {
                get;
                set;
            }

            [MessageIndicator(MessageType.Sale, Order = 11, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.SaleWithPaymentId, Order = 11, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.VasDynamic, Order = 11, AppearIfNull = false)]
            [MessageIndicator(MessageType.Multiplex, Order = 11, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.CreditRemaining, Order = 11, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.Special, Order = 11, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.MultiplexWithPaymentId, Order = 11, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.BillPayment, Order = 11, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.Extra, Order = 11, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.VasMultiplex, Order = 11, AppearIfNull = false)]
            [MessageIndicator(MessageType.ESpecialPayment, Order = 11, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            public string MultiplexAmountQty
            {
                get;
                set;
            }

            [MessageIndicator(MessageType.Sale, Order = 6, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 18, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.SaleWithPaymentId, Order = 6, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 18, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.VasDynamic, Order = 6, AppearIfNull = false, Default = "", DoTrim = true, MaxLenght = 18, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.Multiplex, Order = 6, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 18, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.CreditRemaining, Order = 6, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 18, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.Special, Order = 6, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 18, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.MultiplexWithPaymentId, Order = 6, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 18, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.BillPayment, Order = 6, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 18, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.Extra, Order = 6, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 18, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.VasMultiplex, Order = 6, AppearIfNull = false, Default = "", DoTrim = true, MaxLenght = 18, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.ESpecialPayment, Order = 6, AppearIfNull = true, Default = "", DoTrim = true, MaxLenght = 18, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            public string PaymentId
            {
                get;
                set;
            }

            [MessageIndicator(MessageType.Sale, Order = 10, AppearIfNull = true, Default = "0", DoTrim = true, MaxLenght = 18, MustHaveFixedLenght = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.SaleWithPaymentId, Order = 10, AppearIfNull = true, Default = "0", DoTrim = true, MaxLenght = 18, MustHaveFixedLenght = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.VasDynamic, Order = 10, AppearIfNull = false)]
            [MessageIndicator(MessageType.Multiplex, Order = 10, AppearIfNull = true, Default = "0", DoTrim = true, MaxLenght = 18, MustHaveFixedLenght = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.CreditRemaining, Order = 10, AppearIfNull = true, Default = "0", DoTrim = true, MaxLenght = 18, MustHaveFixedLenght = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.Special, Order = 10, AppearIfNull = true, Default = "0", DoTrim = true, MaxLenght = 18, MustHaveFixedLenght = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.MultiplexWithPaymentId, Order = 10, AppearIfNull = true, Default = "0", DoTrim = true, MaxLenght = 18, MustHaveFixedLenght = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.BillPayment, Order = 10, AppearIfNull = true, Default = "0", DoTrim = true, MaxLenght = 18, MustHaveFixedLenght = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.Extra, Order = 10, AppearIfNull = true, Default = "0", DoTrim = true, MaxLenght = 18, MustHaveFixedLenght = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.VasMultiplex, Order = 10, AppearIfNull = false, Default = "0", DoTrim = true, MaxLenght = 18, MustHaveFixedLenght = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.ESpecialPayment, Order = 10, AppearIfNull = true, Default = "0", DoTrim = true, MaxLenght = 18, MustHaveFixedLenght = false, PaddingOption = PaddingOption.None)]
            public string PaymentNumber
            {
                get;
                set;
            }

            [MessageIndicator(MessageType.Sale, Order = 9, AppearIfNull = true, DoTrim = true, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.SaleWithPaymentId, Order = 9, AppearIfNull = true, DoTrim = true, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.VasDynamic, Order = 9, AppearIfNull = false)]
            [MessageIndicator(MessageType.Multiplex, Order = 9, AppearIfNull = true, DoTrim = true, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.CreditRemaining, Order = 9, AppearIfNull = true, DoTrim = true, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.Special, Order = 9, AppearIfNull = true, DoTrim = true, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.MultiplexWithPaymentId, Order = 9, AppearIfNull = true, DoTrim = true, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.BillPayment, Order = 9, AppearIfNull = true, DoTrim = true, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.Extra, Order = 9, AppearIfNull = true, DoTrim = true, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.VasMultiplex, Order = 9, AppearIfNull = false, DoTrim = true, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.ESpecialPayment, Order = 9, AppearIfNull = true, DoTrim = true, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            public string PaymentNumberLenght
            {
                get
                {
                    string str;
                    string paymentNumber = this.PaymentNumber;
                    if (paymentNumber != null)
                    {
                        str = paymentNumber.Length.ToString();
                    }
                    else
                    {
                        str = null;
                    }
                    return str;
                }
            }

            [MessageIndicator(MessageType.Sale, Order = 19, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 24, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.SaleWithPaymentId, Order = 19, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 24, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.VasDynamic, Order = 19, AppearIfNull = false)]
            [MessageIndicator(MessageType.Multiplex, Order = 19, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 24, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.CreditRemaining, Order = 19, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 24, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.Special, Order = 19, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 24, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.MultiplexWithPaymentId, Order = 19, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 24, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.BillPayment, Order = 19, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 24, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.Extra, Order = 19, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 24, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.VasMultiplex, Order = 19, AppearIfNull = false)]
            [MessageIndicator(MessageType.ESpecialPayment, Order = 19, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 24, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            public string PrintableInfoLine01
            {
                get;
                set;
            }

            [MessageIndicator(MessageType.Sale, Order = 18, AppearIfNull = true, Default = "0", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 2, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.SaleWithPaymentId, Order = 18, AppearIfNull = true, Default = "0", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 2, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.VasDynamic, Order = 18, AppearIfNull = false)]
            [MessageIndicator(MessageType.Multiplex, Order = 18, AppearIfNull = true, Default = "0", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 2, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.CreditRemaining, Order = 18, AppearIfNull = true, Default = "0", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 2, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.Special, Order = 18, AppearIfNull = true, Default = "0", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 2, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.MultiplexWithPaymentId, Order = 18, AppearIfNull = true, Default = "0", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 2, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.BillPayment, Order = 18, AppearIfNull = true, Default = "0", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 2, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.Extra, Order = 18, AppearIfNull = true, Default = "0", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 2, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.VasMultiplex, Order = 18, AppearIfNull = false)]
            [MessageIndicator(MessageType.ESpecialPayment, Order = 18, AppearIfNull = true, Default = "0", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 2, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            public string PrintableInfoLine01Lenght
            {
                get;
                set;
            }

            [MessageIndicator(MessageType.Sale, Order = 21, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 24, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.SaleWithPaymentId, Order = 21, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 24, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.VasDynamic, Order = 21, AppearIfNull = false)]
            [MessageIndicator(MessageType.Multiplex, Order = 21, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 24, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.CreditRemaining, Order = 21, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 24, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.Special, Order = 21, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 24, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.MultiplexWithPaymentId, Order = 21, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 24, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.BillPayment, Order = 21, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 24, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.Extra, Order = 21, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 24, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.VasMultiplex, Order = 21, AppearIfNull = false)]
            [MessageIndicator(MessageType.ESpecialPayment, Order = 21, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 24, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            public string PrintableInfoLine02
            {
                get;
                set;
            }

            [MessageIndicator(MessageType.Sale, Order = 20, AppearIfNull = true, Default = "0", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 2, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.SaleWithPaymentId, Order = 20, AppearIfNull = true, Default = "0", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 2, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.VasDynamic, Order = 20, AppearIfNull = false)]
            [MessageIndicator(MessageType.Multiplex, Order = 20, AppearIfNull = true, Default = "0", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 2, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.CreditRemaining, Order = 20, AppearIfNull = true, Default = "0", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 2, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.Special, Order = 20, AppearIfNull = true, Default = "0", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 2, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.MultiplexWithPaymentId, Order = 20, AppearIfNull = true, Default = "0", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 2, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.BillPayment, Order = 20, AppearIfNull = true, Default = "0", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 2, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.Extra, Order = 20, AppearIfNull = true, Default = "0", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 2, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.VasMultiplex, Order = 20, AppearIfNull = false)]
            [MessageIndicator(MessageType.ESpecialPayment, Order = 20, AppearIfNull = true, Default = "0", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 2, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            public string PrintableInfoLine02Lenght
            {
                get;
                set;
            }

            [MessageIndicator(MessageType.Sale, Order = 23, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 24, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.SaleWithPaymentId, Order = 23, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 24, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.VasDynamic, Order = 23, AppearIfNull = false)]
            [MessageIndicator(MessageType.Multiplex, Order = 23, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 24, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.CreditRemaining, Order = 23, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 24, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.Special, Order = 23, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 24, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.MultiplexWithPaymentId, Order = 23, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 24, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.BillPayment, Order = 23, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 24, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.Extra, Order = 23, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 24, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.VasMultiplex, Order = 23, AppearIfNull = false)]
            [MessageIndicator(MessageType.ESpecialPayment, Order = 23, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 24, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            public string PrintableInfoLine03
            {
                get;
                set;
            }

            [MessageIndicator(MessageType.Sale, Order = 22, AppearIfNull = true, Default = "0", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 2, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.SaleWithPaymentId, Order = 22, AppearIfNull = true, Default = "0", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 2, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.VasDynamic, Order = 22, AppearIfNull = false)]
            [MessageIndicator(MessageType.Multiplex, Order = 22, AppearIfNull = true, Default = "0", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 2, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.CreditRemaining, Order = 22, AppearIfNull = true, Default = "0", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 2, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.Special, Order = 22, AppearIfNull = true, Default = "0", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 2, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.MultiplexWithPaymentId, Order = 22, AppearIfNull = true, Default = "0", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 2, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.BillPayment, Order = 22, AppearIfNull = true, Default = "0", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 2, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.Extra, Order = 22, AppearIfNull = true, Default = "0", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 2, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.VasMultiplex, Order = 22, AppearIfNull = false)]
            [MessageIndicator(MessageType.ESpecialPayment, Order = 22, AppearIfNull = true, Default = "0", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 2, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            public string PrintableInfoLine03Lenght
            {
                get;
                set;
            }

            [MessageIndicator(MessageType.Sale, Order = 25, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 24, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.SaleWithPaymentId, Order = 25, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 24, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.VasDynamic, Order = 25, AppearIfNull = false)]
            [MessageIndicator(MessageType.Multiplex, Order = 25, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 24, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.CreditRemaining, Order = 25, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 24, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.Special, Order = 25, AppearIfNull = false)]
            [MessageIndicator(MessageType.MultiplexWithPaymentId, Order = 25, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 24, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.BillPayment, Order = 25, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 24, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.Extra, Order = 25, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 24, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.VasMultiplex, Order = 25, AppearIfNull = false)]
            [MessageIndicator(MessageType.ESpecialPayment, Order = 25, AppearIfNull = true, Default = "", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 24, MustHaveFixedLenght = false, MustHaveFixedLenghtIfEmpty = false, PaddingOption = PaddingOption.None)]
            public string PrintableInfoLine04
            {
                get;
                set;
            }

            [MessageIndicator(MessageType.Sale, Order = 24, AppearIfNull = true, Default = "0", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 2, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.SaleWithPaymentId, Order = 24, AppearIfNull = true, Default = "0", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 2, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.VasDynamic, Order = 24, AppearIfNull = false)]
            [MessageIndicator(MessageType.Multiplex, Order = 24, AppearIfNull = true, Default = "0", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 2, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.CreditRemaining, Order = 24, AppearIfNull = true, Default = "0", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 2, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.Special, Order = 24, AppearIfNull = true, Default = "0", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 2, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.MultiplexWithPaymentId, Order = 24, AppearIfNull = true, Default = "0", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 2, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.BillPayment, Order = 24, AppearIfNull = true, Default = "0", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 2, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.Extra, Order = 24, AppearIfNull = true, Default = "0", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 2, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.VasMultiplex, Order = 24, AppearIfNull = false)]
            [MessageIndicator(MessageType.ESpecialPayment, Order = 24, AppearIfNull = true, Default = "0", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 2, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            public string PrintableInfoLine04Lenght
            {
                get;
                set;
            }

            [MessageIndicator(MessageType.Sale, Order = 0, AppearIfNull = true, MaxLenght = 12, PaddingOption = PaddingOption.PadLeft, MustHaveFixedLenght = true, DoTrim = true)]
            [MessageIndicator(MessageType.SaleWithPaymentId, Order = 0, AppearIfNull = true, MaxLenght = 12, PaddingOption = PaddingOption.PadLeft, MustHaveFixedLenght = true, DoTrim = true)]
            [MessageIndicator(MessageType.VasDynamic, Order = 0, AppearIfNull = false, MaxLenght = 12, PaddingOption = PaddingOption.PadLeft, MustHaveFixedLenght = true, DoTrim = true)]
            [MessageIndicator(MessageType.Multiplex, Order = 0, AppearIfNull = true, MaxLenght = 12, PaddingOption = PaddingOption.PadLeft, MustHaveFixedLenght = true, DoTrim = true)]
            [MessageIndicator(MessageType.CreditRemaining, Order = 0, AppearIfNull = true, MaxLenght = 12, PaddingOption = PaddingOption.PadLeft, MustHaveFixedLenght = true, DoTrim = true)]
            [MessageIndicator(MessageType.Special, Order = 0, AppearIfNull = true, MaxLenght = 12, PaddingOption = PaddingOption.PadLeft, MustHaveFixedLenght = true, DoTrim = true)]
            [MessageIndicator(MessageType.MultiplexWithPaymentId, Order = 0, AppearIfNull = true, MaxLenght = 12, PaddingOption = PaddingOption.PadLeft, MustHaveFixedLenght = true, DoTrim = true)]
            [MessageIndicator(MessageType.BillPayment, Order = 0, AppearIfNull = true, MaxLenght = 12, PaddingOption = PaddingOption.PadLeft, MustHaveFixedLenght = true, DoTrim = true)]
            [MessageIndicator(MessageType.Extra, Order = 0, AppearIfNull = true, MaxLenght = 12, PaddingOption = PaddingOption.PadLeft, MustHaveFixedLenght = true, DoTrim = true)]
            [MessageIndicator(MessageType.VasMultiplex, Order = 0, AppearIfNull = false, MaxLenght = 12, PaddingOption = PaddingOption.PadLeft, MustHaveFixedLenght = true, DoTrim = true)]
            [MessageIndicator(MessageType.ESpecialPayment, Order = 0, AppearIfNull = true, MaxLenght = 12, PaddingOption = PaddingOption.PadLeft, MustHaveFixedLenght = true, DoTrim = true)]
            public string Serial
            {
                get;
                set;
            }

            [MessageIndicator(MessageType.Sale, Order = 8, AppearIfNull = true, Default = "0", DoTrim = true, MaxLenght = 18, MustHaveFixedLenght = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.SaleWithPaymentId, Order = 8, AppearIfNull = true, Default = "0", DoTrim = true, MaxLenght = 18, MustHaveFixedLenght = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.VasDynamic, Order = 8, AppearIfNull = false)]
            [MessageIndicator(MessageType.Multiplex, Order = 8, Default = "0", DoTrim = true, MaxLenght = 18, MustHaveFixedLenght = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.CreditRemaining, Order = 8, AppearIfNull = true, Default = "0", DoTrim = true, MaxLenght = 18, MustHaveFixedLenght = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.Special, Order = 8, AppearIfNull = true, Default = "0", DoTrim = true, MaxLenght = 18, MustHaveFixedLenght = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.MultiplexWithPaymentId, Order = 8, AppearIfNull = true, Default = "0", DoTrim = true, MaxLenght = 18, MustHaveFixedLenght = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.BillPayment, Order = 8, AppearIfNull = true, Default = "0", DoTrim = true, MaxLenght = 18, MustHaveFixedLenght = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.Extra, Order = 8, AppearIfNull = true, Default = "0", DoTrim = true, MaxLenght = 18, MustHaveFixedLenght = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.VasMultiplex, Order = 8, AppearIfNull = false, Default = "0", DoTrim = true, MaxLenght = 18, MustHaveFixedLenght = false, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.ESpecialPayment, Order = 8, AppearIfNull = true, Default = "0", DoTrim = true, MaxLenght = 18, MustHaveFixedLenght = false, PaddingOption = PaddingOption.None)]
            public string ServiceId
            {
                get;
                set;
            }

            [MessageIndicator(MessageType.Sale, Order = 7, AppearIfNull = true, DoTrim = false, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.SaleWithPaymentId, Order = 7, AppearIfNull = true, DoTrim = false, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.VasDynamic, Order = 7, AppearIfNull = false)]
            [MessageIndicator(MessageType.Multiplex, Order = 7, AppearIfNull = true, DoTrim = false, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.CreditRemaining, Order = 7, AppearIfNull = true, DoTrim = false, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.Special, Order = 7, AppearIfNull = true, DoTrim = false, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.MultiplexWithPaymentId, Order = 7, AppearIfNull = true, DoTrim = false, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.BillPayment, Order = 7, AppearIfNull = true, DoTrim = false, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.Extra, Order = 7, AppearIfNull = true, DoTrim = false, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.VasMultiplex, Order = 7, AppearIfNull = false, DoTrim = false, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            [MessageIndicator(MessageType.ESpecialPayment, Order = 7, AppearIfNull = true, DoTrim = false, MaxLenght = 2, MustHaveFixedLenght = true, PaddingOption = PaddingOption.PadLeft)]
            public string ServiceIdLenght
            {
                get
                {
                    string str;
                    string serviceId = this.ServiceId;
                    if (serviceId != null)
                    {
                        str = serviceId.Length.ToString();
                    }
                    else
                    {
                        str = null;
                    }
                    return str;
                }
            }

            [MessageIndicator(MessageType.Sale, Order = 3, AppearIfNull = true, MaxLenght = 8, DoTrim = true, MustHaveFixedLenght = true, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.SaleWithPaymentId, Order = 3, AppearIfNull = true, MaxLenght = 8, DoTrim = true, MustHaveFixedLenght = true, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.VasDynamic, Order = 3, AppearIfNull = false, MaxLenght = 8, DoTrim = true, MustHaveFixedLenght = true, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.Multiplex, Order = 3, AppearIfNull = true, MaxLenght = 8, DoTrim = true, MustHaveFixedLenght = true, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.CreditRemaining, Order = 3, AppearIfNull = true, MaxLenght = 8, DoTrim = true, MustHaveFixedLenght = true, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.Special, Order = 3, AppearIfNull = true, MaxLenght = 8, DoTrim = true, MustHaveFixedLenght = true, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.MultiplexWithPaymentId, Order = 3, AppearIfNull = true, MaxLenght = 8, DoTrim = true, MustHaveFixedLenght = true, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.BillPayment, Order = 3, AppearIfNull = true, MaxLenght = 8, DoTrim = true, MustHaveFixedLenght = true, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.Extra, Order = 3, AppearIfNull = true, MaxLenght = 8, DoTrim = true, MustHaveFixedLenght = true, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.VasMultiplex, Order = 3, AppearIfNull = false, MaxLenght = 8, DoTrim = true, MustHaveFixedLenght = true, PaddingOption = PaddingOption.None)]
            [MessageIndicator(MessageType.ESpecialPayment, Order = 3, AppearIfNull = true, MaxLenght = 8, DoTrim = true, MustHaveFixedLenght = true, PaddingOption = PaddingOption.None)]
            public string TerminalId
            {
                get;
                set;
            }

            [MessageIndicator(MessageType.VasMultiplex, Order = 11, AppearIfNull = true, Default = "1", DoTrim = true, GenerateDefaultValue = true, MaxLenght = 2, MustHaveFixedLenght = true, MustHaveFixedLenghtIfEmpty = true, PaddingOption = PaddingOption.PadLeft)]
            public string VasMultiplexIndex
            {
                get;
                set;
            }

            public MessageStructure(MessageType type)
            {
                this.MessageType = type;
            }

            public static Total.MessageStructure Create(MessageType type)
            {
                string str;
                object i = Activator.CreateInstance(typeof(Total.MessageStructure), new object[] { type });
                PropertyInfo[] properties = i.GetType().GetProperties();
                for (int num = 0; num < (int)properties.Length; num++)
                {
                    PropertyInfo p = properties[num];
                    object[] att = p.GetCustomAttributes(typeof(Total.MessageIndicator), false);
                    if (att.Length != 0)
                    {
                        Total.MessageIndicator at = null;
                        object[] objArray = att;
                        int num1 = 0;
                        while (num1 < (int)objArray.Length)
                        {
                            Total.MessageIndicator o = (Total.MessageIndicator)objArray[num1];
                            if (o.MessageType == type)
                            {
                                at = o;
                                break;
                            }
                            else
                            {
                                num1++;
                            }
                        }
                        if (at != null)
                        {
                            if (at.GenerateDefaultValue)
                            {
                                string def = at.Default;
                                if (at.MustHaveFixedLenght)
                                {
                                    MethodInfo method = def.GetType().GetMethod(at.PaddingOption.ToString(), new Type[] { typeof(int), typeof(char) });
                                    if (method != null)
                                    {
                                        str = method.Invoke(def, new object[] { at.MaxLenght, '0' }).ToString();
                                    }
                                    else
                                    {
                                        str = null;
                                    }
                                    def = str;
                                }
                                p.SetValue(i, def, null);
                            }
                        }
                    }
                }
                return (Total.MessageStructure)i;
            }

            public override string ToString()
            {
                string str;
                bool flag;
                string str1;
                int? nullable;
                string str2;
                PropertyInfo[] pis = this.GetType().GetProperties();
                string result = string.Empty;
                string[] tmparray = new string[(int)pis.Length - 1];
                PropertyInfo[] propertyInfoArray = pis;
                for (int i = 0; i < (int)propertyInfoArray.Length; i++)
                {
                    PropertyInfo pi = propertyInfoArray[i];
                    object[] att = pi.GetCustomAttributes(typeof(Total.MessageIndicator), false);
                    if (att.Length != 0)
                    {
                        Total.MessageIndicator at = null;
                        object[] objArray = att;
                        for (int j = 0; j < (int)objArray.Length; j++)
                        {
                            Total.MessageIndicator o = (Total.MessageIndicator)objArray[j];
                            if (o.MessageType == this.MessageType)
                            {
                                at = o;
                            }
                        }
                        if (at != null)
                        {
                            object value = pi.GetValue(this, null);
                            if (value != null)
                            {
                                str = value.ToString();
                            }
                            else
                            {
                                str = null;
                            }
                            string tmpv = str;
                            if (at.DoTrim)
                            {
                                if (tmpv != null)
                                {
                                    str2 = tmpv.Trim();
                                }
                                else
                                {
                                    str2 = null;
                                }
                                tmpv = str2;
                            }
                            if (!at.MustHaveFixedLenght)
                            {
                                flag = false;
                            }
                            else
                            {
                                int maxLenght = at.MaxLenght;
                                if (tmpv != null)
                                {
                                    nullable = new int?(tmpv.Length);
                                }
                                else
                                {
                                    nullable = null;
                                }
                                int? nullable1 = nullable;
                                flag = (maxLenght == nullable1.GetValueOrDefault() ? !nullable1.HasValue : true);
                            }
                            if (flag)
                            {
                                if (tmpv != null)
                                {
                                    MethodInfo method = tmpv.GetType().GetMethod(at.PaddingOption.ToString(), new Type[] { typeof(int), typeof(char) });
                                    if (method != null)
                                    {
                                        object obj = method.Invoke(tmpv, new object[] { at.MaxLenght, '0' });
                                        if (obj != null)
                                        {
                                            str1 = obj.ToString();
                                        }
                                        else
                                        {
                                            str1 = null;
                                        }
                                    }
                                    else
                                    {
                                        str1 = null;
                                    }
                                }
                                else
                                {
                                    str1 = null;
                                }
                                tmpv = str1;
                            }
                            if (pi.CanWrite)
                            {
                                pi.SetValue(this, tmpv, null);
                            }
                            tmparray[at.Order] = (at.AppearIfNull || tmpv != null ? tmpv : "?");
                        }
                    }
                }
                for (int counter = 0; counter < (int)tmparray.Length; counter++)
                {
                    result = string.Concat(result, string.Concat(tmparray[counter] ?? string.Empty, (counter == (int)tmparray.Length - 1 || !(tmparray[counter] != "?") ? string.Empty : "|")).Replace("?", string.Empty));
                }
                while (result.LastIndexOf("|") == result.Length - 1)
                {
                    result = result.Remove(result.LastIndexOf("|", StringComparison.Ordinal), 1);
                }
                return result;
            }
        }
    }
}