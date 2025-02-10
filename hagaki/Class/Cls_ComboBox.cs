using System;
using System.Data.SqlClient;
using System.Data;
using System.Text;
using System.Windows.Forms;
using hagaki.StaticClass;

// ---------------------------------------------
//  クラス名   : Cls_ComboBox
//  概要　　　 : コンボボックス操作関係
//  作成日　　 : 2025/02/05
//  作成者　　 : 高橋
//  最終更新日 : 
//  最終更新者 : 
// ---------------------------------------------

namespace hagaki.Class
{
    internal class Cls_ComboBox
    {
        #region 列挙体
        /// <summary>
        /// ブランク判定用
        /// </summary>
        public enum BLANK_FIELD
        {
            BLANK = 0,
            NOT_BLANK
        }
        #endregion

        #region コンボボックスの項目にテーブルの値をセットする
        /// <summary>
        /// コンボボックスの項目にテーブルの値をセットする
        /// </summary>
        /// <param name="Combo">コンボボックス</param>
        /// <param name="TableName">テーブル名</param>
        /// <param name="ValueField">データ項目</param>
        /// <param name="TextField">表示項目</param>
        /// <param name="Sort">並び替え項目</param>
        /// <param name="Filter">WHERE条件</param>
        /// <param name="BlankField">第一インデックスをブランクにするか</param>
        public void Set_ComboBox(ref ComboBox Combo,
                         string TableName,
                         string ValueField,
                         string TextField,
                         string Sort = "",
                         string Filter = "",
                         BLANK_FIELD BlankField = BLANK_FIELD.BLANK)
        {
            DataTable comboDT = new DataTable();
            DataRow row;

            Combo.DataSource = null;

            // DataTableに列を追加
            comboDT.Columns.Add(ValueField, typeof(string));
            comboDT.Columns.Add(TextField, typeof(string));

            // 1行目がブランク設定なら、ブランクを挿入
            if (BlankField == BLANK_FIELD.BLANK)
            {
                row = comboDT.NewRow();
                row[ValueField] = "";
                row[TextField] = "";
                comboDT.Rows.Add(row);
            }

            // SQL文生成
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT ");
            sb.Append(ValueField + ", ");
            sb.Append(TextField + " ");
            sb.Append("FROM ");
            sb.Append(TableName + " ");

            // 条件が設定されていたら付加する
            if (!string.IsNullOrEmpty(Filter))
            {
                sb.Append("WHERE ");
                sb.Append(Filter + " ");
            }

            if (!string.IsNullOrEmpty(Sort))
            {
                sb.Append("ORDER BY ");
                sb.Append(Sort);
            }

            Cls_DBConn cDB = new Cls_DBConn(StCls_Function.Get_SQLConnectString());  // DB操作関係クラス
                                                                    // DB接続
            using (SqlConnection mConn = cDB.SetDBConnction())
            {
                // DB接続確認
                if (mConn == null)
                {
                    return;
                }

                SqlDataReader sDR = cDB.SetDataReader(mConn, sb.ToString());
                try
                {
                    if (sDR.HasRows)
                    {
                        while (sDR.Read())
                        {
                            // DataTableに行を追加
                            row = comboDT.NewRow();
                            row[ValueField] = sDR[ValueField].ToString();
                            row[TextField] = sDR[TextField].ToString();
                            comboDT.Rows.Add(row);
                        }
                    }
                }
                catch (Exception)
                {
                    // エラーハンドリング（必要に応じて）
                }
                finally
                {
                    sDR.Close();
                    sDR = null;
                    sb.Length = 0;
                }
            }
            cDB = null;

            // データテーブルのコミット
            comboDT.AcceptChanges();

            // TextFieldを項目の表示に利用する
            Combo.DisplayMember = TextField;

            // ValueFieldをSelectedValueで取得、設定できる値に利用する
            Combo.ValueMember = ValueField;

            // DataSourceにDataTableを設定
            Combo.DataSource = comboDT;

            // 幅調整
            int maxSize = 0;
            foreach (Object item in Combo.Items)
            {
                // ComboBoxのフォントを使ってサイズ計測 
                maxSize = Math.Max(maxSize, TextRenderer.MeasureText(((DataRowView)item)[1].ToString(), Combo.Font).Width);
            }

            // リストが多い場合はスクロールバーが出るため、その分を追加する（固定20px）
            maxSize += 20;

            // 現在の設定より大きければ置換 
            if (Combo.DropDownWidth < maxSize)
            {
                Combo.DropDownWidth = maxSize;
            }

            // 未選択状態にしておく
            if (Combo.Items.Count > 0)
            {
                Combo.SelectedIndex = 0;
            }
        }
        #endregion

        #region コンボボックスの項目にテーブルの値をセットする（コードも表示させる）
        /// <summary>
        /// コンボボックスの項目にテーブルの値をセットする（コードも表示させる）
        /// </summary>
        /// <param name="Combo">コンボボックス</param>
        /// <param name="TableName">テーブル名</param>
        /// <param name="ValueField">データ項目</param>
        /// <param name="TextField">表示項目</param>
        /// <param name="Sort">並び替え項目</param>
        /// <param name="Filter">WHERE条件</param>
        /// <param name="BlankField">第一インデックスをブランクにするか</param>
        public void Set_ComboBox_Disp(ref ComboBox Combo,
                                       string TableName,
                                       string ValueField,
                                       string TextField,
                                       string Sort = "",
                                       string Filter = "",
                                       BLANK_FIELD BlankField = BLANK_FIELD.BLANK)
        {
            DataTable comboDT = new DataTable();
            DataRow row;

            // DataTableに列を追加
            comboDT.Columns.Add(ValueField, typeof(string));
            comboDT.Columns.Add(TextField, typeof(string));

            // 1行目がブランク設定なら、ブランクを挿入
            if (BlankField == BLANK_FIELD.BLANK)
            {
                row = comboDT.NewRow();
                row[ValueField] = "";
                row[TextField] = "";
                comboDT.Rows.Add(row);
            }

            // SQL文生成
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT ");
            sb.Append(ValueField + ", ");
            sb.Append(ValueField + " + '：' + " + TextField + " AS DISP ");
            sb.Append("FROM ");
            sb.Append(TableName + " ");

            // 条件が設定されていたら付加する
            if (!string.IsNullOrEmpty(Filter))
            {
                sb.Append("WHERE ");
                sb.Append(Filter + " ");
            }

            if (!string.IsNullOrEmpty(Sort))
            {
                sb.Append("ORDER BY ");
                sb.Append(Sort);
            }

            Cls_DBConn cDB = new Cls_DBConn(StCls_Function.Get_SQLConnectString());  // DB操作関係クラス
                                                                    // DB接続
            using (SqlConnection mConn = cDB.SetDBConnction())
            {
                // DB接続確認
                if (mConn == null)
                {
                    return;
                }

                SqlDataReader sDR = cDB.SetDataReader(mConn, sb.ToString());
                try
                {
                    if (sDR.HasRows)
                    {
                        while (sDR.Read())
                        {
                            // DataTableに行を追加
                            row = comboDT.NewRow();
                            row[ValueField] = sDR[ValueField].ToString();
                            row[TextField] = sDR["DISP"].ToString();
                            comboDT.Rows.Add(row);
                        }
                    }
                }
                catch (Exception)
                {
                    // エラーハンドリング（必要に応じて）
                }
                finally
                {
                    sDR.Close();
                    sDR = null;
                    sb.Length = 0;
                }
            }
            cDB = null;

            // データテーブルのコミット
            comboDT.AcceptChanges();

            // TextFieldを項目の表示に利用する
            Combo.DisplayMember = TextField;

            // ValueFieldをSelectedValueで取得、設定できる値に利用する
            Combo.ValueMember = ValueField;

            // DataSourceにDataTableを設定
            Combo.DataSource = comboDT;

            // 未選択状態にしておく
            Combo.SelectedIndex = -1;
        }
        #endregion

        #region コンボボックスの項目に任意の値をセットする
        /// <summary>
        /// コンボボックスの項目に任意の値をセットする
        /// </summary>
        /// <param name="Combo">コンボボックス</param>
        /// <param name="ValueField">データ項目</param>
        /// <param name="TextField">表示項目</param>
        /// <param name="aryValue">データ内容（カンマ区切り）</param>
        /// <param name="aryText">表示内容（カンマ区切り）</param>
        public void Set_ComboBox_Free(ref ComboBox Combo,
                                       string ValueField,
                                       string TextField,
                                       string aryValue,
                                       string aryText)
        {
            DataTable comboDT = new DataTable();
            DataRow row;

            // カンマ区切りで分割して配列に格納
            string[] strVal = aryValue.Split(',');
            string[] strTxt = aryText.Split(',');

            // 要素数が同一かチェック
            if (strVal.Length != strTxt.Length)
            {
                // 指定がおかしい場合は処理を終了
                return;
            }

            // DataTableに列を追加
            comboDT.Columns.Add(ValueField, typeof(string));
            comboDT.Columns.Add(TextField, typeof(string));

            // DataTableに行を追加
            for (int intLoop = 0; intLoop < strVal.Length; intLoop++)
            {
                row = comboDT.NewRow();
                row[ValueField] = strVal[intLoop];
                row[TextField] = strTxt[intLoop];
                comboDT.Rows.Add(row);
            }

            // データテーブルのコミット
            comboDT.AcceptChanges();

            // TextFieldを項目の表示に利用する
            Combo.DisplayMember = TextField;

            // ValueFieldをSelectedValueで取得、設定できる値に利用する
            Combo.ValueMember = ValueField;

            // DataSourceにDataTableを設定
            Combo.DataSource = comboDT;

            // 未選択状態にしておく
            Combo.SelectedIndex = -1;
        }
        #endregion

        #region コンボボックスの項目に任意の値をセットする（コードも表示させる）
        /// <summary>
        /// コンボボックスの項目に任意の値をセットする（コードも表示させる）
        /// </summary>
        /// <param name="Combo">コンボボックス</param>
        /// <param name="ValueField">データ項目</param>
        /// <param name="TextField">表示項目</param>
        /// <param name="aryValue">データ内容（カンマ区切り）</param>
        /// <param name="aryText">表示内容（カンマ区切り）</param>
        public void Set_ComboBox_Free_Disp(ref ComboBox Combo,
                                            string ValueField,
                                            string TextField,
                                            string aryValue,
                                            string aryText)
        {
            DataTable comboDT = new DataTable();
            DataRow row;

            // カンマ区切りで分割して配列に格納
            string[] strVal = aryValue.Split(',');
            string[] strTxt = aryText.Split(',');

            // 要素数が同一かチェック
            if (strVal.Length != strTxt.Length)
            {
                // 指定がおかしい場合は処理を終了
                return;
            }

            // DataTableに列を追加
            comboDT.Columns.Add(ValueField, typeof(string));
            comboDT.Columns.Add(TextField, typeof(string));

            // DataTableに行を追加
            for (int intLoop = 0; intLoop < strVal.Length; intLoop++)
            {
                row = comboDT.NewRow();
                row[ValueField] = strVal[intLoop];
                row[TextField] = strVal[intLoop] + "：" + strTxt[intLoop];
                comboDT.Rows.Add(row);
            }

            // データテーブルのコミット
            comboDT.AcceptChanges();

            // TextFieldを項目の表示に利用する
            Combo.DisplayMember = TextField;

            // ValueFieldをSelectedValueで取得、設定できる値に利用する
            Combo.ValueMember = ValueField;

            // DataSourceにDataTableを設定
            Combo.DataSource = comboDT;

            // 未選択状態にしておく
            Combo.SelectedIndex = -1;
        }
        #endregion

        #region コンボボックスの選択中のコード値を取得する
        /// <summary>
        /// コンボボックスの選択中のコード値を取得する
        /// </summary>
        /// <param name="Combo">コンボボックス</param>
        /// <returns>選択中のコード</returns>
        public string Get_Code(ref ComboBox Combo)
        {
            try
            {
                return Combo.SelectedValue.ToString();
            }
            catch
            {
                // SelectedIndexが-1とかありえるので
                return string.Empty;
            }
        }
        #endregion

        #region コンボボックスの選択中のテキスト値を取得する
        /// <summary>
        /// コンボボックスの選択中のテキスト値を取得する
        /// </summary>
        /// <param name="Combo">コンボボックス</param>
        /// <returns>選択中のテキスト</returns>
        public string Get_Text(ref ComboBox Combo)
        {
            try
            {
                return Combo.Text;
            }
            catch
            {
                // SelectedIndexが-1とかありえるので
                return string.Empty;
            }
        }
        #endregion

        #region コード値から指定コンボボックスを選択する
        /// <summary>
        /// コード値から指定コンボボックスを選択する
        /// </summary>
        /// <param name="Combo">コンボボックス</param>
        /// <param name="strCode">選択するコード</param>
        public void Set_Combo(ref ComboBox Combo, string strCode)
        {
            try
            {
                Combo.SelectedValue = strCode;
            }
            catch
            {
                // エラー
            }
        }
        #endregion
    }
}
