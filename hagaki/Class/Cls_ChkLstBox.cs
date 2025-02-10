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
    internal class Cls_ChkLstBox
    {
        #region チェックリストボックスの項目にテーブルの値をセットする
        /// <summary>
        /// コンボボックスの項目にテーブルの値をセットする
        /// </summary>
        /// <param name="ChkLst">チェックボックス</param>
        /// <param name="TableName">テーブル名</param>
        /// <param name="ValueField">データ項目</param>
        /// <param name="TextField">表示項目</param>
        /// <param name="Sort">並び替え項目</param>
        /// <param name="Filter">WHERE条件</param>
        /// <param name="First_Val">コンボに設定するデータ項目_一番上</param>
        /// <param name="First_Txt">コンボに設定する表示項目_一番上</param>
        /// <param name="Last_Val">コンボに設定するデータ項目_一番下</param>
        /// <param name="Last_Txt">コンボに設定する表示項目_一番下</param>
        public void Set_ChkLstBox(ref CheckedListBox ChkLst,
                           string TableName,
                           string ValueField,
                           string TextField,
                           string Sort = "",
                           string Filter = "",
                           string First_Val = "",
                           string First_Txt = "",
                           string Last_Val = "",
                           string Last_Txt = "")
        {
            DataTable chklstDT = new DataTable();
            BindingSource bindingSource1 = new BindingSource();
            DataRow row;

            ChkLst.DataSource = null;

            // DataTableに列を追加
            chklstDT.Columns.Add(ValueField, typeof(string));
            chklstDT.Columns.Add(TextField, typeof(string));
            bindingSource1.DataSource = chklstDT;

            // 個別に設定する項目があれば追加
            if (!string.IsNullOrEmpty(First_Txt))
            {
                // DataTableに行を追加
                row = chklstDT.NewRow();
                row[ValueField] = First_Val;
                row[TextField] = First_Txt;
                chklstDT.Rows.Add(row);
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

            // DB操作関係クラスを使って接続
            Cls_DBConn cDB = new Cls_DBConn(StCls_Function.Get_SQLConnectString()); // クラスインスタンス生成
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
                            row = chklstDT.NewRow();
                            row[ValueField] = sDR[ValueField].ToString();
                            row[TextField] = sDR[TextField].ToString();
                            chklstDT.Rows.Add(row);
                        }
                    }
                }
                catch (Exception)
                {
                    //return;
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
            if (!string.IsNullOrEmpty(Last_Txt))
            {
                // DataTableに行を追加
                row = chklstDT.NewRow();
                row[ValueField] = Last_Val;
                row[TextField] = Last_Txt;
                chklstDT.Rows.Add(row);
            }

            // データテーブルのコミット
            chklstDT.AcceptChanges();

            // DataSourceにDataTableを設定
            ChkLst.DataSource = bindingSource1;

            // TextFieldを項目の表示に利用する
            ChkLst.DisplayMember = TextField;

            // ValueFieldをSelectedValueで取得、設定できる値に利用する
            ChkLst.ValueMember = ValueField;

            if (ChkLst.MultiColumn)
            {
                int maxSize = ChkLst.ColumnWidth;
                foreach (Object item in ChkLst.Items)
                {
                    // ComboBoxのフォントを使ってサイズ計測
                    maxSize = Math.Max(maxSize, TextRenderer.MeasureText(((DataRowView)item)[1].ToString(), ChkLst.Font).Width);
                }

                ChkLst.ColumnWidth = maxSize + 20;
            }

            // 未選択状態にしておく
            ChkLst.SelectedIndex = -1;
        }
        #endregion
    }
}
