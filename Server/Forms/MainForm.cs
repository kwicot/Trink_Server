using System;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            textBox_maxConnection.Text = Program.Config.PlayersCount.ToString();
            textBox_port.Text = Program.Config.Port.ToString();
            
            Server.Core.Server.OnStatusChanged += OnServerStatusChange;

            if (Program.Config.StartOnLaunch)
            {
                button_start_Click(this, EventArgs.Empty);
            }
        }

        private void OnServerStatusChange()
        {
            label_status.Text = Server.Core.Server.IsRunning ? "Running" : "Stopped";
        }

        private async void button_start_Click(object sender, EventArgs e)
        {
            string portText = textBox_port.Text;
            string maxConnectionsText = textBox_maxConnection.Text;

            ushort port = System.Convert.ToUInt16(portText);
            ushort maxConnections = System.Convert.ToUInt16(maxConnectionsText);

            Program.Config.PlayersCount = maxConnections;
            Program.Config.Port = port;
            
            Program.SaveConfig();
            
            await Server.Core.Server.Start(port, maxConnections);
        }

        private void button_stop_Click(object sender, EventArgs e)
        {
            Server.Core.Server.Stop();
        }

        private void toolStripButton_showLog_Click(object sender, EventArgs e)
        {
            LoggingForm loggingForm = new LoggingForm();
            
            loggingForm.Show();
        }

        private async void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            await Server.Core.Server.Stop();
        }
    }
}