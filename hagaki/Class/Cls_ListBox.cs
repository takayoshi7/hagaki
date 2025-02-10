using System;
using System.Data.SqlClient;
using System.Data;
using System.Text;
using System.Windows.Forms;
using hagaki.StaticClass;

// ---------------------------------------------
//  クラス名   : Cls_ChkLstBox
//  概要　　　 : チェックリストボックス操作関係
//  作成日　　 : 2025/02/05
//  作成者　　 : 高橋
//  最終更新日 : 
//  最終更新者 : 
// ---------------------------------------------

namespace hagaki.Class
{
    internal class Cls_ListBox
    {
        #region リストボックスの項目にテーブルの値をセットする
        /// <summary>
        /// コンボボックスの項目にテーブルの値をセットする
        /// </summary>
        /// <param name="chkLst">リストボックス</param>
        /// <param name="tableName">テーブル名</param>
        /// <param name="valueField">データ項目</param>
        /// <param name="textField">表示項目</param>
        /// <param name="sort">並び替え項目</param>
        /// <param name="filter">WHERE条件</param>
        /// <param name="firstVal">コンボに設定するデータ項目_一番上</param>
        /// <param name="firstTxt">コンボに設定する表示項目_一番上</param>
        /// <param name="lastVal">コンボに設定するデータ項目_一番下</param>
        /// <param name="lastTxt">コンボに設定する表示項目_一番下</param>
        public void SetChkLstBox(ref ListBox chkLst,
                                  string tableName,
                                  string valueField,
                                  string textField,
                                  string sort = "",
                                  string filter = "",
                                  string firstVal = "",
                                  string firstTxt = "",
                                  string lastVal = "",
                                  string lastTxt = "")
        {
            DataTable chkLstDT = new DataTable();
            BindingSource bindingSource1 = new BindingSource();
            DataRow row;

            chkLst.DataSource = null;

            // DataTableに列を追加
            chkLstDT.Columns.Add(valueField, typeof(string));
            chkLstDT.Columns.Add(textField, typeof(string));
            bindingSource1.DataSource = chkLstDT;

            // 個別に設定する項目があれば追加
            if (!string.IsNullOrEmpty(firstTxt))
            {
                // DataTableに行を追加
                row = chkLstDT.NewRow();
                row[valueField] = firstVal;
                row[textField] = firstTxt;
                chkLstDT.Rows.Add(row);
            }

            // SQL文生成
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT ");
            sb.Append(valueField + ", ");
            sb.Append(textField + " ");
            sb.Append("FROM ");
            sb.Append(tableName + " ");

            // 条件が設定されていたら付加する
            if (!string.IsNullOrEmpty(filter))
            {
                sb.Append("WHERE ");
                sb.Append(filter + " ");
            }

            if (!string.IsNullOrEmpty(sort))
            {
                sb.Append("ORDER BY ");
                sb.Append(sort);
            }

            Cls_DBConn cDB = new Cls_DBConn(StCls_Function.Get_SQLConnectString()); // DB接続関係クラス

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
                            row = chkLstDT.NewRow();
                            row[valueField] = sDR[valueField].ToString();
                            row[textField] = sDR[textField].ToString();
                            chkLstDT.Rows.Add(row);
                        }
                    }
                }
                catch (Exception)
                {
                    // 例外処理（必要に応じてログ出力等）
                }
                finally
                {
                    sDR.Close();
                    sDR = null;
                    sb.Length = 0;
                }
            }

            cDB = null;

            // 個別に設定する項目があれば追加
            if (!string.IsNullOrEmpty(lastTxt))
            {
                // DataTableに行を追加
                row = chkLstDT.NewRow();
                row[valueField] = lastVal;
                row[textField] = lastTxt;
                chkLstDT.Rows.Add(row);
            }

            // データテーブルのコミット
            chkLstDT.AcceptChanges();

            // DataSourceにDataTableを設定
            chkLst.DataSource = bindingSource1;

            // TextFieldを項目の表示に利用する
            chkLst.DisplayMember = textField;

            // ValueFieldをSelectedValueで取得、設定できる値に利用する
            chkLst.ValueMember = valueField;

            // MultiColumnの場合、項目の幅を調整
            if (chkLst.MultiColumn)
            {
                int maxSize = chkLst.ColumnWidth;
                foreach (object item in chkLst.Items)
                {
                    // ComboBoxのフォントを使ってサイズ計測
                    maxSize = Math.Max(maxSize, TextRenderer.MeasureText(((DataRowView)item)[1].ToString(), chkLst.Font).Width);
                }

                chkLst.ColumnWidth = maxSize + 20;
            }

            // 未選択状態にしておく
            chkLst.SelectedIndex = -1;
        }
        #endregion
    }
}
