using System;
using System.Collections;
using System.Text;

// ---------------------------------------------
//  クラス名   : StCls_Extension
//  概要　　　 : 拡張メソッドをまとめたモジュール
//  作成日　　 : 2025/02/05
//  作成者　　 : 高橋
//  最終更新日 : 
//  最終更新者 : 
// ---------------------------------------------

namespace hagaki.StaticClass
{
    public static class StCls_Extension
    {
        // 【拡張メソッドとは】
        // 　変数からドットシンタックスで呼び出せるメソッドを開発者が実装できます。
        // 　例. 
        // 　　String型に拡張メソッド「Left」を実装した場合の呼び出し
        // 　　→　strName.Left(5)

        // **************************************************
        // 　String型　拡張メソッド（★既存関数　不具合対応版★）
        // **************************************************

        #region 　　+　Function：　ExIndexOf（IndexOf）
        /// <summary>
        /// ExIndexOf（IndexOf）
        /// </summary>
        /// <param name="str">調査対象の文字列</param>
        /// <param name="value">検索する文字列</param>
        /// <returns>検索結果のインデックス</returns>
        public static int ExIndexOf(this string str, string value)
        {
            // 既存のIndexOfメソッドは、半角濁音/半濁音、全角濁音/半濁音の混同により正しく動作しない場合がある。
            // この関数はComparison指定で半角全角を識別し、正しく評価する
            return str.IndexOf(value, StringComparison.Ordinal);
        }
        #endregion

        #region 　　+　Function：　ExLastIndexOf（LastIndexOf）
        /// <summary>
        /// ExLastIndexOf（LastIndexOf）
        /// </summary>
        /// <param name="str">調査対象の文字列</param>
        /// <param name="value">検索する文字列</param>
        /// <returns>最後に見つかったインデックス</returns>
        public static int ExLastIndexOf(this string str, string value)
        {
            // 既存のLastIndexOfメソッドは、半角濁音/半濁音、全角濁音/半濁音の混同により正しく動作しない場合がある。
            // この関数はComparison指定で半角全角を識別し、正しく評価する
            return str.LastIndexOf(value, StringComparison.Ordinal);
        }
        #endregion

        #region 　　+　Function：　ExStartsWith（StartsWith）
        /// <summary>
        /// ExStartsWith（StartsWith）
        /// </summary>
        /// <param name="str">調査対象の文字列</param>
        /// <param name="value">検索する文字列</param>
        /// <returns>文字列が指定した値で始まるかどうか</returns>
        public static bool ExStartsWith(this string str, string value)
        {
            // 既存のStartsWithメソッドは、半角濁音/半濁音、全角濁音/半濁音の混同により正しく動作しない場合がある。
            // この関数はComparison指定で半角全角を識別し、正しく評価する
            return str.StartsWith(value, StringComparison.Ordinal);
        }
        #endregion

        #region 　　+　Function：　ExEndsWith（EndsWith）
        /// <summary>
        /// ExEndsWith（EndsWith）
        /// </summary>
        /// <param name="str">調査対象の文字列</param>
        /// <param name="value">検索する文字列</param>
        /// <returns>文字列が指定した値で終わるかどうか</returns>
        public static bool ExEndsWith(this string str, string value)
        {
            // StartsWith と違い、EndsWith は正常に動作していたが、足並みを揃えるために ExEndsWith を使用
            return str.EndsWith(value, StringComparison.Ordinal);
        }
        #endregion

        // **************************************************
        // 　String型　拡張メソッド
        // **************************************************

        #region 　　+　Function：　ContainesIn　（String.Containes拡張）
        /// <summary>
        /// String.Contains拡張。指定値のいずれかが含まれるか確認する。
        /// </summary>
        /// <param name="str">調査対象の文字列</param>
        /// <param name="valueDelim">確認する値（カンマ区切り複数可）</param>
        /// <returns>指定値のいずれかが含まれている場合は true、それ以外は false</returns>
        public static bool ContainesIn(this string str, string valueDelim)
        {
            // 不正な引数チェック
            if (string.IsNullOrEmpty(str)) return false;
            if (string.IsNullOrEmpty(valueDelim)) return false;

            // カンマ区切りで分割して処理
            string[] values = valueDelim.Split(',');
            return str.ContainesIn(values);
        }

        /// <summary>
        /// String.Contains拡張。指定値のいずれかが含まれるか確認する。
        /// </summary>
        /// <param name="str">調査対象の文字列</param>
        /// <param name="values">確認する値（コレクションなら何でも受け取る）</param>
        /// <returns>指定値のいずれかが含まれている場合は true、それ以外は false</returns>
        public static bool ContainesIn(this string str, IEnumerable values)
        {
            // 不正な引数チェック
            if (string.IsNullOrEmpty(str)) return false;
            if (values == null) return false;

            // valuesコレクション内のいずれかの値がstrに含まれているかをチェック
            foreach (Object value in values)
            {
                if (str.Contains(value.ToString())) return true;
            }

            return false;
        }
        #endregion

        #region 　　+　Function：　EmptyTo　（NtoVの代替）
        /// <summary>
        /// NtoV の代替
        /// </summary>
        /// <param name="str">対象の文字列</param>
        /// <param name="afterValue">文字列が空またはnullの場合に返す値</param>
        /// <returns>空またはnullの場合は afterValue、それ以外は str</returns>
        public static string EmptyTo(this string str, string afterValue)
        {
            // 文字列が null または 空の場合は afterValue を返す
            if (string.IsNullOrEmpty(str)) return afterValue;
            return str;
        }
        #endregion

        #region 　　+　Function：　EscapeQuote　（CnvQuartの代替）
        /// <summary>
        /// CnvQuart の代替
        /// </summary>
        /// <param name="str">対象の文字列</param>
        /// <returns>シングルクォートをエスケープした文字列</returns>
        public static string EscapeQuote(this string str)
        {
            // 文字列が null または 空の場合は空文字列を返す
            if (string.IsNullOrEmpty(str)) return string.Empty;

            // シングルクォートを2つのシングルクォートに置き換え
            return str.Replace("'", "''");
        }
        #endregion

        #region 　　+　Function：　GetByteCount　（LenBの代替）
        /// <summary>
        /// LenB の代替
        /// </summary>
        /// <param name="str">対象の文字列</param>
        /// <returns>指定した文字列のバイト数</returns>
        public static int GetByteCount(this string str)
        {
            // 文字列が null または 空の場合は 0 を返す
            if (string.IsNullOrEmpty(str)) return 0;

            // Shift_JIS エンコーディングで文字列のバイト数を取得
            return Encoding.GetEncoding("Shift_JIS").GetByteCount(str);
        }
        #endregion

        #region 　　+　Function：　Left　（VbLeftの代替）
        /// <summary>
        /// VbLeft の代替
        /// </summary>
        /// <param name="str">対象の文字列</param>
        /// <param name="length">取得する文字数（String.Length超過時は全桁を返す）</param>
        /// <returns>指定された長さ分の文字列</returns>
        public static string Left(this string str, int length)
        {
            // GUARD: str が null の場合
            if (str == null) return string.Empty;

            // GUARD: length が 1 未満の場合
            if (length < 1) return string.Empty;

            // GUARD: 指定文字数が文字列長を超過している場合
            if (str.Length <= length) return str;

            return str.Substring(0, length);
        }
        #endregion

        #region 　　+　Function：　Right　（VbRightの代替）
        /// <summary>
        /// VbRight の代替
        /// </summary>
        /// <param name="str">対象の文字列</param>
        /// <param name="length">取得する文字数（String.Length超過時は全桁を返す）</param>
        /// <returns>指定された長さ分の文字列</returns>
        public static string Right(this string str, int length)
        {
            if (str == null) return string.Empty;
            if (length < 1) return string.Empty;

            // 指定文字数が文字列長を超過している場合
            if (str.Length < length) return str;

            return str.Substring(str.Length - length, length);
        }
        #endregion

        #region 　　+　Function：　ToDateFormat（日付の書式に変えちゃうよ）★要精査★
        /// <summary>
        /// 8桁の数値文字列または時刻型の日付を、YYYY/MM/DDの形式に変換する
        /// </summary>
        /// <param name="str">対象のオブジェクト</param>
        /// <returns>変換した日付形式の文字列または元の値</returns>
        public static string ToDateFormat(this object str)
        {
            int intResult;
            DateTime dateResult;

            // 日付として解析できる場合
            if (DateTime.TryParse(str.ToString(), out dateResult))
            {
                return dateResult.ToString("yyyy/MM/dd");
            }

            // 8桁の数値文字列かチェック
            string strValue = str.ToString();
            if (strValue.Length != 8 || !int.TryParse(strValue, out intResult))
            {
                return str.ToString(); // 変換できない場合は元の値を返す
            }

            // 8桁の日付文字列の場合、YYYY/MM/DD形式に変換
            return strValue.Substring(0, 4) + "/" + strValue.Substring(4, 2) + "/" + strValue.Substring(6, 2);
        }
        #endregion

        //**************************************************
        //　Date型　拡張メソッド
        //**************************************************

        #region 　　+　Function：　ToUkeDateString　「yyyyMMdd（受付日）」形式の文字列に変換する
        /// <summary>
        /// 「yyyyMMdd（受付日）」形式の文字列に変換する
        /// </summary>
        /// <param name="dt">対象の日付</param>
        /// <returns>yyyyMMdd形式の文字列</returns>
        /// <remarks></remarks>
        public static string ToUkeDateString(this DateTime dt)
        {
            return dt.ToString("yyyyMMdd");
        }
        #endregion

        #region 　　+　Function：　ToFileString　「yyyyMMdd_HHmmss（ファイル名修飾）」形式の文字列に変換する
        /// <summary>
        /// 「yyyyMMdd_HHmmss（ファイル名修飾）」形式の文字列に変換する
        /// </summary>
        /// <param name="dt">対象の日付</param>
        /// <returns>yyyyMMdd_HHmmss形式の文字列</returns>
        /// <remarks></remarks>
        public static string ToFileString(this DateTime dt)
        {
            return dt.ToString("yyyyMMdd_HHmmss");
        }
        #endregion
    }
}
