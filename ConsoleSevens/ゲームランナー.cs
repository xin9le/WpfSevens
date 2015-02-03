using System;
using System.Collections.Generic;

namespace 小島
{
    class ゲームランナー
    {
        class スコア
        {
            static Dictionary<string, int> 成績表 = new Dictionary<string, int>();

            public void 勝者登録(string 勝者の名前)
            {
                int 勝った回数 = 0;
                if (成績表.TryGetValue(勝者の名前, out 勝った回数))
                    成績表[勝者の名前]++;
                else
                    成績表[勝者の名前] = 1;
            }

            public override string ToString()
            {
                string result = string.Empty;
                foreach (var pair in 成績表) {
                    if (!string.IsNullOrWhiteSpace(result))
                        result += "\n";
                    result += string.Format("{0}さん - {1}勝", pair.Key, pair.Value);
                }
                return result;
            }
        }

        public ゲームランナー(int 回数, bool サイレントモード = false)
        {
            var ゲーム = new ゲーム(サイレントモード);
            var スコア = new スコア();

            for (var index = 0; index < 回数; index++)
                スコア.勝者登録(ゲーム.実行());

            Console.WriteLine(スコア);
        }
    }
}
