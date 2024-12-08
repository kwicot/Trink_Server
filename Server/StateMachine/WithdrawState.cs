﻿
using System.Collections.Generic;
using System.Web.Configuration;
using WindowsFormsApp1;

namespace Trink_RiptideServer.Library.StateMachine
{
    public class WithdrawState : GameState
    {
        protected override void OnEnter()
        {
            Tag = $"{_stateMachine.RoomController.Tag}_State_Withdraw";
            Logger.LogInfo(Tag, "Enter");
            
            _stateMachine.BetsData = new BetsData()
            {
                Bets = new Dictionary<int, int>()
            };

            List<int> readySeats = new List<int>();
            var seats = _stateMachine.RoomController.Seats;
            for (int i = 0; i < seats.Length; i++)
            {
                if (seats[i].IsReady && seats[i].SeatData.Balance >= RoomSettings.MinBalance)
                {
                    seats[i].Withdraw(RoomSettings.StartBet);
                    readySeats.Add(i);
                    _stateMachine.BetsData.Bets[i] = RoomSettings.StartBet;
                }
                else
                {
                    seats[i].OfferToTopUpBalance();
                }
            }

            _stateMachine.PlaySeats = readySeats;

            
            _stateMachine.SetState<DealState>();
        }

        protected override void OnTick()
        {
            
        }

        protected override void OnExit()
        {
        }

        public override void Dispose()
        {
            
        }


        public WithdrawState(StateMachine stateMachine) : base(stateMachine)
        {
        }
    }
}