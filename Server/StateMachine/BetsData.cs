
namespace Trink_RiptideServer
{
    [Serializable]
    public class BetsData
    {
        public Dictionary<int, int> Bets = new();
        
        public int TotalBank
        {
            get
            {
                int value = 0;
                if (Bets != null)
                {
                    foreach (var bet in Bets)
                    {
                        value += bet.Value;
                    }
                }

                return value;
            }
        }
        
        public void Remove(int index)
        {
            var playerBet = Bets[index];

            for (int i = 0; i < 20; i++)
            {
                if (Bets.TryGetValue(i, out var bets))
                {
                    Bets[i] -= playerBet;
                }
            }
            
        }

        public int PlayerWin(int index)
        {
            int count = 0;
            foreach (var bet in Bets)
            {
                if (bet.Value > 0)
                    count++;
            }
            return Bets[index] * count;            
        }
    }
}