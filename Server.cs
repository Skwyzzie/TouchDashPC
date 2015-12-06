using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TouchDashPC
{
    public delegate void MessageEventHandler(String message, EventArgs e);
    public class Server
    {
        public event MessageEventHandler MessageRecieved;
        private Socket _srv;
        private IPEndPoint _ip ;

        private byte[] _data;
        private String _message = null;


        public Server(string ip, int port)
        {
            // Decode ip address and make sure port number is safe before creating the server
            try
            {
                //System ports or dynamic private ports
                if (port < 1024 || port > 49151)
                {
                    throw new Exception("Port out of acceptable range.");
                }
                _ip = new IPEndPoint(IPAddress.Parse(ip), port);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Server(string _ip)
            // Call the main constructor with a default port
            : this(_ip, 10413)
        { }

        public void start()
        {
            StringBuilder sb = new StringBuilder();
            _srv = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                _srv.Connect(_ip);
            }
            catch (SocketException)
            {
                throw new Exception("Unable to connect to server.");
            }

            _data = new byte[1024];
            int length = _srv.Receive(_data);
            while (length != 0)
            {
                sb.Append(Encoding.ASCII.GetString(_data, 0, length));
            }
            _message = sb.ToString();
        }

        public void stop()
        {
            _srv.Shutdown(SocketShutdown.Both);
            _srv.Close();
        }

        public String getIp()
        {
            return _ip.Address.ToString();
        }

        public String getPort()
        {
            return _ip.Port.ToString();
        }

        public String getInfo()
        {
            return String.Format("Server listening to {0} on port {1}", _ip.Address.ToString(), _ip.Port.ToString());
        }

        protected virtual void OnMessageRecieved(EventArgs e)
        {
            if (MessageRecieved != null)
                MessageRecieved(_message, e);
        }
    }
}
