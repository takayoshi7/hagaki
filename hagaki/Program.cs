using hagaki.StaticClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace hagaki
{
    internal static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // チェックを実行
            if (StCls_Public.CheckBeforeLaunch(args))
            {
                // チェックが問題ない場合、フォームを起動
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Frm0000_MENU());
            }
            else
            {
                // チェックに失敗した場合はアプリケーションを終了
                Application.Exit();
            }
        }
    }
}
