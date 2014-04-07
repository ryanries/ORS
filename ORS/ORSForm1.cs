// ORS - Office Rageface Sender
// By Ryan Ries, 2012

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using Microsoft.Win32;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ORS
{
    public partial class ORSForm1 : Form
    {
        static string productVersion = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
        public static string yourName = null;
        System.Windows.Forms.Timer _countdownTimer = new System.Windows.Forms.Timer();
        static uint timerTicks = 0;
        static bool netDiscoveryFinished = true;
        static bool broadcastListenerRunning = false;        
        static string[] allowedExtensions = { "png","jpg","jpeg","gif","bmp"};
        static string[] filesInImagesDir;
        static bool needToRequestFile = false;
        static string fileToRequest = null;
        static string ipToRequestFrom = null;
        static int udpPort = 9067;
        static int tcpPort = 9068;
        static TcpListener tcpListenerSocket;
        static string localIP = null;
        static List<string> peers = new List<string>();

        public ORSForm1()
        {
            InitializeComponent();
        }

        private void ORSForm1_Load(object sender, EventArgs e)
        {
            localIP = LocalIPAddress();
            this.Text += " v " + productVersion;
            this.Icon = Properties.Resources.problem;
            notifyIcon1.Icon = Properties.Resources.smallproblem;
            notifyIcon1.Text = "ORS v " + productVersion;
            try
            {
                RegistryKey orsNameRegKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Office Rageface Sender", false);
                yourName = orsNameRegKey.GetValue("Name").ToString();
                orsNameRegKey.Close();
                yourName = yourName.Replace("|", "");
            }
            catch
            {
                EnterNameForm enterNameForm = new EnterNameForm();
                if (enterNameForm.ShowDialog(this) == DialogResult.OK)
                {
                    // Wait for the pop up to complete
                }
                enterNameForm.Dispose();
            }
            if (yourName.Length < 1)
            {
                MessageBox.Show("Unable to get your name from either the registry or the popup box. How did you already mess the application up so badly?", "Y U NO HAVE NAME??", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
            if (yourName.Length > 64)
            {
                MessageBox.Show("Your name can't be longer than 64 characters, you registry-hacking scrub!", "Y U TRY HACK??", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
            toolStripStatusLabel1.Text = "Nickname: " + yourName + ". (Click to change.)";
            if (!Directory.Exists("Images"))
            {
                MessageBox.Show("A directory named 'Images' was just created where this executable is running from. Dump all your favorite images in this directory. New images that others send to you will automatically be dumped here. If you move this executable, be sure to move the images directory too.", "Y U NO HAVE IMAGES??", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Directory.CreateDirectory("Images");
            }
            else
            {
                UpdateImagesListAndContextMenu();          
            }
            
            tcpListenerSocket = new TcpListener(IPAddress.Parse(localIP), tcpPort);              
            BackgroundWorker tcpListenerBackgroundWorker = new BackgroundWorker();
            tcpListenerBackgroundWorker.WorkerReportsProgress = false;
            tcpListenerBackgroundWorker.DoWork += new DoWorkEventHandler(tcpListenerBackgroundWorker_DoWork);
            tcpListenerBackgroundWorker.RunWorkerAsync();            

            _countdownTimer.Interval = 1000; //ms
            _countdownTimer.Tick += new EventHandler(_countdownTimer_Tick);
            _countdownTimer.Enabled = true;
        }

        void UpdateImagesListAndContextMenu()
        {
            sendImgRightClickContextMenu.Items.Clear();
            sendImgRightClickContextMenu.Items.Insert(0, new ToolStripLabel("Images"));
            sendImgRightClickContextMenu.Items.Insert(1, new ToolStripSeparator());
            filesInImagesDir = Directory.GetFiles("Images");
            foreach (string file in filesInImagesDir)
            {
                string[] splitFN = file.Split('.');
                string fileExt = splitFN[splitFN.Count() - 1];
                foreach (string ext in allowedExtensions)
                {
                    if (string.Compare(ext, fileExt, true) == 0)
                    {
                        sendImgRightClickContextMenu.Items.Add(file, null, SendImgContextMenu_Click);                        
                    }
                }
            } 
        }

        void SendImgContextMenu_Click(object sender, EventArgs e)
        {
            try
            {
                string fileToSend = sender.ToString();
                ListViewItem lvi = mainListView.SelectedItems[0];
                string destinationIP = lvi.SubItems[1].Text;
                TcpClient client = new TcpClient();
                IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(destinationIP), tcpPort);
                client.Connect(serverEndPoint);
                NetworkStream clientStream = client.GetStream();
                ASCIIEncoding encoder = new ASCIIEncoding();
                byte[] buffer = encoder.GetBytes("ORS_TCP|ShowRageFace|" + localIP + "|" + fileToSend);
                clientStream.Write(buffer, 0, buffer.Length);
                clientStream.Flush();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not send rageface: " + ex.Message);
            }
        }

        public string LocalIPAddress()
        {
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    localIP = ip.ToString();
                }
            }
            return localIP;
        }

        void _countdownTimer_Tick(object sender, EventArgs e)
        {
            if (needToRequestFile)
            {
                needToRequestFile = false;
                BackgroundWorker RequestFileBGWorker = new BackgroundWorker();
                RequestFileBGWorker.WorkerReportsProgress = false;
                RequestFileBGWorker.DoWork +=new DoWorkEventHandler(RequestFileBGWorker_DoWork);
                RequestFileBGWorker.RunWorkerAsync();                
            }
            if (timerTicks % 2 == 0 && !broadcastListenerRunning)
            {
                broadcastListenerRunning = true;
                BackgroundWorker packetListener = new BackgroundWorker();
                packetListener.WorkerReportsProgress = false;
                packetListener.DoWork += new DoWorkEventHandler(packetListener_DoWork);
                packetListener.RunWorkerAsync();
            }
            if (timerTicks % 7 == 0 && netDiscoveryFinished)
            {
                netDiscoveryFinished = false;
                BackgroundWorker netDiscoveryBGWorker = new BackgroundWorker();
                netDiscoveryBGWorker.WorkerReportsProgress = false;
                netDiscoveryBGWorker.DoWork +=new DoWorkEventHandler(netDiscoveryBGWorker_DoWork);
                netDiscoveryBGWorker.RunWorkerAsync();
            }
            if (timerTicks % 3 == 0)
            {
                UpdateImagesListAndContextMenu();
            }
            if (timerTicks % 5 == 0)
            {
                UpdatePeersView();
            }
            timerTicks++;
        }

        void RequestFileBGWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            TcpClient client = new TcpClient();
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(ipToRequestFrom), tcpPort);
            client.Connect(serverEndPoint);
            NetworkStream clientStream = client.GetStream();
            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] buffer = encoder.GetBytes("ORS_TCP|SendMe|" + localIP + "|" + fileToRequest);
            clientStream.Write(buffer, 0, buffer.Length);
            clientStream.Flush();
            ipToRequestFrom = null;
            fileToRequest = null;
        }

        void tcpListenerBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            tcpListenerSocket.Start();
            while (true)
            {
                //blocks until a client has connected to the server
                TcpClient client = tcpListenerSocket.AcceptTcpClient();
                //create a thread to handle communication with connected client
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                clientThread.IsBackground = true;
                clientThread.Start(client);
            }
        }

        private void HandleClientComm(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();
            byte[] message = new byte[1024 * 2000];
            int bytesRead;
            while (true)
            {
                bytesRead = 0;
                Array.Clear(message, 0, 1024 * 2000);
                try { bytesRead = clientStream.Read(message, 0, 1024 * 2000); } //blocks until a client sends a message
                catch { break; }

                if (bytesRead == 0)
                    break;
                
                //message has successfully been received
                ASCIIEncoding encoder = new ASCIIEncoding();

                if (encoder.GetString(message, 0, bytesRead).StartsWith("ORS_TCP|FileIncoming|"))
                {
                    string[] splitTCPCmd = encoder.GetString(message, 0, 1024).Split('|');
                    string fileToSave = splitTCPCmd[2];
                    int offset = 22 + fileToSave.Length;
                    FileStream fs = new FileStream(fileToSave, FileMode.Create, FileAccess.Write);
                    fs.Write(message, offset, bytesRead);
                    fs.Close();
                    Thread.Sleep(5000);
                    PictureViewer PV = new PictureViewer(fileToSave);
                    PV.ShowDialog();
                }

                if (encoder.GetString(message, 0, bytesRead).StartsWith("ORS_TCP|ShowRageFace|"))
                {
                    //The packet received looks like this: ORS_TCP|ShowRageFace|192.168.1.9|Images\yuno.png
                    string[] splitTCPCmd = encoder.GetString(message, 0, bytesRead).Split('|');
                    string senderIP = splitTCPCmd[2];
                    string fileName = splitTCPCmd[3];

                    if (!File.Exists(fileName))  //senderName + " wants to show you " + fileName + ", but you don't have it!"
                    {
                        //MessageBox.Show(senderIP + " wants to show you " + fileName + ", but you don't have it. Requesting download...");
                        needToRequestFile = true;
                        fileToRequest = fileName;
                        ipToRequestFrom = senderIP;
                    }
                    else
                    {
                        PictureViewer PV = new PictureViewer(fileName);
                        PV.ShowDialog();
                    }
                }
                else if (encoder.GetString(message, 0, bytesRead).StartsWith("ORS_TCP|SendMe|"))
                {
                    string[] splitTCPCmd = encoder.GetString(message, 0, bytesRead).Split('|');
                    string ipToSendTo = splitTCPCmd[2];
                    string fileRequested = splitTCPCmd[3];

                    TcpClient tcpSender = new TcpClient();
                    IPEndPoint sendFileEndPoint = new IPEndPoint(IPAddress.Parse(ipToSendTo),tcpPort);
                    tcpSender.Connect(sendFileEndPoint);
                    NetworkStream senderStream = tcpSender.GetStream();

                    //MessageBox.Show(ipToSendTo + " is requesting " + fileRequested);                    
                    byte[] fileData = File.ReadAllBytes(fileRequested);
                    if (fileData.Length > 1024 * 1999) // Somewhere around 2mb
                    {
                        MessageBox.Show(fileRequested + " is " + fileData.Length + " bytes, which is too large. Please use smaller files.", "Y U NO SEND SMALL FILES??", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    }

                    List<byte> dataToSend = new List<byte>();
                    foreach (byte b in encoder.GetBytes("ORS_TCP|FileIncoming|" + fileRequested + "|"))                    
                        dataToSend.Add(b);

                    foreach (byte b in fileData)
                        dataToSend.Add(b);

                    byte[] finalPacket = new byte[dataToSend.Count];
                    for (int i = 0; i < dataToSend.Count; i++)
                        finalPacket[i] = dataToSend[i];
                    
                    senderStream.Write(finalPacket, 0, finalPacket.Length);
                    senderStream.Flush();
                }
            }
            tcpClient.Close();
        }        

        void UpdatePeersView()
        {
            int selectedIndex = -1;
            try
            {                
                if (mainListView.SelectedIndices.Count > 0)
                    selectedIndex = mainListView.SelectedItems[0].Index;

                mainListView.Items.Clear();
                foreach (string peer in peers)
                {
                    string[] splitPeer = peer.Split('|');
                    string peerIP = splitPeer[0];
                    string peerName = splitPeer[1];
                    string lastSeen = splitPeer[2];
                    ListViewItem item = new ListViewItem();
                    item.Text = peerName;
                    item.SubItems.Add(peerIP);
                    item.SubItems.Add(lastSeen);
                    mainListView.Items.Add(item);
                }
            }
            catch { }
            if (selectedIndex > -1)
                try { mainListView.Items[selectedIndex].Selected = true; }
                catch { }
        }

        void packetListener_DoWork(object sender, DoWorkEventArgs e)
        {
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint iep = new IPEndPoint(IPAddress.Any, udpPort);
            sock.Bind(iep);
            EndPoint ep = (EndPoint)iep;
            byte[] data = new byte[1024];
            int recv = sock.ReceiveFrom(data, ref ep);
            string stringData = Encoding.ASCII.GetString(data, 0, recv);            
            sock.Close();            
            if (stringData.StartsWith("ORS.1.3.3.7"))
            {
                string[] splitPacket = stringData.Split('|');
                string IPInPacket = splitPacket[1];
                string nameInPacket = splitPacket[2];                
                bool duplicate = false;
                bool tooOld = false;
                string dupeString = null;
                string tooOldString = null;
                foreach (string peer in peers)
                {
                    if (peer.StartsWith(IPInPacket + "|" + nameInPacket))
                    {
                        duplicate = true;
                        dupeString = peer;
                    }
                }
                if (duplicate)                
                    peers.Remove(dupeString);
                foreach (string peer in peers)
                {
                    string[] splitLine = peer.Split('|');
                    DateTime date = Convert.ToDateTime(splitLine[2]);
                    if (date < DateTime.Now.AddMinutes(-4))
                    {
                        tooOld = true;
                        tooOldString = peer;
                    }
                }
                if (tooOld)
                    peers.Remove(tooOldString);

                peers.Add(IPInPacket + "|" + nameInPacket + "|" + DateTime.Now);
                peers.Sort();
            }
            broadcastListenerRunning = false;
        }

        void netDiscoveryBGWorker_DoWork(object sender, DoWorkEventArgs e)
        {            
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            IPEndPoint iep = new IPEndPoint(IPAddress.Broadcast, udpPort);
            byte[] data = Encoding.ASCII.GetBytes("ORS.1.3.3.7|" + localIP + "|" + yourName);
            sock.SendTo(data, iep);
            sock.Close();
            netDiscoveryFinished = true;
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
                notifyIcon1.Visible = false;
            }
        }

        void ORSForm1_Resize(object sender, System.EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                notifyIcon1.Visible = true;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {
            EnterNameForm enterNameForm = new EnterNameForm();
            if (enterNameForm.ShowDialog(this) == DialogResult.OK)
            {
                // Wait for the pop up to complete
            }
            enterNameForm.Dispose();
            if (yourName.Length < 1)
            {
                MessageBox.Show("Unable to get your name from either the registry or the popup box. How did you already mess the application up so badly?", "Y U NO HAVE NAME??", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
            if (yourName.Length > 64)
            {
                MessageBox.Show("Your name can't be longer than 128 characters, you registry-hacking scrub!", "Y U TRY HACK??", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
            toolStripStatusLabel1.Text = "Nickname: " + yourName + ". (Click to change.)";
        }

        void sendImgRightClickContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = (mainListView.SelectedIndices.Count < 1);
        }
    }
}
