﻿using System.Collections;
using Riptide.Utils;

namespace Trink_RiptideServer.Library.StateMachine
{
    
    public class DealState : GameState
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
                    Deal().Wait();
                }
                catch (Exception ex)
                {
                    RiptideLogger.Log(LogType.Error, $"Exception in Wait: {ex}");
                }
            }, _cancellationTokenSource.Token);
            
            SendEnterMessage("Роздача");
        }

        protected override void OnTick()
        {
            
        }

        protected override void OnExit()
        {
            _cancellationTokenSource.Cancel();
        }
        

        

        private async Task Deal()
        {
            //_stateMachine.InfoText.text = "Роздача карт";
            await Task.Delay(1000);
            
            _stateMachine.DealerIndex = GetDealer();
            _stateMachine.PlaySeats = GetDealList();
            
            _stateMachine.CardsHolder.Shuffle();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < _stateMachine.PlaySeats.Count; j++)
                {
                    var card = _stateMachine.CardsHolder.GetCard();
                    var seat = _stateMachine.RoomController.Seats[_stateMachine.PlaySeats[j]];
                    seat.AddCard(i, card);
                    await Task.Delay((int)(_stateMachine.DealDelay * 1000));

                }
                await Task.Delay((int)(_stateMachine.DealDelay * 1000));
            }

            await Task.Delay((int)(_stateMachine.TurnDelay * 1000));
            _stateMachine.SetState<TurnState>();
        }

        int GetDealer()
        {
            if (_stateMachine.DealerIndex == -1)
                return GetRandomDealer();

            if (_stateMachine.RoomController.Seats[_stateMachine.DealerIndex].IsFree)
                return GetNextDealer();

            return _stateMachine.DealerIndex;
        }

        List<int> GetDealList()
        {
            var list = new List<int>();

            int index = _stateMachine.DealerIndex;
            int max = _stateMachine.RoomController.Seats.Length;
            
            while (true)
            {
                index++;
                if (index >= max)
                    index = 0;
                
                if(_stateMachine.PlaySeats.Contains(index))
                    list.Add(index);
                if(list.Count == _stateMachine.PlaySeats.Count - 1)
                    break;
            }
            list.Add(_stateMachine.DealerIndex);
            return list;
        }

        private int GetRandomDealer()
        {
            var random = new Random();
            var r = random.Next(0, _stateMachine.PlaySeats.Count);
            return _stateMachine.PlaySeats[r];
        }
        int GetNextDealer()
        {
            //TODO next dealer
            int current = _stateMachine.DealerIndex;
            var next = current + 1;
            
            return 0;
        }


        public DealState(StateMachine stateMachine) : base(stateMachine)
        {
        }
    }
}