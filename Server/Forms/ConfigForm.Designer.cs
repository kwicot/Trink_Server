using System.ComponentModel;

namespace WindowsFormsApp1;

partial class ConfigForm
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
        this.label1 = new System.Windows.Forms.Label();
        this.label2 = new System.Windows.Forms.Label();
        this.label3 = new System.Windows.Forms.Label();
        this.label4 = new System.Windows.Forms.Label();
        this.label5 = new System.Windows.Forms.Label();
        this.label6 = new System.Windows.Forms.Label();
        this.label7 = new System.Windows.Forms.Label();
        this.label8 = new System.Windows.Forms.Label();
        this.label9 = new System.Windows.Forms.Label();
        this.textBox_registerBalance = new System.Windows.Forms.TextBox();
        this.textBox_tableCommission = new System.Windows.Forms.TextBox();
        this.textBox_tableMinBalanceForCommission = new System.Windows.Forms.TextBox();
        this.textBox_maxPlayers = new System.Windows.Forms.TextBox();
        this.textBox_port = new System.Windows.Forms.TextBox();
        this.textBox_gameVersion = new System.Windows.Forms.TextBox();
        this.textBox_adminPassowrd = new System.Windows.Forms.TextBox();
        this.textBox_turnTime = new System.Windows.Forms.TextBox();
        this.textBox_afkMaxTurns = new System.Windows.Forms.TextBox();
        this.button_save = new System.Windows.Forms.Button();
        this.SuspendLayout();
        // 
        // label1
        // 
        this.label1.Location = new System.Drawing.Point(12, 9);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(146, 23);
        this.label1.TabIndex = 0;
        this.label1.Text = "Реєстраційний бонус";
        // 
        // label2
        // 
        this.label2.Location = new System.Drawing.Point(12, 32);
        this.label2.Name = "label2";
        this.label2.Size = new System.Drawing.Size(146, 23);
        this.label2.TabIndex = 1;
        this.label2.Text = "Комісія столів";
        // 
        // label3
        // 
        this.label3.Location = new System.Drawing.Point(12, 55);
        this.label3.Name = "label3";
        this.label3.Size = new System.Drawing.Size(167, 23);
        this.label3.TabIndex = 2;
        this.label3.Text = "Мінімальна сумма для комісії";
        // 
        // label4
        // 
        this.label4.Location = new System.Drawing.Point(12, 78);
        this.label4.Name = "label4";
        this.label4.Size = new System.Drawing.Size(167, 23);
        this.label4.TabIndex = 3;
        this.label4.Text = "Кількість гравців";
        // 
        // label5
        // 
        this.label5.Location = new System.Drawing.Point(12, 101);
        this.label5.Name = "label5";
        this.label5.Size = new System.Drawing.Size(167, 23);
        this.label5.TabIndex = 4;
        this.label5.Text = "Порт";
        // 
        // label6
        // 
        this.label6.Location = new System.Drawing.Point(12, 124);
        this.label6.Name = "label6";
        this.label6.Size = new System.Drawing.Size(167, 23);
        this.label6.TabIndex = 5;
        this.label6.Text = "Версія гри";
        // 
        // label7
        // 
        this.label7.Location = new System.Drawing.Point(12, 147);
        this.label7.Name = "label7";
        this.label7.Size = new System.Drawing.Size(167, 23);
        this.label7.TabIndex = 6;
        this.label7.Text = "Пароль адміну";
        // 
        // label8
        // 
        this.label8.Location = new System.Drawing.Point(12, 170);
        this.label8.Name = "label8";
        this.label8.Size = new System.Drawing.Size(167, 23);
        this.label8.TabIndex = 7;
        this.label8.Text = "Час на хід";
        // 
        // label9
        // 
        this.label9.Location = new System.Drawing.Point(12, 193);
        this.label9.Name = "label9";
        this.label9.Size = new System.Drawing.Size(167, 23);
        this.label9.TabIndex = 8;
        this.label9.Text = "Макс афк ходів";
        // 
        // textBox_registerBalance
        // 
        this.textBox_registerBalance.Location = new System.Drawing.Point(177, 6);
        this.textBox_registerBalance.Name = "textBox_registerBalance";
        this.textBox_registerBalance.Size = new System.Drawing.Size(189, 20);
        this.textBox_registerBalance.TabIndex = 9;
        // 
        // textBox_tableCommission
        // 
        this.textBox_tableCommission.Location = new System.Drawing.Point(177, 29);
        this.textBox_tableCommission.Name = "textBox_tableCommission";
        this.textBox_tableCommission.Size = new System.Drawing.Size(189, 20);
        this.textBox_tableCommission.TabIndex = 10;
        // 
        // textBox_tableMinBalanceForCommission
        // 
        this.textBox_tableMinBalanceForCommission.Location = new System.Drawing.Point(177, 52);
        this.textBox_tableMinBalanceForCommission.Name = "textBox_tableMinBalanceForCommission";
        this.textBox_tableMinBalanceForCommission.Size = new System.Drawing.Size(189, 20);
        this.textBox_tableMinBalanceForCommission.TabIndex = 11;
        // 
        // textBox_maxPlayers
        // 
        this.textBox_maxPlayers.Location = new System.Drawing.Point(177, 75);
        this.textBox_maxPlayers.Name = "textBox_maxPlayers";
        this.textBox_maxPlayers.Size = new System.Drawing.Size(189, 20);
        this.textBox_maxPlayers.TabIndex = 12;
        // 
        // textBox_port
        // 
        this.textBox_port.Location = new System.Drawing.Point(177, 98);
        this.textBox_port.Name = "textBox_port";
        this.textBox_port.Size = new System.Drawing.Size(189, 20);
        this.textBox_port.TabIndex = 13;
        // 
        // textBox_gameVersion
        // 
        this.textBox_gameVersion.Location = new System.Drawing.Point(177, 121);
        this.textBox_gameVersion.Name = "textBox_gameVersion";
        this.textBox_gameVersion.Size = new System.Drawing.Size(189, 20);
        this.textBox_gameVersion.TabIndex = 14;
        // 
        // textBox_adminPassowrd
        // 
        this.textBox_adminPassowrd.Location = new System.Drawing.Point(177, 144);
        this.textBox_adminPassowrd.Name = "textBox_adminPassowrd";
        this.textBox_adminPassowrd.Size = new System.Drawing.Size(189, 20);
        this.textBox_adminPassowrd.TabIndex = 15;
        // 
        // textBox_turnTime
        // 
        this.textBox_turnTime.Location = new System.Drawing.Point(177, 167);
        this.textBox_turnTime.Name = "textBox_turnTime";
        this.textBox_turnTime.Size = new System.Drawing.Size(189, 20);
        this.textBox_turnTime.TabIndex = 16;
        // 
        // textBox_afkMaxTurns
        // 
        this.textBox_afkMaxTurns.Location = new System.Drawing.Point(177, 190);
        this.textBox_afkMaxTurns.Name = "textBox_afkMaxTurns";
        this.textBox_afkMaxTurns.Size = new System.Drawing.Size(189, 20);
        this.textBox_afkMaxTurns.TabIndex = 17;
        // 
        // button_save
        // 
        this.button_save.Location = new System.Drawing.Point(12, 219);
        this.button_save.Name = "button_save";
        this.button_save.Size = new System.Drawing.Size(110, 23);
        this.button_save.TabIndex = 18;
        this.button_save.Text = "Зберегти";
        this.button_save.UseVisualStyleBackColor = true;
        this.button_save.Click += new System.EventHandler(this.button_save_Click);
        // 
        // ConfigForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(800, 450);
        this.Controls.Add(this.button_save);
        this.Controls.Add(this.textBox_afkMaxTurns);
        this.Controls.Add(this.textBox_turnTime);
        this.Controls.Add(this.textBox_adminPassowrd);
        this.Controls.Add(this.textBox_gameVersion);
        this.Controls.Add(this.textBox_port);
        this.Controls.Add(this.textBox_maxPlayers);
        this.Controls.Add(this.textBox_tableMinBalanceForCommission);
        this.Controls.Add(this.textBox_tableCommission);
        this.Controls.Add(this.textBox_registerBalance);
        this.Controls.Add(this.label9);
        this.Controls.Add(this.label8);
        this.Controls.Add(this.label7);
        this.Controls.Add(this.label6);
        this.Controls.Add(this.label5);
        this.Controls.Add(this.label4);
        this.Controls.Add(this.label3);
        this.Controls.Add(this.label2);
        this.Controls.Add(this.label1);
        this.Name = "ConfigForm";
        this.Text = "ConfigForm";
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    private System.Windows.Forms.Button button_save;

    private System.Windows.Forms.TextBox textBox_registerBalance;
    private System.Windows.Forms.TextBox textBox_tableCommission;
    private System.Windows.Forms.TextBox textBox_tableMinBalanceForCommission;
    private System.Windows.Forms.TextBox textBox_maxPlayers;
    private System.Windows.Forms.TextBox textBox_port;
    private System.Windows.Forms.TextBox textBox_gameVersion;
    private System.Windows.Forms.TextBox textBox_adminPassowrd;
    private System.Windows.Forms.TextBox textBox_turnTime;
    private System.Windows.Forms.TextBox textBox_afkMaxTurns;

    private System.Windows.Forms.Label label9;

    private System.Windows.Forms.Label label8;

    private System.Windows.Forms.Label label7;

    private System.Windows.Forms.Label label6;

    private System.Windows.Forms.Label label5;

    private System.Windows.Forms.Label label4;

    private System.Windows.Forms.Label label3;

    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;

    #endregion
}