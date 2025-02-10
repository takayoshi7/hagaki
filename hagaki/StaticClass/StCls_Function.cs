using System;
using System.Collections;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Microsoft.VisualBasic;
using hagaki.Class;

// ---------------------------------------------
//  クラス名   : StCls_Function
//  概要　　　 : モジュール関係
//  作成日　　 : 2025/02/05
//  作成者　　 : 高橋
//  最終更新日 : 
//  最終更新者 : 
// ---------------------------------------------

namespace hagaki.StaticClass
{
    public static class StCls_Function
    {
        #region AppPath メソッド
        /// <summary>
        /// 実行ファイルのパスを返す
        /// </summary>
        /// <returns>実行ファイルのフルパス</returns>
        public static string GetExeAppPath()
        {
            // エントリアセンブリを取得
            Assembly asm = Assembly.GetEntryAssembly();
            if (asm == null)
            {
                return string.Empty;
            }
            else
            {
                // 実行ファイルのディレクトリパスを返す
                return Path.GetDirectoryName(asm.Location) + "\\";
            }
        }
        #endregion

        #region LenB メソッド
        /// <summary>
        /// 指定された文字列のバイト数を返す
        /// </summary>
        /// <param name="strTarget">バイト数取得の対象となる文字列</param>
        /// <returns>半角 1 バイト、全角 2 バイトでカウントされたバイト数</returns>
        public static int LenB(string strTarget)
        {
            // Shift_JIS エンコーディングでバイト数を取得
            return Encoding.GetEncoding("Shift_JIS").GetByteCount(strTarget);
        }
        #endregion

        #region StrConv メソッド
        /// <summary>
        /// 指定された文字列を変換する
        /// </summary>
        /// <param name="strTarget">変換対象の文字列</param>
        /// <param name="conv">変換タイプ</param>
        /// <returns>変換後の文字列</returns>
        public static string VbStrConv(string strTarget, VbStrConv conv)
        {
            return Strings.StrConv(strTarget, conv);
        }
        #endregion

        #region Left メソッド
        /// <summary>
        /// 文字列の左端から指定された文字数分の文字列を返す
        /// </summary>
        /// <param name="strTarget">取り出す元になる文字列</param>
        /// <param name="iLength">取り出す文字数</param>
        /// <returns>左端から指定された文字数分の文字列</returns>
        public static string VbLeft(string strTarget, int iLength)
        {
            if (iLength <= strTarget.Length)
            {
                return strTarget.Substring(0, iLength);
            }

            return strTarget;
        }
        #endregion

        #region Right メソッド
        /// <summary>
        /// 文字列の右端から指定された文字数分の文字列を返す
        /// </summary>
        /// <param name="strTarget">取り出す元になる文字列</param>
        /// <param name="iLength">取り出す文字数</param>
        /// <returns>右端から指定された文字数分の文字列</returns>
        public static string VbRight(string strTarget, int iLength)
        {
            if (iLength <= strTarget.Length)
            {
                return strTarget.Substring(strTarget.Length - iLength);
            }

            return strTarget;
        }
        #endregion

        #region Mid メソッド
        /// <summary>
        /// 文字列の指定された位置以降のすべての文字列を返す
        /// </summary>
        /// <param name="strTarget">取り出す元になる文字列</param>
        /// <param name="iStart">取り出しを開始する位置</param>
        /// <returns>指定された位置以降のすべての文字列</returns>
        public static string VbMid(string strTarget, int iStart)
        {
            if (iStart <= strTarget.Length)
            {
                return strTarget.Substring(iStart - 1);
            }

            return string.Empty;
        }
        #endregion

        #region LeftB メソッド
        /// <summary>
        /// 文字列の左端から指定したバイト数分の文字列を返します。
        /// </summary>
        /// <param name="stTarget">取り出す元になる文字列。</param>
        /// <param name="iByteSize">取り出すバイト数。</param>
        /// <returns>左端から指定されたバイト数分の文字列。</returns>
        public static string LeftB(string stTarget, int iByteSize)
        {
            return MidB(stTarget, 1, iByteSize);
        }
        #endregion

        #region RightB メソッド
        /// <summary>
        /// 文字列の右端から指定されたバイト数分の文字列を返します。
        /// </summary>
        /// <param name="stTarget">取り出す元になる文字列。</param>
        /// <param name="iByteSize">取り出すバイト数。</param>
        /// <returns>右端から指定されたバイト数分の文字列。</returns>
        public static string RightB(string stTarget, int iByteSize)
        {
            Encoding hEncoding = Encoding.GetEncoding("Shift_JIS");
            byte[] btBytes = hEncoding.GetBytes(stTarget);

            return hEncoding.GetString(btBytes, btBytes.Length - iByteSize, iByteSize);
        }
        #endregion

        #region MidB メソッド (+1)
        /// <summary>
        /// 文字列の指定されたバイト位置以降のすべての文字列を返します。
        /// </summary>
        /// <param name="stTarget">取り出す元になる文字列。</param>
        /// <param name="iStart">取り出しを開始する位置（1-based）。</param>
        /// <returns>指定されたバイト位置以降のすべての文字列。</returns>
        public static string MidB(string stTarget, int iStart)
        {
            Encoding hEncoding = Encoding.GetEncoding("Shift_JIS");
            byte[] btBytes = hEncoding.GetBytes(stTarget);

            return hEncoding.GetString(btBytes, iStart - 1, btBytes.Length - (iStart - 1));
        }

        /// <summary>
        /// 文字列の指定されたバイト位置から、指定されたバイト数分の文字列を返します。
        /// </summary>
        /// <param name="stTarget">取り出す元になる文字列。</param>
        /// <param name="iStart">取り出しを開始する位置（1-based）。</param>
        /// <param name="iByteSize">取り出すバイト数。</param>
        /// <returns>指定されたバイト位置から指定されたバイト数分の文字列。</returns>
        public static string MidB(string stTarget, int iStart, int iByteSize)
        {
            Encoding hEncoding = Encoding.GetEncoding("Shift_JIS");
            byte[] btBytes = hEncoding.GetBytes(stTarget);

            return hEncoding.GetString(btBytes, iStart - 1, iByteSize);
        }
        #endregion

        #region ユーザー名取得
        /// <summary>
        /// ユーザー名を取得する
        /// </summary>
        /// <returns>ユーザー名</returns>
        public static string GetUser()
        {
            return Environment.UserName;
        }
        #endregion

        #region PC名取得
        /// <summary>
        /// 端末名（PC名）を取得する
        /// </summary>
        /// <returns>PC名</returns>
        public static string GetPCName()
        {
            return Environment.MachineName;
        }
        #endregion

        #region SQLServer接続文字列を取得
        /// <summary>
        /// SQL Serverへの接続文字列を取得します。
        /// </summary>
        /// <returns>SQL接続文字列</returns>
        public static string Get_SQLConnectString()
        {
            string database;
            string serverId;
            string userId;
            string password;

            string xmlFilePath = @"path_to_your_xml_file.xml";  // XMLファイルのパス
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlFilePath);

            // XMLから接続情報を取得
            database = xmlDoc.SelectSingleNode("SETTING/DB/DATABASE")?.InnerText;
            serverId = xmlDoc.SelectSingleNode("SETTING/DB/SERVER")?.InnerText;
            userId = xmlDoc.SelectSingleNode("SETTING/DB/ID")?.InnerText;
            password = xmlDoc.SelectSingleNode("SETTING/DB/PASSWORD")?.InnerText;

            // 接続文字列を作成
            string connectionString = $"data source={serverId};";
            connectionString += $"initial catalog={database};";
            connectionString += $"User ID={userId};";
            connectionString += $"Password={password};";

            return connectionString;
        }
        #endregion

        #region サーバー時刻取得
        /// <summary>
        /// SQL Serverからサーバー時刻を取得します。
        /// </summary>
        /// <returns>サーバー時刻（フォーマット：yyyy/MM/dd HH:mm:ss）</returns>
        public static string GetServerTime()
        {
            string connectionString = Get_SQLConnectString();
            string query = "SELECT GETDATE()";  // サーバー時刻を取得するSQLクエリ

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        Object result = command.ExecuteScalar();
                        if (result != null)
                        {
                            DateTime serverTime = Convert.ToDateTime(result);
                            return serverTime.ToString("yyyy/MM/dd HH:mm:ss");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 接続エラーや取得エラーが発生した場合、ローカルの時刻を返す
                Console.WriteLine($"Error: {ex.Message}");
            }

            // サーバー時刻が取得できなかった場合、ローカルの日時を返す
            return DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        }
        #endregion

        #region DB存在確認
        /// <summary>
        /// 指定されたテーブルにレコードが存在するかを確認します。
        /// </summary>
        /// <param name="table">テーブル名</param>
        /// <param name="filter">WHERE句（オプション）</param>
        /// <returns>レコードが存在する場合はTrue、存在しない場合はFalse</returns>
        public static bool DBExist(string table, string filter)
        {
            bool exists = false;
            string connectionString = Get_SQLConnectString();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // クエリ文字列を構築
                    StringBuilder query = new StringBuilder();
                    query.Append("SELECT COUNT(*) AS Result FROM ");
                    query.Append(table.Trim());

                    if (!string.IsNullOrEmpty(filter))
                    {
                        query.Append(" WHERE ");
                        query.Append(filter.Trim());
                    }

                    using (SqlCommand cmd = new SqlCommand(query.ToString(), conn))
                    {
                        // クエリを実行して、レコードのカウントを取得
                        object result = cmd.ExecuteScalar();
                        if (result != null && Convert.ToInt32(result) > 0)
                        {
                            exists = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }

            return exists;
        }
        #endregion

        #region 許可文字列判定
        /// <summary>
        /// 許可された文字だけか判定します。
        /// </summary>
        /// <param name="chkStr">検査対象文字列</param>
        /// <param name="allowChars">許可対象文字列</param>
        /// <param name="allowBlank">ブランクを許可するか（デフォルトはfalse）</param>
        /// <returns>許可文字のみならTrue、それ以外ならFalse</returns>
        public static bool IsAllowChar(string chkStr, string allowChars, bool allowBlank = false)
        {
            // NULL検査
            if (string.IsNullOrEmpty(chkStr))
            {
                // ブランクを許可している場合はTrueを返す
                return allowBlank;
            }

            // 文字列を1文字ずつ検査
            foreach (char ch in chkStr)
            {
                // 許可文字列に含まれていない文字があればFalseを返す
                if (!allowChars.Contains(ch))
                {
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region SQL文字列 エスケープ処理
        /// <summary>
        /// 引数に指定された文字列のシングルクォートを変換する。
        /// </summary>
        /// <param name="val">対象文字列</param>
        /// <returns>変換済み文字列</returns>
        public static string CnvQuart(string val)
        {
            return val.Replace("'", "''");
        }
        #endregion

        #region Null or ブランク置換
        /// <summary>
        /// 第一引数がNullまたはブランクの場合、第二引数を返す
        /// </summary>
        /// <param name="tValue">Nullまたはブランク評価値</param>
        /// <param name="fValue">Nullまたはブランク時置換の値</param>
        /// <returns>置換後文字列</returns>
        public static string NtoV(object tValue, string fValue)
        {
            // Nullまたはブランク判定
            if (tValue == null || tValue == DBNull.Value)
            {
                return fValue;
            }
            else if (tValue.ToString().Length == 0)
            {
                return fValue;
            }
            else
            {
                return tValue.ToString();
            }
        }
        #endregion

        #region 制限バイト数チェック
        /// <summary>
        /// サイズチェック：文字列のバイト数が指定された制限を超えているかどうか確認します。
        /// </summary>
        /// <param name="strVal">チェック対象文字列</param>
        /// <param name="IntOver">制限バイト数</param>
        /// <returns>True = 正常、False = 不備</returns>
        public static bool Chk_Size(string strVal, int IntOver)
        {
            // バイト数チェック
            return LenB(strVal) <= IntOver;
        }
        #endregion

        #region タブ・改行の強制変換
        /// <summary>
        /// タブと改行を強制的に変換する
        /// </summary>
        /// <param name="strVal">変換元の文字列</param>
        /// <param name="RepTab">タブを変換した後の文字列</param>
        /// <returns>変換後の文字列</returns>
        public static string Replace_Tab_Crlf(string strVal, string RepTab)
        {
            // タブが入力されていたら指定文字列に変換
            strVal = strVal.Replace("\t", RepTab);

            // 改行除去 (CR + LF)
            strVal = strVal.Replace("\r", "").Replace("\n", "");

            return strVal;
        }
        #endregion

        #region 数値に3桁区切りのカンマをつける
        /// <summary>
        /// 文字列として与えられた数値に 3 桁区切りのカンマをつける
        /// </summary>
        /// <param name="myString">数値の文字列</param>
        /// <returns>3桁区切りのカンマがついた文字列</returns>
        public static string CastNumberSep(string myString)
        {
            long lngNum = 0;

            // 数値に変換できるか確かめる
            if (!long.TryParse(myString, out lngNum))
            {
                return myString; // 変換できない場合、そのまま返す
            }

            try
            {
                // 3桁区切りでフォーマット
                return string.Format("{0:#,##0}", lngNum);
            }
            catch (Exception)
            {
                return myString; // エラー発生時、そのまま返す
            }
        }
        #endregion

        #region 文字列→数値変換

        /// <summary>
        /// 文字列を整数（int）に変換します。変換できない場合は0を返します。
        /// </summary>
        /// <param name="myString">変換する文字列</param>
        /// <returns>変換後の整数</returns>
        public static int CastInt(string myString)
        {
            try
            {
                // Null、DBNull、またはブランクなら0を返す
                if (NtoV(myString, "0") == "0")
                {
                    return 0;
                }

                return int.Parse(myString);
            }
            catch (Exception)
            {
                return 0; // 変換失敗時は0を返す
            }
        }

        /// <summary>
        /// 文字列を長整数（long）に変換します。変換できない場合は0を返します。
        /// </summary>
        /// <param name="myString">変換する文字列</param>
        /// <returns>変換後の長整数</returns>
        public static long CastLng(string myString)
        {
            // Null、DBNull、またはブランクなら0を返す
            if (NtoV(myString, "0") == "0")
            {
                return 0;
            }

            try
            {
                return long.Parse(myString);
            }
            catch (Exception)
            {
                return 0; // 変換失敗時は0を返す
            }
        }

        #endregion

        #region 桁数にあわせて0埋め
        /// <summary>
        /// 文字列を指定された桁数に合わせて0埋めします。
        /// </summary>
        /// <param name="argData">埋める対象の文字列</param>
        /// <param name="argLength">埋めるべき桁数</param>
        /// <returns>指定された桁数に合わせて0埋めされた文字列</returns>
        public static string GetFormatZero(string argData, int argLength)
        {
            string tmpType = new string('0', argLength); // "0"をargLength回繰り返した文字列

            if (argData.Length > argLength)
            {
                return argData.Substring(argData.Length - argLength); // argDataが長すぎる場合は後ろの桁数を取る
            }
            else
            {
                return tmpType.Substring(0, argLength - argData.Length) + argData; // 必要な0を埋める
            }
        }
        #endregion

        #region 文字列の日付をyyyyMMddhhmmssに変換する
        /// <summary>
        /// 文字列の日付をyyyyMMddHHmmss形式に変換します。
        /// </summary>
        /// <param name="strValue">変換対象の日付文字列</param>
        /// <returns>yyyyMMddHHmmss形式の日付文字列、または空文字列（無効な日付の場合）</returns>
        public static string GetFormatNow(string strValue)
        {
            // 日付チェック
            DateTime parsedDate;
            if (!DateTime.TryParse(strValue, out parsedDate))
            {
                return string.Empty;
            }

            // 年月日を取得
            string sYYYY = parsedDate.Year.ToString();
            string sMM = parsedDate.Month.ToString("D2");  // 2桁形式で表示
            string sDD = parsedDate.Day.ToString("D2");

            // 時分秒を取得
            string sHH = parsedDate.Hour.ToString("D2");
            string sNN = parsedDate.Minute.ToString("D2");
            string sSS = parsedDate.Second.ToString("D2");

            // フォーマットした日付と時間を結合
            return sYYYY + sMM + sDD + sHH + sNN + sSS;
        }
        #endregion

        #region Excelがインストールされているかチェックする
        /// <summary>
        /// 実行端末にExcelがインストールされているかチェックします。
        /// </summary>
        /// <param name="aplName">チェック対象のアプリケーション名（デフォルトは "Excel.Application"）</param>
        /// <returns>True: インストールされている、False: インストールされていない</returns>
        public static bool CheckExcelInstallation(string aplName = "Excel.Application")
        {
            try
            {
                // ExcelのProgIDが存在するか確認
                Type excelApp = Type.GetTypeFromProgID(aplName);
                return excelApp != null;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region コントロール列挙
        /// <summary>
        /// フォーム上のコントロールを全て取得します。
        /// </summary>
        /// <param name="top">取得を開始するトップレベルのコントロール。</param>
        /// <returns>コントロールの配列。</returns>
        public static Control[] GetAllControls(Control top)
        {
            ArrayList buf = new ArrayList();

            // トップレベルのコントロールのすべての子コントロールを取得
            foreach (Control c in top.Controls)
            {
                buf.Add(c);
                // 子コントロールがさらに子を持っていれば再帰的に呼び出して追加
                buf.AddRange(GetAllControls(c));
            }

            // 配列として返却
            return (Control[])buf.ToArray(typeof(Control));
        }
        #endregion

        #region IMEモード
        /// <summary>
        /// 引数に対応するIMEモードを返します。
        /// </summary>
        /// <param name="ImeFlg">IMEモードを示す文字列（0〜8のいずれか）</param>
        /// <returns>対応するImeMode</returns>
        public static ImeMode GetImeMode(string ImeFlg)
        {
            switch (ImeFlg)
            {
                case "0":
                    return ImeMode.NoControl;
                case "1":
                    return ImeMode.On;
                case "2":
                    return ImeMode.Off;
                case "3":
                    return ImeMode.Disable;
                case "4":
                    return ImeMode.Hiragana;
                case "5":
                    return ImeMode.Katakana;
                case "6":
                    return ImeMode.KatakanaHalf;
                case "7":
                    return ImeMode.AlphaFull;
                case "8":
                    return ImeMode.Alpha;
                default:
                    return ImeMode.NoControl;
            }
        }
        #endregion

        #region フォームの初期設定
        public static void SetForm(ref Form frm,
                    int formColor,
                    int foreColor = StCls_Public.DEF_FORE_COLOR,
                    bool groupFlg = true,
                    bool labelFlg = true,
                    bool checkFlg = true,
                    bool radioFlg = true,
                    bool tabFlg = true,
                    bool pictureFlg = true)
        {
            // フォームキャプションにタイトルをつける
            // frm.Text = frm.Text;

            // 背景色設定
            StCls_Public.pb_cForm.SetFormColor(ref frm, formColor, foreColor, groupFlg, labelFlg, checkFlg, radioFlg, tabFlg, pictureFlg);

            // キーイベントを受け取るようにする (Enter でコントロール移動に使用)
            frm.KeyPreview = true;
        }
        #endregion

        #region RGB→色構造体変換
        public static System.Drawing.Color GetColorTranslator(int rgbColor)
        {
            return System.Drawing.ColorTranslator.FromOle(rgbColor);
        }
        #endregion

        #region  + Function: メソッドがどのメソッドから呼び出されたかチェックする
        /// <summary>
        /// メソッドがどのメソッドから呼び出されたかチェックする
        /// </summary>
        /// <param name="methodName">呼び出し元のメソッド</param>
        /// <returns>TRUE：呼び出された　FALSE：呼び出されていない</returns>
        /// <remarks></remarks>
        public static bool CheckCaller(string methodName)
        {
            // スタックトレースを作成する（現在のメソッドと呼び出し元のメソッドを除く）
            StackTrace st = new StackTrace(2);

            for (int i = 0; i < st.FrameCount; i++)
            {
                StackFrame sf = st.GetFrame(i);
                MethodBase mi = sf.GetMethod();

                // メソッド名が一致すれば終了
                if (mi.Name == methodName)
                {
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region  + Sub: 【DEBUG調査用】呼び出し元までのトレース履歴を書き出す
        /// <summary>呼び出し元までのトレース履歴を書き出す</summary>
        /// <remarks></remarks>
        public static void DebugPrintTraceHistory()
        {
            Debug.Print("");

            foreach (StackFrame frame in (new StackTrace()).GetFrames())
            {
                Debug.Print(frame.GetMethod().Name);
            }
        }
        #endregion

        #region  + Sub: 汎用イベント設定 Control.Textコピー Control_FROM → Control_TO

        //ADDBY 2014/11/19 matsuda
        /// <summary>
        /// 汎用イベント設定　Control.Textコピー　Control_FROM → Control_TO
        /// </summary>
        /// <param name="target">コントロールFrom</param>
        /// <remarks></remarks>
        public static void SetEvent_CopyTextFromTo(Control target)
        {
            //Leaveイベントに処理を紐付ける
            target.Leave += CopyTextFromTo_Leave;
        }

        //ADDBY 2014/11/19 matsuda
        /// <summary>コントロールFromからコントロールToへText値をコピーします。</summary>
        private static void CopyTextFromTo_Leave(object sender, EventArgs e)
        {
            //【Name命名ルールあり】
            //　Control.Name　は　"共通部分" & "From・To"　で設定する必要があります。
            //例）
            //　　From側　"Txt_KanriNo_" & "From"
            //　　To側　  "Txt_KanriNo_" & "To"
            //
            //上記設定していなければフォーカス移動時に警告表示します。

            string baseName = "";
            Control ctrlFrom = null;
            Control[] ctrlTo = null;

            try
            {
                ctrlFrom = (Control)sender;

                //From側　設定もれ。Name修正必要。
                if (!ctrlFrom.Name.ToLower().EndsWith("from"))
                {
                    throw new Exception("コントロールFromが見つかりません。");
                }

                //コントロールFromから、Nameの共通部分を取得。
                baseName = ctrlFrom.Name.Remove(ctrlFrom.Name.Length - 4);

                //コンテナからコントロールToを取得する。（Name完全一致）
                ctrlTo = ctrlFrom.Parent.Controls.Find(baseName + "To", false);
                if (ctrlTo.Length == 0) ctrlTo = ctrlFrom.Parent.Controls.Find(baseName + "TO", false);
                if (ctrlTo.Length == 0) ctrlTo = ctrlFrom.Parent.Controls.Find(baseName + "to", false);

                //To側　設定もれ。Name修正必要。
                if (ctrlTo.Length == 0)
                {
                    throw new Exception("コントロールToが見つかりません。");
                }

                //下記ならコピーしない
                if (string.IsNullOrEmpty(ctrlFrom.Text)) return;
                if (!string.IsNullOrEmpty(ctrlTo[0].Text)) return;

                ctrlTo[0].Text = ctrlFrom.Text;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"CopyTextFromTo_Leave.　Name:{ctrlFrom.Name}　{ex.Message}");
            }
        }

        #endregion

        #region  + Sub: 汎用イベント設定 「出力先を開く」

        /// <summary>
        /// 汎用イベント設定　「出力先を開く」
        /// </summary>
        /// <param name="target">イベントを紐付けるボタン</param>
        /// <param name="outPath">出力先パスを表示しているコントロール</param>
        /// <remarks></remarks>
        public static void SetEvent_OpenOutPath(System.Windows.Forms.Button target, Control outPath)
        {
            //実処理の紐付け
            OpenOutPathEventHelper helper = new OpenOutPathEventHelper(target, outPath);
        }

        /// <summary>
        /// 「出力先を開く」処理をカプセル化
        /// </summary>
        /// <remarks></remarks>
        private class OpenOutPathEventHelper
        {
            private System.Windows.Forms.Button _target;   //出力先ボタン
            private Control _outPath; //出力先パス

            public OpenOutPathEventHelper(System.Windows.Forms.Button target, Control outPath)
            {
                _target = target;
                _outPath = outPath;
                _target.Click += Btn_OpenOutPath_Click;
            }

            /// <summary>「出力先を開く」ボタンの実処理</summary>
            public void Btn_OpenOutPath_Click(object sender, EventArgs e)
            {
                try
                {
                    //存在しなければ作成する
                    if (!System.IO.Directory.Exists(_outPath.Text))
                    {
                        System.IO.Directory.CreateDirectory(_outPath.Text);
                    }

                    //開く
                    System.Diagnostics.Process.Start(_outPath.Text);
                }
                catch (Exception)
                {
                    MessageBox.Show("フォルダの表示に失敗しました！", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        #region Sub: 汎用イベント設定 「参照する」
        /// <summary>
        /// 汎用イベント設定 「参照する」
        /// </summary>
        /// <param name="target">イベントを紐付けるボタン</param>
        /// <param name="filePath">参照ダイアログからパスを受け取るコントロール</param>
        /// <param name="effectPath">参照ダイアログ：有効ディレクトリパス</param>
        /// <param name="initPath">参照ダイアログ：初期表示パス</param>
        /// <param name="filter">参照ダイアログ：拡張子フィルタ</param>
        public static void SetEvent_OpenDialog(System.Windows.Forms.Button target, Control filePath, string effectPath, string initPath = "", Frm9000_FileOpenDialog.ExtensionFilter filter = Frm9000_FileOpenDialog.ExtensionFilter.All)
        {
            // 実処理の紐付け
            OpenDialogEventHelper helper = new OpenDialogEventHelper(target, filePath, effectPath, initPath, filter);
        }

        /// <summary>
        /// 「参照する」をカプセル化
        /// </summary>
        private class OpenDialogEventHelper
        {
            private System.Windows.Forms.Button _target;
            private Control _filePath;
            private string _effectPath;
            private string _initPath;
            private Frm9000_FileOpenDialog.ExtensionFilter _filter;

            public OpenDialogEventHelper(System.Windows.Forms.Button target, Control filePath, string effectPath, string initPath = "", Frm9000_FileOpenDialog.ExtensionFilter filter = Frm9000_FileOpenDialog.ExtensionFilter.All)
            {
                _target = target;
                _filePath = filePath;
                _effectPath = effectPath;
                _initPath = initPath;
                _filter = filter;
                _target.Click += Btn_OpenDialog_Click;
            }

            /// <summary>「参照する」ボタンの実処理</summary>
            public void Btn_OpenDialog_Click(object sender, EventArgs e)
            {
                try
                {
                    string filePath = Frm9000_FileOpenDialog.OpenDialog(_effectPath, _initPath, _filter);

                    if (!string.IsNullOrEmpty(filePath))
                    {
                        _filePath.Text = filePath;
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("フォルダの表示に失敗しました！", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion

        #region Function: バージョン情報を取得する
        /// <summary>
        /// バージョン情報を取得します。（プロパティ> アプリケーション> アセンブリ情報より）
        /// </summary>
        /// <param name="getToMinor">マイナーバージョンまで取得する</param>
        /// <param name="getToBuild">ビルドバージョンまで取得する</param>
        /// <param name="getToRevision">リビジョンバージョンまで取得する</param>
        /// <returns></returns>
        public static string GetVersion(bool getToMinor = false, bool getToBuild = false, bool getToRevision = false)
        {
            Version vi = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            if (getToRevision) return string.Format("{0}.{1}.{2}.{3}", vi.Major, vi.Minor, vi.Build, vi.Revision);
            if (getToBuild) return string.Format("{0}.{1}.{2}", vi.Major, vi.Minor, vi.Build);
            if (getToMinor) return string.Format("{0}.{1}", vi.Major, vi.Minor);

            // 既定はメジャーバージョンのみ返す
            return string.Format("{0}", vi.Major);
        }

        /// <summary>
        /// バージョン情報を取得します。（プロパティ> 発行> ClickOnceより）
        /// </summary>
        /// <param name="getToMinor">マイナーバージョンまで取得する</param>
        /// <param name="getToBuild">ビルドバージョンまで取得する</param>
        /// <param name="getToRevision">リビジョンバージョンまで取得する</param>
        /// <returns></returns>
        public static string GetVersion_ClickOnce(bool getToMinor = false, bool getToBuild = false, bool getToRevision = false)
        {
            // ビルド後のみ値を返す
            if (!System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed) return "";

            Version vi = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion;

            if (getToRevision) return string.Format("{0}.{1}.{2}.{3}", vi.Major, vi.Minor, vi.Build, vi.Revision);
            if (getToBuild) return string.Format("{0}.{1}.{2}", vi.Major, vi.Minor, vi.Build);
            if (getToMinor) return string.Format("{0}.{1}", vi.Major, vi.Minor);

            // 既定はメジャーバージョンのみ返す
            return string.Format("{0}", vi.Major);
        }
        #endregion

        #region Sub: ListControlにデータソースを設定する（DBより）

        /// <summary>
        /// ListControlにデータソースを設定する（DBより）
        /// </summary>
        /// <param name="ctrl">対象のコントロール（ComboBox/ListBoxなど）</param>
        /// <param name="tableName">DBテーブル名</param>
        /// <param name="valueField">DBフィールド名（Value値用）</param>
        /// <param name="textField">DBフィールド名（Text値用）</param>
        /// <param name="sort">ソート条件</param>
        /// <param name="filter">抽出条件</param>
        /// <param name="showBlankItem">先頭に空項目を表示するか</param>
        /// <param name="showTextWithValue">Text値を「Text:Value」形式で表示するか</param>
        /// <param name="firstValue">DBとは別に追加する先頭項目（Value値）</param>
        /// <param name="firstText">DBとは別に追加する先頭項目（Text値）</param>
        /// <param name="lastValue">DBとは別に追加する末尾項目（Value値）</param>
        /// <param name="lastText">DBとは別に追加する末尾項目（Text値）</param>
        /// <remarks></remarks>
        public static void SetDataSource(ListControl ctrl, string tableName, string valueField, string textField,
                                   string sort = "", string filter = "", bool showBlankItem = true,
                                   bool showTextWithValue = false, string firstValue = "", string firstText = "",
                                   string lastValue = "", string lastText = "")
        {
            System.Data.DataTable dt = new System.Data.DataTable();
            StringBuilder sb = new StringBuilder();
            Cls_DBConn cDB = new Cls_DBConn(Get_SQLConnectString());

            try
            {
                ctrl.DataSource = null;

                // DataTableに列を追加
                dt.Columns.Add(valueField, typeof(string));
                dt.Columns.Add(textField, typeof(string));

                // SQL文生成
                sb.Clear();
                sb.AppendLine("SELECT ");
                sb.AppendLine(valueField + ", ");

                if (showTextWithValue)
                {
                    sb.AppendLine(valueField + " + '：' + " + textField + " as DISP ");
                }
                else
                {
                    sb.AppendLine(textField + " ");
                }

                sb.AppendLine("FROM ");
                sb.AppendLine(tableName + " ");

                if (!string.IsNullOrEmpty(filter))
                {
                    sb.AppendLine("WHERE ");
                    sb.AppendLine(filter + " ");
                }

                if (!string.IsNullOrEmpty(sort))
                {
                    sb.AppendLine("ORDER BY ");
                    sb.AppendLine(sort);
                }

                using (SqlConnection mConn = cDB.SetDBConnction())
                {
                    using (SqlDataReader sDR = cDB.SetDataReader(mConn, sb.ToString()))
                    {
                        if (sDR.HasRows)
                        {
                            while (sDR.Read())
                            {
                                // DataTableに行を追加
                                if (showTextWithValue)
                                {
                                    dt.Rows.InsertAt(GetDataRow(dt, valueField, textField, sDR[valueField].ToString(), sDR["DISP"].ToString()), dt.Rows.Count);
                                }
                                else
                                {
                                    dt.Rows.InsertAt(GetDataRow(dt, valueField, textField, sDR[valueField].ToString(), sDR[textField].ToString()), dt.Rows.Count);
                                }
                            }
                        }
                    }
                }

                // ブランク行
                if (showBlankItem)
                {
                    dt.Rows.InsertAt(GetDataRow(dt, valueField, textField, "", ""), 0);
                }

                // 先頭行
                if (!string.IsNullOrEmpty(firstText))
                {
                    dt.Rows.InsertAt(GetDataRow(dt, valueField, textField, firstValue, firstText), 0);
                }

                // 末尾行
                if (!string.IsNullOrEmpty(lastText))
                {
                    dt.Rows.InsertAt(GetDataRow(dt, valueField, textField, lastValue, lastText), dt.Rows.Count);
                }

                // データテーブルのコミット
                dt.AcceptChanges();

                ctrl.DataSource = dt;
                ctrl.DisplayMember = textField;  // TextField → ListControl.Text
                ctrl.ValueMember = valueField;   // ValueField → ListControl.SelectedValue
                ctrl.SelectedIndex = -1;         // 初期Index = 未選択

                // ドロップダウン時の幅設定
                if (ctrl is ComboBox)
                {
                    SetDropDownWidth((ComboBox)ctrl);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                cDB = null;
            }
        }

        /// <summary>DataRowを作成するだけのヘルパーメソッド</summary>
        private static DataRow GetDataRow(System.Data.DataTable dt, string valueField, string textField, string value, string text)
        {
            DataRow result = dt.NewRow();
            result[valueField] = value;
            result[textField] = text;
            return result;
        }

        /// <summary>
        /// ComboBoxリスト部分の横幅調整
        /// </summary>
        /// <param name="ctrl"></param>
        /// <remarks></remarks>
        private static void SetDropDownWidth(ComboBox ctrl)
        {
            int maxSize = 0;

            foreach (Object item in ctrl.Items)
            {
                DataRowView dataRowView = item as DataRowView;
                if (dataRowView != null)
                {
                    // ComboBoxのフォントを使ってサイズ計測
                    maxSize = Math.Max(maxSize, TextRenderer.MeasureText(dataRowView[1].ToString(), ctrl.Font).Width);
                }
            }

            // リストが多い場合はスクロールバーが出るため、その分を追加する（固定20px） 
            maxSize += 20;

            // 現在の設定より大きければ置換 
            if (ctrl.DropDownWidth < maxSize)
            {
                ctrl.DropDownWidth = maxSize;
            }
        }

        #endregion

        #region Sub: ListControlにデータソースを設定する（フリー設定）
        /// <summary>
        /// ListControlにデータソースを設定する（フリー設定）
        /// </summary>
        /// <param name="ctrl">対象のコントロール（ComboBox/ListBoxなど）</param>
        /// <param name="valueCollection">ソート条件</param>
        /// <param name="textCollection">抽出条件</param>
        /// <param name="showBlankItem">先頭に空項目を表示するか</param>
        /// <param name="showTextWithValue">Text値を「Text:Value」形式で表示するか</param>
        /// <remarks></remarks>
        public static void SetDataSourceFree(ListControl ctrl, ICollection valueCollection, ICollection textCollection,
                                       bool showBlankItem = true, bool showTextWithValue = false)
        {
            System.Data.DataTable dt = new System.Data.DataTable();

            try
            {
                ctrl.DataSource = null;

                // 要素数が不一致なら終了
                if (valueCollection.Count != textCollection.Count)
                    return;

                // DataTableに列を追加
                dt.Columns.Add("Value", typeof(string));
                dt.Columns.Add("Text", typeof(string));

                // DataTableに行を追加
                int i = 0;
                foreach (Object value in valueCollection)
                {
                    DataRow row = dt.NewRow();

                    // 実値を表示（Enum定義が KanriNo = 0 なら 0を表示したい）
                    // String型変換エラーになるならそのとき考えましょう！
                    row["Value"] = value.ToString();

                    if (showTextWithValue)
                    {
                        row["Text"] = row["Value"] + "：" + textCollection.Cast<object>().ElementAt(i).ToString();
                    }
                    else
                    {
                        row["Text"] = textCollection.Cast<object>().ElementAt(i).ToString();
                    }

                    dt.Rows.Add(row);
                    i++;
                }

                // ブランク行
                if (showBlankItem)
                {
                    dt.Rows.InsertAt(GetDataRow(dt, "Value", "Text", "", ""), 0);
                }

                // データテーブルのコミット
                dt.AcceptChanges();

                ctrl.ValueMember = "Value";      // ValueField → ListControl.SelectedValue
                ctrl.DisplayMember = "Text";     // TextField → ListControl.Text
                ctrl.DataSource = dt;
                ctrl.SelectedIndex = -1;         // 初期Index = 未選択

                // ドロップダウン時の幅設定
                if (ctrl is ComboBox)
                {
                    SetDropDownWidth((ComboBox)ctrl);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Function: ListControl.SelectedValueを取得します（不正なIndex-1ならブランク）
        /// <summary>
        /// ListControl.SelectedValueを取得します（不正なIndex-1ならブランク）
        /// </summary>
        /// <param name="ctrl">対象のコントロール</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string GetSelectedValue(ListControl ctrl)
        {
            if (ctrl.SelectedIndex == -1)
            {
                return "";
            }
            return ctrl.SelectedValue.ToString();
        }
        #endregion

        #region Function: ListControl.Textを取得します（不正なIndex-1ならブランク）
        /// <summary>
        /// ListControl.Textを取得します（不正なIndex-1ならブランク）
        /// </summary>
        /// <param name="ctrl">対象のコントロール</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string GetSelectedText(ListControl ctrl)
        {
            if (ctrl.SelectedIndex == -1)
            {
                return "";
            }
            return ctrl.Text;
        }
        #endregion

        #region Function: MessageBox.Show 各アイコン用ショートカット

        /// <summary>
        /// MessageBox.Show 「i」アイコンを表示する
        /// </summary>
        /// <param name="text">メッセージ</param>
        /// <param name="caption">タイトル</param>
        /// <param name="buttuns">表示ボタン</param>
        /// <param name="defaultButtun">初期選択ボタン</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static DialogResult MsgBox_Information(string text,
                                                string caption = "確認",
                                                MessageBoxButtons buttuns = MessageBoxButtons.OK,
                                                MessageBoxDefaultButton defaultButtun = MessageBoxDefaultButton.Button1)
        {
            return MessageBox.Show(text, caption, buttuns, MessageBoxIcon.Information, defaultButtun);
        }

        /// <summary>
        /// MessageBox.Show 「?」アイコンを表示する
        /// </summary>
        /// <param name="text">メッセージ</param>
        /// <param name="caption">タイトル</param>
        /// <param name="buttuns">表示ボタン</param>
        /// <param name="defaultButtun">初期選択ボタン</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static DialogResult MsgBox_Question(string text,
                                             string caption = "確認",
                                             MessageBoxButtons buttuns = MessageBoxButtons.YesNo,
                                             MessageBoxDefaultButton defaultButtun = MessageBoxDefaultButton.Button2)
        {
            return MessageBox.Show(text, caption, buttuns, MessageBoxIcon.Question, defaultButtun);
        }

        /// <summary>
        /// MessageBox.Show 「!」アイコンを表示する
        /// </summary>
        /// <param name="text">メッセージ</param>
        /// <param name="caption">タイトル</param>
        /// <param name="buttuns">表示ボタン</param>
        /// <param name="defaultButtun">初期選択ボタン</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static DialogResult MsgBox_Exclamation(string text,
                                                string caption = "確認",
                                                MessageBoxButtons buttuns = MessageBoxButtons.OK,
                                                MessageBoxDefaultButton defaultButtun = MessageBoxDefaultButton.Button1)
        {
            return MessageBox.Show(text, caption, buttuns, MessageBoxIcon.Exclamation, defaultButtun);
        }

        /// <summary>
        /// MessageBox.Show 「×」アイコンを表示する
        /// </summary>
        /// <param name="text">メッセージ</param>
        /// <param name="caption">タイトル</param>
        /// <param name="buttuns">表示ボタン</param>
        /// <param name="defaultButtun">初期選択ボタン</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static DialogResult MsgBox_Error(string text,
                                          string caption = "エラー",
                                          MessageBoxButtons buttuns = MessageBoxButtons.OK,
                                          MessageBoxDefaultButton defaultButtun = MessageBoxDefaultButton.Button1)
        {
            return MessageBox.Show(text, caption, buttuns, MessageBoxIcon.Error, defaultButtun);
        }

        #endregion

        #region Function: ファイルの行数を取得する
        /// <summary>
        /// ファイルの行数を取得する
        /// </summary>
        /// <param name="path">ファイルパス</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static int GetFileLine(string path)
        {
            using (System.IO.StreamReader cReader = new System.IO.StreamReader(path))
            {
                int lineCount = 0;
                while (cReader.Peek() > -1)
                {
                    cReader.ReadLine();
                    lineCount++;
                }

                return lineCount;
            }
        }
        #endregion
    }
}
