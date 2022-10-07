using System;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    public partial class Server : Form
    {
        TcpListener TCPListener;
        TcpClient TCPClient;
        byte[] message;

        public Server()
        {
            InitializeComponent();
        }

        private void btn_Start_Click(object sender, EventArgs e)
        {
            IPAddress IP;
            int Port;

            if (string.IsNullOrEmpty(txt_IP.Text) || string.IsNullOrEmpty(txt_IP.Text))
            {
                MessageBox.Show("Vui lòng nhập IP và Port từ 1000 trở lên!", "Thông báo!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!int.TryParse(txt_Port.Text, out Port)) // covert text port to PORT integer
            {
                MessageBox.Show("Vui lòng nhập Port từ 1000 trở lên!", "Thông báo!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!IPAddress.TryParse(txt_IP.Text, out IP))
            {
                MessageBox.Show("Vui lòng nhập đúng địa chỉ IP!", "Thông báo!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            TCPListener = new TcpListener(IP, Port);
            TCPListener.Start();
            TCPListener.BeginAcceptTcpClient(AcceptTcpClient, TCPListener);
            btn_Start.Enabled = false;
            txt_message.Enabled = true;
            btn_Send.Enabled = true;
            txt_IP.Enabled = false;
            txt_Port.Enabled = false;
            ShowMessage("Server Starting...");

        }

        void AcceptTcpClient(IAsyncResult iar)
        {
            TcpListener TCP = (TcpListener)iar.AsyncState;
            try
            {
                TCPClient = TCP.EndAcceptTcpClient(iar);
                ShowMessage("Client Connected...");
                message = new byte[512];
                TCPClient.GetStream().BeginRead(message, 0, message.Length, ReadMessageFromTCPClientStream, TCPClient);

            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void ReadMessageFromTCPClientStream(IAsyncResult iar)
        {
            TcpClient TCP;
            int nCountReadBytes = 0;
            string strRecv;

            try
            {
                TCP = (TcpClient)iar.AsyncState;
                nCountReadBytes = TCP.GetStream().EndRead(iar);

                if (nCountReadBytes == 0)
                {
                    MessageBox.Show("Client disconnected.","Thông báo!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                strRecv = Encoding.UTF8.GetString(message, 0, nCountReadBytes);
                ShowMessage(strRecv);
                message = new byte[512];
                TCP.GetStream().BeginRead(message, 0, message.Length, ReadMessageFromTCPClientStream, TCP);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void ShowMessage(string _strPrint)
        {
            Invoke(new Action<string>(doInvoke), _strPrint);
        }

        public void doInvoke(string _strPrint)
        {
            txt_result.Text = _strPrint + Environment.NewLine + txt_result.Text;
        }

        private void btn_Send_Click(object sender, EventArgs e)
        {
            
            if (string.IsNullOrEmpty(txt_message.Text))
            {
                MessageBox.Show("Vui lòng nhập nội dung tin nhắn!", "Thông báo!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            };

            try
            {
                if (TCPClient != null)
                {
                    if (TCPClient.Client.Connected)
                    {
                        byte[] message = Encoding.UTF8.GetBytes("Server: " + txt_message.Text);
                        TCPClient.GetStream().BeginWrite(message, 0, message.Length, WriteMessageToClientStream, TCPClient);
                    }
                }
                else
                {
                    MessageBox.Show("Client chưa kết nối Server!", "Thông báo!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Thông báo!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void WriteMessageToClientStream(IAsyncResult iar)
        {
            try
            {
                TcpClient TCP = (TcpClient)iar.AsyncState;
                TCP.GetStream().EndWrite(iar);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Thông báo!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btn_Stop_Click(object sender, EventArgs e)
        {
            
        }

        private void txt_IP_TextChanged(object sender, EventArgs e)
        {

        }

        private void txt_Port_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
