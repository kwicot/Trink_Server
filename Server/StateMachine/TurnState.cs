using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Riptide.Utils;
using WindowsFormsApp1;
using LogType = Riptide.Utils.LogType;

namespace Trink_RiptideServer.Library.StateMachine
{
    public class TurnState : GameState
    {
        private int _currentTurn = 0;
        
        private Task _task;
        private CancellationTokenSource _cancellationTokenSource;
        public int CurrentTurnSeatIndex => _stateMachine.PlaySeats[_currentTurn]; 
        
        protected override void OnEnter()
        {
            Tag = $"{_stateMachine.RoomController.Tag}_State_Turn";
            Logger.LogInfo(Tag, "Enter");
            
            _currentTurn = 0;
            _stateMachine.LapTurns = 1;
            _stateMachine.LapBets = new Dictionary<int, TurnType>();
            _stateMachine.IsHideTurn = true;
            _stateMachine.PlayerCheckedCards = false;
            
            _stateMachine.OnStartTurns();
            _stateMachine.BetsData = new BetsData();
            
            _cancellationTokenSource = new CancellationTokenSource();
            _task = TurnWait();
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

        async Task TurnWait()
        {
            Logger.LogInfo(Tag, $"New turn wait [{_currentTurn}]");
            
            int seatIndex = _stateMachine.PlaySeats[_currentTurn];
            var seat = _stateMachine.RoomController.Seats[seatIndex];

            if (_stateMachine.LapBets.TryGetValue(_stateMachine.PlaySeats[_currentTurn], out var turn) &&
                turn == TurnType.Pass | turn == TurnType.AllIn) // Skip if pass
            {
                _stateMachine.LapTurns++;
                Next();
                return;
            }

            if (seat.SeatData.Balance > 0 ||(_stateMachine.IsHideTurn && seat.SeatData.Balance >= _stateMachine.Bet *2))
            {
                await Task.Delay((int)(Config.TurnDelay));
               
                if (_stateMachine.IsHideTurn && seat.SeatData.Balance < _stateMachine.Bet * 2)
                {
                    _stateMachine.IsHideTurn = false;
                    _stateMachine.OnSeatCheckCards();
                }
                
                bool lastTurnOnLap = _currentTurn >= _stateMachine.InGameSeats - 1;
                seat.Turn(_stateMachine.IsHideTurn, Config.TurnWait, lastTurnOnLap, _stateMachine.Bet);
                
                float time = (int)Config.TurnWait;
                while ( time > 0)
                {
                    await Task.Delay(100);
                    time -= 100;
                    Logger.LogInfo(Tag, $"Time left: {time}");
                    if(_cancellationTokenSource.IsCancellationRequested)
                        return;
                }
                
                seat.EndTurn();
                OnTurn(seatIndex, -1);
            }
            else
            {
                OnTurn(seatIndex, 0);
            }
        }

        public void OnTurn(int seatIndex, int value)
        {
            _stateMachine.LapTurns++;
            
            _cancellationTokenSource.Cancel();

            if (value == -1)
                OnPassTurn(seatIndex);
            
            else if (_stateMachine.IsHideTurn && value == _stateMachine.Bet * 2)
                OnHideBet(seatIndex);
            
            else if (value == _stateMachine.RoomController.Seats[seatIndex].SeatData.Balance)
                OnAllIn(seatIndex);
            
            else if (value < _stateMachine.Bet)
                OnMinBet(seatIndex, value);
            
            else if (value >= _stateMachine.Bet)
                OnIncrease(seatIndex, value);
            
            _stateMachine.SendData();
        }
        int CanBet(int value, out int maxValue)
        {
            int canBet = 0;
            maxValue = 0;

            for (int i = 0; i < _stateMachine.PlaySeats.Count; i++)
            {
                int seatIndex = _stateMachine.PlaySeats[i];
                var seatController = _stateMachine.RoomController.Seats[seatIndex];
                if (_stateMachine.LapBets.TryGetValue(seatIndex, out var turn))
                {
                    if (turn != TurnType.Pass)
                    {
                        if (!seatController.IsFree &&seatController.SeatData.Balance >= value)
                        {
                            canBet++;
                        }
                        else
                        {
                            if (seatController.SeatData.Balance > maxValue)
                                maxValue = seatController.SeatData.Balance;
                        }
                    }
                }
                else
                {
                    if (!seatController.IsFree && seatController.SeatData.Balance >= value)
                    {
                        canBet++;
                    }
                    else
                    {
                        if (seatController.SeatData.Balance > maxValue)
                            maxValue = seatController.SeatData.Balance;
                    }
                }
            }
            return canBet;
        }

        void Next()
        {

            _cancellationTokenSource.Cancel();

            _currentTurn++;
            Logger.LogInfo(Tag, $"Next turn [{_currentTurn}]");

            bool allTurn = _stateMachine.LapTurns >= _stateMachine.PlaySeats.Count;
            bool onlyOne = _stateMachine.InGameSeats == 1;
            bool lastTurnOnLap = _currentTurn >= _stateMachine.PlaySeats.Count;
            int haveMoney = 0;
            int canBet = CanBet(_stateMachine.Bet, out int maxBet);

            for (int i = 0; i < _stateMachine.PlaySeats.Count; i++)
            {
                if (i != _currentTurn - 1)
                {
                    if (_stateMachine.RoomController.Seats[_stateMachine.PlaySeats[i]].SeatData.Balance > 0)
                        haveMoney++;
                }
            }

            if (haveMoney == 0)
            {
                OnEndLap();
                return;
            }

            if (allTurn && lastTurnOnLap)
            {
                OnEndLap();
                return;
            }

            if (allTurn && canBet < 1)
            {
                OnEndLap();
                return;
            }

            if (onlyOne)
            {
                OnEndLap();
                return;
            }

            _task = TurnWait();
        }

        void OnEndLap()
        {
            Logger.LogInfo(Tag, "OnEndLap");
            
            _cancellationTokenSource.Cancel();
            
            if (CanBet(_stateMachine.Bet, out var max) > 0)
            {
                int increaseCount = 0;
                foreach (var lapBet in _stateMachine.LapBets)
                {
                    if (lapBet.Value == TurnType.Increase)
                        increaseCount++;
                    
                    if (increaseCount > 1)
                    {
                        _currentTurn = 0;
                        _task = TurnWait();
                        return;
                    }
                }
            }

            LogInfo("All bets is min. End turns");
            _stateMachine.SetState<CalcState>();
        }

        void OnPassTurn(int seatIndex)
        {
            LogInfo($"On pass turn {seatIndex}");
            //_stateMachine.PlaySeats.Remove(seatIndex);
            _stateMachine.LapBets[seatIndex] = TurnType.Pass;
            //_stateMachine.BetsData.Bets[seatIndex] = 0;
            
            _stateMachine.PlayerCheckedCards = false;
            
            Next();
        }
        
        private void OnHideBet(int seatIndex)
        {
            int value = _stateMachine.Bet * 2;
            
            if(_stateMachine.BetsData.Bets.ContainsKey(seatIndex))
                _stateMachine.BetsData.Bets[seatIndex] += value;
            else
                _stateMachine.BetsData.Bets[seatIndex] = value;
            
            LogInfo($"On hide bet seat: {seatIndex} bet: {value}. Total bets {_stateMachine.BetsData.Bets[seatIndex]} ");
            
            _stateMachine.LapBets[seatIndex] = TurnType.Hide;
            
             var playIndex = GetPlayIndex(seatIndex);
             ShiftTurnQueue(playIndex);
             

            _stateMachine.RoomController.Seats[seatIndex].Withdraw(value);
            
            _stateMachine.Bet = value;


            if (_stateMachine.IsHideTurn && _stateMachine.PlayerCheckedCards)
                OnPlayerCheckCards();
            
            Next();
        }

        private void OnAllIn(int seatIndex)
        {
            var playerData = _stateMachine.RoomController.Seats[seatIndex];
            _stateMachine.BetsData.Bets[seatIndex] += playerData.SeatData.Balance;
            
            LogInfo($"On all in seat: {seatIndex} balance: {_stateMachine.RoomController.Seats[seatIndex].SeatData.Balance}. Total bets {_stateMachine.BetsData.Bets[seatIndex]}");

            _stateMachine.LapBets[seatIndex] = TurnType.AllIn;

            if (playerData.SeatData.Balance > _stateMachine.Bet)
                _stateMachine.Bet = playerData.SeatData.Balance;
            
            _stateMachine.RoomController.Seats[seatIndex].Withdraw(playerData.SeatData.Balance);
            
            var playIndex = GetPlayIndex(seatIndex);
            ShiftTurnQueue(playIndex);
            
            if (_stateMachine.IsHideTurn && _stateMachine.PlayerCheckedCards)
                OnPlayerCheckCards();

            Next();
        }

        void OnMinBet(int seatIndex, int bet)
        {
            _stateMachine.BetsData.Bets[seatIndex] += bet;
            
            LogInfo($"On min bet seat: {seatIndex} bet: {bet}. Total bets {_stateMachine.BetsData.Bets[seatIndex]}");
            _stateMachine.RoomController.Seats[seatIndex].Withdraw(bet);
            
            _stateMachine.LapBets[seatIndex] = TurnType.Min;
            
            if (_stateMachine.IsHideTurn && _stateMachine.PlayerCheckedCards)
                OnPlayerCheckCards();

            Next();
        }

        void OnIncrease(int seatIndex, int bet)
        {
            LogInfo($"OnIncrease seat:{seatIndex} bet:{bet}. Total bets {_stateMachine.BetsData.Bets[seatIndex] + bet}");
            
            if (_stateMachine.IsHideTurn && _stateMachine.PlayerCheckedCards)
                OnPlayerCheckCards();
            
            var playIndex = GetPlayIndex(seatIndex);

            if (CanBet(bet, out int max) > 1) //Если кто-то может ответить на ставку
            {
                LogInfo("CanBet > 1");
                
                ShiftTurnQueue(playIndex);
                
                //Debug.Log($"Seat index {seatIndex}, play 0 {_stateMachine.PlaySeats[0]}, play 1 {_stateMachine.PlaySeats[1]}");
            
                _stateMachine.RoomController.Seats[seatIndex].Withdraw(bet);
            
                _stateMachine.LapBets[seatIndex] = TurnType.Increase;

                _stateMachine.BetsData.Bets[seatIndex] += bet;
                _stateMachine.Bet = bet;

                Next();
            }
            else
            {
                LogInfo("CanBet < 1");
                if (max >= _stateMachine.Bet * 2) //Если никто не сможет ответить на ставку но ответная ставка может быть Х2
                {
                    LogInfo("Max >= minBet * 2");
                    
                    ShiftTurnQueue(playIndex);
            
                    _stateMachine.RoomController.Seats[seatIndex].Withdraw(max);
                    
                    _stateMachine.LapBets[seatIndex] = TurnType.Increase;

                    _stateMachine.BetsData.Bets[seatIndex] += max;
                    _stateMachine.Bet = max;

                    Next();
                }
                else //Если никто не может ответить на ставку и ответная ставка не может біть Х2
                {
                    LogInfo("Max < minBet * 2");
                    _stateMachine.RoomController.Seats[seatIndex].Withdraw(max);
                    
                    _stateMachine.LapBets[seatIndex] = TurnType.Normal;

                    _stateMachine.BetsData.Bets[seatIndex] += max;
                    _stateMachine.Bet = max;

                    Next();
                }
            }
        }

        void OnPlayerCheckCards()
        {
            _stateMachine.IsHideTurn = false;
            
            for (int i = 0; i < _stateMachine.PlaySeats.Count; i++)
            {
                _stateMachine.RoomController.Seats[_stateMachine.PlaySeats[i]].ShowCardsLocal();
            }
        }

        void ShiftTurnQueue(int playIndex)
        {
            _stateMachine.LapTurns = 1;
            //LogInfo("Before shift");
            //LogSeatsIndexes();
                    
            _stateMachine.PlaySeats.ShiftLeft(playIndex);
            _currentTurn = 0;
                    
            //LogInfo("After shift");
            //LogSeatsIndexes();
        }

        int GetPlayIndex(int index)
        {
            Logger.LogInfo(Tag, $"Get play [{index}] index");
            for (int i = 0; i < _stateMachine.PlaySeats.Count; i++)
            {
                if (_stateMachine.PlaySeats[i] == index)
                    return i;
            }

            throw new NullReferenceException("Cant get play index");
        }
        
        
        int[] ShiftArrayLeft(ref int[] array, int positions)
        {
            int length = array.Length;
            int[] result = new int[length];  // Массив для результата с сдвигом

            for (int i = 0; i < length; i++)
            {
                if (i + positions < length)
                {
                    result[i] = array[i + positions];  // Сдвиг элементов влево
                }
                else
                {
                    result[i] = 0;  // Заполнение нулями (или другим значением)
                }
            }

            return result;
        }

        public void SendStateToPlayer(ushort clientId)
        {
            
        }

        public TurnState(StateMachine stateMachine) : base(stateMachine)
        {
        }
    }
}