using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
        private TcpClient client;
        private int packetSize;
        private System.Threading.Timer timer;
        private delegate void SafeCallDelegate(Result result);

        public Main()
        {
            InitializeComponent();

        }

        private void button4_Click(object sender, EventArgs e)
        {
            startSending();
        }


        private void SendData(object state)
        {
            try
            {
                Random rand = new Random();

                byte[] data = new byte[packetSize];
                rand.NextBytes(data);
                Buffer.BlockCopy(BitConverter.GetBytes(packetSize), 0, data, 0, 4);

                Stopwatch stopWatch = new Stopwatch();

                stopWatch.Reset();

                stopWatch.Start();

                NetworkStream stream = client.GetStream();
                stream.Write(data, 0, data.Length);

                stopWatch.Stop();



                Result result = new Result()
                {
                    Size = packetSize,
                    Type = "Sent",
                    Time = stopWatch.ElapsedTicks / 10000.0D,
                    Speed = Convert.ToDouble(packetSize) / (stopWatch.ElapsedTicks / 10000.0D)
                };

                var d = new SafeCallDelegate(AppendRow);
                this.listView1.Invoke(d, result);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Can not send package to Server. Message: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                

                
                

                while (client.Connected)
                {
                    int actualReadBytes = stream.Read(hbuffer, 0, hbuffer.Length);
                    int size = BitConverter.ToInt32(hbuffer, 0);
                    

                    stopWatch.Start();

                    while (actualReadBytes < size)
                    {
                        actualReadBytes = actualReadBytes + stream.Read(buffer, 0, Math.Min(buffer.Length,size - actualReadBytes));
                    }
                    stopWatch.Stop();

                    var d = new SafeCallDelegate(AppendRow);

                    m_server.BeginAcceptTcpClient(new AsyncCallback(AccepConnection), m_server);

                    Result result = new Result()
                    {
                        Size = size,
                        Type = "Recieved",
                        Time = stopWatch.ElapsedTicks / 10000.0D,
                        Speed = size / (stopWatch.ElapsedTicks / 10000.0D)
                    };
                    this.listView1.Invoke(d, result);

                    stopWatch.Reset();
                }
            }
            catch
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

        private void button3_Click(object sender, EventArgs e)
        {
            this.listView1.Clear();
        }

        private void export2File()
        {
            string filename = "";
            SaveFileDialog sfd = new SaveFileDialog();

            sfd.Title = "Export Results";
            sfd.Filter = "Text File (.csv) | *.csv";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                filename = sfd.FileName.ToString();
                if (filename != "")
                {
                    using (StreamWriter sw = new StreamWriter(filename))
                    {
                        sw.WriteLine("{0},{1},{2},{3}", "Type", "Size (B)","Time (ms)","Speed (KB/s)");

                        foreach (ListViewItem item in this.listView1.Items)
                        {
                            sw.WriteLine("{0},{1},{2},{3}", item.SubItems[0].Text, item.SubItems[1].Text, item.SubItems[2].Text, item.SubItems[3].Text);
                        }
                    }
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            export2File();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            stopSending();
        }


        private void stopSending()
        {
            try
            {
                timer.Change(Timeout.Infinite, Timeout.Infinite);
                NetworkStream stream = client.GetStream();
                stream.Close();
                client.Close();
            }
            finally
            {
                button4.Enabled = true;
                button6.Enabled = false;
                textBoxPacketSize.Enabled = true;
                textBoxClientHost.Enabled = true;
                textBoxClientPort.Enabled = true;
            }
            
        }

        private void startSending()
        {
            try
            {
                Random rand = new Random();
                packetSize = Convert.ToInt32(textBoxPacketSize.Text);

                if (packetSize < 8)
                {
                    MessageBox.Show("Please specify a packetsize > 8B", "Invalid PacketSize", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int port = Convert.ToInt32(textBoxClientPort.Text);
                string host = textBoxClientHost.Text;

                client = new TcpClient(host, port);

                timer = new System.Threading.Timer(SendData, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

                button6.Enabled = true;
                button4.Enabled = false;
                textBoxPacketSize.Enabled = false;
                textBoxClientHost.Enabled = false;
                textBoxClientPort.Enabled = false;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Can not connect to Server. Message: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

}
