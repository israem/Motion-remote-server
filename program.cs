using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.ServiceProcess;
using System.Diagnostics;

namespace Motion_remote_UI
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }

    public class UdpServer 
    {
        private const int udpPort = 5555;
        public List<IPAddress> ipAddresses;
        public bool startListening = false;
        public String dataReceived;
        public int ipaddressIndex = 0;
        public int Speed = 5;
        public crusor c = new crusor();
        Thread UdpThread;
   
        public UdpServer()
        {
            try
            {
                //!!!TODO exlude ipv6 from list
                ipAddresses = Dns.GetHostAddresses(Dns.GetHostName()).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }
        public void UdpThreadConstructor()
        {
            UdpThread = new Thread(new ThreadStart(StartReceiveFrom2));
            UdpThread.Priority = ThreadPriority.Highest;
            UdpThread.Start();
        }

        public void StartReceiveFrom2()
        {
            byte[] r = new byte[64];
            EndPoint remoteEP;
            try
            {
                Socket soUdp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                IPEndPoint localIpEndPoint = new IPEndPoint(ipAddresses[ipaddressIndex], udpPort);
                soUdp.Bind(localIpEndPoint);
                while (startListening)
                {
                    {          
                        IPEndPoint tmpIpEndPoint = new IPEndPoint(ipAddresses[ipaddressIndex], udpPort);
                        remoteEP = (tmpIpEndPoint);
                        soUdp.Receive(r);
                        dataReceived = System.Text.Encoding.ASCII.GetString(r);
                        r = new byte[64];
                        MassageCenter(dataReceived);
                    }
                }
                
            }
            catch (SocketException se)
            {
                Console.WriteLine("A Socket Exception has occurred!" + se.ToString());
            }
        }

        public void MassageCenter(String dataReceived)
        {
            String[] header = dataReceived.Split('.');
            switch (header[0])
            {
                case "motion":
                    {
                        c.MoveCursor(mouseDataAnalization(header[1]));
                        break;
                    }
                case "info":
                    {
                        Console.WriteLine(header[1]);
                        break;
                    }
                case "settings":
                    {
                        Speed = int.Parse(header[1]);
                        c.setSpeed(int.Parse(header[1]));
                        break;
                    }
                case "keys":
                    {
                        c.mouseKeyLongPress(keysDataAnalization(header[1]));
                        break;
                    }
            }
            dataReceived = null;
        }

        public int keysDataAnalization(String s)
        {
            String[] s1 = s.Split(',');
            int keys = int.Parse(s1[0]);
            return keys;
        }

        public int[] mouseDataAnalization(String s)
        {
            String[] s1 = s.Split(',');

            int[] gravity = new int[s1.Length - 1];

            for (int i = 1; i < gravity.Length; i++)
            {
                gravity[i - 1] = int.Parse(s1[i]);
            }       

            return gravity;
        }
    }

    public class crusor :System.Windows.Forms.Control
    {
        private const int MOUSEEVENTF_LEFTDOWN = 0x0002; /* left button down */
        private const int MOUSEEVENTF_LEFTUP = 0x0004; /* left button up */
        private const int MOUSEEVENTF_RIGHTDOWN = 0x0008; /* right button down */
        private const int MOUSEEVENTF_RIGHTUP = 0x0010;
        const int MOUSEEVENTF_MOVE = 0x0001;
        const int MOUSEEVENTF_XDOWN = 0x0080;
        const int MOUSEEVENTF_XUP = 0x0100;
        const int MOUSEEVENTF_WHEEL = 0x0800;
        const int MOUSEEVENTF_HWHEEL = 0x01000;
        

        int counter = 0;
        int speed = 5;
        int width = Screen.PrimaryScreen.Bounds.Width;
        int height = Screen.PrimaryScreen.Bounds.Height;

        [DllImport("user32")]
        public static extern int SetCursorPos(int x, int y);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        public crusor()
        {
        }
        public void setSpeed(int i)
        {
            speed = i;
        }
        public int getSpeed()
        {
            return speed;
        }
        public int getCounter()
        {
            return counter;
        }
        public void setCounter(int i)
        {
            counter = i;
        }

        public void MoveCursor(int[] gravity)
        {            
            convertNumbers(gravity);
            Point p = new Point(Cursor.Position.X - gravity[0],Cursor.Position.Y - gravity[1]);
            mouse_event(MOUSEEVENTF_MOVE, -gravity[0], -gravity[1], 0, 0);
        }

        public void mouseKeyLongPress(int keys)
        {
            Point p = new Point(Cursor.Position.X,Cursor.Position.Y);
            
            if (keys == 1)
            {
                mouse_event(MOUSEEVENTF_LEFTDOWN, p.X, p.Y, 0, 0);
                Thread.Sleep(100);
                mouse_event(MOUSEEVENTF_LEFTUP, p.X, p.Y, 0, 0);
            }
            else if(keys == 2)
            {
                mouse_event(MOUSEEVENTF_RIGHTDOWN, p.X, p.Y, 0, 0);
                Thread.Sleep(100);
                mouse_event(MOUSEEVENTF_RIGHTUP, p.X, p.Y, 0, 0);
            }
        }

        public void convertNumbers(int[] i)
        {
            int tmpy = 0, tmpx = 0;
            tmpx = i[0];
            tmpy = i[1];
            //!!!TODO find the right function
            i[0] = ((tmpx / speed));
            i[1] = ((tmpy / speed));
        }

    }
}
