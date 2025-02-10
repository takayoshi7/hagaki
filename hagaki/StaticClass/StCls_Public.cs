using hagaki.Class;
using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

// ---------------------------------------------
//  クラス名   : StCls_Public
//  概要　　　 : エントリポイント
//  作成日　　 : 2025/02/05
//  作成者　　 : 高橋
//  最終更新日 : 
//  最終更新者 : 
// ---------------------------------------------

namespace hagaki.StaticClass
{
    public static class StCls_Public
    {
        public static string pb_XmlPath = string.Empty;               // Xmlファイルパス
        public static string pb_XmlFilePath = string.Empty;           // Xmlファイルフルパス（ファイル名込み）
        private static string Xml_NAME = "KensyuSys.xml";             // Xmlファイル名

        public static bool pb_SQLLogFlg;                              // SQLログの出力フラグ
        public static string pb_SQLLogPath;                           // SQLログの出力パス
        public static bool pb_SysErrLogFlg;                           // システムエラーログの出力フラグ
        public static string pb_SysErrLogPath;                        // システムエラーログの出力パス
        public static bool pb_WorkLogFlg;                             // 作業ログ出力フラグ
        public static string pb_WorkLogPath;                          // 作業ログ出力パス

        public static Cls_FormColor pb_cForm;                         // Cls_FormColor
        public static long pb_form_color;                             // フォーム背景色
        public static long pb_fore_color;                             // フォーム文字色

        // 定義済み色値
        public const long HAITA_COLOR = 0xC0C0C0;                     // 排他ロック時の画面背景色（グレー）
        public const long HAITA_FORE_COLOR = 0x80000012;              // 排他ロック時の文字色（黒）

        // テキストボックス背景色
        public const int NORMAL_COLOR = unchecked((int)0xFFFFFF);     // 通常（白）
        public const int LOCK_COLOR = unchecked((int)0xCED3D6);       // 使用不能（グレー）
        public const int NG_COLOR = unchecked((int)0xD0D0FF);         // 内容不備（赤）
        public const int NULL_COLOR = unchecked((int)0xFFFFC0);       // 未入力（水色）
        public const int OVER_COLOR = unchecked((int)0x80FFFF);       // 長さ限界超過（黄色）
        public const int DEF_BACK_COLOR = unchecked((int)0x8000000F); // デフォルトの背景色（白）
        public const int DEF_FORE_COLOR = unchecked((int)0x80000012); // デフォルトの文字色（グレー）

        public static bool pb_TestDBFlg;                              // テスト環境かどうかを判断するフラグ
        private static string pb_DBName;                              // データベース名
        private const string DATABASE = "SETTING/DB/DATABASE";        // タグ：DATABASE
        private const string TEST_DB_HEADER = "_TST_";                // テスト環境DBの先頭5バイト
        private const string CONN_PASS = "honban_";                   // 本番環境時、Exe直起動の際入力するパスワードの固定値

        #region 構造体
        // コマンドライン構造体
        public static LG_CMD LOGIN_CMD = new LG_CMD();

        // コマンドライン構造体の定義
        public struct LG_CMD
        {
            public string LG_Ank_CD;       // 案件コード
            public string LG_Ank_Name;     // 案件名
            public string LG_CP_CD;        // ＣＰコード
            public string LG_CP_Name;      // ＣＰ名
            public string LG_LoginID;      // ログインＩＤ
            public string LG_Authority_Flg; // 権限
            public string LG_ColorVal;     // 色
            public string LG_RunApiCd;     // アプリケーションコード
            public string LG_PortNo;       // ポート番号
            public string LG_KanjiSei;     // 漢字_姓
            public string LG_KanjiMei;     // 漢字_名
            public string LG_KanjiShimei;  // 漢字_氏名
            public string LG_KanaSei;      // カナ_セイ
            public string LG_KanaMei;      // カナ_メイ
            public string LG_KanaShimei;   // カナ_シメイ
            public string LG_IniPath;      // INIファイル格納パス
        }

        // ユーザ情報構造体
        public static USERINFO pb_User = new USERINFO();

        // ユーザ情報構造体の定義
        public struct USERINFO
        {
            public string Login;            // ログインID
            public string Password;         // パスワード
            public int Authority;           // 権限
            public string PCName;           // 端末名
            public string KanjiSei;         // 漢字_姓
            public string KanjiMei;         // 漢字_名
            public string KanjiShimei;      // 漢字_氏名
            public string KanaSei;          // カナ_セイ
            public string KanaMei;          // カナ_メイ
            public string KanaShimei;       // カナ_シメイ
        }
        #endregion

        #region 列挙体
        // コマンドラインの並び
        public enum emCMD
        {
            AnkCd = 0,           // 案件コード
            AnkName,             // 案件名
            CPCD,                // ＣＰコード
            CPName,              // ＣＰ名
            LoginID,             // ログインＩＤ
            Authority,           // 権限
            ColorVal,            // 色
            RunApiCd,            // アプリケーションコード
            PortNo,              // ポート番号
            KanjiSei,            // 漢字_姓
            KanjiMei,            // 漢字_名
            KanjiShimei,         // 漢字_氏名
            KanaSei,             // カナ_セイ
            KanaMei,             // カナ_メイ
            KanaShimei,          // カナ_シメイ
            IniPath,             // INIファイル格納パス

            MaxCol               // 最大列数
        }

        // 権限リスト
        public enum EmAuthority
        {
            Admin = 0,           // システム管理者
            DataManager,         // データ運用者
            Director,            // ディレクター
            CallSV,              // コールＳＶ
            CallOP               // コールＯＰ
        }
        #endregion

        #region エントリポイント(アプリケーションのスタート地点)
        public static bool CheckBeforeLaunch(string[] CmdArgs)
        {
            bool isValid = true; // チェック結果
            bool FlgCmdLine = false;
            string InPutPass = string.Empty;

            // Windows XP の視覚テーマを有効にする
            Application.EnableVisualStyles();

            // 二重起動をチェックする
            if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
            {
                // すでに起動していると判断して終了
                MessageBox.Show("2重起動は行なえません。");
                return !isValid;
            }

            // コマンドライン取得
            StringBuilder sb = new StringBuilder();
            string strAnd = string.Empty;

            // 実行ファイルが.Applicationか.exeかをCmdArgsの長さで判断
            if (CmdArgs.Length != 0)
            {
                // 何かしらのコマンドラインを受け取って実行された.exeと判断
                for (int i = 0; i < CmdArgs.Length; i++)
                {
                    // 半角スペースがあると無条件に区切られるので結合する
                    sb.Append(strAnd + CmdArgs[i]);
                    strAnd = " ";
                }
            }
            else
            {
                // コマンドラインを受け取っていない→.Applicationと判断
            }

            FlgCmdLine = Get_CmdLine(sb.ToString());

            // Xmlファイルパスの初期設定
            pb_XmlPath = Application.StartupPath + "\\";
            pb_XmlFilePath = pb_XmlPath + Xml_NAME;

            // コマンドラインが取得できた場合はINIファイル格納パスを取得
            if (FlgCmdLine)
            {
                if (!string.IsNullOrEmpty(LOGIN_CMD.LG_IniPath))
                {
                    pb_XmlPath = LOGIN_CMD.LG_IniPath + LOGIN_CMD.LG_Ank_CD + "\\";
                    pb_XmlFilePath = pb_XmlPath + Xml_NAME;

                    // 設定ファイルが見つからない場合はExeと同じ場所を参照
                    // （通常はありえないがイレギュラーを想定して）
                    if (!StCls_File.FF_IsFile(pb_XmlFilePath))
                    {
                        pb_XmlPath = Application.StartupPath + "\\";
                        pb_XmlFilePath = pb_XmlPath + Xml_NAME;
                    }
                }
            }

            // XMLファイル存在確認
            if (!StCls_File.FF_IsFile(pb_XmlFilePath))
            {
                MessageBox.Show("設定ファイルが存在しませんでした。" + pb_XmlFilePath);
                return !isValid;
            }

            Cls_Xml Xml = new Cls_Xml(pb_XmlFilePath);
            pb_DBName = Xml.ReadXmlText(DATABASE);
            Xml = null; // 明示的に開放

            // テスト環境かどうかを判断する
            if (StCls_Function.VbLeft(pb_DBName.ToUpper(), 5) == TEST_DB_HEADER)
            {
                pb_TestDBFlg = true;
            }
            else
            {
                pb_TestDBFlg = false;
            }

            // フォームクラスをセット（色設定）
            pb_cForm = new Cls_FormColor();

            // コマンドライン取得
            if (!FlgCmdLine)
            {
            // ログインチェック
            LOGIN_CHECK:

                // リリースモード時のみパスワード入力を要求
#if !DEBUG
        // テストDBであればパスワード認証は行わない
        if (!pb_TestDBFlg)
        {
            InPutPass = InputBox("Login管理システムを通さずに本番環境への接続をしようとしております。パスワードを入力して下さい。", "パスワード入力", "");

            if (InPutPass.Length == 0)
            {
                MessageBox.Show("キャンセルされました。システムを終了します。", "終了", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            else if (InPutPass != CONN_PASS + DateTime.Now.ToString("yyyyMMdd"))
            {
                MessageBox.Show("パスワードが違います", "確認", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                goto LOGIN_CHECK;
            }
        }
        MessageBox.Show("ID=Admin" + Environment.NewLine + "管理者権限でログインします。", "承認", MessageBoxButtons.OK, MessageBoxIcon.Information);
#endif

                // ログインID取得&権限
                pb_User.Login = "Admin";
                pb_User.Authority = (int)EmAuthority.Admin;
                pb_form_color = 0xFFD5AA;
            }
            else
            {
                // ログインID取得&権限
                pb_User.Login = LOGIN_CMD.LG_LoginID;
                pb_User.Authority = int.Parse(LOGIN_CMD.LG_Authority_Flg);
                pb_form_color = int.Parse(LOGIN_CMD.LG_ColorVal);

                pb_User.KanjiSei = LOGIN_CMD.LG_KanjiSei;
                pb_User.KanjiMei = LOGIN_CMD.LG_KanjiMei;
                pb_User.KanjiShimei = LOGIN_CMD.LG_KanjiShimei;
                pb_User.KanaSei = LOGIN_CMD.LG_KanaSei;
                pb_User.KanaMei = LOGIN_CMD.LG_KanaMei;
                pb_User.KanaShimei = LOGIN_CMD.LG_KanaShimei;
            }

            pb_User.PCName = StCls_Function.GetPCName();

            // 各種ログファイル設定
            Cls_Xml logXml = new Cls_Xml(pb_XmlFilePath);

            // ログ出力設定
            if (logXml.ReadXmlText("SETTING/DIR/OUT_IN_ERROR_FLDPATH") == "C:\\test\\取込エラー\\")
            {
                pb_SQLLogFlg = true;
                pb_SQLLogPath = logXml.ReadXmlText("//SETTING/DIR/LOG") + DateTime.Now.ToString("yyyyMMdd") + "\\";
                StCls_File.FF_CreateFolder(pb_SQLLogPath);
                pb_SQLLogPath += "SQL_" + pb_User.PCName + "_" + pb_User.Login + "_" + DateTime.Now.ToString("yyyyMMdd") + ".log";
            }

            if (logXml.ReadXmlText("//SETTING/DIR/OUT_IN_ERROR_FLDPATH") == "1")
            {
                pb_SysErrLogFlg = true;
                pb_SysErrLogPath = logXml.ReadXmlText("//SETTING/DIR/LOG") + DateTime.Now.ToString("yyyyMMdd") + "\\";
                StCls_File.FF_CreateFolder(pb_SysErrLogPath);
                pb_SysErrLogPath += "SysErr_" + pb_User.PCName + "_" + pb_User.Login + "_" + DateTime.Now.ToString("yyyyMMdd") + ".log";
            }

            if (logXml.ReadXmlText("//SETTING/DIR/OUT_IN_ERROR_FLDPATH") == "1")
            {
                pb_WorkLogFlg = true;
                pb_WorkLogPath = logXml.ReadXmlText("//SETTING/DIR/LOG") + DateTime.Now.ToString("yyyyMMdd") + "\\";
                StCls_File.FF_CreateFolder(pb_WorkLogPath);
                pb_WorkLogPath += "Work_" + pb_User.PCName + "_" + pb_User.Login + "_" + DateTime.Now.ToString("yyyyMMdd") + ".log";
            }

            logXml = null;

            // 初期フォーム起動
            return isValid;
        }
        #endregion

        // **************************************************
        // 　関数
        // **************************************************

        #region コマンドライン取得関数
        public static bool Get_CmdLine(string Args)
        {
            string[] cmdline;

            // 実行ファイルが.Applicationか.exeかをArgsの長さで判断
            if (Args.Length != 0)
            {
                // 何かしらのコマンドラインを受け取って実行された.exeと判断
                cmdline = Args.Split(',');
            }
            else
            {
                // コマンドラインを受け取っていない→.Applicationと判断
                string envArgs = Environment.GetEnvironmentVariable("ARGS", EnvironmentVariableTarget.User);

                // 環境変数ARGSが設定されているか確認
                if (!string.IsNullOrEmpty(envArgs))
                {
                    cmdline = envArgs.Split(new string[] { "','" }, StringSplitOptions.None); // 環境変数から引数を取得して配列にセット
                    Environment.SetEnvironmentVariable("ARGS", null, EnvironmentVariableTarget.User); // 登録した環境変数を消す
                }
                else
                {
                    // 環境変数が設定されていない場合、空の配列を設定
                    cmdline = new string[0];
                }
            }

            // コマンドラインが取得できなかった場合
            if (cmdline.Length != (int)emCMD.MaxCol)
            {
                return false;
            }

            LOGIN_CMD.LG_Ank_CD = cmdline[(int)emCMD.AnkCd].Replace("'", "");
            LOGIN_CMD.LG_Ank_Name = cmdline[(int)emCMD.AnkName].Replace("'", "");
            LOGIN_CMD.LG_CP_CD = cmdline[(int)emCMD.CPCD].Replace("'", "");
            LOGIN_CMD.LG_CP_Name = cmdline[(int)emCMD.CPName].Replace("'", "");
            LOGIN_CMD.LG_LoginID = cmdline[(int)emCMD.LoginID].Replace("'", "");
            LOGIN_CMD.LG_Authority_Flg = cmdline[(int)emCMD.Authority].Replace("'", "");
            LOGIN_CMD.LG_ColorVal = cmdline[(int)emCMD.ColorVal].Replace("'", "");
            LOGIN_CMD.LG_RunApiCd = cmdline[(int)emCMD.RunApiCd].Replace("'", "");
            LOGIN_CMD.LG_PortNo = cmdline[(int)emCMD.PortNo].Replace("'", "");
            LOGIN_CMD.LG_KanjiSei = cmdline[(int)emCMD.KanjiSei].Replace("'", "");
            LOGIN_CMD.LG_KanjiMei = cmdline[(int)emCMD.KanjiMei].Replace("'", "");
            LOGIN_CMD.LG_KanjiShimei = cmdline[(int)emCMD.KanjiShimei].Replace("'", "");
            LOGIN_CMD.LG_KanaSei = cmdline[(int)emCMD.KanaSei].Replace("'", "");
            LOGIN_CMD.LG_KanaMei = cmdline[(int)emCMD.KanaMei].Replace("'", "");
            LOGIN_CMD.LG_KanaShimei = cmdline[(int)emCMD.KanaShimei].Replace("'", "");
            LOGIN_CMD.LG_IniPath = cmdline[(int)emCMD.IniPath].Replace("'", "");

            return true;
        }
        #endregion
    }
}
