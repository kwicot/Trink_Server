using Core.Scripts;
using Riptide.Utils;

namespace Trink_RiptideServer.Library.Cards
{
    public class CardsHolder
    {
        private static CardModel[] _cards = new[]
            {
                new CardModel() { Id = 0, Number = CardNumber.Six, Suit = CardSuit.Clubs },
                new CardModel() { Id = 1, Number = CardNumber.Seven, Suit = CardSuit.Clubs },
                new CardModel() { Id = 2, Number = CardNumber.Eight, Suit = CardSuit.Clubs },
                new CardModel() { Id = 3, Number = CardNumber.Nine, Suit = CardSuit.Clubs },
                new CardModel() { Id = 4, Number = CardNumber.Ten, Suit = CardSuit.Clubs },
                new CardModel() { Id = 5, Number = CardNumber.Jack, Suit = CardSuit.Clubs },
                new CardModel() { Id = 6, Number = CardNumber.Queen, Suit = CardSuit.Clubs },
                new CardModel() { Id = 7, Number = CardNumber.King, Suit = CardSuit.Clubs },
                new CardModel() { Id = 8, Number = CardNumber.Ace, Suit = CardSuit.Clubs },

                new CardModel() { Id = 9, Number = CardNumber.Six, Suit = CardSuit.Spades },
                new CardModel() { Id = 10, Number = CardNumber.Seven, Suit = CardSuit.Spades },
                new CardModel() { Id = 11, Number = CardNumber.Eight, Suit = CardSuit.Spades },
                new CardModel() { Id = 12, Number = CardNumber.Nine, Suit = CardSuit.Spades },
                new CardModel() { Id = 13, Number = CardNumber.Ten, Suit = CardSuit.Spades },
                new CardModel() { Id = 14, Number = CardNumber.Jack, Suit = CardSuit.Spades },
                new CardModel() { Id = 15, Number = CardNumber.Queen, Suit = CardSuit.Spades },
                new CardModel() { Id = 16, Number = CardNumber.King, Suit = CardSuit.Spades },
                new CardModel() { Id = 17, Number = CardNumber.Ace, Suit = CardSuit.Spades },

                new CardModel() { Id = 18, Number = CardNumber.Six, Suit = CardSuit.Diamonds },
                new CardModel() { Id = 19, Number = CardNumber.Seven, Suit = CardSuit.Diamonds },
                new CardModel() { Id = 20, Number = CardNumber.Eight, Suit = CardSuit.Diamonds },
                new CardModel() { Id = 21, Number = CardNumber.Nine, Suit = CardSuit.Diamonds },
                new CardModel() { Id = 22, Number = CardNumber.Ten, Suit = CardSuit.Diamonds },
                new CardModel() { Id = 23, Number = CardNumber.Jack, Suit = CardSuit.Diamonds },
                new CardModel() { Id = 24, Number = CardNumber.Queen, Suit = CardSuit.Diamonds },
                new CardModel() { Id = 25, Number = CardNumber.King, Suit = CardSuit.Diamonds },
                new CardModel() { Id = 26, Number = CardNumber.Ace, Suit = CardSuit.Diamonds },

                new CardModel() { Id = 27, Number = CardNumber.Six, Suit = CardSuit.Hearts },
                new CardModel() { Id = 28, Number = CardNumber.Seven, Suit = CardSuit.Hearts },
                new CardModel() { Id = 29, Number = CardNumber.Eight, Suit = CardSuit.Hearts },
                new CardModel() { Id = 30, Number = CardNumber.Nine, Suit = CardSuit.Hearts },
                new CardModel() { Id = 31, Number = CardNumber.Ten, Suit = CardSuit.Hearts },
                new CardModel() { Id = 32, Number = CardNumber.Jack, Suit = CardSuit.Hearts },
                new CardModel() { Id = 33, Number = CardNumber.Queen, Suit = CardSuit.Hearts },
                new CardModel() { Id = 34, Number = CardNumber.King, Suit = CardSuit.Hearts },
                new CardModel() { Id = 35, Number = CardNumber.Ace, Suit = CardSuit.Hearts },
            };
        
        public CardModel[] CardModels => _cards;

        public List<int> _shuffledCards = new List<int>();


        public void Shuffle()
        {
            RiptideLogger.Log(LogType.Info, "Shuffle cards");
            List<int> cardsIds = new List<int>();
            _shuffledCards = new List<int>();

            //Fill cards list
            for (int i = 0; i < _cards.Length; i++)
                cardsIds.Add(_cards[i].Id);

            //GetRandom
            Random r = new Random();
            for (int i = 0; i < _cards.Length; i++)
            {
                
                int random = r.Next(0, cardsIds.Count);
                _shuffledCards.Add(cardsIds[random]);
                cardsIds.RemoveAt(random);
            }
        }

        public int GetCard()
        {
            var card = _shuffledCards[0];
            _shuffledCards.RemoveAt(0);
            return card;
        }

        public static int GetCardsSum(List<int> cardIds)
        {
            var cards = new int[cardIds.Count];
            for (int i = 0; i < cardIds.Count; i++)
                cards[i] = cardIds[i];

            return GetCardsSum(cards);
        }

        public static int GetCardsSum(int[] cardIds)
        {
            if (cardIds == null || cardIds.Length != 3)
                return 0;

            var sum = 0;

            CardModel first;
            CardModel second;
            CardModel three;

            int sixCount = 0;
            bool haveSevenSpades = false;
            int aceCount = 0;

            first = GetCard(cardIds[0]);
            second = GetCard(cardIds[1]);
            three = GetCard(cardIds[2]);


            if (first.Number == CardNumber.Six) sixCount++;
            if (second.Number == CardNumber.Six) sixCount++;
            if (three.Number == CardNumber.Six) sixCount++;

            if (first.IsJoker) haveSevenSpades = true;
            if (second.IsJoker) haveSevenSpades = true;
            if (three.IsJoker) haveSevenSpades = true;

            if (first.Number == CardNumber.Ace) aceCount++;
            if (second.Number == CardNumber.Ace) aceCount++;
            if (three.Number == CardNumber.Ace) aceCount++;

            if (sixCount == 3) return 36;
            if (sixCount == 2 & haveSevenSpades) return 36;
            if (sixCount == 2) return 24;
            if (sixCount == 1 & haveSevenSpades) return 24;

            if (aceCount == 3) return 33;
            if (aceCount == 2 & haveSevenSpades) return 33;
            if (aceCount == 2) return 22;
            if (aceCount == 1 & haveSevenSpades) return 22;


            if (first.Suit == second.Suit && first.Suit == three.Suit)
                return first.Value + second.Value + three.Value;

            if (first.Suit == second.Suit)
                return first.Value + second.Value;

            if (first.Suit == three.Suit)
                return first.Value + three.Value;

            if (three.Suit == second.Suit)
                return three.Value + second.Value;

            int a = first.Value;
            int b = second.Value;
            int c = three.Value;

            int max = b;

            if (a > max)
                max = a;

            if (c > max)
                max = c;

            return max;
        }

        int GetSum(List<int> numbers, CardSuit suit)
        {
            if (suit == CardSuit.Spades)
            {
                if (numbers.Contains(6) && numbers.Contains(7))
                {
                    numbers.Remove(6);
                    numbers.Remove(7);
                    numbers.Add(24);
                }
            }

            int sum = 0;
            foreach (var number in numbers)
            {
                sum += number;
            }

            return sum;
        }

        public static CardModel GetCard(int id)
        {
            foreach (var cardModel in _cards)
            {
                if (cardModel.Id == id)
                    return cardModel;
            }

            return null;
        }
    }
}