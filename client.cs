using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace ArduinoSocketClient
{

    public class SensorAddress
    {
        private int _port = 0;
        private string _address = "";
        public SensorAddress(string Address, int Port)
        {
            _port = Port;
            _address = Address;
        }
        public int GetPort 
        { 
            get { 
                return _port; 
            } 
            
        }
        public string GetAddress
        {
            get {
                return _address;
            }
        }
    }

    public class DataEventArgs:EventArgs
    {
        private string _data = "";
 
        public string Value {
            get
            {
                return _data;        
            }

            set
            {
                _data = value;
            }
    
        }
    }

    public class ConnectionStateEventArgs : EventArgs
    {
        private int _state = -1;
        private string _message = "";

        public ConnectionStateEventArgs(int state, string message) {
            _state = state;
            _message = message;
        }

        public int GetState
        {
            get
            {
                return _state;
            }
        }
        public string GetMessage
        {
            get
            {
                return _message;
            }
        }

        public const int DISCONNECTED = 0;
        public const int CONNECTED = 1;
        public const int READY = 2;
    }

    /// <summary>
    /// Simple implementaton of propritery request and response sensor protocol
    /// Author: p4r1tyb1t
    /// </summary>
    public class SensorProtocol
    {
        private TcpClient _tcpclnt = null;
        private Stream _stm = null;
        private Boolean IsSystemReady = false;
        private ASCIIEncoding _enc = new ASCIIEncoding();

 
        public delegate void DataRecievedHandler(DataEventArgs e);
        public delegate void ConnectionStateHandler(ConnectionStateEventArgs e);
        public event DataRecievedHandler DataRecieved;
        public event ConnectionStateHandler ConnectionState;
 
        public SensorProtocol()
        {
        }


        public void Connect(SensorAddress connection)
        {
            try
            {
                _tcpclnt = new TcpClient();
                _tcpclnt.Connect(connection.GetAddress, connection.GetPort);
                ConnectionState(new ConnectionStateEventArgs(ConnectionStateEventArgs.CONNECTED, "Connected"));

                _stm = _tcpclnt.GetStream();
                if (_stm != null)
                {
                    byte[] RecievedBytes = new byte[100];
                    int BytesRead = _stm.Read(RecievedBytes, 0, 100);
                    if (BytesRead > 0)
                    {
                        if (ByteArrayToString(RecievedBytes).Contains("Ready!"))
                        {
                            ConnectionState(new ConnectionStateEventArgs(ConnectionStateEventArgs.READY, "Ready!"));
                            IsSystemReady = true;
                        }
                    }
                }

            }
            catch (NullReferenceException nex)
            {
                Console.WriteLine(nex.Message);
            }
            catch (Exception ex)
            {
                ConnectionState(new ConnectionStateEventArgs(ConnectionStateEventArgs.DISCONNECTED, ex.Message));
            }
            finally
            {

            }
 
        }

        /// <summary>
        /// Request Data from Sensor
        /// </summary>
        /// <returns></returns>
        public Boolean RequestData()
        {
            byte[] SendBytes = null;
            byte[] RecvBytes = new byte[100];

            int BytesRead = 0;

            if (IsSystemReady)
            {
                SendBytes = _enc.GetBytes("SEND!.");
                _stm.Write(SendBytes, 0, SendBytes.Length);
                Thread.Sleep(60);
                BytesRead = _stm.Read(RecvBytes, 0, 100);
                if (BytesRead > 0)
                {
                    String rd = ByteArrayToString(RecvBytes);
                    DataEventArgs data = new DataEventArgs();
                    data.Value = rd;
                    DataRecieved(data);
                    return true;
               }

            }
            return false;
        }

        /// <summary>
        /// Convert Byte Array to String
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private string ByteArrayToString(byte[] data)
        {
            return System.Text.ASCIIEncoding.ASCII.GetString(data);
        }

        /// <summary>
        /// Initialize byte array
        /// </summary>
        /// <param name="arr"></param>
        private void InitByteArray(byte[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = 0;
            }
        }

        public void Disconnect()
        {

        }
    
    } //End Class

    /// <summary>
    /// Simple test client
    /// </summary>
    public class SensorClient
    {
        private TcpClient _tcpclnt = new TcpClient();
        private SensorForm _frm = null;
        private Stream _stm = null;
        private SensorProtocol _p = null;
        //
        public const String IP_ADDRESS = "192.168.0.9";
        public const int IP_PORT = 1000;
         

        public SensorClient(SensorForm frm) {
            _frm = frm;
             _p = new SensorProtocol();
             if (_p != null)
             {
                 _p.ConnectionState += new SensorProtocol.ConnectionStateHandler(p_ConnectionState);
                 _p.DataRecieved += new SensorProtocol.DataRecievedHandler(p_DataRecieved);
                 _p.Connect(new SensorAddress(IP_ADDRESS, IP_PORT));
             }

        }

        private void p_ConnectionState(ConnectionStateEventArgs args)
        {
            _frm.printOutput("Connnecting..." + args.GetMessage);
        }

        private void p_DataRecieved(DataEventArgs args)
        {
            _frm.printOutput(args.Value);
            _frm.updateProgress(args.Value);
        }

        private string readStringFromData(byte[] data)
        {
            return System.Text.ASCIIEncoding.ASCII.GetString(data);
        }

        private void InitByteArray(byte[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = 0;
            }
        }

        /// <summary>
        /// Poll for data from sensor
        /// </summary>
        public void PollForData()
        {
            string rd = "";
            try
            {
                while (true)
                {
                    rd = _p.RequestData().ToString();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error..... " + e.StackTrace);
            }
            
        }


    }
}
