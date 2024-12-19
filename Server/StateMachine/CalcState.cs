using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WindowsFormsApp1;
using WindowsFormsApp1.StateMachine;

namespace Trink_RiptideServer.Library.StateMachine
{
    public class CalcState : GameState
    {
        private Task _task;
        private CancellationTokenSource _cancellationTokenSource;
        private bool percentTaked = false;

        protected override void OnEnter()
        {
            Tag = $"{_stateMachine.RoomController.Tag}_State_Calc";
            Logger.LogInfo(Tag, "Enter");
            percentTaked = false;

            
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
            
            _stateMachine.SendStatus("Підрахунок");
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
            _cancellationTokenSource?.Dispose();
        }


        async Task CalcScores()
        {
            await Task.Delay((int)Config.DebugDelay);

            await Task.Delay(2000);
            await CalcReturns();
            await Task.Delay(2000);
            
            

            //TODO calc table percent
            if (!percentTaked)
            {
                _stateMachine.WinsData = new WinsData(this,_stateMachine.BetsData);
                await CalcBankPercent();
            }

            await Task.Delay(2000);

            var scores = GetScores();
            var bestScores = GetBestScores(scores);
            if (_stateMachine.InGameSeats == 1)
            {
                int win = _stateMachine.Balance;
                _stateMachine.BetsData.Bets.Clear();

                int index = _stateMachine.FirstInGameSeatIndex;
                if (index == -1)
                {
                    _stateMachine.SetState<EndState>();
                    return;
                }

                _stateMachine.RoomController.Seats[index].Win(win);

                await Task.Delay(2000);

                _stateMachine.SetState<EndState>();
            }

            else if (bestScores.Count > 1) // 1+ winners
            {
                CalcWins(bestScores);
            }


            else if (bestScores.Count == 0) //End game
            {
                Logger.LogInfo(Tag, $"Dealer [{_stateMachine.DealerIndex}] win");

                int value = _stateMachine.Balance;
                await Win(_stateMachine.DealerIndex, value);
                
                if(_stateMachine.WinsData.HaveWins())
                    _task = Task.Run(CalcScores, _cancellationTokenSource.Token);
                else
                    _stateMachine.SetState<EndState>();
                
            }
            else //Only 1 with biggest score
            {
                int seatIndex = bestScores[0];

                var playerMaxWin = _stateMachine.WinsData.GetPlayerWin(seatIndex);

                if (playerMaxWin >= _stateMachine.Balance || _stateMachine.PlaySeats.Count == 1) // All win
                {
                    Logger.LogInfo(Tag, $"Only 1 win [{seatIndex}] All win {playerMaxWin}");
                    
                    await Win(seatIndex, playerMaxWin);
                    
                    _stateMachine.SetState<EndState>();
                }
                else // Win part of balance
                {
                    Logger.LogInfo(Tag, $"Part win [{seatIndex}]");

                    await Win(seatIndex, playerMaxWin);

                    if (_stateMachine.WinsData.HaveWins())
                        _task = Task.Run(CalcScores, _cancellationTokenSource.Token);
                    else
                        _stateMachine.SetState<EndState>();
                }
            }
        }


        async Task Win(int seatIndex, int value)
        {
            var seat = _stateMachine.RoomController.Seats[seatIndex];
            
            seat.ShowCardsToAll();
            _stateMachine.SendStatus($"{seat.UserData.UserProfile.NickName} виграв {value}");

            _stateMachine.BetsData.Bets[seatIndex] = 0;
            _stateMachine.PlaySeats.Remove(seatIndex);

            _stateMachine.RoomController.Seats[seatIndex].Win(value);
            _stateMachine.SendData();
            
            await Task.Delay(10000);
        }

        async Task CalcReturns()
        {
            List<int> bets = new List<int>();
            for (int i = 0; i < _stateMachine.PlaySeats.Count; i++)
            {
                bets.Add(_stateMachine.BetsData.Bets[_stateMachine.PlaySeats[i]]);
            }
            
            bets.Sort();
            if (bets.Count > 1)
            {
                if (bets[bets.Count - 1] != bets[bets.Count - 2])
                {
                    int maxBet = bets[bets.Count - 1];
                    int smallerBet = bets[bets.Count - 2];
                    int returnValue = maxBet - smallerBet;
                    int returnSeat = GetSeatWithBet(maxBet);

                    _stateMachine.BetsData.Bets[returnSeat] -= returnValue;

                    _stateMachine.SendStatus(
                        $"Повернення {returnValue} гравцю {_stateMachine.RoomController.Seats[returnSeat].UserData.UserProfile.NickName}");
                    _stateMachine.SendData();

                    await Task.Delay(1000);
                    _stateMachine.RoomController.Seats[returnSeat].Return(returnValue);
                }
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
            int percent = Program.Config.TablePercent;
            int minBalance = Program.Config.MinBalanceForCommission;

            int bets = 0;
            foreach (var dataBet in _stateMachine.BetsData.Bets)
            {
                if (dataBet.Value > _stateMachine.RoomController.RoomInfo.RoomSettings.StartBet)
                    bets++;
            }
            
            
            if (_stateMachine.WinsData.TotalBank >= minBalance && bets > 1)
            {
                _stateMachine.WinsData.ApplyCommission();

                _stateMachine.SendStatus($"Коммісія столу: [{percent}] Сумма: [{_stateMachine.WinsData.CommissionSum}]");
                _stateMachine.SendData();
                
                await Task.Delay(1000);
            }

            percentTaked = true;
        }

        async void CalcWins(List<int> winsIndexes)
        {
            var wins = _stateMachine.WinsData.CalculatePlayerWins(winsIndexes);
            
            foreach (var winData in wins)
            {
                var seat = _stateMachine.RoomController.Seats[winData.Key];
                
                _stateMachine.SendStatus($"Гравець {seat.UserData.UserProfile.NickName} виграв {winData.Value}");
                seat.ShowCardsToAll();
                seat.Win(winData.Value);

                await Task.Delay(5000);
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
                    Log($"Add {seatIndex} to win list. Last tun {_stateMachine.LapBets[seatIndex]}");
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