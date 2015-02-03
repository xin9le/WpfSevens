using System;
using System.Collections.Generic;
using System.Linq;
using WpfSevens;

namespace 小島
{
    class ゲーム
    {
        Table         _Table      = null;
        List<IPlayer> _PlayerList = null;

        string        message     = string.Empty;
        readonly bool サイレントモード;

        public static ゲーム インスタンス { get; set; }

        public ゲーム(bool サイレントモード = false)
        {
            this.サイレントモード = サイレントモード;
            インスタンス          = this;
        }

        public string 実行()
        {
            初期化();
            while (ターン())
                ;
            return 勝者の名前();
        }

        Dictionary<IPlayer, int> 成績 = new Dictionary<IPlayer, int>();

        void 初期化()
        {
            _Table      = new Table();

            _PlayerList = new List<IPlayer>();
            _PlayerList.Add(new PlayerSuzuki());
            _PlayerList.Add(new PlayerKojima());
            _PlayerList.Add(new PlayerIshino());

            _Table.GameStart(_PlayerList);
            _Table.CheckStartPlayer();

            リフレクター.遅延処理();
        }

        bool ターン()
        {
            var player = _Table.GetPlayer();
            var card   = _Table.Trun(ref message);
            出力(player, card);
            return !_Table.IsGameEnd;
        }

        void 出力(IPlayer player, Card card)
        {
            if (サイレントモード)
                return;
            Console.WriteLine("{0}さん: {1}", player.GetPalyerName(), card == null ? "パス" : card.ToString());

            if (!string.IsNullOrEmpty(message))
                Console.WriteLine(message);
        }

        string 勝者の名前()
        {
            var winner = _PlayerList.FirstOrDefault(player => message.Contains(player.GetPalyerName()));
            return winner == null ? string.Empty : winner.GetPalyerName();
        }
    }
}
