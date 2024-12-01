using System.ComponentModel;

namespace WindowsFormsApp1
{
    partial class LoggingForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.listBox_logs = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // listBox_logs
            // 
            this.listBox_logs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBox_logs.FormattingEnabled = true;
            this.listBox_logs.Location = new System.Drawing.Point(0, 0);
            this.listBox_logs.Name = "listBox_logs";
            this.listBox_logs.Size = new System.Drawing.Size(684, 428);
            this.listBox_logs.TabIndex = 0;
            // 
            // LoggingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 428);
            this.Controls.Add(this.listBox_logs);
            this.Name = "LoggingForm";
            this.Text = "Loging";
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.ListBox listBox_logs;

        #endregion
    }
}