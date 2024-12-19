using System;
using Core.Scripts;

namespace Trink_RiptideServer.Library.Cards
{
    public class CardModel
    {
        public int Id;
        public CardSuit Suit;
        public CardNumber Number;

        public int Value
        {
            get
            {
                switch (Number)
                {
                    case CardNumber.Six:
                        return 6;
                    case CardNumber.Seven:
                        return 7;
                    case CardNumber.Eight:
                        return 8;
                    case CardNumber.Nine:
                        return 9;
                    case CardNumber.Ten:
                    case CardNumber.Jack:
                    case CardNumber.Queen:
                    case CardNumber.King:
                        return 10;
                    case CardNumber.Ace:
                    case CardNumber.Joker:
                        return 11;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        public string ViewValue
        {
            get
            {
                switch (Number)
                {
                    case CardNumber.Six:
                        return 6.ToString();
                    case CardNumber.Seven:
                        return 7.ToString();
                    case CardNumber.Eight:
                        return 8.ToString();
                    case CardNumber.Nine:
                        return 9.ToString();
                    case CardNumber.Ten:
                        return 10.ToString();
                    case CardNumber.Jack:
                        return "J";
                    case CardNumber.Queen:
                        return "Q";
                    case CardNumber.King:
                        return "K";
                    case CardNumber.Ace:
                        return "A";
                    case CardNumber.Joker:
                        return"Joker";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public bool CombineWith(CardModel cardModel)
        {
            if (cardModel.Suit == Suit)
                return true;
            
            return false;
        }
    }
}