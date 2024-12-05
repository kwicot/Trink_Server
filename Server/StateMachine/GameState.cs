
using Riptide.Utils;

namespace Trink_RiptideServer.Library.StateMachine
{
    public abstract class GameState
    {
        protected StateMachine _stateMachine;

        public GameState(StateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public void Enter()
        {
            LogInfo($"EnterState: {this}");
            OnEnter();
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

        protected abstract void OnEnter();
        protected abstract void OnTick();
        protected abstract void OnExit();

        protected virtual void SendEnterMessage(string text)
        {
            _stateMachine.RoomController.OnEnterState(text);
        }

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
    }
}