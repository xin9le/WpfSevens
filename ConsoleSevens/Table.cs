using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfSevens
{
    public class Table
    {
        private const int MAX_PASS = 3;

        private List<Card> _CardList = new List<Card>();
        private List<Card> _PutCardList = new List<Card>();
        private int _PlayerTurnIndex = 0;
        private bool _IsGameEnd = false;

        private Dictionary<IPlayer, List<Card>> _PlayerCard = new Dictionary<IPlayer, List<Card>>();
        private Dictionary<IPlayer, int> _PlayerPassCount = new Dictionary<IPlayer, int>();
        private Dictionary<IPlayer, bool> _PlayerAlive = new Dictionary<IPlayer, bool>(); 

        public Table()
        {

        }

        public bool IsGameEnd
        {
            get
            {
                return _IsGameEnd;
            }
        }

        public void GameStart(IList<IPlayer> playerList)
        {
            ShuffleCard();

            _PlayerTurnIndex = 0;
            foreach (var player in playerList)
            {
                _PlayerCard.Add(player, new List<Card>());
                _PlayerPassCount.Add(player, 0);
                _PlayerAlive.Add(player, true);
            }

            var playerIndex = 0;
            foreach (var card in _CardList)
            {
                _PlayerCard[playerList[playerIndex]].Add(card);
                playerIndex++;
                if (playerList.Count <= playerIndex)
                    playerIndex = 0;
            }
            foreach (var player in playerList)
            {
                _PlayerCard[player] = _PlayerCard[player].OrderBy(row => row.CardType).ThenBy(row => row.CardNumber).ToList();
            }
        }

        public IPlayer GetPlayer()
        {
            return _PlayerCard.Keys.ToArray()[_PlayerTurnIndex];
        }

        public void CheckStartPlayer()
        {
            for (var index = 0; index < _PlayerCard.Keys.Count; index++)
            {
                var player = _PlayerCard.Keys.ToArray()[index];
                var cardList = _PlayerCard[player].Where(row => row.CardNumber == 7);

                if (cardList.Any(row => row.CardType == Card.CardTypeEnum.Diamonds))
                {
                    _PlayerTurnIndex = index;
                }
                _PutCardList.AddRange(cardList);
                foreach (var card in cardList.ToList())
                {
                    _PlayerCard[player].Remove(card);
                }

            }
        }

        public Card Trun(ref string message)
        {
            var player = GetPlayer();
            var card = player.GetPutCard(_PlayerCard[player], _PutCardList);

            if (card == null)
            {
                _PlayerPassCount[player]++;
                if (_PlayerPassCount[player] > MAX_PASS)
                {
                    _PlayerAlive[player] = false;
                    message = string.Format("{0}さんは、パスが{1}回になりました。負けです。", player.GetPalyerName(), _PlayerPassCount[player]);
                    _PutCardList.AddRange(_PlayerCard[player]);
                    _PlayerCard[player].Clear();
                }
                else
                {
                    message = string.Format("{0}さんは、パスです。{1}回目です。", player.GetPalyerName(), _PlayerPassCount[player]);
                }
            }
            else
            {
                _PutCardList.Add(card);
                _PlayerCard[player].Remove(card);

                //勝敗チェック
                if (_PlayerCard[player].Count == 0)
                {
                    message = string.Format("{0}さんの勝ちです。", player.GetPalyerName());
                    _IsGameEnd = true;
                }

            }

            if (!_IsGameEnd)
            {
                //次のターン
                do
                {
                    _PlayerTurnIndex++;
                    if (_PlayerTurnIndex >= _PlayerAlive.Count)
                    {
                        _PlayerTurnIndex = 0;
                    }
                }
                while (!_PlayerAlive[GetPlayer()]) ;
            }

            return card;
        }

 

        private void ShuffleCard()
        {
            _CardList.Clear();
            _PutCardList.Clear();
            _CardList.AddRange(Card.GetCards());

            var random = new Random();

            for (var count = 0; count < _CardList.Count * 3; count++)
            {
                var card = _CardList[random.Next(_CardList.Count)];
                _CardList.Remove(card);

                if (count % 2 == 0)
                {
                    _CardList.Insert(0, card);
                }
                else
                {
                    _CardList.Add(card);
                }
            }

        }

        public IList<Card> GetPlayerCards(IPlayer player)
        {
            return _PlayerCard[player];
        }

        public IList<Card> GetPutCards()
        {
            return _PutCardList;
        }

        public IList<Card> GetAllCards()
        {
            return _CardList;
        }

        public static IList<Card> GetPutPossibleCards(IList<Card> playerCards, IList<Card> putCards)
        {
            var returnValue = new List<Card>();

            foreach (Card.CardTypeEnum cardType in Enum.GetValues(typeof(Card.CardTypeEnum)))
            {
                for (var index = 7; index >= Card.START_CARD_NUMBER; index--)
                {
                    if (!putCards.Where(row => row.CardType == cardType && row.CardNumber == index).Any())
                    {
                        var card = playerCards.Where(row => row.CardType == cardType && row.CardNumber == index).FirstOrDefault();
                        if (card != null)
                        { 
                            returnValue.Add(card);
                        }
                        break;
                    }
                }

                for (var index = 7; index <= Card.END_CARD_NUMBER; index++)
                {
                    if (!putCards.Where(row => row.CardType == cardType && row.CardNumber == index).Any())
                    {
                        var card = playerCards.Where(row => row.CardType == cardType && row.CardNumber == index).FirstOrDefault();
                        if (card != null)
                        {
                            returnValue.Add(card);
                        }
                        break;
                    }
                }
            }

            return returnValue;
        }
    }
}
