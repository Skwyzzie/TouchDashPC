using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace TouchDashPC
{
    public partial class Form1 : Form
    {
        public string _IP_ADDR { get; set; }
        public bool _LOG { get; set; }
        public int _PORT { get; set; }
        public string _STATUS { get; set; }
        public String IP_ADDR
        {
            get { return _IP_ADDR; }
            set
            {
                _IP_ADDR = value;
                IPLabel.Text = _IP_ADDR;
                if (_IP_ADDR.Equals("No IP."))
                {
                    IPLabel.ForeColor = Color.Red;
                }
                else
                {
                    IPLabel.ForeColor = Color.Black;
                }
            }
        }
        public bool LOG
        {
            get { return _LOG; }
            set
            {
                _LOG = value;
                if (_LOG)
                {
                    log("***LOGGING STARTED***");
                    updateSettings("properties", "log", _LOG);
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    string str = DateTime.Now.ToString("hh.mm.ss.fff");
                    sb.Append(textBox1.Text);
                    sb.Append("[" + str + "] > ***LOGGING STOPPED***\r\n");
                    textBox1.Text = sb.ToString();
                    SETTINGS.Set("properties", "log", _LOG);
                }
                textBox1.Enabled = _LOG;
            }
        }
        public int PORT
        {
            get { return _PORT; }
            set
            {
                _PORT = value;
                PortLabel.Text = _PORT.ToString();
            }
        }
        public String STATUS
        {
            get { return _STATUS; }
            set
            {
                _STATUS = value;
                StatusLabel.Text = _STATUS;
                if (_STATUS.Equals("Stopped"))
                {
                    StatusLabel.ForeColor = Color.Red;
                }
                else
                {
                    StatusLabel.ForeColor = Color.LimeGreen;
                }
            }
        }
        public IniFile SETTINGS;
        public Server server;
        public KeyEngine KeyEngine;
        public MemoryReaderEngine MREngine;
        public MemoryObject cruise;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SETTINGS = new IniFile("Settings.ini");
            LOG = SETTINGS.GetBool("properties", "log");

            loadKeybinds();
            getLocalIPAddress();
            getPortNumber();
            loadServer();
            loadKeyEngine();

            checkBox1.Checked = SETTINGS.GetBool("properties", "startsrv");
            if (checkBox1.Checked)
                startServerBtn_Click(null, null);
        }

        public void loadKeybinds()
        {
            log("Loading keybinds...");

            IniFile.IniSection keybinds = SETTINGS["keybinds"];
            IniFile.IniSection.IniKeyValuePair[] commands = keybinds.KeyValuePairs.ToArray();
            for (int i = 0; i < commands.Length; i++)
            {
                String command = commands[i].Key;
                String[] val = { commands[i].Value };
                val = val[0].Split(';');
                String description = val[0];
                String keybind = val[1];
                dataGridView1.Rows.Add(command, description, keybind);
            }
            foreach (IniFile.IniSection.IniKeyValuePair keyValue in keybinds.KeyValuePairs)
            {
                String[] val = keyValue.Value.Split(';');
                log(keyValue.Key + "=" + val[1]);
            }

            this.dataGridView1.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.keybindChanged);
            log("Done loading keybinds.");
        }

        public void getLocalIPAddress()
        {
            log("Getting local IP address...");
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    log("Got IP address.");
                    IP_ADDR = ip.ToString();
                    log(IP_ADDR);
                    return;
                }
            }
            log("Local IP Address Not Found!");
            IP_ADDR = "NO IP.";
            log(IP_ADDR);
        }

        public void getPortNumber()
        {
            log("Getting port...");
            PORT = SETTINGS.GetInt("properties", "port");
            log("Got port.");
            log(PORT.ToString());
        }

        public void loadServer()
        {
            log("Loading server module...");
            server = new Server(IP_ADDR, PORT);
            server.MessageRecieved += new MessageEventHandler(onMessageRecieved);
            log("Server loaded.");
        }

        public void loadKeyEngine()
        {
            log("Loading KeyEngine module...");
            String appname = SETTINGS.Get("properties", "appname");
            KeyEngine = new KeyEngine(appname);
            log("KeyEngine module loaded.");
        }

        public void loadMemoryEngine()
        {
            log("Loading memory reader engine...");
            MREngine = new MemoryReaderEngine(SETTINGS.Get("properties", "appname"));
            cruise = new MemoryObject(Int32.Parse(SETTINGS.Get("properties","cruiseAddr")), MREngine);
            log("Memory reader engine loaded.");
        }

        private void onMessageRecieved(String message, EventArgs e)
        {
            /**
             * Examples of message:
             * 
             * {command:'iradio'}
             * 
             * or
             * 
             * {command:'volume',
             *  value:58}
             */
            Command cmd = JsonConvert.DeserializeObject<Command>(message);
            if (cmd.VAL >= 0)
            {
                // System operation (like volume, cruise, etc.
                if (cmd.CMD.Equals("volume"))
                {
                    uint volume = (uint)cmd.VAL;
                    waveOutSetVolume(IntPtr.Zero, volume);
                }
                if (cmd.CMD.Equals("cruise"))
                {
                    int currentCruise = cruise.readInt();
                    int setSpeed = cmd.VAL;
                    bool pos = true;
                    int count = currentCruise - setSpeed;
                    if (count > 0)
                    {
                        pos = true;
                    }
                    else
                    {
                        pos = false;
                        count = -count;
                    }
                    
                    for (int i = 0; i < count; i++)
                    {
                        if(pos)
                            KeyEngine.send(SETTINGS.Get("keybinds","cruiseup"));
                        else
                            KeyEngine.send(SETTINGS.Get("keybinds", "cruisedown"));
                    }
                }
            }
            else
            {
                // Keybind operation, send a keypress
                KeyEngine.send(cmd.CMD);
            }
        }
                
        public void log(String message)
        {
            if (LOG)
            {
                StringBuilder sb = new StringBuilder();
                string str = DateTime.Now.ToString("hh.mm.ss.fff");
                sb.Append("[" + str + "] > ");
                sb.Append(message);
                sb.Append("\r\n");
                textBox1.AppendText(sb.ToString());
            }
        }
        
        private void startServerBtn_Click(object sender, EventArgs e)
        {
            log("Starting server...");
            try
            {
                server.start();
                StatusLabel.Text = "Running";
                StatusLabel.ForeColor = Color.LimeGreen;
                log("Server started.");
            }
            catch (Exception ex)
            {
                log(ex.ToString());
                StatusLabel.Text = "Stopped";
                StatusLabel.ForeColor = Color.Red;
                log("Server failed to start.");
            }
        }

        private void stopServerBtn_Click(object sender, EventArgs e)
        {
            log("Stopping server...");
            server.stop();
            StatusLabel.Text = "Stopped";
            StatusLabel.ForeColor = Color.Red;
            log("Server stopped.");

        }

        private void startLoggingBtn_Click(object sender, EventArgs e)
        {
            LOG = true;
        }

        private void stopLoggingBtn_Click(object sender, EventArgs e)
        {
            LOG = false;
        }

        private void keybindChanged(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                String command = dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();
                String description = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();
                String keybind = dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString();

                updateSettings("keybinds", command, description + ";" + keybind);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                log(ex.ToString());
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                updateSettings("properties", "startsrv", "true");
            }
            else
            {
                updateSettings("properties", "startsrv", "false");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
        }

        private void updateSettings(string section, string key, object value)
        {
            log(String.Format("Updated property {0} to {1}", key, value));
            SETTINGS.Set(section, key, value);
        }

        [DllImport("winmm.dll")]
        public static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);
    }
}
