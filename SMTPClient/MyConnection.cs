using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

using System.Net;

namespace Client {
    public sealed class MyConnection {

        // SINGLETON PART
        private static readonly MyConnection instance = new MyConnection();

        static MyConnection() { }
        private MyConnection() { }
        public static MyConnection Instance {
            get { return instance; }
        }
        // END OF SINGLETON PART



        //IPAddress[] ips = Dns.GetHostAddresses("ns1.razvi.rest");

        //important constants
        System.Net.Sockets.TcpClient m_clientSocket;
        NetworkStream m_dataStream;
        const string m_ipServer = "10.0.0.4";
        const int m_portServer = 25;
        const int m_streamSize = Protocol.MESSAGE_SIZE;

        System.IO.StreamReader m_reader;
        System.IO.StreamWriter m_writer;


        //public functions
        public void ConnectToServer() {

            m_clientSocket = new System.Net.Sockets.TcpClient();
            try {

                m_clientSocket.Connect(m_ipServer, m_portServer);
                m_dataStream = m_clientSocket.GetStream();

                m_reader = new System.IO.StreamReader(m_dataStream);
                m_writer = new System.IO.StreamWriter(m_dataStream);
            }
            catch (SocketException e) {

                //Log error message somewhere
            }
        }
        public void SendToServer(Message message) {

            if (!m_dataStream.CanWrite) {

                //log error message
                return;
            }
            byte[] bytes = Protocol.ConvertToBytes(message);
            m_dataStream.Write(bytes, 0, bytes.Length);
            m_dataStream.Flush();
        }
        public Message GetFromServer() {

            byte[] bytes = new byte[m_streamSize];
            m_dataStream.Read(bytes, 0, bytes.Length);
            return Protocol.ConvertFromBytes(bytes);
        }

        public void SMTP_read() {

            string result = null;
            if (m_dataStream.CanRead)
                result = m_reader.ReadLine();

            Console.WriteLine(result);
        }
        public void SMTP_write(string text) {

            m_writer.WriteLine(text);
            m_writer.Flush();
            Console.WriteLine("eu: " + text);
        }
    }
}
