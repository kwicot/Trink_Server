using System.Linq;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class LoggingForm : Form
    {
        public LoggingForm()
        {
            InitializeComponent();

            Logger.OnLogsChanged += UpdateLogs;
            UpdateLogs();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Logger.OnLogsChanged -= UpdateLogs;
        }

        private void UpdateLogs()
        {
            listBox_logs.Items.Clear();

            foreach (var logData in Logger.Logs.ToList())
            {
                listBox_logs.Items.Add(logData);
            }
        }
    }
}