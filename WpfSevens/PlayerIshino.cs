using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfSevens
{
    public class PlayerIshino : IPlayer
	{
        class CardStatus
        {
            public int Distance { get; set; }
            public int Weight { get; set; }
            public int MyCardCount { get; set; }

            public int CardSpaceCount { get; set; }

            public int IsLastCard { get; set; }
            public int IsNextCard { get; set; }
        }

        private int _PassCount = 0;

        public string GetPalyerName()
        {
            return "石野";
        }

        public string GetPalyerImageName()
        {
            return "ishino.png";
        }

        public Card GetPutCard(IList<Card> playerCards, IList<Card> putCards)
        {
            var random = new Random();
            var cards = Table.GetPutPossibleCards(playerCards, putCards);

            if (cards.Count == 0)
                return Pass();
            else if (playerCards.Count == 1)
                return cards.First();
            else
            {
                Dictionary<Card, CardStatus> cardDictionary = new Dictionary<Card, CardStatus>();

                foreach (var card in cards)
                {
                    cardDictionary.Add(card, new CardStatus
                    {
                        CardSpaceCount = SpaceCardCount(card),
                        MyCardCount = MyCardCount(card, playerCards),
                        IsNextCard = IsNextCard(card, playerCards, putCards),
                        IsLastCard = IsLastCard(card, playerCards, putCards)
                    });
                }

                if (_PassCount < 3)
                {
                    var card = cardDictionary.Where(row => row.Value.IsLastCard == 0).OrderByDescending(row => row.Value.IsNextCard).ThenByDescending(row => row.Value.MyCardCount).ThenBy(row => row.Value.CardSpaceCount).FirstOrDefault();

                    if (card.Key == null)
                    {
                        return Pass();
                    }
                    else
                    {
                        return card.Key;
                    }
                }
                else
                {
                    return cardDictionary.OrderByDescending(row => row.Value.IsNextCard).ThenByDescending(row => row.Value.MyCardCount).ThenBy(row => row.Value.CardSpaceCount).First().Key;
                }
            }
        }

        private Card Pass()
        {
            _PassCount++;
            return default(Card);
        }

        private int SpaceCardCount(Card card)
        {
            if (card.CardNumber > 7)
            {
                return 14 - card.CardNumber;
            }
            else
            {
                return card.CardNumber;
            }

        }

        private int MyCardCount(Card card, IList<Card> playerCards)
        {
            var cards = playerCards.Where(row => row.CardType == card.CardType);

            if (card.CardNumber > 7)
            {
                cards = cards.Where(row => row.CardNumber > 7);
            }
            else
            {
                cards = cards.Where(row => row.CardNumber < 7);
            }

            return cards.Count();
        }

        private int IsLastCard(Card card, IList<Card> playerCards, IList<Card> putCards)
        {
            if (card.CardNumber == 1 || card.CardNumber == 13)
                return 0;

            var cards = playerCards.Where(row => row.CardType == card.CardType).Union(putCards.Where(row => row.CardType == card.CardType));

            if (card.CardNumber > 7)
            {
                cards = cards.Where(row => row.CardNumber > card.CardNumber);
            }
            else
            {
                cards = cards.Where(row => row.CardNumber < card.CardNumber);
            }

            if (cards.Count() == 0)
                return 1;
            else
                return 0;
        }

        private int IsNextCard(Card card, IList<Card> playerCards, IList<Card> putCards)
        {
            var cards = playerCards.Where(row => row.CardType == card.CardType);

            if (card.CardNumber == 1 || card.CardNumber == 13)
                return 1;

            var returnValue = 0;

            if (card.CardNumber > 7)
            {
                returnValue =  playerCards.Any(row => row.CardNumber == card.CardNumber + 1) ? 1 : 0;
            }
            else
            {
                returnValue =  playerCards.Any(row => row.CardNumber == card.CardNumber - 1) ? 1 : 0;
            }

            if (returnValue == 0)
            {
                //ゲームオーバー用のカードも確認
                var localCards = cards.Union(putCards.Where(row => row.CardType == card.CardType));
                if (card.CardNumber > 7)
                {
                    localCards = localCards.Where(row => row.CardNumber > 7);
                }
                else
                {
                    localCards = localCards.Where(row => row.CardNumber < 7);
                }
                if (localCards.Count() == 6)
                {
                    returnValue = 1;
                }

            }

            return returnValue;
        }
    }
}
