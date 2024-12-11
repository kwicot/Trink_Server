using System.Linq;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class LoggingForm : Form
    {
        public LoggingForm()
        {
            InitializeComponent();

            Logger.OnLogAdded += OnLogAdded;
            Initialize();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Logger.OnLogAdded -= OnLogAdded;
        }

        void Initialize()
        {
            listBox_logs.Items.Clear();
            
            foreach (var logData in Logger.Logs.ToList())
            {
                listBox_logs.Items.Add(logData);
            }
        }

        private void OnLogAdded(string newLog)
        {
            listBox_logs.Items.Add(newLog);
        }
    }
}