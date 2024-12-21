using System;
using System.Windows.Forms;
using Server.Core;

namespace WindowsFormsApp1;

public partial class ConfigForm : Form
{
    private Config Config => Program.Config;
    public ConfigForm()
    {
        InitializeComponent();
        InitTextBoxes();
    }

    void InitTextBoxes()
    {
        textBox_registerBalance.Text = Config.RegisterBalance.ToString();
        textBox_tableCommission.Text = Config.TablePercent.ToString();
        textBox_tableMinBalanceForCommission.Text = Config.MinBalanceForCommission.ToString();
        textBox_maxPlayers.Text = Config.PlayersCount.ToString();
        textBox_port.Text = Config.Port.ToString();
        textBox_gameVersion.Text = Config.GameVersion;
        textBox_adminPassowrd.Text = Config.AdminPassword;
        textBox_turnTime.Text = Config.StateMachineConfig.TurnWait.ToString();
        textBox_afkMaxTurns.Text = Config.StateMachineConfig.AfkTurnsMax.ToString();
    }


    private void button_save_Click(object sender, EventArgs e)
    {
        string registerText = textBox_registerBalance.Text;
        string tableCommissionText = textBox_tableCommission.Text;
        string tableMinBalanceText = textBox_tableMinBalanceForCommission.Text;
        string maxPlayersText = textBox_maxPlayers.Text;
        string portText = textBox_port.Text;
        string gameVersionText = textBox_gameVersion.Text;
        string adminPasswordText = textBox_adminPassowrd.Text;
        string turnTimeText = textBox_turnTime.Text;
        string afkMaxTurnsText = textBox_afkMaxTurns.Text;
        
        int registerBalance = int.Parse(registerText);
        int tableCommission = int.Parse(tableCommissionText);
        int tableMinBalanceForCommission = int.Parse(tableMinBalanceText);
        ushort maxPlayers = ushort.Parse(maxPlayersText);
        ushort port = ushort.Parse(portText);
        int turnTime = int.Parse(turnTimeText);
        int afkMaxTurns = int.Parse(afkMaxTurnsText);
        
        Config.RegisterBalance = registerBalance;
        Config.TablePercent = tableCommission;
        Config.MinBalanceForCommission = tableMinBalanceForCommission;
        Config.GameVersion = gameVersionText;
        Config.PlayersCount = maxPlayers;
        Config.Port = port;
        Config.StateMachineConfig.TurnWait = turnTime;
        Config.StateMachineConfig.AfkTurnsMax = afkMaxTurns;
        Config.AdminPassword = adminPasswordText;
        
        Program.SaveConfig();
    }
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
}