using System;

// ---------------------------------------------
// クラス名   : StCls_Check
// 概要       : データチェック関係
// 作成日　　 : 2025/02/05
// 作成者　　 : 高橋
// 最終更新日 : 
// 最終更新者 : 
// ---------------------------------------------

namespace hagaki.StaticClass
{
    internal class StCls_Check
    {
        #region 列挙体
        // エラーコード
        public enum ERRCODE
        {
            ERR_NONE = 0,    // なし
            ERR_BLANK = 1,   // NULL or ブランク
            ERR_UNKNOWN = 2, // 判読不明
            ERR_NG = 4,      // 内容不備
            ERR_LEN = 8,     // 長さ超過
            ERR_REDEF = 16,  // 重複
            ERR_GAIJI = 32,  // 外字を含む
            ERR_MAX = 64     // 最大値
        }
        #endregion

        #region 定数
        // 半角判読不明文字
        private const string HAN_NGCHAR = "?";

        // 全角判読不明文字
        private const string ZEN_NGCHAR = "？";

        // 外字
        private const string GAIJI = "●";
        #endregion

        #region 数値検査
        //************************************************************************************************************
        // 処理名        : 十進数字検査
        // 処理概要      : 文字列が数字か調べる
        // 引数          : data                  検査文字列
        //                 maxlength             最大文字数
        //                 maxbytes              最大バイト数
        // 戻り値        : ERRCODE.ERR_NONE      エラー無し
        //                 ERRCODE.ERR_BLANK      空文字列
        //                 ERRCODE.ERR_LEN       バイト数超過
        //                 ERRCODE.ERR_UNKNOWN   判読不明文字有
        //                 ERRCODE.ERR_NG        形式不正
        //************************************************************************************************************
        public static long CHF_Decimal(object Data, int maxlength = 0, int maxbytes = 0)
        {
            long result = (long)ERRCODE.ERR_NONE;

            string strVal = Data.ToString();

            // NULL検査
            if (string.IsNullOrEmpty(strVal))
            {
                return (long)ERRCODE.ERR_BLANK;
            }

            // 文字列長検査
            if (maxlength != 0 && strVal.Length > maxlength)
            {
                result |= (long)ERRCODE.ERR_LEN;
            }

            // バイト長検査
            if (maxbytes != 0 && System.Text.Encoding.Default.GetByteCount(strVal) > maxbytes)
            {
                result |= (long)ERRCODE.ERR_LEN;
            }

            // 判読不明文字検査
            if (strVal.Contains(HAN_NGCHAR) || strVal.Contains(ZEN_NGCHAR))
            {
                result |= (long)ERRCODE.ERR_UNKNOWN;
            }

            // 文字列を半角に変換
            strVal = strVal.Normalize(System.Text.NormalizationForm.FormKC); // 半角化（例: 全角→半角）

            // １文字ずつ数字か検査
            foreach (char ch in strVal)
            {
                if (!char.IsDigit(ch))
                {
                    result |= (long)ERRCODE.ERR_NG;
                    break;
                }
            }

            return result;
        }
        #endregion

        #region 日付検査

        //************************************************************************************************************
        // 処理名        : 日付検査
        // 処理概要      : 文字列が日付か調べる
        // 引数          : data                  検査文字列
        // 戻り値        : ERRCODE.ERR_NONE      エラー無し
        //                 ERRCODE.ERR_BLANK      空文字列
        //                 ERRCODE.ERR_LEN       長さ超過
        //                 ERRCODE.ERR_UNKNOWN   判読不明文字有
        //                 ERRCODE.ERR_NG        日付不正
        //************************************************************************************************************
        public static long CHF_Date(object Data)
        {
            long result = (long)ERRCODE.ERR_NONE;

            string strVal = Data.ToString();

            // 文字列検査（Null・判読不明文字）
            result = CHF_String(strVal);
            if (result != (long)ERRCODE.ERR_NONE)
            {
                return result; // 各エラー値を返す
            }

            // 長さ検査
            switch (System.Text.Encoding.Default.GetByteCount(strVal))
            {
                case 8:
                    // この時点では正常
                    break;

                case 10:
                    // スラッシュ区切り
                    string[] stArrayData = strVal.Split('/');

                    // スラッシュがある場合、yyyy/mm/ddの場合に限り除去する
                    if (stArrayData.Length != 3)
                    {
                        return (long)ERRCODE.ERR_NG; // スラッシュの数がおかしい
                    }
                    if (stArrayData[0].Length != 4)
                    {
                        return (long)ERRCODE.ERR_NG; // 年（yyyy）部分の桁数がおかしい
                    }
                    if (stArrayData[1].Length != 2)
                    {
                        return (long)ERRCODE.ERR_NG; // 月（mm）部分の桁数がおかしい
                    }
                    if (stArrayData[2].Length != 2)
                    {
                        return (long)ERRCODE.ERR_NG; // 日（dd）部分の桁数がおかしい
                    }

                    // 正しい場合はスラッシュを除去した値を格納
                    strVal = stArrayData[0] + stArrayData[1] + stArrayData[2];
                    break;

                default:
                    return (long)ERRCODE.ERR_LEN; // 長さ超過
            }

            // 数値検査
            result = CHF_Decimal(strVal);
            if (result != (long)ERRCODE.ERR_NONE)
            {
                return result;
            }

            const string pFormat = "yyyyMMdd";

            // 日付検査
            DateTime dt;
            if (!DateTime.TryParseExact(strVal, pFormat, null, System.Globalization.DateTimeStyles.None, out dt))
            {
                return (long)ERRCODE.ERR_NG; // 日付不正
            }

            return result;
        }

        #endregion

        #region 時刻検査
        //************************************************************************************************************
        // 処理名        : 時刻検査
        // 処理概要      : 文字列が時刻か調べる
        // 引数          : data                  検査文字列
        // 戻り値        : ERRCODE.ERR_NONE      エラー無し
        //                 ERRCODE.ERR_BLANK      空文字列
        //                 ERRCODE.ERR_LEN       長さ超過
        //                 ERRCODE.ERR_UNKNOWN   判読不明文字有
        //                 ERRCODE.ERR_NG        時刻不正
        //************************************************************************************************************
        public static long CHF_TIME(object Data)
        {
            long result = (long)ERRCODE.ERR_NONE;

            // ":" を取り除く
            string strVal = Data.ToString().Replace(":", "");

            // 数値検査
            result = CHF_Decimal(strVal, 0, 4);
            if (result != (long)ERRCODE.ERR_NONE)
            {
                return result;
            }

            if (strVal.Length != 4)
            {
                return (long)ERRCODE.ERR_NG;
            }

            // 一度、数値型に格納
            if (!int.TryParse(strVal, out int parsedResult))
            {
                // 数値検査済みなのでここには入らないはず
                return (long)ERRCODE.ERR_NG;
            }

            // 時刻検査
            DateTime dt;
            if (!DateTime.TryParse(Data.ToString() + ":00", out dt))
            {
                return (long)ERRCODE.ERR_NG;
            }

            return result;
        }
        #endregion

        #region 文字列検査
        //************************************************************************************************************
        // 処理名        : 文字列検査
        // 処理概要      : 文字列の状態を調べる
        // 引数          : data                  検査文字列
        //                 maxlength             最大文字数
        //                 maxbytes              最大バイト数
        // 戻り値        : ERRCODE.ERR_NONE      エラー無し
        //                 ERRCODE.ERR_BLANK      空文字列
        //                 ERRCODE.ERR_LEN       長さ超過
        //                 ERRCODE.ERR_UNKNOWN   判読不明文字有
        //************************************************************************************************************
        public static long CHF_String(object Data, int maxlength = 0, int maxbytes = 0)
        {
            long result = (long)ERRCODE.ERR_NONE;

            string strVal = Data.ToString();

            // NULL検査
            if (string.IsNullOrEmpty(strVal))
            {
                return (long)ERRCODE.ERR_BLANK;
            }

            // 文字列長検査
            if (maxlength != 0 && strVal.Length > maxlength)
            {
                result |= (long)ERRCODE.ERR_LEN;
            }

            // バイト長検査
            if (maxbytes != 0 && System.Text.Encoding.Default.GetByteCount(strVal) > maxbytes)
            {
                result |= (long)ERRCODE.ERR_LEN;
            }

            // 判読不明文字検査
            if (strVal.IndexOf(HAN_NGCHAR) > -1 || strVal.IndexOf(ZEN_NGCHAR) > -1)
            {
                result |= (long)ERRCODE.ERR_UNKNOWN;
            }

            // 外字検査
            if (strVal.IndexOf(GAIJI) > -1)
            {
                result |= (long)ERRCODE.ERR_GAIJI;
            }

            return result;
        }
        #endregion

        #region 半角文字検査
        //************************************************************************************************************
        // 処理名        : 半角文字検査
        // 処理概要      : 文字列が半角文字のみで構成されているか調べる
        // 引数          : data                  検査文字列
        // 戻り値        : ERRCODE.ERR_NONE      エラー無し
        //                 ERRCODE.ERR_BLANK      空文字列
        //                 ERRCODE.ERR_NG        全角文字有
        //************************************************************************************************************
        public static long CHF_Narrow(object Data)
        {
            long result = (long)ERRCODE.ERR_NONE;

            string strVal = Data.ToString();

            // NULL検査
            if (string.IsNullOrEmpty(strVal))
            {
                return (long)ERRCODE.ERR_BLANK;
            }

            // 半角文字検査（バイト長が文字列長と一致しない場合、全角文字が含まれている）
            if (strVal.Length != System.Text.Encoding.Default.GetByteCount(strVal))
            {
                return (long)ERRCODE.ERR_NG;
            }

            return result;
        }
        #endregion

        #region 全角文字検査
        //************************************************************************************************************
        // 処理名        : 全角文字検査
        // 処理概要      : 文字列が全角文字のみで構成されているか調べる
        // 引数          : data                  検査文字列
        // 戻り値        : ERRCODE.ERR_NONE      エラー無し
        //                 ERRCODE.ERR_BLANK      空文字列
        //                 ERRCODE.ERR_NG        半角文字有
        //**************************************************
        public static int CHF_Wide(object Data)
        {
            int result = (int)ERRCODE.ERR_NONE;

            string strVal = Data.ToString();

            // NULL検査
            if (string.IsNullOrEmpty(strVal))
            {
                return (int)ERRCODE.ERR_BLANK;
            }

            // 全角文字検査（文字列長の2倍がバイト長と一致しない場合、半角文字が含まれている）
            if (strVal.Length * 2 != System.Text.Encoding.Default.GetByteCount(strVal))
            {
                return (int)ERRCODE.ERR_NG;
            }

            return result;
        }
        #endregion
    }
}
