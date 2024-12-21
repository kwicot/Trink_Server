namespace Model
{
    public class StateMachineConfig
    {
        public float StartDelay = 5f * 1000;
        public float DealInterval = 0.25f * 1000;
        public float TurnDelay = 2f * 1000;
        public int AfkTurnsMax = 2;
        public float TurnWait = 20f * 1000;
        public float EndDelay = 2f * 1000;

        public float DebugDelay = 2f * 1000;
    }
}