using System;
using System.Collections.Generic;
using System.Linq;
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
                new CardModel() { Id = 10, Number = CardNumber.Joker, Suit = CardSuit.Joker },
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

            var cards = new List<CardModel> { GetCard(cardIds[0]), GetCard(cardIds[1]), GetCard(cardIds[2]) };

            int sixCount = cards.Count(card => card.Number == CardNumber.Six);
            int aceCount = cards.Count(card => card.Number == CardNumber.Ace);
            bool haveJoker = cards.Any(card => card.Number == CardNumber.Joker);

            // Обработка комбинаций из трёх шестёрок
            if (sixCount == 3 || (sixCount == 2 && haveJoker))
                return 36;

            // Обработка комбинаций из двух шестёрок
            if (sixCount == 2)
                return 24;

            // Обработка одной шестёрки и джокера
            if (sixCount == 1 && haveJoker)
            {
                var sixCard = cards.First(card => card.Number == CardNumber.Six);
                var otherCards = cards.Where(card => card != sixCard && card.Number != CardNumber.Joker).ToList();

                if (otherCards.Count == 1 && otherCards[0].Suit == sixCard.Suit)
                    return 17 + otherCards[0].Value;
                
                return 24;
            }

            // Обработка трёх тузов
            if (aceCount == 3 || (aceCount == 2 && haveJoker))
                return 33;

            // Обработка двух тузов
            if (aceCount == 2)
                return 22;

            // Обработка одного туза и джокера
            if (aceCount == 1 && haveJoker)
            {
                var aceCard = cards.First(card => card.Number == CardNumber.Ace);
                var otherCards = cards.Where(card => card != aceCard && card.Number != CardNumber.Joker).ToList();

                if (otherCards.Count == 1 && otherCards[0].Suit == aceCard.Suit)
                    return 22 + otherCards[0].Value;
                return 22;
            }

            
            // Проверка на карты одной масти и суммирование
            var groupedBySuit = cards.GroupBy(card => card.Suit).Where(g => g.Count() > 1);
            if (groupedBySuit.Any())
            {
                var bestGroup = groupedBySuit.OrderByDescending(g => g.Sum(card => card.Value)).First();
                var sum = bestGroup.Sum(card => card.Value);

                if (haveJoker)
                    sum += 11; // Джокер дополняет комбинацию

                return sum;
            }

            // Если карты не могут быть объединены, найти максимальную карту
            int maxCardValue = cards.Where(card => card.Number != CardNumber.Joker).Max(card => card.Value);

            if (haveJoker)
                return maxCardValue + 11; // Джокер создаёт комбинацию с наивысшей картой


            return maxCardValue;
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