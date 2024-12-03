using System;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            Server.Core.Server.OnStatusChanged += OnServerStatusChange;
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
    }
}