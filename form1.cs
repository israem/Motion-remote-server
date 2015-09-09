using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Motion_remote_UI
{
    public partial class Form1 : Form
    {
        UdpServer sts;
        public Form1()
        {
            sts = new UdpServer();
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {   
            comboBox1.DataSource = sts.ipAddresses;
            label1.Text = "The speed is - " + sts.Speed;
            trackBar1.Value = sts.Speed;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!sts.startListening)
            {
                sts.startListening = true;
                button1.Text = "Stop Listening";
                ovalShape1.FillColor = Color.Green;
                sts.UdpThreadConstructor();   
            }
            else
            {
                sts.startListening = false;
                button1.Text = "Start Listening";
                ovalShape1.FillColor = Color.Red;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            sts.ipaddressIndex = comboBox1.SelectedIndex;
        }

        private void label4_Click(object sender, EventArgs e)
        {
            
            
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            sts.Speed = trackBar1.Value;
            label1.Text = "The speed is - " + sts.Speed;
            sts.c.setSpeed(trackBar1.Value);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            sts.Speed = trackBar1.Value;
            label1.Text = "The speed is - " + sts.Speed;
        }
    } 
}
