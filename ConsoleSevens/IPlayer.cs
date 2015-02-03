using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfSevens
{
    public interface IPlayer
    {
        string GetPalyerName();
        string GetPalyerImageName();
        Card GetPutCard(IList<Card> playerCards, IList<Card> putCards);
    }
}
