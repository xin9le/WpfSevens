using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfSevens
{
    public class PlayerSuzuki : IPlayer
	{
        Random random = new Random();

        public string GetPalyerName()
        {
            return "鈴木";
        }

        public string GetPalyerImageName()
        {
            return "suzuki.png";
        }

        public Card GetPutCard(IList<Card> playerCards, IList<Card> putCards)
        {
            var cards = Table.GetPutPossibleCards(playerCards, putCards);

            if (cards.Count == 0)
                return null;
            else
                return cards[random.Next(cards.Count)];
        }
    }
}
