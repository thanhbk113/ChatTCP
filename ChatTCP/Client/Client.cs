using System;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace Client
{
    public partial class Client : Form
    {
        TcpClient TCPClient;
        byte[] message;
        public Client()
        {
            InitializeComponent();
        }

        private void btn_Connect_Click(object sender, EventArgs e)
        {
            IPAddress IP;
            int Port;

            try
            {
                if (string.IsNullOrEmpty(txt_IP.Text) || string.IsNullOrEmpty(txt_IP.Text))
                {
                    MessageBox.Show("Vui lòng nhập IP và Port từ 1000 trở lên!", "Thông báo!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (!IPAddress.TryParse(txt_IP.Text, out IP))
                {
                    MessageBox.Show("Vui lòng nhập đúng địa chỉ IP!");
                    return;
                }

                if (!int.TryParse(txt_Port.Text, out Port))
                {
                    MessageBox.Show("Vui lòng nhập Port từ 1000 trở lên!", "Thông báo!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                TCPClient = new TcpClient();
                var result = TCPClient.BeginConnect(IP, Port, Connect, TCPClient);
                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));
                if (success)
                {
                    btn_Connect.Enabled = false;
                    txt_message.Enabled = true;
                    btn_Send.Enabled = true;
                    txt_IP.Enabled = false;
                    txt_Port.Enabled = false;
                    ShowMessage("Connected Server!");
                }
                else
                {
                    MessageBox.Show("Vui lòng Start Server", "Thông báo!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                    
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Thông báo!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        void Connect(IAsyncResult iar)
        {
            TcpClient TCPClient;

            try
            {
                TCPClient = (TcpClient)iar.AsyncState;
                TCPClient.EndConnect(iar);
                message = new byte[512];
                TCPClient.GetStream().BeginRead(message, 0, message.Length, ReadMessageFromServerStream, TCPClient);

            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Thông báo!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        void ReadMessageFromServerStream(IAsyncResult iar)
        {
            TcpClient TCPClient;
            int nCountBytesReceivedFromServer;
            string strReceived;

            try
            {
                TCPClient = (TcpClient)iar.AsyncState;
                nCountBytesReceivedFromServer = TCPClient.GetStream().EndRead(iar);

                if (nCountBytesReceivedFromServer == 0)
                {
                    MessageBox.Show("Connection broken.", "Thông báo!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                strReceived = Encoding.UTF8.GetString(message, 0, nCountBytesReceivedFromServer);
                ShowMessage(strReceived);
                message = new byte[512];
                TCPClient.GetStream().BeginRead(message, 0, message.Length, ReadMessageFromServerStream, TCPClient);

            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Thông báo!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void ShowMessage(string message)
        {
            txt_result.Invoke(new Action<string>(doInvoke), message);
        }

        public void doInvoke(string message)
        {
            txt_result.Text = message + Environment.NewLine + txt_result.Text;
        }


        private void bnt_Send_Click(object sender, EventArgs e)
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
                        byte[] message = Encoding.UTF8.GetBytes("Client: " + txt_message.Text);
                        TCPClient.GetStream().BeginWrite(message, 0, message.Length, WriteMessageToServer, TCPClient);
                    }
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Thông báo!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        void WriteMessageToServer(IAsyncResult iar)
        {
            TcpClient TCPClient;
            try
            {
                TCPClient = (TcpClient)iar.AsyncState;
                TCPClient.GetStream().EndWrite(iar);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Thông báo!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
