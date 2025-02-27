using hagaki.Class;
using hagaki.StaticClass;
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static hagaki.My_Function;

namespace hagaki
{
    public partial class Frm0500_MAINTENANCE : Form
    {
        #region メンバ変数
        private string connectionString = string.Empty;                    // 接続文字列
        private My_Function _func;                                         // My_Functionを使えるように
        private string selectedKanriNo = string.Empty;                     // 現在選択されている事務局管理番号
        private DataTable searchResultData = new DataTable();              // 検索画面で絞り込まれた全データ
        private List<string> searchResultKanriNoList = new List<string>(); // 検索結果の事務局管理番号リスト
        private int currentPage = 0;                                       // 検索結果のデータの要素番号（+1で現在のページ番号）
        private int maxPage = 0;                                           // 検索結果のデータの要素数（最大ページ数）
        private DataRow rowData;                                           // 選択されたレコード情報
        private DataTable fullErrorCodeData;                               // D_ERRORテーブルに登録されている全エラーデータ
        #endregion

        #region 定数
        private const string D_MAIN = "D_MAIN";
        private const string D_ERROR = "D_ERROR";
        private const string M_ERROR = "M_ERROR";
        private const string EXCEPTION_ERROR_TITLE = "例外エラー";
        #endregion

        #region コンストラクタ
        public Frm0500_MAINTENANCE()
        {
            InitializeComponent();
        }

        public Frm0500_MAINTENANCE(string connestStr, string kanriNo, List<string> fromSearchPageData)
        {
            InitializeComponent();

            connectionString = connestStr;
            selectedKanriNo = kanriNo;
            searchResultKanriNoList = fromSearchPageData;
        }
        #endregion

        #region ロードイベント
        private void Frm0500_MAINTENANCE_Load(object sender, EventArgs e)
        {
            try
            {
                // My_Functionクラスをインスタンス化
                _func = new My_Function();

                // 初期フォーカスを郵便番号欄にする
                ActiveControl = ZipCdText;

                // 最大ページ数取得
                maxPage = searchResultKanriNoList.Count();

                // 現在のページ取得
                for (int i = 0; i < searchResultKanriNoList.Count; i++)
                {
                    if (searchResultKanriNoList[i] == selectedKanriNo)
                    {
                        currentPage = i;
                        break;
                    }
                }

                // 最初のページもしくは最後のページを表示する場合のボタン制御
                if (currentPage == 0)
                {
                    PrevButton.BackColor = SystemColors.ControlDark;
                    PrevButton.Cursor = Cursors.Default;

                }
                if (currentPage == maxPage - 1)
                {
                    NextButton.BackColor = SystemColors.ControlDark;
                    NextButton.Cursor = Cursors.Default;
                }

                // ページ表示
                DispPage();

                // 絞り込まれたデータレコード情報を取得
                GetSeachResultData(searchResultKanriNoList);

                // 選択されている管理番号のレコード情報取得
                GetCurrentRecord(selectedKanriNo);

                // D_ERRORテーブルに登録されているエラーコード情報取得
                GetFullErrorCode();

                // 選択したレコード情報表示
                SetTextBox();

                // エラーコードがあれば取得
                GetCurrentErrorCode();

                // 数値のみの入力制御
                ZipCdText.KeyPress += new KeyPressEventHandler(_func.NumTextKeyPress);
                TelNoText.KeyPress += new KeyPressEventHandler(_func.NumTextKeyPress);
                Ank1Text.KeyPress += new KeyPressEventHandler(_func.NumTextKeyPress);
                Ank2Text.KeyPress += new KeyPressEventHandler(_func.NumTextKeyPress);
                Ank3Text.KeyPress += new KeyPressEventHandler(_func.NumTextKeyPress);
                NgOutKbText.KeyPress += new KeyPressEventHandler(_func.NumTextKeyPress);
                HisoOutKbText.KeyPress += new KeyPressEventHandler(_func.NumTextKeyPress);

                // フォーカス外れたときにエラーチェック
                ZipCdText.Leave += new EventHandler(LeaveErrorCheck);
                Add1Text.Leave += new EventHandler(LeaveErrorCheck);
                Add2Text.Leave += new EventHandler(LeaveErrorCheck);
                Add3Text.Leave += new EventHandler(LeaveErrorCheck);
                Add4Text.Leave += new EventHandler(LeaveErrorCheck);
                MeiText.Leave += new EventHandler(LeaveErrorCheck);
                SeiText.Leave += new EventHandler(LeaveErrorCheck);
                TelNoText.Leave += new EventHandler(LeaveErrorCheck);
                Ank1Text.Leave += new EventHandler(LeaveErrorCheck);
                Ank2Text.Leave += new EventHandler(LeaveErrorCheck);
                Ank3Text.Leave += new EventHandler(LeaveErrorCheck);
                NgOutKbText.Leave += new EventHandler(LeaveErrorCheck);
                HisoOutKbText.Leave += new EventHandler(LeaveErrorCheck);

                // 移動ボタンクリック時制御
                PrevButton.Click += new EventHandler(MoveRecord);
                NextButton.Click += new EventHandler(MoveRecord);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, EXCEPTION_ERROR_TITLE);
            }
        }
        #endregion

        #region 更新
        private void UpdateButton_Click(object sender, EventArgs e)
        {
            try
            {
                // 状態区分のチェック状況取得
                string JyotaiKb = string.Empty;

                if (OK_RadioButton.Checked)
                {
                    JyotaiKb = ((int)My_Function.JyotaiKb.Ok).ToString();
                }
                else if (NG_RadioButton.Checked)
                {
                    JyotaiKb = ((int)My_Function.JyotaiKb.Ng).ToString();
                }
                else if (KEEP_RadioButton.Checked)
                {
                    JyotaiKb = ((int)My_Function.JyotaiKb.Hold).ToString();
                }
                else if (CANCEL_RadioButton.Checked)
                {
                    JyotaiKb = ((int)My_Function.JyotaiKb.Cancel).ToString();
                }

                // 各項目の入力値を配列に格納
                string[] newDataArray = new string[16];
                newDataArray[(int)MainTableColumn.KanriNo] = selectedKanriNo;
                newDataArray[(int)MainTableColumn.UkeDate] = UkeDateText.Text;
                newDataArray[(int)MainTableColumn.ZipCd] = ZipCdText.Text;
                newDataArray[(int)MainTableColumn.Add1] = Add1Text.Text;
                newDataArray[(int)MainTableColumn.Add2] = Add2Text.Text;
                newDataArray[(int)MainTableColumn.Add3] = Add3Text.Text;
                newDataArray[(int)MainTableColumn.Add4] = Add4Text.Text;
                newDataArray[(int)MainTableColumn.NameSei] = SeiText.Text;
                newDataArray[(int)MainTableColumn.NameMei] = MeiText.Text;
                newDataArray[(int)MainTableColumn.TelNo] = TelNoText.Text;
                newDataArray[(int)MainTableColumn.Ank1] = Ank1Text.Text;
                newDataArray[(int)MainTableColumn.Ank2] = Ank2Text.Text;
                newDataArray[(int)MainTableColumn.Ank3] = Ank3Text.Text;
                newDataArray[(int)MainTableColumn.JyotaiKb] = JyotaiKb;
                newDataArray[(int)MainTableColumn.NgOutKb] = NgOutKbText.Text;
                newDataArray[(int)MainTableColumn.HisoOutKb] = HisoOutKbText.Text;

                // エラーコードリスト
                List<int> errorList = _func.ErrorCheck(newDataArray);

                // エラーコードレベル
                int errorCdLevel = 0;

                if (errorList.Count != 0)
                {
                    foreach (int errorCd in errorList)
                    {
                        switch (errorCd)
                        {
                            case 102:
                                // 取込OKエラー（エラーレベル1）
                                // エラーレベルが2でなければ1にする
                                if (errorCdLevel != 2)
                                {
                                    errorCdLevel = 1;
                                }
                                break;
                            case int n when (n != 116):
                                // 取込NGエラー（エラーレベル2）
                                errorCdLevel = 2;
                                break;
                        }
                    }
                }

                // エラーレベルによる分岐処理
                if (errorCdLevel == 2 && JyotaiKb == ((int)My_Function.JyotaiKb.Ok).ToString())
                {
                    MessageBox.Show("ＯＫにできないエラーが存在しますので、更新できません。", "警告");
                    return;
                }
                else if (errorCdLevel == 1 && JyotaiKb == ((int)My_Function.JyotaiKb.Ok).ToString())
                {
                    // 確認ダイアログ表示
                    DialogResult result = MessageBox.Show("不備項目がありますが、ＯＫで更新してもよろしいでしょうか？", "確認", MessageBoxButtons.YesNo);

                    // いいえを選択したら更新キャンセル
                    if (result == DialogResult.No)
                    {
                        return;
                    }
                }
                else
                {
                    // 確認ダイアログ表示
                    DialogResult updateResult = MessageBox.Show("更新してもよろしいでしょうか？", "確認", MessageBoxButtons.YesNo);

                    // いいえを選択したら更新キャンセル
                    if (updateResult == DialogResult.No)
                    {
                        return;
                    }

                    // エラーレベル0かつ状態区分がNGであればOKに変更（保留とキャンセルの場合はそのまま）
                    if (errorCdLevel == 0 && JyotaiKb == ((int)My_Function.JyotaiKb.Ng).ToString())
                    {
                        newDataArray[(int)MainTableColumn.JyotaiKb] = "0";
                    }
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    // 接続を開く
                    connection.Open();

                    // トランザクション開始
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {

                        // D_ERRORデリートSQL文の生成
                        string dErrorDeleteSqlStr = _func.MakeDeleteSql(D_ERROR, selectedKanriNo);

                        // パラメータ
                        Dictionary<string, object> kanriNoParameter = new Dictionary<string, object>
                        {
                            { "@KanriNo", selectedKanriNo }
                        };

                        // D_ERRORデリートSQL文を実行
                        bool dErrorDeleteExcuteCheck = _func.Execute(connection, transaction, dErrorDeleteSqlStr, kanriNoParameter);

                        if (!dErrorDeleteExcuteCheck)
                        {
                            throw new Exception("D_ERRORテーブルから削除できませんでした。");
                        }

                        if (errorList.Count != 0)
                        {
                            // 新しいエラーコードでD_ERRORに登録
                            foreach (int errorCd in errorList)
                            {
                                // D_ERRORインサートSQL文の生成
                                string dErrorSqlStr = $"INSERT INTO {D_ERROR}(KANRI_NO, ERR_CD) VALUES (@KanriNo, '{errorCd}')";

                                // SQL文を実行
                                bool dErrorExcuteCheck = _func.Execute(connection, transaction, dErrorSqlStr, kanriNoParameter);

                                if (!dErrorExcuteCheck)
                                {
                                    throw new Exception("D_ERRORテーブルに登録できませんでした。");
                                }
                            }
                        }

                        // D_MAINアップデートSQL文の作成
                        string dMainUpdateSql = _func.MakeUpdateSql(D_MAIN);

                        // 項目ごとのパラメータを辞書で管理
                        Dictionary<string, object> parameters = _func.KeyValuePairs(newDataArray);
                        parameters.Add("@JyotaiKb", newDataArray[(int)MainTableColumn.JyotaiKb]);
                        parameters.Add("@NgOutKb", newDataArray[(int)MainTableColumn.NgOutKb]);
                        parameters.Add("@HisoOutKb", newDataArray[(int)MainTableColumn.HisoOutKb]);

                        // D_MAINアップデートSQL文を実行
                        bool dMainUpdateExcuteCheck = _func.Execute(connection, transaction, dMainUpdateSql, parameters);

                        if (!dMainUpdateExcuteCheck)
                        {
                            throw new Exception("D_MAINテーブルを更新できませんでした。");
                        }

                        // コミット
                        transaction.Commit();
                    }
                }

                // 絞り込まれたデータレコード情報を最新に
                GetSeachResultData(searchResultKanriNoList);

                // D_ERRORテーブルに登録されているエラーコード情報取得
                GetFullErrorCode();

                // 選択されている管理番号のレコード情報取得
                GetCurrentRecord(selectedKanriNo);

                // 選択したレコード情報表示
                SetTextBox();

                // エラーコードがあれば取得
                GetCurrentErrorCode();

                MessageBox.Show("データを更新しました。", "確認");
            }
            catch (SqlException sqlex)
            {
                MessageBox.Show(sqlex.Message, EXCEPTION_ERROR_TITLE);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, EXCEPTION_ERROR_TITLE);
            }
        }
        #endregion

        #region 終了
        private void EndButton_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion

        #region ページ表示
        /// <summary>
        /// 現在のページと最大ページを表示する
        /// </summary>
        private void DispPage()
        {
            PageLabel.Text = $"{currentPage + 1}/{maxPage}";
        }
        #endregion

        #region ページ移動
        /// <summary>
        /// ページ移動ボタンクリック時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveRecord(object sender, EventArgs e)
        {
            // 押されたボタンのName取得
            string selectMove = ((Button)sender).Name;

            // 移動先のレコードが無ければメッセージ表示
            switch (selectMove)
            {
                case "PrevButton":
                    if (currentPage == 0)
                    {
                        MessageBox.Show("これ以上先にレコードは存在しません。", "警告");
                        return;
                    }
                    break;
                case "NextButton":
                    if (currentPage == maxPage - 1)
                    {
                        MessageBox.Show("これ以上先にレコードは存在しません。", "警告");
                        return;
                    }
                    break;
            }

            // 内容変更後、更新せず移動しようとしているかチェック
            bool changeDataFlag = ChangeDataCheck();

            // 内容変更して更新していなければ確認
            if (!changeDataFlag)
            {
                // 確認ダイアログ表示
                DialogResult result = MessageBox.Show("更新されていません。移動しますが、よろしいでしょうか？", "確認", MessageBoxButtons.YesNo);

                if (result == DialogResult.No)
                {
                    return;
                }
            }

            // 押されたボタンによって現在のページ番号を増減
            switch (selectMove)
            {
                case "PrevButton":
                    if (currentPage == 1)
                    {
                        PrevButton.BackColor = SystemColors.ControlDark;
                        PrevButton.Cursor = Cursors.Default;
                    }
                    NextButton.BackColor = Color.MediumSlateBlue;
                    NextButton.Cursor = Cursors.Hand;

                    // 現在のページ番号を一つ減らす
                    currentPage -= 1;
                    break;
                case "NextButton":
                    if (currentPage == maxPage - 2)
                    {
                        NextButton.BackColor = SystemColors.ControlDark;
                        NextButton.Cursor = Cursors.Default;
                    }
                    PrevButton.BackColor = Color.MediumSlateBlue;
                    PrevButton.Cursor = Cursors.Hand;

                    // 現在のページ番号を一つ増やす
                    currentPage += 1;
                    break;
            }

            // 取得用事務局管理番号取得
            selectedKanriNo = searchResultKanriNoList[currentPage];

            // 選択されている管理番号のレコード情報取得
            GetCurrentRecord(selectedKanriNo);

            // 選択したレコード情報表示
            SetTextBox();

            // エラーコードがあれば取得
            GetCurrentErrorCode();

            // ページ表示
            DispPage();
        }
        #endregion

        #region 管理番号リストから該当のデータを取得
        /// <summary>
        /// 管理番号リストからD_MAINテーブルに該当するデータを取得
        /// </summary>
        /// <param name="kanriNoList">絞り込まれたデータの管理番号のみのリスト</param>
        private void GetSeachResultData(List<string> kanriNoList)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // 接続を開く
                connection.Open();

                using (SqlCommand command = connection.CreateCommand())
                {
                    DataSet dataSet = new DataSet();

                    // レコードのエラーコード取得SQL文の生成
                    StringBuilder query = new StringBuilder();
                    query.AppendLine($"SELECT * FROM {D_MAIN} WHERE KANRI_NO = @kanriNo0");

                    // パラメータ作成用
                    Dictionary<string, object> parameters = new Dictionary<string, object>();
                    parameters.Add("@KanriNo0", $"{searchResultKanriNoList[0]}");

                    if (searchResultKanriNoList.Count() > 1)
                    {
                        for (int i = 1; i < searchResultKanriNoList.Count(); i++)
                        {
                            query.AppendLine($" OR KANRI_NO = @KanriNo{i}");
                            parameters.Add($"@KanriNo{i}", $"{searchResultKanriNoList[i]}");
                        }
                    }

                    // 検索結果取得
                    _func.FillDataTable(dataSet, connection, null, query.ToString(), parameters, "SEARCH_DATA");

                    searchResultData = dataSet.Tables["SEARCH_DATA"];
                }
            }
        }
        #endregion

        #region 選択されている管理番号のレコード取得
        /// <summary>
        /// 選択されている管理番号のレコードを取得してDataRowに代入
        /// </summary>
        /// <param name="selectedKanriNo">選択されている管理番号</param>
        private void GetCurrentRecord(string selectedKanriNo)
        {
            foreach (DataRow rows in searchResultData.Rows)
            {
                if (rows["KANRI_NO"].ToString() != selectedKanriNo) continue;

                rowData = rows;
            }
        }
        #endregion

        #region D_ERRORテーブルに登録されているエラーコード情報取得
        /// <summary>
        /// D_ERRORテーブルに登録されているエラーコード情報取得してデータテーブルにセットする
        /// </summary>
        private void GetFullErrorCode()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // 接続を開く
                connection.Open();

                using (SqlCommand command = connection.CreateCommand())
                {
                    DataSet dataSet = new DataSet();

                    // レコードのエラーコード取得SQL文の生成
                    string query = $"SELECT {D_ERROR}.KANRI_NO, {D_ERROR}.ERR_CD, {M_ERROR}.ERR_MONGON FROM {D_ERROR} " +
                                            $"INNER JOIN {M_ERROR} ON {D_ERROR}.ERR_CD = {M_ERROR}.ERR_CD " +
                                            $"ORDER BY {D_ERROR}.KANRI_NO ASC";

                    // 検索結果取得
                    _func.FillDataTable(dataSet, connection, null, query, null, "ErrorCode");

                    // DataSetにDataTableが存在すれば、ErrorCodeテーブルを変数にセット
                    if (dataSet.Tables.Count > 0)
                    {
                        fullErrorCodeData = dataSet.Tables["ErrorCode"];
                    }
                }
            }
        }
        #endregion

        #region 各項目欄に表示
        /// <summary>
        /// 選択された事務局管理番号の情報を表示する
        /// </summary>
        public void SetTextBox()
        {
            // エラー項目の表をクリア
            ErrorDataGridView.Rows.Clear();

            // 状態区分のトグルをクリア
            OK_RadioButton.Checked = false;
            NG_RadioButton.Checked = false;
            KEEP_RadioButton.Checked = false;
            CANCEL_RadioButton.Checked = false;

            // テキストボックスの背景色を初期化
            ZipCdText.BackColor = SystemColors.Window;
            Add1Text.BackColor = SystemColors.Window;
            Add2Text.BackColor = SystemColors.Window;
            Add3Text.BackColor = SystemColors.Window;
            Add4Text.BackColor = SystemColors.Window;
            MeiText.BackColor = SystemColors.Window;
            SeiText.BackColor = SystemColors.Window;
            TelNoText.BackColor = SystemColors.Window;
            Ank1Text.BackColor = SystemColors.Window;
            Ank2Text.BackColor = SystemColors.Window;
            Ank3Text.BackColor = SystemColors.Window;
            NgOutKbText.BackColor = SystemColors.Window;
            HisoOutKbText.BackColor = SystemColors.Window;

            // 各項目データを入力欄に表示
            kanriNoText.Text = rowData["KANRI_NO"].ToString();
            UkeDateText.Text = rowData["UKE_DATE"].ToString();
            ZipCdText.Text = rowData["ZIP_CD"].ToString();
            Add1Text.Text = rowData["ADD_1"].ToString();
            Add2Text.Text = rowData["ADD_2"].ToString();
            Add3Text.Text = rowData["ADD_3"].ToString();
            Add4Text.Text = rowData["ADD_4"].ToString();
            SeiText.Text = rowData["NAME_SEI"].ToString();
            MeiText.Text = rowData["NAME_MEI"].ToString();
            TelNoText.Text = rowData["TEL_NO"].ToString();
            Ank1Text.Text = rowData["ANK_1"].ToString();
            Ank2Text.Text = rowData["ANK_2"].ToString();
            Ank3Text.Text = rowData["ANK_3"].ToString();
            switch (int.Parse(rowData["JYOTAI_KB"].ToString()))
            {
                case (int)My_Function.JyotaiKb.Ok:
                    OK_RadioButton.Checked = true;
                    break;
                case (int)My_Function.JyotaiKb.Ng:
                    NG_RadioButton.Checked = true;
                    break;
                case (int)My_Function.JyotaiKb.Hold:
                    KEEP_RadioButton.Checked = true;
                    break;
                case (int)My_Function.JyotaiKb.Cancel:
                    CANCEL_RadioButton.Checked = true;
                    break;
            }
            NgOutKbText.Text = rowData["NG_OUT_KB"].ToString();
            NgOutDateTimeText.Text = rowData["NG_OUT_DATETIME"].ToString();
            HisoOutKbText.Text = rowData["HISO_OUT_KB"].ToString();
            HisoOutDateTimeText.Text = rowData["HISO_OUT_DATETIME"].ToString();
            RegistDateTimeText.Text = rowData["REGIST_DATETIME"].ToString();
            RegistLoginIdText.Text = rowData["REGIST_LOGINID"].ToString();
            UpdateDateTimeText.Text = rowData["UPDATE_DATETIME"].ToString();
            UpdateLoginIdText.Text = rowData["UPDATE_LOGINID"].ToString();
        }
        #endregion

        #region 選択されている管理番号のエラーコードを取得して表示
        /// <summary>
        /// 選択されている管理番号のエラーコードを取得して表示
        /// </summary>
        private void GetCurrentErrorCode()
        {
            // 事務局管理番号のみのリスト作成
            foreach (DataRow rows in fullErrorCodeData.Rows)
            {
                if (rows["KANRI_NO"].ToString() != selectedKanriNo) continue;

                // エラー表示
                ErrorDataGridView.Rows.Add(rows["ERR_CD"], rows["ERR_MONGON"]);

                // 追加設定（仕様書に無い）
                // データ表示時にエラーの項目は背景を赤に
                switch (rows["ERR_CD"])
                {
                    case "100":
                    case "101":
                        ZipCdText.BackColor = Color.Red;
                        break;
                    case "102":
                    case "103":
                    case "104":
                        Add1Text.BackColor = Color.Red;
                        break;
                    case "105":
                    case "106":
                    case "107":
                        Add2Text.BackColor = Color.Red;
                        break;
                    case "108":
                    case "109":
                    case "110":
                        Add3Text.BackColor = Color.Red;
                        break;
                    case "111":
                    case "112":
                        Add4Text.BackColor = Color.Red;
                        break;
                    case "113":
                    case "114":
                    case "115":
                        SeiText.BackColor = Color.Red;
                        break;
                    case "117":
                    case "118":
                        MeiText.BackColor = Color.Red;
                        break;
                }
            }
        }
        #endregion

        #region エラーによる背景色変更
        /// <summary>
        /// フォーカスを外したらエラーチェックを行い、エラーであれば背景色を赤に変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LeaveErrorCheck(object sender, EventArgs e)
        {
            // フォーカスを外されたテキストボックスName
            string outForcus = ((TextBox)sender).Name;

            if (outForcus == null) return;

            // エラーチェック（true：エラーあり、false：エラーなし）
            bool error = false;
            int errorCheck = 0;
            switch (outForcus)
            {
                case "ZipCdText":
                    // 半角変換
                    ZipCdText.Text = StCls_Function.VbStrConv(ZipCdText.Text, (VbStrConv)8);

                    // エラーチェック
                    errorCheck = _func.ZipCdCheck(ZipCdText.Text);
                    error = (errorCheck != 0);
                    break;
                case "Add1Text":
                    // 全角変換
                    Add1Text.Text = StCls_Function.VbStrConv(Add1Text.Text, (VbStrConv)4);

                    errorCheck = _func.Add1Check(Add1Text.Text);
                    error = (errorCheck == 103 || errorCheck == 104);
                    break;
                case "Add2Text":
                    // 全角変換
                    Add2Text.Text = StCls_Function.VbStrConv(Add2Text.Text, (VbStrConv)4);

                    errorCheck = _func.Add2Check(Add2Text.Text);
                    error = (errorCheck != 0);
                    break;
                case "Add3Text":
                    // 全角変換
                    Add3Text.Text = StCls_Function.VbStrConv(Add3Text.Text, (VbStrConv)4);

                    errorCheck = _func.Add3Check(Add3Text.Text);
                    error = (errorCheck != 0);
                    break;
                case "Add4Text":
                    // 全角変換
                    Add4Text.Text = StCls_Function.VbStrConv(Add4Text.Text, (VbStrConv)4);

                    errorCheck = _func.Add4Check(Add4Text.Text);
                    error = (errorCheck != 0);
                    break;
                case "SeiText":
                    // 全角変換
                    SeiText.Text = StCls_Function.VbStrConv(SeiText.Text, (VbStrConv)4);

                    errorCheck = _func.SeiCheck(SeiText.Text);
                    error = (errorCheck != 0);
                    break;
                case "MeiText":
                    // 全角変換
                    MeiText.Text = StCls_Function.VbStrConv(MeiText.Text, (VbStrConv)4);

                    errorCheck = _func.MeiCheck(MeiText.Text);
                    error = (errorCheck == 117 || errorCheck == 118);
                    break;
                case "TelNoText":
                    // 半角変換
                    TelNoText.Text = StCls_Function.VbStrConv(TelNoText.Text, (VbStrConv)8);

                    error = (TelNoText.Text.Length < 10);
                    break;
                case "Ank1Text":
                    // 半角変換
                    Ank1Text.Text = StCls_Function.VbStrConv(Ank1Text.Text, (VbStrConv)8);

                    error = (!new[] { "1", "2", "9" }.Contains(Ank1Text.Text));
                    break;
                case "Ank2Text":
                    // 半角変換
                    Ank2Text.Text = StCls_Function.VbStrConv(Ank2Text.Text, (VbStrConv)8);

                    error = (!new[] { "1", "2", "3", "4", "9" }.Contains(Ank2Text.Text));
                    break;
                case "Ank3Text":
                    // 半角変換
                    Ank3Text.Text = StCls_Function.VbStrConv(Ank3Text.Text, (VbStrConv)8);

                    error = (!new[] { "1", "2", "3", "4", "5", "9" }.Contains(Ank3Text.Text));
                    break;
                case "NgOutKbText":
                    // 半角変換
                    NgOutKbText.Text = StCls_Function.VbStrConv(NgOutKbText.Text, (VbStrConv)8);

                    error = (NgOutKbText.Text != "0" && NgOutKbText.Text != "1");
                    break;
                case "HisoOutKbText":
                    // 半角変換
                    HisoOutKbText.Text = StCls_Function.VbStrConv(HisoOutKbText.Text, (VbStrConv)8);

                    error = (HisoOutKbText.Text != "0" && HisoOutKbText.Text != "1");
                    break;
            }

            TextBox textBox = this.Controls.Find(outForcus, true).FirstOrDefault() as TextBox;

            // エラーがあれば背景色を赤に変更
            textBox.BackColor = error ? Color.Red : SystemColors.Window;
        }
        #endregion

        #region 更新前データ変更チェック
        /// <summary>
        /// 登録済みデータと入力値が同じかチェック
        /// </summary>
        /// <returns>true:変更されていない  / false:変更されている</returns>
        private bool ChangeDataCheck()
        {
            // 状態区分のチェック状況取得
            string JyotaiKb = string.Empty;

            if (OK_RadioButton.Checked)
            {
                JyotaiKb = ((int)My_Function.JyotaiKb.Ok).ToString();
            }
            else if (NG_RadioButton.Checked)
            {
                JyotaiKb = ((int)My_Function.JyotaiKb.Ng).ToString();
            }
            else if (KEEP_RadioButton.Checked)
            {
                JyotaiKb = ((int)My_Function.JyotaiKb.Hold).ToString();
            }
            else if (CANCEL_RadioButton.Checked)
            {
                JyotaiKb = ((int)My_Function.JyotaiKb.Cancel).ToString();
            }

            // フィールドとデータを比較する配列
            (string, string)[] fieldsToCheck = new (string field, string textBoxValue)[]
            {
                ("ZIP_CD", ZipCdText.Text),
                ("ADD_1", Add1Text.Text),
                ("ADD_2", Add2Text.Text),
                ("ADD_3", Add3Text.Text),
                ("ADD_4", Add4Text.Text),
                ("NAME_SEI", SeiText.Text),
                ("NAME_MEI", MeiText.Text),
                ("TEL_NO", TelNoText.Text),
                ("ANK_1", Ank1Text.Text),
                ("ANK_2", Ank2Text.Text),
                ("ANK_3", Ank3Text.Text),
                ("JYOTAI_KB", JyotaiKb),
                ("NG_OUT_KB", NgOutKbText.Text),
                ("NG_OUT_DATETIME", NgOutDateTimeText.Text),
                ("HISO_OUT_KB", HisoOutKbText.Text)
            };

            // 変更確認フラグ
            bool changeCheckflag = true;

            // 各フィールドのチェック
            foreach ((string, string) field in fieldsToCheck)
            {
                if (field.Item2 != rowData[field.Item1].ToString())
                {
                    changeCheckflag = false;
                    break;
                }
            }

            return changeCheckflag;
        }
        #endregion
    }
}
