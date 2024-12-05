using Riptide.Utils;
using Server;

namespace Trink_RiptideServer.Library.StateMachine
{
    public class CalcState : GameState
    {
        private Task _task;
        private CancellationTokenSource _cancellationTokenSource;
        protected override void OnEnter()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _task = Task.Run(() => 
            {
                try
                {
                    CalcScores().Wait();
                }
                catch (Exception ex)
                {
                    RiptideLogger.Log(LogType.Error, $"Exception in Wait: {ex}");
                }
            }, _cancellationTokenSource.Token);
            
            SendEnterMessage("Підрахунок");
        }
        
        protected override void OnTick()
        {
        }

        protected override void OnExit()
        {
            _cancellationTokenSource.Cancel();
        }

        void CheckSvara()
        {
            
        }
        
        void CalcReturns()
        {
            LogInfo("Calc returns");

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
                
                LogInfo($"Max bet {maxBet} seat {returnSeat}. Smaller bet {smallerBet} seat {GetSeatWithBet(smallerBet)}. Return {returnValue}");

                //_stateMachine.InfoText.text =
                    //$"Повернення ({returnValue}) до {_stateMachine.Seats[returnSeat].SeatData.Nickname}({returnSeat})";
                    
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
            
            _stateMachine.RoomController.OnTakeTablePercent((int)sum);
            await Task.Delay(1000);
        }


        async Task CalcScores()
        {
            if (_stateMachine.InGameSeats > 1)
            {
                await Task.Delay(2000);
                CalcReturns();
            }

            await CalcBankPercent();
            
            await Task.Delay(5000);
            LogInfo("Calc score");
            var scores = GetScores();
            var bestScores = GetBestScores(scores);
            if (_stateMachine.InGameSeats == 1)
            {
                int win = _stateMachine.Balance;
                _stateMachine.BetsData.Bets.Clear();

                _stateMachine.RoomController.Seats[_stateMachine.DealerIndex].Win(win);

                await Task.Delay(5000);

                _stateMachine.SetState<EndState>();
            }
            
            else if (bestScores.Count > 1) // 1+ winners
            {
                await Task.Delay(3000);
                
                CalcWins(bestScores);
            }
            
            
            else if (bestScores.Count == 0) //End game
            {
                _stateMachine.RoomController.Seats[_stateMachine.DealerIndex].ShowCardsToAll();

                await Task.Delay(3000);

                int win = _stateMachine.Balance;
                _stateMachine.BetsData.Bets.Clear();

                _stateMachine.RoomController.Seats[_stateMachine.DealerIndex].Win(win);

                await Task.Delay(3000);

                _stateMachine.SetState<EndState>();
            }
            else //Only 1 with biggest score
            {
                var playerMaxWin = _stateMachine.BetsData.PlayerWin(bestScores[0]);
                TurnType lastTurn;
                if(!_stateMachine.LapBets.TryGetValue(bestScores[0], out lastTurn))
                    lastTurn = TurnType.No;
                
                LogInfo($"Player max win [{playerMaxWin}] Player last turn [{lastTurn}] table bank [{_stateMachine.Balance}]");

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

        void CalsPart()
        {
            
        }

        void CalcSvara(List<int> svaraIndexes)
        {
            LogInfo("Calc svara");

            foreach (var svaraIndex in svaraIndexes)
            {
                Log($"Svara {svaraIndex}");
            }
            
            _stateMachine.SvaraEnterPrice = _stateMachine.Balance / svaraIndexes.Count;
            _stateMachine.PlaySeats.Clear();
            
            _stateMachine.PlaySeats.AddRange(svaraIndexes);
            
            _stateMachine.SetState<SvaraState>();
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

        bool HaveSvara(List<int> scores, int max, out List<int> matchIndexes)
        {
            matchIndexes = new List<int>();
            for (int i = 0; i < scores.Count; i++)
            {
                if(scores[i] == max)
                    matchIndexes.Add(i);
            }

            return matchIndexes.Count > 1;
        }

        public CalcState(StateMachine stateMachine) : base(stateMachine) { }
    }
}