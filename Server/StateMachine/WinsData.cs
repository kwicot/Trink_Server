using System;
using System.Collections.Generic;
using System.Linq;
using Server.Core;
using Trink_RiptideServer;
using Trink_RiptideServer.Library.StateMachine;

namespace WindowsFormsApp1.StateMachine;

public class WinsData
{
    private Dictionary<int, int> _betsMap;
    private Dictionary<int, int> _banksMap;
    private Dictionary<int, int> _playersBankMap;
    private CalcState _state;
    private string Tag => _state.Tag;

    public int TotalBank
    {
        get
        {
            int value = 0;
            foreach (var bankData in _banksMap.Values)
            {
                value += bankData;
            }

            return value;
        }
    }
    public int CommissionSum { get; private set; }

    public WinsData(CalcState state, BetsData betsData)
    {
        this._state = state;

        _betsMap = new Dictionary<int, int>();
        foreach (var data in betsData.Bets)
        {
            _betsMap[data.Key] = data.Value;
        }
        
        _banksMap = new Dictionary<int, int>();
        _playersBankMap = new Dictionary<int, int>();
        
        CalcBanks();
    }

    void CalcBanks()
    {
        Logger.LogInfo(Tag, "Start calc banks");
        int index = 0;
        
        while (_betsMap.Any(betsData => betsData.Value > 0))
        {
            var minBank = _betsMap.Where(bets => bets.Value > 0).Min(bets => bets.Value);
            
            int minBankPlayer = PlayerOfBet(minBank);
            
            Logger.LogInfo(Tag, $"Created bank[{index}] with min min balance {minBank} of player {minBankPlayer}");

            if (minBankPlayer != -1)
            {
                int value = 0;
                
                foreach (var betsData in _betsMap.ToList())
                {
                    if (betsData.Value >= minBank)
                    {
                        _betsMap[betsData.Key] -= minBank;
                        value += minBank;
                        _playersBankMap[betsData.Key] = index;
                        Logger.LogInfo(Tag, $"Add player {betsData.Key} to bank {index}");
                    }
                }
                
                Logger.LogInfo(Tag, $"Bank[{index}] total sum {value}");
                _banksMap[index] = value;
            }

            index++;
        }
        
        Logger.LogInfo(Tag, $"Total created {_banksMap.Count} banks.");
        foreach (var bankData in _banksMap)
            Logger.LogInfo(Tag, $"Bank[{bankData.Key}] has {bankData.Value}");
        
        
    }

    int PlayerOfBet(int bet)
    {
        foreach (var betsData in _betsMap)
        {
            if (betsData.Value == bet)
                return betsData.Key;
        }

        return -1;
    }

    public int GetPlayerWin(int seatIndex)
    {
        if (_playersBankMap.TryGetValue(seatIndex, out int playerBankIndex))
        {
            int value = 0;
            for (int i = 0; i <= playerBankIndex; i++)
            {
                int bankValue = _banksMap[i];
                value += bankValue;
                _banksMap[i] = 0;
            }
            return value;
        }
        else
            return -1;
    }

    public Dictionary<int, int> CalculatePlayerWins(List<int> winningPlayers)
    {
        Logger.LogInfo(Tag, $"Start calculate player wins. Players {winningPlayers.Count}");
        var playerWins = new Dictionary<int, int>(); // Итоговые выигрыши игроков

        // Инициализируем выигрыши игроков нулями
        foreach (var player in winningPlayers)
        {
            playerWins[player] = 0;
        }

        // Обрабатываем все банки
        for (int bankIndex = 0; bankIndex < _banksMap.Count; bankIndex++)
        {
            int bankValue = _banksMap[bankIndex]; // Сумма в банке
            if (bankValue == 0) continue; // Пропускаем пустые банки

            // Определяем всех игроков, претендующих на этот банк
            var contenders = winningPlayers
                .Where(player => _playersBankMap.TryGetValue(player, out int maxBankIndex) && bankIndex <= maxBankIndex)
                .ToList();

            if (contenders.Count == 1)
            {
                // Банк принадлежит только одному игроку
                int player = contenders[0];
                
                Logger.LogInfo(Tag, $"Player {player} can take all {bankIndex} bank.");

                playerWins[player] += bankValue;
                _banksMap[bankIndex] = 0; // Обнуляем банк
            }
            else if (contenders.Count > 1)
            {
                // Банк делится между несколькими игроками
                int splitValue = bankValue / contenders.Count; // Сумма, которую получает каждый
                Logger.LogInfo(Tag, $"Bank {bankIndex} split to {contenders.Count()} players.");
                foreach (var player in contenders)
                {
                    playerWins[player] += splitValue;
                }

                _banksMap[bankIndex] = 0; // Обнуляем банк
            }
        }

        return playerWins; // Возвращаем результаты
    }

    public bool HaveWins()
    {
        foreach (var bankData in _banksMap)
        {
            if (bankData.Value > 0)
                return true;
        }

        return false;
    }

    public void ApplyCommission()
    {
        var commission = Program.Config.TablePercent;

        int commissionSum = 0;
        
        if (commission > 0)
        {
            foreach (var bankData in _banksMap.ToList())
            {
                float bank = _banksMap[bankData.Key];
                int commissionValue = (int)Math.Ceiling(bank * (commission / 100f));

                Logger.LogInfo(Tag, $"Take commission {commissionValue} from bank[{bankData.Key}] {bankData.Value}");
                
                _banksMap[bankData.Key] -= commissionValue;
                commissionSum += commissionValue;
            }
        }

        CommissionSum = commissionSum;
    }
}