using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfSevens
{
    public class Card
    {
        public const int START_CARD_NUMBER = 1;
        public const int END_CARD_NUMBER = 13;
        public enum CardTypeEnum
        {
            Spades = 0,
            Hearts = 1,
            Clubs = 2,
            Diamonds = 3,
        };

        public CardTypeEnum CardType
        {
            get;
            private set;
        }

        private int _CardNumber = 1;
        public int CardNumber
        {
            get
            {
                return _CardNumber;
            }

            private set 
            {
                if (value < START_CARD_NUMBER || value > END_CARD_NUMBER)
                {
                    throw new Exception();
                }
                _CardNumber = value;
            }

        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", this.CardType, this.CardNumber);
        }

        public Card(CardTypeEnum cardType, int cardNumber)
        {
            this.CardType = cardType;
            this.CardNumber = cardNumber;
        }


        public static Card[] GetCards()
        {
            var cardList = new List<Card>();

            foreach (CardTypeEnum cardType in Enum.GetValues(typeof(CardTypeEnum)))
            {
                for (int cardNumber = START_CARD_NUMBER; cardNumber <= END_CARD_NUMBER; cardNumber++)
                {
                    cardList.Add(new Card(cardType, cardNumber));
                }
            }

            return cardList.ToArray();
        }
    }
}
