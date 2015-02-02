using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;



namespace WpfSevens
{
    /// <summary>
    /// 鈴木が考えたAIを提供します。
    /// </summary>
    public class PlayerSuzuki : IPlayer
	{
        #region 内部クラス
        /// <summary>
        /// 手持ちのカード情報を表します。
        /// </summary>
        private class RestCard
        {
            #region プロパティ
            /// <summary>
            /// カードを取得します。
            /// </summary>
            public Card Card { get; private set; }


            /// <summary>
            /// カード番号を取得します。
            /// </summary>
            public int Number { get { return this.Card.CardNumber; } }


            /// <summary>
            /// カードの柄を取得します。
            /// </summary>
            public Card.CardTypeEnum Mark { get { return this.Card.CardType; } }


            /// <summary>
            /// 先頭の配置可能なカードかどうかを取得します。
            /// </summary>
            public bool IsHead { get; private set; }


            /// <summary>
            /// 末尾のカードかどうかを取得します。
            /// </summary>
            public bool IsTail { get; private set; }


            /// <summary>
            /// 配置可能かどうかを取得します。
            /// </summary>
            public bool CanPut { get; private set; }


            /// <summary>
            /// 中央からのカードの距離を取得します。
            /// </summary>
            public int Distance { get { return Math.Abs(this.Number - 7); } }
            #endregion


            #region コンストラクタ
            /// <summary>
            /// インスタンスを生成します。
            /// </summary>
            /// <param name="card">カード</param>
            /// <param name="isHead">先頭のカード</param>
            /// <param name="isTail">末尾のカード</param>
            /// <param name="canPut">配置可能か</param>
            public RestCard(Card card, bool isHead, bool isTail, bool canPut)
            {
                this.Card = card;
                this.IsHead = isHead;
                this.IsTail = isTail;
                this.CanPut = canPut;
            }
            #endregion
        }


        /// <summary>
        /// 手持ちのカードをマークと大小でグループ化したものを表します。
        /// </summary>
        private class RestCardGroup : IReadOnlyCollection<RestCard>
        {
            #region フィールド
            /// <summary>
            /// カードを保持します。
            /// </summary>
            private readonly IReadOnlyList<RestCard> cards;
            #endregion


            #region プロパティ
            /// <summary>
            /// カードの区分を取得します。
            /// </summary>
            public NumberSection NumberSection
            {
                get
                {
                    return  this.cards.First().Number > 7
                        ?   NumberSection.GreaterThan7
                        :   NumberSection.LessThan7;
                }
            }


            /// <summary>
            /// カードの柄を取得します。
            /// </summary>
            public Card.CardTypeEnum Mark { get { return this.cards.First().Mark; } }


            /// <summary>
            /// すべてのカードが連続しているかどうかを取得します。
            /// </summary>
            public bool IsContinuous
            {
                get
                {
                    return  this.cards.Zip
                            (
                                this.cards.Skip(1),
                                (c1, c2) => Math.Abs(c1.Number - c2.Number) == 1
                            )
                            .All(x => x);
                }
            }


            /// <summary>
            /// 最初のカードの番号を取得します。
            /// </summary>
            public int HeadNumber { get { return this.cards.First(x => x.IsHead).Number; } }


            /// <summary>
            /// 最後のカードの番号を取得します。
            /// </summary>
            public int TailNumber { get { return this.cards.Last(x => x.IsTail).Number; } }


            /// <summary>
            /// 配置可能なカードがあるかどうかを取得します。
            /// </summary>
            public bool HasPuttable { get { return this.cards.Any(x => x.CanPut); } }


            /// <summary>
            /// 配置可能なカードを取得します。
            /// </summary>
            public RestCard PuttableCard { get { return this.cards.FirstOrDefault(x => x.CanPut); } }
            #endregion


            #region コンストラクタ
            /// <summary>
            /// インスタンスを生成します。
            /// </summary>
            /// <param name="cards">カードのコレクション</param>
            private RestCardGroup(IReadOnlyList<RestCard> cards)
            {
                this.cards = cards;
            }
            #endregion


            #region 生成
            /// <summary>
            /// 手持ちカードグループのコレクションを生成します。
            /// </summary>
            /// <param name="playerCards">プレイヤーのカードのコレクション</param>
            /// <param name="putCards">カードテーブルに配置されているカードのコレクション</param>
            /// <returns>グループのコレクション</returns>
            public static IReadOnlyCollection<RestCardGroup> From(IList<Card> playerCards, IList<Card> putCards)
            {
                const int centerNumber = 7;
                var puttableCards   = Table.GetPutPossibleCards(playerCards, putCards)
                                    .ToLookup(x => x.CardType)
                                    .ToDictionary(x => x.Key, x => x.ToDictionary(y => y.CardNumber));
                return  playerCards
                        .GroupBy(x => x.CardType)
                        .SelectMany(x => new []
                        {
                            x.Where(y => y.CardNumber < centerNumber).OrderByDescending(y => y.CardNumber),
                            x.Where(y => y.CardNumber > centerNumber).OrderBy(y => y.CardNumber),
                        })
                        .Select(x => x.ToArray())
                        .Where(x => x.Any())
                        .Select(x =>
                        {
                            var result = new List<RestCard>();
                            for (int i = 0; i < x.Length; i++)
                            {
                                var card = x[i];
                                result.Add(new RestCard
                                (
                                    card,
                                    i == 0,
                                    i == x.Length - 1,
                                    puttableCards.ContainsKey(card.CardType) && puttableCards[card.CardType].ContainsKey(card.CardNumber)
                                ));
                            }
                            return new RestCardGroup(result);
                        })
                        .ToArray();
            }
            #endregion


            #region IReadOnlyCollection<T> メンバー
            /// <summary>
            /// カードの数を取得します。
            /// </summary>
            public int Count { get { return this.cards.Count; } }
            #endregion


            #region IEnumerable<T> メンバー
            /// <summary>
            /// 列挙子を取得します。
            /// </summary>
            /// <returns>列挙子</returns>
            public IEnumerator<RestCard> GetEnumerator()
            {
                return this.cards.GetEnumerator();
            }
            #endregion


            #region IEnumerable メンバー
            /// <summary>
            /// 列挙子を取得します。
            /// </summary>
            /// <returns>列挙子</returns>
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
            #endregion
        }


        /// <summary>
        /// カード番号の区分を表します。
        /// </summary>
        private enum NumberSection
        {
            /// <summary>
            /// 7より大きい数字
            /// </summary>
            GreaterThan7 = 0,

            /// <summary>
            /// 7より小さい数字
            /// </summary>
            LessThan7,
        }
        #endregion


        #region IPlayer メンバー
        /// <summary>
        /// プレイヤーの名前を取得します。
        /// </summary>
        /// <returns>プレイヤーの名前</returns>
        public string GetPalyerName()
        {
            return "鈴木";
        }


        /// <summary>
        /// プレイヤーの画像ファイル名を取得します。
        /// </summary>
        /// <returns>プレイやの画像ファイル名</returns>
        public string GetPalyerImageName()
        {
            return "suzuki.png";
        }


        /// <summary>
        /// 配置するカードを取得します。
        /// </summary>
        /// <param name="playerCards">プレイヤーのカードのコレクション</param>
        /// <param name="putCards">カードテーブルに配置されているカードのコレクション</param>
        /// <returns>配置するカード。nullの場合はパス。</returns>
        public Card GetPutCard(IList<Card> playerCards, IList<Card> putCards)
        {
            var result  = RestCardGroup.From(playerCards, putCards)     //--- カードの柄と大小で8グループに分割
                        .Where(x => x.HasPuttable)                      //--- 配置可能なもののみ
                        .OrderBy(x => x.PuttableCard.IsTail)            //--- 配置可能なカードが末尾カードであれば後回し
                        .ThenByDescending(x => x.Max(y => y.Distance))  //--- 末尾カードの距離が遠いものを先に処理する
                        .ThenBy(x => x.IsContinuous)                    //--- 連続していないものが優先
                        .SelectMany(x => x)
                        .FirstOrDefault(x => x.CanPut);
            if (result == null)
                return null;  //--- 配置できるものがない

            const int maxPassDistance = 5;
            const int maxPassCount = 3;
            if (playerCards.Count > 1)
            if (result.IsTail)
            if (result.Distance <= maxPassDistance)
            if (this.PassCount() < maxPassCount)
                return null;    //--- 戦略的パス

            return result.Card;
        }
        #endregion


        #region 補助
        /// <summary>
        /// 現在のパスの回数を取得します。
        /// </summary>
        /// <returns>パスの回数</returns>
        private int PassCount()
        {
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var table = typeof(MainWindow).GetField("_Table", flags).GetValue(App.Current.MainWindow) as Table;
            var passCounts = typeof(Table).GetField("_PlayerPassCount", flags).GetValue(table) as IDictionary<IPlayer, int>;
            return passCounts[this];
        }
        #endregion
    }
}