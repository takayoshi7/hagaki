using hagaki.StaticClass;
using System;
using System.Data.SqlClient;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

// ---------------------------------------------
//  クラス名   : Cls_DataGrid
//  概要　　　 : データグリッド操作クラス
//  作成日　　 : 2025/02/05
//  作成者　　 : 高橋
//  最終更新日 : 
//  最終更新者 : 
// ---------------------------------------------

namespace hagaki.Class
{
    internal class Cls_DataGrid
    {
        #region コンストラクタ
        public Cls_DataGrid()
        {
            // デフォルトコンストラクタ
            // 引数なし
        }

        // 【v1.0.1】
        public Cls_DataGrid(DataGridView target)
        {
            // 初期化時にGridViewを指定
            DataGridObj = target;
        }
        #endregion

        #region プロパティ

        public DataGridView DataGridObj { get; set; }

        /// <summary>
        /// グリッドビューの現在のMax行数
        /// </summary>
        public int MaxRows
        {
            get
            {
                // 現在の行数を返す
                return DataGridObj.Rows.Count;
            }
            set
            {
                int _MaxRows = value;
                int rowCount = DataGridObj.Rows.Count;

                if (_MaxRows > rowCount)
                {
                    // 現在の行数が指定行数未満なら、追加してあげる
                    DataGridObj.Rows.Add(_MaxRows - rowCount);
                }
                else if (_MaxRows < rowCount)
                {
                    // 描画を一時停止
                    DataGridObj.Invalidate();

                    // 現在の行数が指定行数以上なら、指定した行数になるまで最終行を削除する
                    while (_MaxRows != DataGridObj.Rows.Count)
                    {
                        DataGridObj.Rows.RemoveAt(DataGridObj.Rows.Count - 1);
                    }

                    // 描画再開
                    DataGridObj.Update();
                }
            }
        }

        /// <summary>
        /// グリッドビューの現在のMax列数
        /// </summary>
        public int MaxCols
        {
            get { return DataGridObj.Columns.Count; }
        }

        /// <summary>
        /// アクティブセルの行番号
        /// </summary>
        public int ActiveRow
        {
            get { return DataGridObj.CurrentCell.RowIndex; }
        }

        /// <summary>
        /// アクティブセルの列番号
        /// </summary>
        public int ActiveCol
        {
            get { return DataGridObj.CurrentCell.ColumnIndex; }
        }

        #endregion

        #region 初期設定
        /// <summary>
        /// 初期設定
        /// </summary>
        /// <param name="aRead">読取専用</param>
        /// <param name="aAdd">行追加の可否</param>
        /// <param name="aDelete">行削除の可否</param>
        /// <param name="aReSizeCol">列サイズ変更の可否</param>
        /// <param name="aReSizeRow">行サイズ変更の可否</param>
        /// <param name="aUserToOrderCol">列位置変更の可否</param>
        /// <param name="aAutoCol">列の自動生成</param>
        /// <param name="aMultiSelect">複数セル選択の可否</param>
        /// <param name="aSelectionMode">選択モード</param>
        /// <param name="aDrawRowNo">行番号描画の可否</param>
        public void InitSetting(
            bool aRead = false,
            bool aAdd = false,
            bool aDelete = false,
            bool aReSizeCol = false,
            bool aReSizeRow = false,
            bool aUserToOrderCol = false,
            bool aAutoCol = false,
            bool aMultiSelect = false,
            DataGridViewSelectionMode aSelectionMode = DataGridViewSelectionMode.CellSelect,
            bool aDrawRowNo = true)
        {
            DataGridObj.ReadOnly = aRead;                            // ReadOnlyにするか
            DataGridObj.AllowUserToAddRows = aAdd;                   // 行追加を許可するか
            DataGridObj.AllowUserToDeleteRows = aDelete;             // 行削除を許可するか
            DataGridObj.AllowUserToResizeColumns = aReSizeCol;       // 列サイズ変更を許可するか
            DataGridObj.AllowUserToResizeRows = aReSizeRow;          // 行サイズ変更を許可するか
            DataGridObj.AllowUserToOrderColumns = aUserToOrderCol;   // 列位置の変更を許可するか
            DataGridObj.AutoGenerateColumns = aAutoCol;              // 列を自動的に作成するか
            DataGridObj.MultiSelect = aMultiSelect;                  // 複数選択を許可するか
            DataGridObj.SelectionMode = aSelectionMode;              // 選択モードの設定

            if (aDrawRowNo)                                               // 行番号を描画するか
            {
                DataGridObj.CellPainting += DataGridObj_CellPainting;
            }
        }
        #endregion

        #region 行ヘッダーのセット
        /// <summary>
        /// 行ヘッダーのセット
        /// </summary>
        /// <param name="Col">列番号</param>
        /// <param name="HederName">ヘッダ文言</param>
        /// <param name="ColWidth">列幅</param>
        /// <param name="HAlign">水平位置</param>
        /// <param name="FontSize">フォントサイズ</param>
        /// <param name="sort_Mode">ソートモード</param>
        /// <param name="read_only">読取専用</param>
        /// <param name="Hide">列の非表示</param>
        /// <remarks>ソートモードONの場合は、水平位置をCenterにしてもインジケータの分ずれるので注意</remarks>
        public void SetHeader(
            int Col,
            string HederName,
            int ColWidth = 100,
            DataGridViewContentAlignment HAlign = DataGridViewContentAlignment.MiddleCenter,
            int FontSize = 9,
            DataGridViewColumnSortMode sort_Mode = DataGridViewColumnSortMode.Automatic,
            bool read_only = true,
            bool Hide = false)
        {
            DataGridViewColumn column = DataGridObj.Columns[Col];

            column.HeaderText = HederName;
            column.Width = ColWidth;
            column.HeaderCell.Style.Alignment = HAlign;
            column.DefaultCellStyle.Alignment = HAlign;
            column.HeaderCell.Style.Font = new Font(DataGridObj.DefaultCellStyle.Font.FontFamily, FontSize, FontStyle.Regular);
            column.SortMode = sort_Mode;
            column.ReadOnly = read_only;
            column.Visible = !Hide;
        }
        #endregion

        #region セルスタイルの設定
        /// <summary>
        /// セルスタイルの設定
        /// </summary>
        /// <param name="Col"></param>
        /// <param name="FontSize"></param>
        /// <param name="HAlign"></param>
        /// <param name="wrap_mode"></param>
        public void SetDefaultCellStyle(
            int Col,
            int FontSize = 9,
            DataGridViewContentAlignment HAlign = DataGridViewContentAlignment.MiddleLeft,
            DataGridViewTriState wrap_mode = DataGridViewTriState.False)
        {
            // ここで設定しないと、ヘッダーの設定と同じになる
            DataGridViewCellStyle style = DataGridObj.Columns[Col].DefaultCellStyle;
            style.Font = new Font(DataGridObj.DefaultCellStyle.Font.FontFamily, FontSize, FontStyle.Regular);
            style.Alignment = HAlign;
            style.WrapMode = wrap_mode;
        }
        #endregion

        #region テーブルの値をセットする
        /// <summary>
        /// テーブルの値をセットする
        /// </summary>
        /// <param name="strSQL">SQL文</param>
        /// <returns>抽出した件数（エラーの場合は-1）</returns>
        public int SetDataSource(string strSQL)
        {
            // 描画更新を一時停止
            DataGridObj.Invalidate();

            // 初期化
            DataGridObj.DataSource = null;

            // DB接続
            Cls_DBConn cDB = new Cls_DBConn(StCls_Function.Get_SQLConnectString()); // DB操作関係クラス
            DataSet ds = new DataSet();

            using (SqlConnection mConn = cDB.SetDBConnction())
            {
                // DB接続確認
                if (mConn == null)
                {
                    return -1;
                }

                // データセット取得
                ds = cDB.SetDataSet(mConn, strSQL, "Table");

                // 表示するテーブルを指定する
                DataGridObj.DataMember = "Table";

                // シートに連結する
                DataGridObj.DataSource = ds.Tables["Table"];

                // バインドするDBフィールド名を設定
                for (int iColCnt = 0; iColCnt < ds.Tables["Table"].Columns.Count; iColCnt++)
                {
                    DataGridObj.Columns[iColCnt].DataPropertyName = ds.Tables["Table"].Columns[iColCnt].ColumnName;
                }

                return ds.Tables["Table"].Rows.Count;
            }
        }
        #endregion

        #region 設定したセルの値を取得
        /// <summary>
        /// 設定したセルの値を取得
        /// </summary>
        /// <param name="aCol">列番号</param>
        /// <param name="aRow">行番号</param>
        /// <returns>セルの値</returns>
        public string GetValue(int aCol, int aRow)
        {
            return StCls_Function.NtoV(DataGridObj[aCol, aRow].Value, "").ToString();
        }
        #endregion

        #region 指定したセルに値をセット

        /// <summary>
        /// 指定したセルに値をセット
        /// </summary>
        /// <param name="aCol">列番号</param>
        /// <param name="aRow">行番号</param>
        /// <param name="aVal">値</param>
        public void SetValue(int aCol, int aRow, string aVal)
        {
            DataGridObj[aCol, aRow].Value = aVal;
        }

        /// <summary>
        /// 指定されたセルに値をセット（チェックボックス型）
        /// </summary>
        /// <param name="aCol"></param>
        /// <param name="aRow"></param>
        /// <param name="aVal"></param>
        public void SetChecked(int aCol, int aRow, bool aVal)
        {
            DataGridObj[aCol, aRow].Value = aVal;
        }

        #endregion

        #region 指定したセルの背景色を変える
        /// <summary>
        /// 指定したセルの背景色を変える
        /// </summary>
        /// <param name="aCol">列番号（-1で行全体）</param>
        /// <param name="aRow">行番号（-1で列全体）</param>
        /// <param name="aColor">色値（数値）</param>
        public void SetBackColor(int aCol, int aRow, int aColor)
        {
            Color colorTrans = StCls_Function.GetColorTranslator(aColor);

            if (aCol == -1 && aRow == -1)
            {
                // 列・行ともに-1なら全体の背景色を変更
                DataGridObj.DefaultCellStyle.BackColor = colorTrans;
            }
            else if (aCol == -1)
            {
                // 列が-1なら、指定行の背景色を変更
                DataGridObj.Rows[aRow].DefaultCellStyle.BackColor = colorTrans;
            }
            else if (aRow == -1)
            {
                // 行が-1なら、指定列の背景色を変更
                DataGridObj.Columns[aCol].DefaultCellStyle.BackColor = colorTrans;
            }
            else
            {
                // 指定セルの背景色を変更
                DataGridObj[aCol, aRow].Style.BackColor = colorTrans;
            }
        }
        #endregion

        #region 指定したセルの前景色を変える
        /// <summary>
        /// 指定したセルの前景色を変える
        /// </summary>
        /// <param name="aCol">列番号（-1で行全体）</param>
        /// <param name="aRow">行番号（-1で列全体）</param>
        /// <param name="aColor">色値</param>
        public void SetForeColor(int aCol, int aRow, int aColor)
        {
            Color colorTrans = StCls_Function.GetColorTranslator(aColor);

            if (aCol == -1 && aRow == -1)
            {
                // 列・行ともに-1なら全体の前景色を変更
                DataGridObj.DefaultCellStyle.ForeColor = colorTrans;
            }
            else if (aCol == -1)
            {
                // 列が-1なら、指定行の前景色を変更
                DataGridObj.Rows[aRow].DefaultCellStyle.ForeColor = colorTrans;
            }
            else if (aRow == -1)
            {
                // 行が-1なら、指定列の前景色を変更
                DataGridObj.Columns[aCol].DefaultCellStyle.ForeColor = colorTrans;
            }
            else
            {
                // 指定セルの前景色を変更
                DataGridObj[aCol, aRow].Style.ForeColor = colorTrans;
            }
        }
        #endregion

        #region 表示内容を新規Excelブックに出力
        /// <summary>
        /// 表示内容を新規Excelブックに出力
        /// </summary>
        /// <param name="ExpHeader">ヘッダ出力の有無</param>
        /// <param name="ExpFPath">出力ファイルパス（ブランクの場合は表示のみ）</param>
        /// <param name="NonExpCol">非出力列の指定（カンマ区切りで複数列指定可能）</param>
        /// <returns>True:成功　False:失敗</returns>
        /// <remarks></remarks>
        public bool ExportExcel(bool ExpHeader, string ExpFPath = "", string NonExpCol = "")
        {
            // ヘッダー出力の有無
            int intHead = 0;
            if (ExpHeader)
            {
                intHead = 1;
            }

            // スプレッドのデータ件数がExcelに出力可能か確認
            if (DataGridObj.RowCount == 0)
            {
                MessageBox.Show("出力対象がありません", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return true;
            }
            else if (DataGridObj.RowCount >= 65536 - intHead)
            {
                // FIXED: 最大件数は引数指定可が理想。（検索結果が最大件数以上の場合、使用者に優しくない）
                MessageBox.Show("出力対象が多すぎます" + Environment.NewLine + "件数を絞り込んでから出力してください", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return true;
            }
            else if (MessageBox.Show("Excelに検索結果を出力をします。" + Environment.NewLine + "よろしいですか？", "確認", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel)
            {
                return true;
            }

            Cls_Excel excel = new Cls_Excel();
            int shtNo = 1;
            int intMaxColumns = DataGridObj.ColumnCount - 1;
            int intMaxRows = DataGridObj.RowCount - 1;

            string[] strNonCol = NonExpCol.Split(',');   // カンマ区切りで分割して配列に格納する
            string[] ArrHead = new string[intMaxColumns]; // ヘッダ文字列を格納する配列
            int intHeadCnt = 0;                          // 列飛ばすと出力列もずれるので別カウンタ利用
            int intNonHeadCnt = 0;                       // 出力しない列の数

            try
            {
                // 開く
                excel.Xls_NewOpen(shtNo);

                // セルの書式を文字列に設定します
                excel.Xls_CellType_String();

                // ヘッダ出力
                if (ExpHeader)
                {
                    // 出力する
                    for (int intCol = 0; intCol <= intMaxColumns; intCol++)
                    {
                        // 非出力列は書き込まない
                        if (Array.IndexOf(strNonCol, (intCol + 1).ToString()) == -1)
                        {
                            ArrHead[intHeadCnt] = DataGridObj.Columns[intCol].HeaderText;
                            intHeadCnt++;
                        }
                        else
                        {
                            intNonHeadCnt++;
                        }
                    }

                    // 配列の要素数を変更
                    Array.Resize(ref ArrHead, intMaxColumns - intNonHeadCnt);

                    // ヘッダ書き込み
                    excel.Xls_WriteRange(1, 1, 1, ArrHead.Length, ArrHead);
                }
                else
                {
                    // 出力しないので何もしない
                    intHead = 0;
                }

                // 書き込み用の2次元配列を定義します
                object[,] arrMain = new object[intMaxRows + 1, intMaxColumns - intNonHeadCnt + 1];

                // 配列セット
                for (int intRow = 0; intRow <= intMaxRows; intRow++)
                {
                    intHeadCnt = 0;
                    for (int intCol = 0; intCol <= intMaxColumns; intCol++)
                    {
                        // 非出力列は書き込まない
                        if (Array.IndexOf(strNonCol, (intCol + 1).ToString()) == -1)
                        {
                            // FIXED: セル内改行ある場合例外が発生する。（発生時は個別対応。このメソッド自体いつか見直す必要あり）
                            arrMain[intRow, intHeadCnt] = GetValue(intCol, intRow);
                            intHeadCnt++;
                        }
                    }
                }

                // 一括書き込み
                excel.Xls_WriteRange(1 + intHead, 1, (intMaxRows + 1) + intHead, ArrHead.Length, arrMain);

                // 列の幅や行の高さを内容に合わせて調節
                excel.Xls_AutoFit();

                // シート選択位置の初期化
                excel.Xls_ShtActive(shtNo);

                // 選択セルの初期化
                excel.Xls_SelectCell(1, 1);

                // システムメッセージＯＮ
                excel.Xls_Alerts(true);

                // 出力先が指定されているなら保存、されていないなら表示
                if (!string.IsNullOrEmpty(ExpFPath))
                {
                    // 出力先に保存
                    excel.Xls_SaveAs(ExpFPath);
                    excel.Xls_Close();
                    MessageBox.Show("出力しました。" + Environment.NewLine + Environment.NewLine + "↓" + Environment.NewLine + Environment.NewLine + ExpFPath, "出力完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // エクセル表示
                    excel.Xls_Visible(true);
                }

                return true;
            }
            catch (Exception)
            {
                // エクセルを閉じる
                excel.Xls_Close();
                return false;
            }
            finally
            {
                // エクセル解放処理
                excel.Xls_Dispose();
            }
        }
        #endregion

        #region 表示内容を新規テキストファイルに出力
        /// <summary>
        /// 表示内容を新規テキストファイルに出力
        /// </summary>
        /// <param name="ExpHeader">ヘッダ出力の有無</param>
        /// <param name="ExpFPath">出力ファイル名</param>
        /// <param name="Delimiter">区切り文字</param>
        /// <param name="NonExpCol">非出力列の指定（カンマ区切りで複数列指定可能）</param>
        /// <param name="DelCrLf">改行コードを削除するかどうか</param>
        /// <returns>True:成功　False:失敗</returns>
        /// <remarks></remarks>
        public bool ExportText(bool ExpHeader, string ExpFPath, string Delimiter = ",", string NonExpCol = "", bool DelCrLf = false)
        {
            // 出力確認
            if (DataGridObj.RowCount == 0)
            {
                MessageBox.Show("出力対象がありません", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return true;
            }
            else if (string.IsNullOrEmpty(Path.GetFileName(ExpFPath)))
            {
                MessageBox.Show("出力ファイル名が未指定です。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return true;
            }
            else if (StCls_File.FF_IsFile(ExpFPath))
            {
                if (MessageBox.Show("すでに出力先に同名のファイルが存在しますが、上書きしてもよろしいですか？", "確認", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel)
                {
                    return true;
                }
            }
            else if (MessageBox.Show("テキストファイルに検索結果を出力をします。" + Environment.NewLine + "よろしいですか？", "確認", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel)
            {
                return true;
            }

            // フォルダチェック
            if (!StCls_File.FF_IsFolder(Path.GetDirectoryName(ExpFPath)))
            {
                if (!StCls_File.FF_CreateFolder(Path.GetDirectoryName(ExpFPath)))
                {
                    MessageBox.Show("出力先のフォルダが存在しないため、出力できません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return true;
                }
            }

            int intMaxColumns = DataGridObj.ColumnCount - 1;
            int intMaxRows = DataGridObj.RowCount - 1;
            string[] strNonCol = NonExpCol.Split(','); // 非出力列

            using (StreamWriter SWriter = new StreamWriter(ExpFPath, false, Encoding.GetEncoding("shift_jis")))
            {
                StringBuilder sbAll = new StringBuilder(); // 全体情報書き込み用
                StringBuilder sbRow = new StringBuilder(); // 行情報書き込み用

                try
                {
                    // ヘッダ出力
                    if (ExpHeader)
                    {
                        sbRow.Clear();

                        // 出力する
                        for (int intCol = 0; intCol <= intMaxColumns; intCol++)
                        {
                            if (Array.IndexOf(strNonCol, (intCol + 1).ToString()) == -1)
                            {
                                if (intCol != 0)
                                {
                                    // 値があるなら区切り文字セット
                                    sbRow.Append(Delimiter);
                                }
                                string sTmp = DataGridObj.Columns[intCol].HeaderText;
                                if (DelCrLf)
                                {
                                    sTmp = sTmp.Replace(Environment.NewLine, "").Replace("\r", "").Replace("\n", "");
                                }
                                sbRow.Append(sTmp);
                            }
                        }

                        // ヘッダ書き込み
                        SWriter.WriteLine(sbRow.ToString());
                    }

                    // メイン出力
                    for (int intRow = 0; intRow <= intMaxRows; intRow++)
                    {
                        sbRow.Clear();

                        // 行の値を取得
                        for (int intCol = 0; intCol <= intMaxColumns; intCol++)
                        {
                            if (Array.IndexOf(strNonCol, (intCol + 1).ToString()) == -1)
                            {
                                if (intCol != 0)
                                {
                                    // 値があるなら区切り文字セット
                                    sbRow.Append(Delimiter);
                                }
                                string sTmp = GetValue(intCol, intRow);
                                if (DelCrLf)
                                {
                                    sTmp = sTmp.Replace(Environment.NewLine, "").Replace("\r", "").Replace("\n", "");
                                }
                                sbRow.Append(sTmp);
                            }
                        }

                        // 行の情報をメインのビルダーに書き込み
                        sbAll.Append(sbRow.ToString() + Environment.NewLine);
                    }

                    // 一括書き込み
                    SWriter.Write(sbAll.ToString());

                    MessageBox.Show("検索結果の出力が完了しました。", "確認", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
        #endregion

        #region 行番号の描画
        /// <summary>
        /// 行番号の描画
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks></remarks>
        private void DataGridObj_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            // 列ヘッダーかどうか調べる
            if (e.ColumnIndex < 0 && e.RowIndex >= 0)
            {
                // セルを描画する
                e.Paint(e.ClipBounds, DataGridViewPaintParts.All);

                // 行番号を描画する範囲を決定する
                // e.AdvancedBorderStyleやe.CellStyle.Paddingは無視しています
                Rectangle indexRect = e.CellBounds;
                indexRect.Inflate(-2, -2);

                // 行番号を描画する
                TextRenderer.DrawText(e.Graphics,
                                        (e.RowIndex + 1).ToString(),
                                        e.CellStyle.Font,
                                        indexRect,
                                        e.CellStyle.ForeColor,
                                        TextFormatFlags.Right | TextFormatFlags.VerticalCenter);

                // 描画が完了したことを知らせる
                e.Handled = true;
            }
        }
        #endregion

        #region 　　+　Function：　Row　テーブルの値をセットする（DataSourceプロパティ非使用/編集時列ズレ対応）
        /// <summary>
        /// Row テーブルの値をセットする（DataSourceプロパティ非使用/編集時列ズレ対応）
        /// </summary>
        /// <param name="strSQL">SQL文</param>
        /// <returns>表示件数（エラーの場合は-1）</returns>
        /// <remarks></remarks>
        public int SetDataSourceAsAddRow(string strSQL)
        {
            //【SortMode = Automatic設定時】
            // ①DataSourceで追加した行列 + ②個別にAddした列 が混在する場合、
            // セル値を編集・ソートすると列ズレが発生する現象あり。
            // DataSourceプロパティを使用せず、すべてRows.Addすることで対応。

            // あらかじめ全行削除
            DataGridObj.Rows.Clear();

            try
            {
                Cls_DBConn cDB = new Cls_DBConn(StCls_Function.Get_SQLConnectString());

                using (SqlConnection mConn = cDB.SetDBConnction())
                {
                    if (mConn == null) return -1;

                    // データソース取得
                    DataSet ds = cDB.SetDataSet(mConn, strSQL, "Table");
                    DataTable dt = ds.Tables["Table"];

                    // バインドではなく、1行ずつ追加する
                    foreach (DataRow dr in dt.Rows)
                    {
                        DataGridObj.Rows.Add(dr.ItemArray);
                    }
                }
            }
            catch (Exception)
            {
                return -1;
            }

            return DataGridObj.Rows.Count;
        }
        #endregion

        #region 　　+　Sub：　Sort　改良版ソート機能を設定する（編集時列ズレ対応）

        /// <summary>
        /// Sort　改良版ソート機能を設定する（編集時列ズレ対応）
        /// </summary>
        /// <remarks></remarks>
        public void SetSortModeProgrammatic()
        {
            //【注意】
            //当メソッドは、GridViewの各列設定まで完了後に呼び出す想定！

            //【SortMode = Automatic設定時】
            //　セル値変更時、即ソートが実行されるため変更行の行位置が変わってしまう。
            //　SortMode = AutomaticであればProgrammaticに変更し、上記の仕様をつぶす。

            for (int i = 0; i < DataGridObj.ColumnCount; i++)
            {
                if (DataGridObj.Columns[i].SortMode == DataGridViewColumnSortMode.NotSortable)
                {
                    // ソート不要なら処理しない
                    continue;
                }
                else
                {
                    // Automatic、Programmaticにかかわらず上書きする
                    DataGridObj.Columns[i].SortMode = DataGridViewColumnSortMode.Programmatic;
                }
            }

            // ソートイベントの紐付け
            DataGridObj.ColumnHeaderMouseClick += new DataGridViewCellMouseEventHandler(DataGridView_ColumnHeaderMouseClick);
        }

        /// <summary>DataGridView_ColumnHeaderMouseClick　ソートの制御</summary>
        private void DataGridView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            // sort: ヘッダに表示されるソート矢印
            // direction: ソートで使用する値比較方法
            DataGridViewColumn currentColumn = null;
            System.Windows.Forms.SortOrder sort = System.Windows.Forms.SortOrder.None;
            ListSortDirection direction = ListSortDirection.Ascending;

            try
            {
                currentColumn = DataGridObj.Columns[e.ColumnIndex];

                if (currentColumn.SortMode == DataGridViewColumnSortMode.NotSortable)
                {
                    // 非ソート時は終了
                    return;
                }

                // クリックされた列のソート状態取得
                sort = currentColumn.HeaderCell.SortGlyphDirection;

                if (sort == System.Windows.Forms.SortOrder.Descending)
                {
                    // 降順 → 昇順
                    sort = System.Windows.Forms.SortOrder.Ascending;
                    direction = ListSortDirection.Ascending;
                }
                else
                {
                    // 昇順 → 降順
                    sort = System.Windows.Forms.SortOrder.Descending;
                    direction = ListSortDirection.Descending;
                }

                // ソート適用
                currentColumn.HeaderCell.SortGlyphDirection = sort;
                DataGridObj.Sort(currentColumn, direction);
            }
            catch (Exception ex)
            {
                MessageBox.Show("エラーが発生しました！ DataGridView_ColumnHeaderMouseClick" + Environment.NewLine + ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region 　　+　Sub：　Column　列追加＆列プロパティ設定

        /// <summary>
        /// DataGridViewColumnの各コントロール型を表す列挙体
        /// </summary>
        public enum ColType
        {
            Button = 0,
            CheckBox,
            ComboBox,
            Image,
            Link,
            TextBox
        }

        /// <summary>
        /// Column　列追加＆列プロパティ設定
        /// </summary>
        /// <param name="type">列のコントロール型</param>
        /// <param name="headerName">ヘッダ名</param>
        /// <param name="width">幅</param>
        /// <param name="hAlign">横方向の配置</param>
        /// <param name="fontSize">フォントサイズ</param>
        /// <param name="sortMode">ソート設定</param>
        /// <param name="readOnly">読み取りのみであるか</param>
        /// <param name="hide">非表示にするか</param>
        /// <remarks></remarks>
        public void AddColumn(ColType type,
                              string headerName,
                              int width = 100,
                              DataGridViewContentAlignment hAlign = DataGridViewContentAlignment.MiddleCenter,
                              int fontSize = 9,
                              DataGridViewColumnSortMode sortMode = DataGridViewColumnSortMode.Automatic,
                              bool readOnly = true,
                              bool hide = false)
        {
            DataGridViewColumn column = null;

            // コントロールタイプに応じて列を作成
            switch (type)
            {
                case ColType.Button:
                    column = new DataGridViewButtonColumn();
                    break;
                case ColType.CheckBox:
                    column = new DataGridViewCheckBoxColumn();
                    break;
                case ColType.ComboBox:
                    column = new DataGridViewComboBoxColumn();
                    break;
                case ColType.Image:
                    column = new DataGridViewImageColumn();
                    break;
                case ColType.Link:
                    column = new DataGridViewLinkColumn();
                    break;
                case ColType.TextBox:
                    column = new DataGridViewTextBoxColumn();
                    break;
                default:
                    throw new Exception("Cls_DataGrid.AddColumn　不正な型が指定されました！");
            }

            // 列のプロパティ設定
            column.HeaderText = headerName;
            column.Width = width;
            column.HeaderCell.Style.Alignment = hAlign;
            column.DefaultCellStyle.Alignment = hAlign;
            column.HeaderCell.Style.Font = new Font(DataGridObj.DefaultCellStyle.Font.FontFamily, fontSize, FontStyle.Regular);
            column.SortMode = sortMode;
            column.ReadOnly = readOnly;
            column.Visible = !hide;

            // DataGridViewに列を追加
            DataGridObj.Columns.Add(column);
        }

        #endregion

        #region 　　+　Sub：　Column　コンボボックス列　データソースを設定する（DBより）

        /// <summary>
        /// Column コンボボックス列 データソースを設定する（DBより）
        /// </summary>
        /// <param name="cDB">DB操作クラス</param>
        /// <param name="columnIndex">対象列のIndex</param>
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
        public void SetDataSourceCombo(Cls_DBConn cDB,
                                       int columnIndex,
                                       string tableName,
                                       string valueField,
                                       string textField,
                                       string sort = "",
                                       string filter = "",
                                       bool showBlankItem = true,
                                       bool showTextWithValue = false,
                                       string firstValue = "",
                                       string firstText = "",
                                       string lastValue = "",
                                       string lastText = "")
        {
            DataTable dt = new DataTable();
            StringBuilder sb = new StringBuilder();
            DataGridViewComboBoxColumn column = null;

            try
            {
                column = (DataGridViewComboBoxColumn)DataGridObj.Columns[columnIndex];
                column.DataSource = null;

                // DataTableに列を追加
                dt.Columns.Add(valueField, typeof(string));
                dt.Columns.Add(textField, typeof(string));

                // SQL文生成
                sb.Length = 0;
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

                // 空行
                if (showBlankItem)
                    dt.Rows.InsertAt(GetDataRow(dt, valueField, textField, "", ""), 0);

                // 先頭行
                if (!string.IsNullOrEmpty(firstText))
                    dt.Rows.InsertAt(GetDataRow(dt, valueField, textField, firstValue, firstText), 0);

                // 末尾行
                if (!string.IsNullOrEmpty(lastText))
                    dt.Rows.InsertAt(GetDataRow(dt, valueField, textField, lastValue, lastText), dt.Rows.Count);

                // データテーブルのコミット
                dt.AcceptChanges();

                column.DataSource = dt;
                column.DisplayMember = textField;
                column.ValueMember = valueField;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>DataRowを作成するだけのヘルパーメソッド</summary>
        [DebuggerStepThrough]
        private DataRow GetDataRow(DataTable dt, string valueField, string textField, string value, string text)
        {
            DataRow result = dt.NewRow();
            result[valueField] = value;
            result[textField] = text;
            return result;
        }

        #endregion

        #region 　　+　Sub：　Column　コンボボックス列　データソースを設定する（フリー設定）

        /// <summary>
        /// Column コンボボックス列 データソースを設定する（フリー設定）
        /// </summary>
        /// <param name="columnIndex">対象列のIndex</param>
        /// <param name="valueCollection">ソート条件</param>
        /// <param name="textCollection">抽出条件</param>
        /// <param name="showBlankItem">先頭に空項目を表示するか</param>
        /// <param name="showTextWithValue">Text値を「Text:Value」形式で表示するか</param>
        /// <remarks></remarks>
        public void SetDataSourceComboFree(int columnIndex,
                                            ICollection valueCollection,
                                            ICollection textCollection,
                                            bool showBlankItem = true,
                                            bool showTextWithValue = false)
        {
            DataTable dt = new DataTable();
            DataGridViewComboBoxColumn column = null;

            try
            {
                column = (DataGridViewComboBoxColumn)DataGridObj.Columns[columnIndex];
                column.DataSource = null;

                // 要素数が不一致なら終了
                if (valueCollection.Count != textCollection.Count)
                    return;

                // DataTableに列を追加
                dt.Columns.Add("Value", typeof(string));
                dt.Columns.Add("Text", typeof(string));

                // DataTableに行を追加
                for (int i = 0; i < valueCollection.Count; i++)
                {
                    DataRow row = dt.NewRow();

                    // 実値を表示（Enum定義が KanriNo = 0 なら 0を表示したい）
                    // String型変換エラーになるならそのとき考えましょう！
                    row["Value"] = valueCollection.Cast<object>().ElementAt(i).ToString();

                    if (showTextWithValue)
                    {
                        row["Text"] = row["Value"] + "：" + textCollection.Cast<object>().ElementAt(i).ToString();
                    }
                    else
                    {
                        row["Text"] = textCollection.Cast<object>().ElementAt(i).ToString();
                    }

                    dt.Rows.Add(row);
                }

                // ブランク行
                if (showBlankItem)
                    dt.Rows.InsertAt(GetDataRow(dt, "Value", "Text", "", ""), 0);

                // データテーブルのコミット
                dt.AcceptChanges();

                column.ValueMember = "Value";
                column.DisplayMember = "Text";
                column.DataSource = dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}
