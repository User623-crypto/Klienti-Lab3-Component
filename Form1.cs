using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Klienti1
{
    public partial class Form1 : Form
    {
        System.Net.Sockets.TcpClient clientSocket = new System.Net.Sockets.TcpClient();
        bool didConnect = false;
        private const int port = 8808;
        bool gati = false;
        public delegate void InvokeDelegate(string s); // Duhet për të aksesuar elementët e gui-it në një thread tjetër. Sipas dokumentacionit zyrtar.
        TcpClient client;
        NetworkStream stream;
        string downloadPath = string.Empty;
        string dosjaESelektuar = string.Empty;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (didConnect)
            {
                return;
            }
             client = new TcpClient();
            try
            {
                client.Connect("localhost", port);
                stream = client.GetStream();
                write(stream, Encoding.UTF8.GetBytes("më jep listën"));
                didConnect = true;
                new Thread(()=>
                {
                    while (true)
                    {
                        if (stream.DataAvailable)
                        {
                            byte[] data = ReadToEnd(stream);
                            if (!gati)
                            {
                                String message = Encoding.UTF8.GetString(data);
                                label1.BeginInvoke(new InvokeDelegate(InvokeMethod), "U dergua nga serveri");
                                comboBox1.BeginInvoke(new InvokeDelegate(InvokeMethod2),message);

                            }
                            else
                            {
                                //To do Download the file into the specified location
                               // string extension = dosjaESelektuar.Split('.')[1];
                               // string filename = System.IO.Path.GetTempFileName() + "." + extension; // Makes something like "C:\Temp\blah.tmp.pdf"
                                string filename = downloadPath + "\\" + dosjaESelektuar;
                                Console.WriteLine(filename);
                                File.WriteAllBytes(filename, data);
                                label1.BeginInvoke(new InvokeDelegate(InvokeMethod), "U dergua nga serveri. Shkarkimi me sukses Shkarko prap");

                                
                            }
                            
                            
                            
                            gati = true;
                            
                        }
                        else Thread.Sleep(1);
                    }
                }).Start();
                button1.Hide();
                button2.Show();
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
                MessageBox.Show("Nuk lidhet dot me Serverin. Provo më vonë");
            }
            
            /*serverStream = clientSocket.GetStream();
            byte[] outStream = System.Text.Encoding.ASCII.GetBytes("Message from Client$");
            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();

            byte[] inStream = new byte[10025];
            if (serverStream.CanRead)
            {
                serverStream.Read(inStream, 0, inStream.Length);
                string returndata = System.Text.Encoding.ASCII.GetString(inStream);
                msg("Data from Server : " + returndata);
            }
            else
            {
                label1.Text="Serveri u shkeput";
                serverStream.Close();
                clientSocket.Close();
            }*/
           
        }


        private byte[] ReadToEnd(NetworkStream stream)
        {
            List<Byte> recivedBytes = new List<byte>();

            while (stream.DataAvailable)
            {

                byte[] buffer = new byte[1024];
                stream.Read(buffer, 0, buffer.Length);
                recivedBytes.AddRange(buffer);//Shtohet ne fund te listes
            }

            recivedBytes.RemoveAll(b => b == 0); //Njëri nga mesazhet nuk do te përmbushi dot 1024 => pjesa e pa përmbushur do të hiqet

            return recivedBytes.ToArray();
        }

        public void write(NetworkStream stream, byte[] data)
        {
            stream.Write(data, 0, data.Length);
        }
        public void InvokeMethod(String message)
        {
            label1.Text = message;
            if (message.Length >= 25)
            {
                button2.Show();
            }
        }
        public void InvokeMethod2(string message) {
            Console.WriteLine(message);
            string[] lista = message.Split(' ');
            for (int i = 0; i < lista.Length-1; i++)
            {
                comboBox1.Items.Insert(i, lista[i]);
            }
            comboBox1.SelectedIndex=0;
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button2.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            { 
                this.downloadPath = fbd.SelectedPath.ToString();
                this.dosjaESelektuar = comboBox1.Text;
                Console.WriteLine(dosjaESelektuar);
                write(stream, Encoding.UTF8.GetBytes(dosjaESelektuar));
                button2.Hide();
                
            }
        }

        /*private void Form1_Load(object sender, EventArgs e)
        {
            msg("Client Started");
            try
            {
                clientSocket.Connect("127.0.0.1", port);
                label1.Text = "Client Socket Program - Server Connected ...";
            }
            catch (Exception)
            {

                label1.Text = "Client Socket Program - Server cant connect ...";
            }
           
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            clientSocket.Close();
            serverStream.Close();

        }*/
    }
}
