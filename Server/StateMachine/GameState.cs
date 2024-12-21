
using System;
using System.Threading.Tasks;
using Model;
using Riptide.Utils;
using WindowsFormsApp1;
using WindowsFormsApp1.Room;
using LogType = Riptide.Utils.LogType;

namespace Trink_RiptideServer.Library.StateMachine
{
    public abstract class GameState : IDisposable
    {
        protected StateMachine _stateMachine;
        protected StateMachineConfig Config => Program.Config.StateMachineConfig;
        protected RoomSettings RoomSettings => _stateMachine.RoomController.RoomInfo.RoomSettings;
        public string Tag { get; protected set; }
        
        protected bool WaitingEnd = false;


        public GameState(StateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public void Enter()
        {
            LogInfo($"EnterState: {this}");
            OnEnter();
            WaitingEnd = false;
        }

        public void Tick()
        {
            OnTick();
        }

        public void Exit()
        {
            LogInfo($"ExitState: {this}");
            OnExit();
        }

        public async Task ProcessServerStopping()
        {
            WaitingEnd = true;
            while (WaitingEnd)
            {
                await Task.Delay(1000);
            }
        }

        protected abstract void OnEnter();
        protected abstract void OnTick();
        protected abstract void OnExit();

        protected virtual void Log(string str)
        {
            RiptideLogger.Log(LogType.Debug, str);
        }
        
        protected virtual void LogInfo(string str)
        {
            RiptideLogger.Log(LogType.Info, str);
        }
        
        protected virtual void LogWarning(string str)
        {
            RiptideLogger.Log(LogType.Warning, str);
        }
        
        protected virtual void LogError(string str)
        {
            RiptideLogger.Log(LogType.Error, str);
        }

        public abstract void Dispose();
    }
}