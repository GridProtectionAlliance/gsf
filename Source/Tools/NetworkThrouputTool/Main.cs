using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetworkThrouputTool
{
    public partial class Main : Form
    {
        private TcpListener m_server;
        private delegate void SafeCallDelegate(Result result);

        public Main()
        {
            InitializeComponent();

        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                Random rand = new Random();
                int size = Convert.ToInt32(textBoxPacketSize.Text);

                if (size < 8)
                {
                    MessageBox.Show("Please specify a packetsize > 8B", "Invalid PacketSize", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Byte[] data = new byte[size];
                rand.NextBytes(data);
                Buffer.BlockCopy(BitConverter.GetBytes(size), 0, data, 0, 4);

                int port = Convert.ToInt32(textBoxClientPort.Text);

                Stopwatch stopWatch = new Stopwatch();
                Stopwatch stopWatchEstablish = new Stopwatch();
                string host = textBoxClientHost.Text;

                stopWatch.Reset();
                stopWatchEstablish.Reset();
                stopWatchEstablish.Start();
                
                
                TcpClient client = new TcpClient(host, port);
                stopWatchEstablish.Stop();

                NetworkStream stream = client.GetStream();

                stopWatch.Start();

                stream.Write(data, 0, data.Length);

                stopWatch.Stop();
                stream.Close();
                client.Close();
                

                Result result = new Result()
                {
                    Size = size,
                    Type = "Sent",
                    Time = stopWatch.ElapsedTicks/ 10000.0D,
                    Speed = Convert.ToDouble(size) / (stopWatch.ElapsedTicks / 10000.0D)
                };
                AppendRow(result);
            }
            catch (Exception ex)
            { 
                MessageBox.Show($"Can not connect to Server. Message: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); 
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                IPAddress localAddr = IPAddress.Any;
                int port = Convert.ToInt32(textBoxServerPort.Text);
                m_server = new TcpListener(localAddr, port);
                m_server.Start();
                button1.Enabled = false;
                button2.Enabled = true;

                m_server.BeginAcceptTcpClient(new AsyncCallback(AccepConnection), m_server);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Can not start Server. Message: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            m_server.Stop();
            button1.Enabled = true;
            button2.Enabled = false;
        }

        // Process the client connection.
        public void AccepConnection(IAsyncResult ar)
        {
            try
            {
                // Get the listener that handles the client request.
                TcpListener listener = (TcpListener)ar.AsyncState;

                // End the operation and display the received data on 
                // the console.
                TcpClient client = listener.EndAcceptTcpClient(ar);
                
                NetworkStream stream = client.GetStream();
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Reset();
                

                Byte[] hbuffer = new Byte[4]; // 4 Byte buffer for size
                Byte[] buffer = new Byte[1024]; // 8 Byte buffer for size
                
                stopWatch.Start();
                int actualReadBytes = stream.Read(hbuffer, 0, hbuffer.Length);
                int size = BitConverter.ToInt32(hbuffer, 0);

                

                while (actualReadBytes < size)
                {
                    actualReadBytes = actualReadBytes + stream.Read(buffer, 0, buffer.Length);
                }

                stopWatch.Stop();

                var d = new SafeCallDelegate(AppendRow);

                m_server.BeginAcceptTcpClient(new AsyncCallback(AccepConnection), m_server);

                Result result = new Result() { 
                    Size = size, 
                    Type = "Recieved",
                    Time = stopWatch.ElapsedTicks / 10000.0D,
                    Speed = size / (stopWatch.ElapsedTicks / 10000.0D)
                };
                this.listView1.Invoke(d, result);
            }
            catch (Exception ex)
            {
            }
        }

        private void AppendRow(Result result)
        {
            ListViewItem it = new ListViewItem(result.Type);
            
            it.SubItems.Add(result.Size.ToString());
            it.SubItems.Add(result.Time.ToString());
            it.SubItems.Add(result.Speed.ToString());
            this.listView1.Items.Add(it);
        }

        private class Result
        {
            public int Size;
            public string Type;
            public double Time;
            public double Speed;

        }
    }

}
