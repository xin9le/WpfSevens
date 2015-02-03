#define Console
#define JustDoIt
//#define Test

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#region 小島以外は見ないでね

#if JustDoIt

using ちゃんとしたプレイヤー = 小島.裏工作をするプレイヤー;

namespace 小島
{
#if !Console
    using System.Windows;
    using System.Windows.Threading;
#endif

    using System.Reflection;
    using WpfSevens;

    static class リフレクター
    {
        public static TResult EvaluateIfNotNull<T, TResult>(this T item, Func<TResult> func)
            where T       : class
            where TResult : class
        { return item == null ? null : func(); }

        public static FieldInfo GetNonPublicInstanceField(this Type type, string fieldName)
        { return type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic); }

#if Console
        static Action action = null;

        public static void DelayInvoke(this Action action)
        { リフレクター.action = action; }

        public static void 遅延処理()
        {
            if (action != null)
                action();
        }
#else
        public static void DelayInvoke(this Action action)
        { Dispatcher.CurrentDispatcher.BeginInvoke(action, DispatcherPriority.Background, null); }
#endif
    }

    static class 偽装工作
    {
        abstract class 偽プレイヤー : IPlayer
        {
            public abstract string GetPalyerName();
            public abstract string GetPalyerImageName();

            public Card GetPutCard(IList<Card> playerCards, IList<Card> putCards)
            { return null; }
        }

        class 偽鈴木 : 偽プレイヤー
        {
            public override string GetPalyerName()
            { return "どうみても本物の鈴木"; }

            public override string GetPalyerImageName()
            { return "suzuki.png"; }
        }

        class 偽石野 : 偽プレイヤー
        {
            public override string GetPalyerName()
            { return "疑いようのない真の石野"; }

            public override string GetPalyerImageName()
            { return "ishino.png"; }
        }

        static IList<IPlayer> GetPlayerList<T>(T ゲーム)
        {
            var playerListFieldInfo = typeof(T).GetNonPublicInstanceField("_PlayerList");
            return playerListFieldInfo.EvaluateIfNotNull(() => playerListFieldInfo.GetValue(ゲーム) as IList<IPlayer>);
        }

        static Table GetTable<T>(T ゲーム)
        {
            var tableFieldInfo = typeof(T).GetNonPublicInstanceField("_Table");
            return tableFieldInfo.EvaluateIfNotNull(() => tableFieldInfo.GetValue(ゲーム) as Table);
        }

        static Dictionary<IPlayer, List<Card>> GetPlayerCard<T>(T ゲーム)
        {
            var playerCardFieldInfo = typeof(Table).GetNonPublicInstanceField("_PlayerCard");
            return playerCardFieldInfo.EvaluateIfNotNull(() => playerCardFieldInfo.GetValue(GetTable<T>(ゲーム)) as Dictionary<IPlayer, List<Card>>);
        }

        static Dictionary<IPlayer, int> GetPlayerPassCount<T>(T ゲーム)
        {
            var playerPassCountFieldInfo = typeof(Table).GetNonPublicInstanceField("_PlayerPassCount");
            return playerPassCountFieldInfo.EvaluateIfNotNull(() => playerPassCountFieldInfo.GetValue(GetTable<T>(ゲーム)) as Dictionary<IPlayer, int>);
        }

        static Dictionary<IPlayer, bool> GetPlayerAlive<T>(T ゲーム)
        {
            var playerAliveFieldInfo = typeof(Table).GetField("_PlayerAlive", BindingFlags.Instance | BindingFlags.NonPublic);
            return playerAliveFieldInfo.EvaluateIfNotNull(() => playerAliveFieldInfo.GetValue(GetTable<T>(ゲーム)) as Dictionary<IPlayer, bool>);
        }

        public static void 開始<T>(T ゲーム)
        {
            Action action = () => 偽装工作.Run(ゲーム);
            action.DelayInvoke();
        }

        static void Run<T>(T ゲーム)
        {
            var playerList = GetPlayerList(ゲーム);
            if (playerList == null)
                return;

            for (var index = 0; index < playerList.Count; index++) {
                var player = playerList[index];
                if (player is PlayerSuzuki)
                    偽プレイヤーの送り込み(ゲーム, playerList, index, new 偽鈴木());
                else if (player is PlayerIshino)
                    偽プレイヤーの送り込み(ゲーム, playerList, index, new 偽石野());
            }
        }

        static void 偽プレイヤーの送り込み<T>(T ゲーム, IList<IPlayer> playerList, int index, IPlayer 偽プレイヤー)
        {
            var 真のプレイヤー = playerList[index];

            偽プレイヤーの送り込み(ゲーム, 偽プレイヤー, 真のプレイヤー);
            playerList[index] = 偽プレイヤー;
        }

        static void 偽プレイヤーの送り込み<T>(T ゲーム, IPlayer 偽プレイヤー, IPlayer 真のプレイヤー)
        {
            var playerCard      = GetPlayerCard(ゲーム);
            var playerPassCount = GetPlayerPassCount(ゲーム);
            var playerAlive     = GetPlayerAlive(ゲーム);
            if (playerCard == null || playerPassCount == null || playerAlive == null)
                return;

            List<Card> card;
            if (playerCard.TryGetValue(真のプレイヤー, out card)) {
                playerCard.Remove(真のプレイヤー);
                playerCard.Add(偽プレイヤー, card);
            }
            int passCount;
            if (playerPassCount.TryGetValue(真のプレイヤー, out passCount)) {
                playerPassCount.Remove(真のプレイヤー);
                playerPassCount.Add(偽プレイヤー, passCount);
            }
            bool alive;
            if (playerAlive.TryGetValue(真のプレイヤー, out alive)) {
                playerAlive.Remove(真のプレイヤー);
                playerAlive.Add(偽プレイヤー, alive);
            }
        }
    }

    abstract class 裏工作をするプレイヤー : IPlayer
    {
        int GetPutCardが呼ばれた回数 { get; set; }

        public abstract string GetPalyerName();
        public abstract string GetPalyerImageName();

#if Console
        static ゲーム ゲーム
        {
            get { return ゲーム.インスタンス; }
        }
#else
        static MainWindow ゲーム
        {
            get { return Application.Current.MainWindow as MainWindow; }
        }
#endif

        public 裏工作をするプレイヤー()
        {
            偽装工作.開始(ゲーム);
            自分のことは棚に上げてイカサマされたときに訴える();
        }

        public Card GetPutCard(IList<Card> playerCards, IList<Card> putCards)
        {
            GetPutCardが呼ばれた回数++;
            return Get_PutCard(playerCards, putCards);
        }

        protected abstract Card Get_PutCard(IList<Card> playerCards, IList<Card> putCards);

        void 自分のことは棚に上げてイカサマされたときに訴える()
        {
#if !Console
            const int 最大のパスの回数 = 3;

            Application.Current.MainWindow.Closing += (sender, e) => {
                if (GetPutCardが呼ばれた回数 < 最大のパスの回数)
                    MessageBox.Show("イカサマだ!!", "或るプレイヤーからの訴え", MessageBoxButton.OK, MessageBoxImage.Warning);
            };
#endif
        }
    }
}

#else

using ちゃんとしたプレイヤー = 小島.不正をしないプレイヤー;

namespace 小島
{
    using WpfSevens;

#if Console
    static class リフレクター
    {
        public static void 遅延処理()
        {}
    }
#endif

    abstract class 不正をしないプレイヤー : IPlayer
    {
        public abstract string GetPalyerName();
        public abstract string GetPalyerImageName();

        public Card GetPutCard(IList<Card> playerCards, IList<Card> putCards)
        { return Get_PutCard(playerCards, putCards); }

        protected abstract Card Get_PutCard(IList<Card> playerCards, IList<Card> putCards);
    }
}

#endif

#endregion // 小島以外は見ないでね

namespace 小島
{
    using WpfSevens;

    static class 戦略
    {
        public abstract class 俺の回
        {
            class 手
            {
                public Card 札     { get; set; }
                public int  評価点 { get; set; }

                public bool 有効
                {
                    get { return 札 != null; }
                }
            }

            readonly IList<Card> 手札    ;
            readonly IList<Card> 出せる札;

            public 俺の回(IList<Card> 手札, IList<Card> 場札)
            {
                this.手札 = 手札;
                出せる札  = Table.GetPutPossibleCards(手札, 場札);
            }

            public Card 出す札(bool パス可能)
            {
                var 最善手 = this.最善手;
                return 最善手 == null || (パス可能 && 最善手.評価点 != パスできてもしない評価点())
                       ? null
                       : 最善手.札;
            }

            手 最善手
            {
                get
                {
                    var 最善手 = new 手 { 評価点 = int.MinValue };
                    foreach (var 札 in 出せる札) {
                        var 評価点 = 評価(札);
                        if (評価点 > 最善手.評価点) {
                            最善手.札 = 札;
                            最善手.評価点 = 評価点;
                        }
                    }
                    return 最善手.有効 ? 最善手 : null;
                }
            }

            protected abstract int 評価(Card 札);
            protected abstract int パスできてもしない評価点();

            protected bool 持ってる(Card 札)
            { return 手札.Any(手札の中の一枚 => 手札の中の一枚.同じ(札)); }

            protected bool 後一枚()
            { return 手札.Count == 1; }
        }

        public static Card 次(this Card 札)
        {
            return (札 == null || 札.CardNumber == 1 || 札.CardNumber == 7 || 札.CardNumber == 13)
                    ? null
                    : new Card(札.CardType, 札.CardNumber < 7 ? 札.CardNumber - 1 : 札.CardNumber + 1);
        }

        public static bool 同じ(this Card @this, Card card)
        { return @this != null && card != null && @this.CardType == card.CardType && @this.CardNumber == card.CardNumber; }

        [Conditional("Test")]
        public static void テスト()
        {
            Debug.Assert(new Card(Card.CardTypeEnum.Clubs   ,  3)     .同じ(new Card(Card.CardTypeEnum.Clubs   ,  3)));
            Debug.Assert(new Card(Card.CardTypeEnum.Diamonds,  6).次().同じ(new Card(Card.CardTypeEnum.Diamonds,  5)));
            Debug.Assert(new Card(Card.CardTypeEnum.Spades  , 12).次().同じ(new Card(Card.CardTypeEnum.Spades  , 13)));
            Debug.Assert(new Card(Card.CardTypeEnum.Spades  ,  7).次() == null);
            Debug.Assert(new Card(Card.CardTypeEnum.Clubs   ,  1).次() == null);
            Debug.Assert(new Card(Card.CardTypeEnum.Hearts  , 13).次() == null);
        }
    }

    static class 戦略その1
    {
        class 俺の回 : 戦略.俺の回
        {
            const int 最高評価点                                       =         10;
            const int 最後の札                                         = 最高評価点;
            const int 次などない                                       = 最高評価点;
            const int 次を持ってる                                     = 最高評価点;
            const int 次を持ってない場合に持ってる続くやつ一枚あたり   =          1;
            const int 次を持ってない場合に持ってない続くやつ一枚あたり =         -1;

            public 俺の回(IList<Card> 手札, IList<Card> 場札) : base(手札, 場札)
            {}

            protected override int 評価(Card 札)
            {
                if (後一枚())
                    return 最後の札;

                var 次のカード = 札.次();
                if (次のカード == null)
                    return 次などない;

                if (持ってる(次のカード))
                    return 次を持ってる;

                var 評価点 = 0;
                for (var 評価するカード = 次のカード; 評価するカード != null; 評価するカード = 評価するカード.次())
                    評価点 += 持ってる(評価するカード) ? 次を持ってない場合に持ってる続くやつ一枚あたり
                                                       : 次を持ってない場合に持ってない続くやつ一枚あたり;
                return 評価点;
            }

            protected override int パスできてもしない評価点()
            { return 最高評価点; }
        }

        public static Card 出す札(IList<Card> 手札, IList<Card> 場札, bool パス可能)
        { return new 俺の回(手札, 場札).出す札(パス可能); }
    }

    static class 戦略その2
    {
        class 俺の回 : 戦略.俺の回
        {
            const int 基礎評価点                       =  20;
            const int 札は最後の札                     = 150;
            const int 次以降の札を持ってる場合の基礎点 = 札は最後の札 - 基礎評価点;

            public 俺の回(IList<Card> 手札, IList<Card> 場札) : base(手札, 場札)
            {}

            protected override int 評価(Card 札)
            {
                return 後一枚()
                       ? 札は最後の札
                       : Math.Max(基礎評価(札), 次以降の手札の評価(札));
            }

            protected override int パスできてもしない評価点()
            { return 次以降の札を持ってる場合の基礎点; }

            int 次以降の手札の評価(Card 札)
            {
                return 次以降の札を持ってる場合の基礎点
                       - 基礎評価点 * 次まで何枚あいてるか(札)
                       + 次から何枚続けて持ってるか(札);
            }

            int 次まで何枚あいてるか(Card 札)
            {
                var 次までにあいてる数 = 0;
                for (var 次の札 = 札.次(); 次の札 != null; 次の札 = 次の札.次()) {
                    if (持ってる(次の札))
                        return 次までにあいてる数;
                    else
                        次までにあいてる数++;
                }
                return 7;
            }

            int 次から何枚続けて持ってるか(Card 札)
            {
                var 次から続けて持ってる数 = 0;
                for (var 次の札 = 札.次(); 持ってる(次の札); 次の札 = 次の札.次())
                    次から続けて持ってる数++;
                return 次から続けて持ってる数;
            }

            int 次から何枚持ってるか(Card 札)
            {
                var 次から持ってる数 = 0;
                for (var 次の札 = 札.次(); 次の札 != null; 次の札 = 次の札.次()) {
                    if (持ってる(次の札))
                        次から持ってる数++;
                }
                return 次から持ってる数;
            }

            static int 基礎評価(Card 札)
            {
                var 評価点 = 基礎評価点 * 札.中央の7からの距離();
                return 札.はAかK() ? 評価点 + 基礎評価点 : 評価点;
            }

            [Conditional("Test")]
            public static void テスト()
            {
                var 手札 = new List<Card> {
                    new Card(Card.CardTypeEnum.Spades  ,  2),
                    new Card(Card.CardTypeEnum.Spades  ,  3),
                    new Card(Card.CardTypeEnum.Spades  ,  4),
                    new Card(Card.CardTypeEnum.Spades  ,  5),
                    new Card(Card.CardTypeEnum.Clubs   ,  8),
                    new Card(Card.CardTypeEnum.Clubs   , 10),
                    new Card(Card.CardTypeEnum.Clubs   , 11),
                    new Card(Card.CardTypeEnum.Diamonds,  9),
                    new Card(Card.CardTypeEnum.Diamonds, 13)
                };
                var 場札 = new List<Card> {
                    new Card(Card.CardTypeEnum.Spades  , 7),
                    new Card(Card.CardTypeEnum.Hearts  , 7),
                    new Card(Card.CardTypeEnum.Clubs   , 7),
                    new Card(Card.CardTypeEnum.Diamonds, 7)
                };

                var 俺の回 = new 俺の回(手札, 場札);

                Debug.Assert( 俺の回.持ってる(new Card(Card.CardTypeEnum.Clubs, 10)));
                Debug.Assert(!俺の回.持ってる(new Card(Card.CardTypeEnum.Clubs,  9)));
                Debug.Assert(俺の回.次から何枚続けて持ってるか(new Card(Card.CardTypeEnum.Spades  , 5)) == 3);
                Debug.Assert(俺の回.次まで何枚あいてるか      (new Card(Card.CardTypeEnum.Spades  , 4)) == 0);
                Debug.Assert(俺の回.次まで何枚あいてるか      (new Card(Card.CardTypeEnum.Clubs   , 8)) == 1);
                Debug.Assert(俺の回.次まで何枚あいてるか      (new Card(Card.CardTypeEnum.Diamonds, 9)) == 3);
                Debug.Assert(俺の回.次から何枚持ってるか      (new Card(Card.CardTypeEnum.Clubs   , 8)) == 2);

                var 評価点 = 俺の回.評価(new Card(Card.CardTypeEnum.Spades  ,  2));
                評価点     = 俺の回.評価(new Card(Card.CardTypeEnum.Spades  ,  3));
                評価点     = 俺の回.評価(new Card(Card.CardTypeEnum.Spades  ,  4));
                評価点     = 俺の回.評価(new Card(Card.CardTypeEnum.Spades  ,  5));
                評価点     = 俺の回.評価(new Card(Card.CardTypeEnum.Clubs   ,  8));
                評価点     = 俺の回.評価(new Card(Card.CardTypeEnum.Clubs   , 10));
                評価点     = 俺の回.評価(new Card(Card.CardTypeEnum.Clubs   , 11));
                評価点     = 俺の回.評価(new Card(Card.CardTypeEnum.Diamonds,  9));
                評価点     = 俺の回.評価(new Card(Card.CardTypeEnum.Diamonds, 13));
            }
        }

        public static Card 出す札(IList<Card> 手札, IList<Card> 場札, bool パス可能)
        { return new 俺の回(手札, 場札).出す札(パス可能); }

        static bool はAかK(this Card 札)
        { return 札.CardNumber == 1 || 札.CardNumber == 13; }

        static int 中央の7からの距離(this Card 札)
        { return Math.Abs(札.CardNumber - 7); }

        [Conditional("Test")]
        public static void テスト()
        {
            俺の回.テスト();
        }
    }
}

namespace WpfSevens {
    class PlayerKojima : ちゃんとしたプレイヤー
    {
        const int 最大のパスの回数 = 3;

        int パスの回数 { get; set; }

        bool パス可能
        {
            get { return パスの回数 < 最大のパスの回数; }
        }

        bool パス()
        {
            if (パス可能) {
                パスの回数++;
                return true;
            }
            return false;
        }

        public override string GetPalyerName()
        { return "小島"; }

        public override string GetPalyerImageName()
        { return "kojima.png"; }

        protected override Card Get_PutCard(IList<Card> 手札, IList<Card> 場札)
        {
            var 出す札 = 小島.戦略その2.出す札(手札, 場札, パス可能);
            if (出す札 == null)
                パス();
            return 出す札;

            //var random = new Random();
            //var cards = Table.GetPutPossibleCards(手札, 場札);
            //return cards.Count == 0 ? null
            //                        : cards[random.Next(cards.Count)];
        }

#if Test
        public PlayerKojima()
        {
            小島.戦略     .テスト();
            小島.戦略その2.テスト();
        }
#endif
    }
}
