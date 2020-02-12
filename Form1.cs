using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace LanChat
{
    public partial class Form1 : Form
    {
        Socket socket;
        EndPoint EPLocal, EPRemote;

        public Form1()
        {
            InitializeComponent();

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            LocalIPEdit.Text = GetLocalIP();
            RecieverIPEdit.Text = GetLocalIP();

        }

        private string GetLocalIP()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }            
            }

            return "127.0.0.1"; //localhost
        }

        private void MessageCallBack(IAsyncResult asyncResult)
        {
            try
            {
                int size = socket.EndReceiveFrom(asyncResult, ref EPRemote);

                if (size > 0)
                {
                    byte[] recievedData = (byte[])asyncResult.AsyncState;

                    ASCIIEncoding encoding = new ASCIIEncoding();
                    string recievedMessage = encoding.GetString(recievedData);

                    listBox1.Items.Add("recieved: " + recievedMessage);
                }

                byte[] buffer = new byte[1500];
                socket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref EPRemote, new AsyncCallback(MessageCallBack), buffer);
                    
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                EPLocal = new IPEndPoint(IPAddress.Parse(LocalIPEdit.Text), Convert.ToInt32(LocalPortEdit.Text));
                socket.Bind(EPLocal);

                EPRemote = new IPEndPoint(IPAddress.Parse(RecieverIPEdit.Text), Convert.ToInt32(RecieverPortEdit.Text));
                socket.Connect(EPRemote);
                // listen to ports, sending msg
                byte[] buffer = new byte[1500];
                socket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref EPRemote, new AsyncCallback(MessageCallBack), buffer);

                button1.Text = "connected.";
                button1.Enabled = false;

                button2.Enabled = true;
                InputBox.Focus();
            }
            catch(Exception ee)
            {
                MessageBox.Show(ee.ToString());
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
                byte[] message = new byte[1500];
                message = encoding.GetBytes(InputBox.Text);

                socket.Send(message);

                listBox1.Items.Add("sent: " + InputBox.Text);
                InputBox.Clear();
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.ToString());
            }
        }
    }
}
