using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Riptide.Utils;
using Server;
using WindowsFormsApp1;
using LogType = Riptide.Utils.LogType;

namespace Trink_RiptideServer.Library.StateMachine
{
    public class CalcState : GameState
    {
        private Task _task;
        private CancellationTokenSource _cancellationTokenSource;
        protected override void OnEnter()
        {
            Tag = $"{_stateMachine.RoomController.Tag}_State_Calc";
            Logger.LogInfo(Tag, "Enter");
            
            _cancellationTokenSource = new CancellationTokenSource();
            _task = Task.Run(() => 
            {
                try
                {
                    CalcScores().Wait();
                }
                catch (Exception ex)
                {
                    Logger.LogError(Tag,$"Exception : {ex}");
                }
            }, _cancellationTokenSource.Token);
        }
        
        protected override void OnTick()
        {
        }

        protected override void OnExit()
        {
            _cancellationTokenSource.Cancel();
        }

        public override void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _task?.Dispose();
            _cancellationTokenSource?.Dispose();
        }
        
        
        async Task CalcScores()
        {
            if (_stateMachine.InGameSeats > 1)
            {
                await Task.Delay(2000);
                CalcReturns();
            }

            //TODO calc table percent
           // await CalcBankPercent();
            
            var scores = GetScores();
            var bestScores = GetBestScores(scores);
            if (_stateMachine.InGameSeats == 1)
            {
                int win = _stateMachine.Balance;
                _stateMachine.BetsData.Bets.Clear();

                _stateMachine.RoomController.Seats[_stateMachine.DealerIndex].Win(win);

                _stateMachine.SetState<EndState>();
            }
            
            else if (bestScores.Count > 1) // 1+ winners
            {
                CalcWins(bestScores);
            }
            
            
            else if (bestScores.Count == 0) //End game
            {
                _stateMachine.RoomController.Seats[_stateMachine.DealerIndex].ShowCardsToAll();

                int win = _stateMachine.Balance;
                _stateMachine.BetsData.Bets.Clear();

                _stateMachine.RoomController.Seats[_stateMachine.DealerIndex].Win(win);

                _stateMachine.SetState<EndState>();
            }
            else //Only 1 with biggest score
            {
                var playerMaxWin = _stateMachine.BetsData.PlayerWin(bestScores[0]);
                TurnType lastTurn;
                if(!_stateMachine.LapBets.TryGetValue(bestScores[0], out lastTurn))
                    lastTurn = TurnType.No;
                
                if (playerMaxWin >= _stateMachine.Balance || _stateMachine.PlaySeats.Count == 1) // All win
                {
                    LogInfo("All win");
                   
                    //playerData.Balance += win;
                    foreach (var score in scores)
                    {
                        int index = score.Key;
                        _stateMachine.RoomController.Seats[index].ShowCardsToAll();
                    }
                    
                    //_stateMachine.InfoText.text = $"[{bestScores[0]}] Виграв {win}";
                    await Task.Delay(3000);
                    
                    int win = _stateMachine.Balance;
                    _stateMachine.BetsData.Bets.Clear();
                    
                    _stateMachine.RoomController.Seats[bestScores[0]].Win(win);
                    
                    await Task.Delay(3000);

                    _stateMachine.SetState<EndState>();
                }
                else // Win part of balance
                {
                    LogInfo($"Win part [{playerMaxWin}]");
                    
                    
                    //_stateMachine.Seats[bestScores[1]].ShowCardsToOther();
                    _stateMachine.RoomController.Seats[bestScores[0]].ShowCardsToAll();

                    await Task.Delay(3000);
                    
                    _stateMachine.BetsData.Remove(bestScores[0]);
                    _stateMachine.BetsData.Bets[bestScores[0]] = 0;
                    _stateMachine.PlaySeats.Remove(bestScores[0]);
                    
                    _stateMachine.RoomController.Seats[bestScores[0]].Win(playerMaxWin);

                    //_stateMachine.InfoText.text = $"[{bestScores[0]}] Виграв {playerData}";
                    await Task.Delay(3000);
                    
                    _task = Task.Run(CalcScores, _cancellationTokenSource.Token);
                }
            }
        }

        void CalcReturns()
        {
            List<int> bets = new List<int>();
            for (int i = 0; i < _stateMachine.PlaySeats.Count; i++)
            {
                bets.Add(_stateMachine.BetsData.Bets[_stateMachine.PlaySeats[i]]);
            }
            
            bets.Sort();
            if (bets[^1] != bets[^2])
            {
                int maxBet = bets[^1];
                int smallerBet = bets[^2];
                int returnValue = maxBet - smallerBet;
                int returnSeat = GetSeatWithBet(maxBet);
                
                _stateMachine.BetsData.Bets[returnSeat] -= returnValue;
                _stateMachine.RoomController.Seats[returnSeat].Return(returnValue);
            }
        }

        int GetSeatWithBet(int bet)
        {
            for (int i = 0; i < _stateMachine.RoomController.Seats.Length; i++)
            {
                if (_stateMachine.BetsData.Bets.TryGetValue(i, out var value) && value == bet)
                    return i;
            }

            return -1;
        }

        async Task CalcBankPercent()
        {
            int percent = 3;
            var sum = Math.Ceiling(_stateMachine.Balance * (percent / 100.0));
            
            //_stateMachine.RoomController.OnTakeTablePercent((int)sum);
            await Task.Delay(1000);
        }

        async void CalcWins(List<int> winsIndexes)
        {
            int win = _stateMachine.Balance / winsIndexes.Count;
            
            _stateMachine.BetsData.Bets.Clear();

            foreach (var score in winsIndexes)
            {
                _stateMachine.RoomController.Seats[score].ShowCardsToAll();
                _stateMachine.RoomController.Seats[score].Win(win);
            }
                    
            await Task.Delay(3000);

            _stateMachine.SetState<EndState>();
        }

        

        Dictionary<int, int> GetScores()
        {
            Dictionary<int, int> scoresMap = new Dictionary<int, int>();
            for (int i = 0; i < _stateMachine.PlaySeats.Count; i++)
            {
                int seatIndex = _stateMachine.PlaySeats[i];
                if (_stateMachine.LapBets.TryGetValue(seatIndex, out var turn) && turn != TurnType.Pass)
                {
                    Log($"Add {seatIndex} to win list");
                    var seat = _stateMachine.RoomController.Seats[seatIndex];
                    scoresMap.Add(seatIndex, seat.CardsSum);
                }
            }

            return scoresMap;
        }
        List<int> GetBestScores(Dictionary<int,int> score)
        {
            int max = int.MinValue;
            var indexesList = new List<int>();

            foreach (var i in score)
            {
                if (i.Value > max)
                {
                    indexesList = new List<int>() { i.Key };
                    max = i.Value;
                }
                else if (i.Value == max)
                {
                    indexesList.Add(i.Key);
                }
            }
            
            return indexesList;
        }

        public CalcState(StateMachine stateMachine) : base(stateMachine) { }
    }
}