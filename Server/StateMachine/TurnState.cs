﻿using Riptide.Utils;
using Server;

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
            _currentTurn = 0;
            _stateMachine.LapTurns = 0;
            _stateMachine.LapBets = new Dictionary<int, TurnType>();
            _stateMachine.IsHideTurn = true;
            _stateMachine.PlayerCheckedCards = false;
            _stateMachine.MinBet = _stateMachine.EnterPrice * 2;
            _stateMachine.HideBet = _stateMachine.EnterPrice * 2;
            
            _cancellationTokenSource = new CancellationTokenSource();
            _task = Task.Run(() => 
            {
                try
                {
                    TurnWait().Wait();
                }
                catch (Exception ex)
                {
                    RiptideLogger.Log(LogType.Error, $"Exception in Wait: {ex}");
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
        
        async Task TurnWait()
        {
            LogInfo($"Turn Wait {_currentTurn}");
            
            
            int seatIndex = _stateMachine.PlaySeats[_currentTurn];
            var seat = _stateMachine.RoomController.Seats[seatIndex];
            //_stateMachine.InfoText.text = $"Хід: {seat.SeatData.Nickname}({seatIndex})";
            
            SendEnterMessage($"Хід {seat.Player.NickName}");
            _stateMachine.RoomController.SendRoomInfoToAll();

            if (_stateMachine.LapBets.TryGetValue(_stateMachine.PlaySeats[_currentTurn], out var turn) &&
                turn == TurnType.Pass | turn == TurnType.AllIn) // Skip if pass
            {
                _stateMachine.LapTurns++;
                Next();
                return;
            }

            if (seat.Balance > 0 ||(_stateMachine.IsHideTurn && seat.Balance >= _stateMachine.HideBet))
            {
                await Task.Delay((int)(_stateMachine.TurnDelay * 1000));
               
                if (_stateMachine.IsHideTurn && seat.Balance < _stateMachine.HideBet)
                {
                    _stateMachine.IsHideTurn = false;
                    _stateMachine.OnSeatCheckCards();
                }

                bool lastTurnOnLap = _currentTurn >= _stateMachine.InGameSeats - 1;
                seat.Turn(_stateMachine.IsHideTurn, _stateMachine.TurnWait, lastTurnOnLap, _stateMachine.MinBet, _stateMachine.HideBet);

                float time = _stateMachine.TurnWait;
                while (time >= 0)
                {
                    
                    time-- ;
                    await Task.Delay((int)(1 * 1000));
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
            
            else if (_stateMachine.IsHideTurn && value == _stateMachine.HideBet)
                OnHideBet(seatIndex);
            
            else if (value == _stateMachine.RoomController.Seats[seatIndex].Balance)
                OnAllIn(seatIndex);
            
            else if (value < _stateMachine.MinBet * 2)
                OnMinBet(seatIndex, value);
            
            else if (value >= _stateMachine.MinBet * 2)
                OnIncrease(seatIndex, value);
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
                        if (!seatController.IsFree &&seatController.Balance >= value)
                        {
                            //LogInfo($"Seat {_stateMachine.Seats[i].name} can bet {value}");
                            canBet++;
                        }
                        else
                        {
                            //LogInfo($"Seat {_stateMachine.Seats[i].name} cant bet {value}. max {playerData.Balance}");
                            if (seatController.Balance > maxValue)
                                maxValue = seatController.Balance;
                        }
                    }
                }
                else
                {
                    if (!seatController.IsFree && seatController.Balance >= value)
                    {
                        //LogInfo($"Seat {_stateMachine.Seats[i].name} can bet {value}");
                        canBet++;
                    }
                    else
                    {
                        //LogInfo($"Seat {_stateMachine.Seats[i].name} cant bet {value}. max {playerData.Balance}");
                        if (seatController.Balance > maxValue)
                            maxValue = seatController.Balance;
                    }
                }
            }
            return canBet;
        }

        void Next()
        {
            LogInfo("Next");

            _cancellationTokenSource.Cancel();

            _currentTurn++;
            if (_stateMachine.PlaySeats.Count > _currentTurn)
            {
                // LogInfo($"Current turn [{_currentTurn}] seatIndex [{_stateMachine.PlaySeats[_currentTurn]}] \n " +
                //         $"Max turn {_stateMachine.PlaySeats.Count - 1} \n " +
                //         $"LapTurns [{_stateMachine.LapTurns}] InGameSeats [{_stateMachine.InGameSeats}] \n " +
                //         $"CanBet {CanBet(_stateMachine.MinBet, out var bet)} Max {bet}");
            }
            else
            {
                // LogInfo($"Current turn [{_currentTurn}] out of seats index \n " +
                //         $"Max turn {_stateMachine.PlaySeats.Count - 1} \n " +
                //         $"LapTurns [{_stateMachine.LapTurns}] InGameSeats [{_stateMachine.InGameSeats}] \n " +
                //         $"CanBet {CanBet(_stateMachine.MinBet, out var bet)} Max {bet}");
            }


            bool allTurn = _stateMachine.LapTurns >= _stateMachine.PlaySeats.Count;
            bool onlyOne = _stateMachine.InGameSeats == 1;
            bool lastTurnOnLap = _currentTurn >= _stateMachine.PlaySeats.Count;
            int haveMoney = 0;
            int canBet = CanBet(_stateMachine.MinBet, out int maxBet);

            for (int i = 0; i < _stateMachine.PlaySeats.Count; i++)
            {
                if (i != _currentTurn - 1)
                {
                    if (_stateMachine.RoomController.Seats[_stateMachine.PlaySeats[i]].Balance > 0)
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

            TurnWait();
        }

        void OnEndLap()
        {
            LogInfo("On end lap");
            
            _cancellationTokenSource.Cancel();
            
            if (CanBet(_stateMachine.MinBet, out var max) > 0)
            {
                int increaseCount = 0;
                foreach (var lapBet in _stateMachine.LapBets)
                {
                    if (lapBet.Value == TurnType.Increase)
                        increaseCount++;
                    
                    if (increaseCount > 1)
                    {
                        _currentTurn = 0;
                        _task = Task.Run(TurnWait, _cancellationTokenSource.Token);
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
            _stateMachine.Actions.Add($"Pass: {_stateMachine.RoomController.Seats[seatIndex]} ({seatIndex})");
            //_stateMachine.PlaySeats.Remove(seatIndex);
            _stateMachine.LapBets[seatIndex] = TurnType.Pass;
            //_stateMachine.BetsData.Bets[seatIndex] = 0;
            
            _stateMachine.PlayerCheckedCards = false;
            
            Next();
        }
        
        private void OnHideBet(int seatIndex)
        {
            _stateMachine.BetsData.Bets[seatIndex] += _stateMachine.HideBet;
            LogInfo($"On hide bet seat: {seatIndex} bet: {_stateMachine.HideBet}. Total bets {_stateMachine.BetsData.Bets[seatIndex]} ");
            
            _stateMachine.LapBets[seatIndex] = TurnType.Hide;
            
             var playIndex = GetPlayIndex(seatIndex);
             ShiftTurnQueue(playIndex);
             

            _stateMachine.RoomController.Seats[seatIndex].Withdraw(_stateMachine.HideBet);
            
            _stateMachine.MinBet = _stateMachine.HideBet;
            _stateMachine.HideBet *= 2;
            


            if (_stateMachine.IsHideTurn && _stateMachine.PlayerCheckedCards)
                OnPlayerCheckCards();
            
            Next();
        }

        private void OnAllIn(int seatIndex)
        {
            var playerData = _stateMachine.RoomController.Seats[seatIndex];
            _stateMachine.BetsData.Bets[seatIndex] += playerData.Balance;
            
            LogInfo($"On all in seat: {seatIndex} balance: {_stateMachine.RoomController.Seats[seatIndex].Balance}. Total bets {_stateMachine.BetsData.Bets[seatIndex]}");

            _stateMachine.LapBets[seatIndex] = TurnType.AllIn;

            if (playerData.Balance > _stateMachine.MinBet)
                _stateMachine.MinBet = playerData.Balance;
            
            _stateMachine.RoomController.Seats[seatIndex].Withdraw(playerData.Balance);
            
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
                _stateMachine.MinBet = bet;

                Next();
            }
            else
            {
                LogInfo("CanBet < 1");
                if (max >= _stateMachine.MinBet * 2) //Если никто не сможет ответить на ставку но ответная ставка может быть Х2
                {
                    LogInfo("Max >= minBet * 2");
                    
                    ShiftTurnQueue(playIndex);
            
                    _stateMachine.RoomController.Seats[seatIndex].Withdraw(max);
                    
                    _stateMachine.LapBets[seatIndex] = TurnType.Increase;

                    _stateMachine.BetsData.Bets[seatIndex] += max;
                    _stateMachine.MinBet = max;

                    Next();
                }
                else //Если никто не может ответить на ставку и ответная ставка не может біть Х2
                {
                    LogInfo("Max < minBet * 2");
                    _stateMachine.RoomController.Seats[seatIndex].Withdraw(max);
                    
                    _stateMachine.LapBets[seatIndex] = TurnType.Normal;

                    _stateMachine.BetsData.Bets[seatIndex] += max;
                    _stateMachine.MinBet = max;

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