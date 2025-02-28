using hagaki.Class;
using hagaki.StaticClass;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace hagaki
{
    public partial class Frm0100_IN_DATA : Form
    {
        #region メンバ変数
        private string userName = string.Empty;         // ユーザー名
        private string selectedFilePath = string.Empty; // 選択したファイルパス
        private string outputFolderPath = string.Empty; // 出力先フォルダパス
        private string connectionString = string.Empty; // 接続文字列
        private My_Function _func;                      // My_Functionを使えるように
        #endregion

        #region 定数
        private const string D_MAIN = "D_MAIN";                              // メインテーブル
        private const string D_ERROR = "D_ERROR";                            // エラーテーブル
        private const string WK_MAIN = "WK_IN_MAIN";                         // 取込データ登録テーブル
        private const string WK_MAIN_ERROR = "WK_IN_MAIN_ERROR";             // 取込可能エラー登録テーブル
        private const string WK_MAIN_INSERT_ERROR = "WK_IN_MAIN_INSERT_ERR"; // 取込不可エラー登録テーブル
        private const string OPERATION_XML = "KensyuSys.xml";                // XMLファイル名
        private const string OUTPUT_PATH_NODE = "DIR/OUT_IN_ERROR_FLDPATH";  // ノード
        private const string EXCEPTION_ERROR_TITLE = "例外エラー";           // 例外時表示メッセージタイトル
        #endregion

        #region コンストラクタ
        public Frm0100_IN_DATA()
        {
            InitializeComponent();
        }

        public Frm0100_IN_DATA(string connestStr)
        {
            InitializeComponent();

            connectionString = connestStr;
        }
        #endregion

        #region ロードイベント
        private void Frm0100_IN_DATA_Load(object sender, EventArgs e)
        {
            try
            {
                // ログインユーザー名取得
                userName = StCls_Function.GetUser();

                // XMLファイルを読込
                XElement xEle = XElement.Load(OPERATION_XML);

                // パスを分解
                string[] pathParts = OUTPUT_PATH_NODE.Split('/');

                // DIR要素を取得
                XElement dirElement = xEle.Element(pathParts[0]);

                // 値を取得
                outputFolderPath = dirElement?.Element(pathParts[1])?.Value ?? "";

                // エラーファイル出力先パス表示
                OutputPathLabel.Text = outputFolderPath;

                // My_Functionクラスをインスタンス化
                _func = new My_Function();

                // 件数初期化
                InitCount();
            }
            catch (IOException ioex)
            {
                MessageBox.Show(ioex.Message, EXCEPTION_ERROR_TITLE);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, EXCEPTION_ERROR_TITLE);
            }
        }
        #endregion

        #region 参照（ファイル選択）
        private void ReferenceButton_DataImport_Click(object sender, EventArgs e)
        {
            try
            {
                // ファイル選択（デスクトップをデフォルトディレクトリに設定した）
                selectedFilePath = StCls_File.FF_FileDialog("", $@"C:\Users\{userName}\Desktop", "", "テキスト文書(*.txt)|*.txt", 1);

                // 選択失敗したら終了
                if (string.IsNullOrEmpty(selectedFilePath))
                {
                    MessageBox.Show("選択することができませんでした。", "確認");
                    return;
                }

                // 選択したファイルパスを表示
                InputDataPathTextBox.Text = selectedFilePath;

                // 件数初期化
                InitCount();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, EXCEPTION_ERROR_TITLE);
            }
        }
        #endregion

        #region エラーファイル出力先のフォルダを開く
        private void OutputButton_DataImport_Click(object sender, EventArgs e)
        {
            try
            {
                // 出力先フォルダの存在チェック
                if (!Directory.Exists(outputFolderPath))
                {
                    // フォルダがない場合作成する
                    Directory.CreateDirectory(outputFolderPath);
                }

                // フォルダを開く
                bool openFileCheck = StCls_File.WindowOpen(outputFolderPath);

                if (!openFileCheck)
                {
                    MessageBox.Show("開くことができませんでした。", "確認");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, EXCEPTION_ERROR_TITLE);
            }
        }
        #endregion

        #region 読込
        private void ReadButton_DataImport_Click(object sender, EventArgs e)
        {
            try
            {
                // テキストファイルが選択されていなければ処理終了
                if (string.IsNullOrEmpty(selectedFilePath))
                {
                    MessageBox.Show("ファイルが選択されていません。", "確認");
                    return;
                }

                // 確認ダイアログ表示
                DialogResult dialog = MessageBox.Show("ファイルを読み込みます。よろしいでしょうか？", "確認", MessageBoxButtons.YesNo);

                // いいえを選択したら処理終了
                if (dialog == DialogResult.No)
                {
                    return;
                }

                // 件数初期化
                InitCount();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    // 接続を開く
                    connection.Open();

                    // トランザクション開始
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        using (SqlCommand command = connection.CreateCommand())
                        {
                            // WKテーブル初期化
                            InitWK_Table(connection, transaction);

                            // 行番号
                            int lineNo = 1;

                            // DataSetを作成
                            DataSet dataSet = new DataSet();

                            // WK_IN_MAINテーブルのカラムのサイズを取得SQL文の生成（DBサイズエラーチェック用）
                            string getColmunSizeSqlStr = $"SELECT COLUMN_NAME, CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{WK_MAIN}'";

                            // データを取得してDataSetに追加
                            _func.FillDataTable(dataSet, connection, transaction, getColmunSizeSqlStr, null, WK_MAIN + "_SIZE");

                            // カラムのサイズ用リスト
                            List<int> sizeList = new List<int>();

                            // 13項目のサイズをリストに入れる
                            for (int i = 0; i < 13; i++)
                            {
                                sizeList.Add((int)dataSet.Tables[WK_MAIN + "_SIZE"].Rows[i]["CHARACTER_MAXIMUM_LENGTH"]);
                            }

                            // D_MAINテーブルのKANRI_NOカラムの値を取得SQL文の生成（DBとの重複チェック用）
                            string getKanriNoSqlStr = $"SELECT KANRI_NO FROM {D_MAIN}";


                            _func.FillDataTable(dataSet, connection, transaction, getKanriNoSqlStr, null, D_MAIN);

                            DataTable mainTable = dataSet.Tables[D_MAIN];


                            // Shift-JISエンコーディングを指定してStreamReaderを作成
                            using (StreamReader sr = new StreamReader(selectedFilePath, Encoding.GetEncoding("shift_jis")))
                            {
                                // ファイルの最後まで繰り返し
                                while (!sr.EndOfStream)
                                {
                                    // 1レコード取り出し
                                    string line = sr.ReadLine();

                                    // タブで区切って配列に入れる
                                    string[] dataArray = line.Split('\t');

                                    // 1レコードのパラメータを辞書で管理
                                    Dictionary<string, object> lineParameters = new Dictionary<string, object>
                                    {
                                        { "@Line", line }
                                    };

                                    #region 取り込み不可エラーチェック
                                    #region **********項目数（レイアウトエラー）チェック**********
                                    // エラーだった場合、WK_IN_MAIN_INSERT_ERRテーブルに登録
                                    if (!_func.ItemsNumCheck(dataArray))
                                    {
                                        // SQL文の生成
                                        string mainInsErrorSql = _func.MakeInsertSql(WK_MAIN_INSERT_ERROR, lineNo, (int)My_Function.ErrorCd.LayoutError);

                                        // SQL文を実行
                                        bool mainInsErrorExcuteCheck = _func.Execute(connection, transaction, mainInsErrorSql, lineParameters);

                                        // 次の繰り返し処理へ
                                        if (mainInsErrorExcuteCheck)
                                        {
                                            lineNo += 1;
                                            continue; // 次の繰り返しへ
                                        }
                                        else
                                        {
                                            MessageBox.Show("WK_IN_MAIN_INSERT_ERRテーブルの登録に失敗しました。", "エラー");
                                            return;
                                        }
                                    }
                                    #endregion

                                    // 管理番号
                                    string kanriNo = dataArray[(int)My_Function.MainTableColumn.KanriNo].ToString();
                                    // 受付日
                                    string ukeDate = dataArray[(int)My_Function.MainTableColumn.UkeDate].ToString();
                                    // 登録不可エラー番号リスト
                                    List<int> registrationErrorNoList = new List<int>();

                                    #region **********事務局管理番号チェック**********
                                    // エラーであればリストに追加
                                    if (!_func.KanriNoCheck(kanriNo))
                                    {
                                        registrationErrorNoList.Add((int)My_Function.ErrorCd.IncorrectControlNumber);
                                    }
                                    #endregion

                                    #region **********事務局管理番号が既にDBに登録されていないかチェック**********
                                    bool dupliCheckDB = false;

                                    // 重複チェック
                                    foreach (DataRow drow in mainTable.Rows)
                                    {
                                        if (drow["KANRI_NO"].ToString() == kanriNo)
                                        {
                                            dupliCheckDB = true;
                                        }
                                    }

                                    // 重複していればリストに追加
                                    if (dupliCheckDB)
                                    {
                                        registrationErrorNoList.Add((int)My_Function.ErrorCd.ImportedControlNumber);
                                    }
                                    #endregion

                                    #region **********受付日チェック**********
                                    // エラーであればリストに追加
                                    if (!_func.UkeDateCheck(ukeDate))
                                    {
                                        registrationErrorNoList.Add((int)My_Function.ErrorCd.IncorrectReceptionDate);
                                    }
                                    #endregion

                                    #region **********DBサイズエラーチェック**********
                                    bool DBSizeErrorFlg = false;

                                    // 郵便番号からアンケート（職業）までのサイズチェック
                                    for (int i = 2; i <= 12; i++)
                                    {
                                        if (sizeList[i] < dataArray[i].Length)
                                        {
                                            DBSizeErrorFlg = true;
                                            break;
                                        }
                                    }

                                    // サイズエラーであればリストに追加
                                    if (DBSizeErrorFlg)
                                    {
                                        registrationErrorNoList.Add((int)My_Function.ErrorCd.DBSizeError);
                                    }
                                    #endregion

                                    // 登録不可エラーがあれば
                                    if (registrationErrorNoList.Count != 0)
                                    {
                                        // 昇順に並び替え
                                        registrationErrorNoList.Sort();

                                        foreach (int errCd in registrationErrorNoList)
                                        {
                                            // SQL文の生成
                                            string mainInsErrorSql = _func.MakeInsertSql(WK_MAIN_INSERT_ERROR, lineNo, errCd);

                                            // SQL文を実行
                                            bool mainInsErrorExcuteCheck = _func.Execute(connection, transaction, mainInsErrorSql, lineParameters);

                                            // 次の繰り返し処理へ
                                            if (!mainInsErrorExcuteCheck)
                                            {
                                                MessageBox.Show("WK_IN_MAIN_INSERT_ERRテーブルの登録に失敗しました。", "エラー");
                                                return;
                                            }
                                        }
                                    }
                                    #endregion
                                    #region 取り込み可能エラーチェック
                                    else
                                    {
                                        // エラーコードによる状態区分
                                        int jotaiKb = 0;

                                        // 取込不可エラーが無い場合、エラーコードのエラーチェック
                                        List<int> errorCdList = _func.ErrorCheck(dataArray);

                                        //エラーコードがある場合
                                        if (errorCdList.Count != 0)
                                        {
                                            foreach (int errorCd in errorCdList)
                                            {
                                                // WK_IN_MAIN_ERRORテーブルに登録が不要なエラーコード（エラーレベル0）の場合
                                                if (errorCd == 116)
                                                {
                                                    continue;
                                                }

                                                // WK_IN_MAIN_ERRORテーブルに登録が必要なエラーコード（エラーレベル1または2）の場合
                                                // SQL文の生成
                                                string mainErrorSql = _func.MakeInsertSql(WK_MAIN_ERROR, lineNo, errorCd);

                                                // 1レコードのパラメータを辞書で管理
                                                Dictionary<string, object> kanriNoParameter = new Dictionary<string, object>
                                                {
                                                    { "@KanriNo", kanriNo }
                                                };

                                                // SQL文を実行
                                                bool mainErrorExcuteCheck = _func.Execute(connection, transaction, mainErrorSql, kanriNoParameter);

                                                if (!mainErrorExcuteCheck)
                                                {
                                                    MessageBox.Show("WK_IN_MAIN_ERRORテーブルの登録に失敗しました。", "エラー");
                                                    return;
                                                }

                                                // エラーレベルが2であれば状態区分を1（NG）に
                                                if (errorCd != 102)
                                                {
                                                    jotaiKb = 1;
                                                }
                                            }
                                        }

                                        // 値をテーブル登録用に強制変換
                                        dataArray = _func.ForcedConversion(dataArray);

                                        // 項目ごとのパラメータを辞書で管理
                                        Dictionary<string, object> parameters = _func.KeyValuePairs(dataArray, line);

                                        // WK_IN_MAINテーブルに登録
                                        // SQL文の生成
                                        string mainStrSql = _func.MakeInsertSql(WK_MAIN, lineNo, jotaiKb);

                                        // SQL文を実行
                                        bool mainExcuteCheck = _func.Execute(connection, transaction, mainStrSql, parameters);

                                        if (!mainExcuteCheck)
                                        {
                                            MessageBox.Show("WK_IN_MAINテーブルの登録に失敗しました。", "エラー");
                                        }
                                    }
                                    #endregion

                                    // 次の繰り返し処理へ
                                    lineNo += 1;
                                }
                            }

                            #region **********事務局管理番号ファイル内重複チェック**********
                            // WK_IN_MAINテーブルの重複するKANRI_NOカラムの値を取得SQL文の生成
                            string getDupliKanriNoSqlStr = $"SELECT KANRI_NO, COUNT(KANRI_NO) AS COUNT FROM {WK_MAIN} GROUP BY KANRI_NO HAVING COUNT(KANRI_NO) > 1";

                            // データを取得してDataSetに追加
                            _func.FillDataTable(dataSet, connection, transaction, getDupliKanriNoSqlStr, null, WK_MAIN + "_COUNT");

                            // 重複した事務局管理番号用リスト
                            List<string> dupliKanriNoList = new List<string>();

                            // 重複した事務局管理番号をセット
                            for (int i = 0; i < dataSet.Tables[WK_MAIN + "_COUNT"].Rows.Count; i++)
                            {
                                dupliKanriNoList.Add((string)dataSet.Tables[WK_MAIN + "_COUNT"].Rows[i]["KANRI_NO"]);
                            }

                            foreach (string kanriNo in dupliKanriNoList)
                            {
                                // WK_IN_MAINテーブルの重複したレコードを取得SQL文の生成
                                string getDupliDataSqlStr = $"SELECT KANRI_NO, OFFSET, LINE_DATA FROM {WK_MAIN} WHERE KANRI_NO = {kanriNo}";

                                // 1レコードのパラメータを辞書で管理
                                Dictionary<string, object> dupliKanriNoParameter = new Dictionary<string, object>
                                {
                                    { "@KanriNo", kanriNo }
                                };

                                // データを取得してDataSetに追加
                                _func.FillDataTable(dataSet, connection, transaction, getDupliDataSqlStr, null, WK_MAIN + "_DUPLI");

                                for (int i = 0; i < dataSet.Tables[WK_MAIN + "_DUPLI"].Rows.Count; i++)
                                {
                                    // 重複データは取込不可エラーに（WK_IN_MAIN_INSERT_ERRORテーブルに登録）
                                    // インサートSQL文の生成
                                    string mainInsErrorSqlStr = _func.MakeInsertSql(WK_MAIN_INSERT_ERROR, (int)dataSet.Tables[WK_MAIN + "_DUPLI"].Rows[i]["OFFSET"], (int)My_Function.ErrorCd.DuplicateControlNumber);

                                    // 1レコードのパラメータを辞書で管理
                                    Dictionary<string, object> lineParameters = new Dictionary<string, object>
                                    {
                                        { "@line", dataSet.Tables[WK_MAIN + "_DUPLI"].Rows[i]["LINE_DATA"] }
                                    };

                                    // インサートSQL文を実行
                                    bool mainInsErrorExcuteCheck = _func.Execute(connection, transaction, mainInsErrorSqlStr, lineParameters);

                                    if (!mainInsErrorExcuteCheck)
                                    {
                                        MessageBox.Show("WK_IN_MAIN_INSERT_ERRテーブルの登録に失敗しました。", "エラー");
                                        return;
                                    }

                                    // 重複データをWK_IN_MAINテーブルから削除
                                    // デリートSQL文の生成
                                    string mainDeleteSqlStr = _func.MakeDeleteSql(WK_MAIN, (string)dataSet.Tables[WK_MAIN + "_DUPLI"].Rows[i]["KANRI_NO"], (int)dataSet.Tables[WK_MAIN + "_DUPLI"].Rows[i]["OFFSET"]);

                                    // 1レコードのパラメータを辞書で管理
                                    Dictionary<string, object> deleteParameter = new Dictionary<string, object>
                                    {
                                        { "@KanriNo", dataSet.Tables[WK_MAIN + "_DUPLI"].Rows[i]["KANRI_NO"] }
                                    };

                                    // インサートSQL文を実行
                                    bool mainDeleteExcuteCheck = _func.Execute(connection, transaction, mainDeleteSqlStr, deleteParameter);

                                    if (!mainDeleteExcuteCheck)
                                    {
                                        MessageBox.Show("WK_IN_MAINテーブルの削除に失敗しました。", "エラー");
                                        return;
                                    }
                                }
                            }
                            #endregion

                            // 件数表示
                            Total_Count.Text = (lineNo - 1).ToString();
                            OK_Count.Text = (_func.getRecordCount(connection, transaction, WK_MAIN, "JYOTAI_KB", "JYOTAI_KB = 0")).ToString();
                            NG_Count.Text = (_func.getRecordCount(connection, transaction, WK_MAIN, "JYOTAI_KB", "JYOTAI_KB = 1")).ToString();
                            Layout_Error.Text = (_func.getRecordCount(connection, transaction, WK_MAIN_INSERT_ERROR, "ERR_NO", $"ERR_NO = {(int)My_Function.ErrorCd.LayoutError}")).ToString();
                            ControlNumber_Error.Text = (_func.getRecordCount(connection, transaction, WK_MAIN_INSERT_ERROR, "ERR_NO", $"ERR_NO = {(int)My_Function.ErrorCd.IncorrectControlNumber}")).ToString();
                            Imported_Error.Text = (_func.getRecordCount(connection, transaction, WK_MAIN_INSERT_ERROR, "ERR_NO", $"ERR_NO = {(int)My_Function.ErrorCd.ImportedControlNumber}")).ToString();
                            Duplication_Error.Text = (_func.getRecordCount(connection, transaction, WK_MAIN_INSERT_ERROR, "ERR_NO", $"ERR_NO = {(int)My_Function.ErrorCd.DuplicateControlNumber}")).ToString();
                            ReceptionDate_Error.Text = (_func.getRecordCount(connection, transaction, WK_MAIN_INSERT_ERROR, "ERR_NO", $"ERR_NO = {(int)My_Function.ErrorCd.IncorrectReceptionDate}")).ToString();
                            DB_Size_Error.Text = (_func.getRecordCount(connection, transaction, WK_MAIN_INSERT_ERROR, "ERR_NO", $"ERR_NO =  {(int)My_Function.ErrorCd.DBSizeError}")).ToString();

                            // 取り込み不可エラーがある場合
                            if (_func.getRecordCount(connection, transaction, WK_MAIN_INSERT_ERROR, "ERR_NO") > 0)
                            {
                                // エラーログファイル作成
                                MakeErrLogFile(dataSet, connection, transaction);

                                MessageBox.Show("エラーログファイルを作成しました。", "確認");
                            }

                            // コミット
                            transaction.Commit();
                        }
                    }
                }

                // テキストボックスから取得した値を整数に変換
                int okValue = int.Parse(OK_Count.Text);
                int ngValue = int.Parse(NG_Count.Text);

                // 登録可能件数が1件以上であれば、取込ボタンをクリック可能にする
                if ((okValue + ngValue) > 0)
                {
                    ImportButton_DataImport.Enabled = true;
                    ImportButton_DataImport.BackColor = SystemColors.GradientActiveCaption;
                    ImportButton_DataImport.Cursor = Cursors.Hand;
                }
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

        #region 取込
        private void ImportButton_DataImport_Click(object sender, EventArgs e)
        {
            try
            {
                // 確認ダイアログ表示
                DialogResult dialog = MessageBox.Show("ファイルを取り込みます。よろしいでしょうか？", "確認", MessageBoxButtons.YesNo);

                // いいえを選択したら処理終了
                if (dialog == DialogResult.No)
                {
                    return;
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // トランザクション開始
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        // DataSetを作成
                        DataSet dataSet = new DataSet();

                        #region WK_IN_MAINのデータをD_MAINにコピー
                        // WK_IN_MAINテーブルのデータを取得SQL文の生成
                        string getWkMainSqlStr = $"SELECT * FROM {WK_MAIN}";

                        // データを取得してDataSetに追加
                        _func.FillDataTable(dataSet, connection, transaction, getWkMainSqlStr, null, WK_MAIN);

                        // レコードごとの処理
                        foreach (DataRow row in dataSet.Tables[WK_MAIN].Rows)
                        {
                            // DataSetのテーブルから値をオブジェクト配列に取得
                            object[] itemArray = row.ItemArray;

                            // オブジェクト配列からストリング配列に変換
                            string[] dataArray = Array.ConvertAll(itemArray, item => item.ToString());

                            // D_MAINテーブルのSQL文生成
                            string dMainStrSql = _func.MakeInsertSql(D_MAIN, 0, int.Parse(dataArray[13]));

                            // 項目ごとのパラメータを辞書で管理
                            Dictionary<string, object> parameters = _func.KeyValuePairs(dataArray);

                            // SQL文を実行
                            bool dMainExcuteCheck = _func.Execute(connection, transaction, dMainStrSql, parameters);

                            if (!dMainExcuteCheck)
                            {
                                MessageBox.Show("D_MAINテーブルの登録に失敗しました。", "エラー");
                                return;
                            }
                        }
                        #endregion

                        #region WK_IN_MAIN_ERRORのデータをD_ERRORにコピー
                        // WK_IN_MAIN_ERRORテーブルのデータを取得SQL文の生成
                        string getWkMainErrSqlStr = $"SELECT * FROM {WK_MAIN_ERROR}";

                        // データを取得してDataSetに追加
                        _func.FillDataTable(dataSet, connection, transaction, getWkMainErrSqlStr, null, WK_MAIN_ERROR);

                        // レコードごとの処理
                        foreach (DataRow row in dataSet.Tables[WK_MAIN_ERROR].Rows)
                        {
                            // D_ERRORテーブルのSQL文生成
                            string dErrorStrSql = _func.MakeInsertSql(D_ERROR, 0, int.Parse(row["ERR_CD"].ToString()));

                            // 1レコードのパラメータを辞書で管理
                            Dictionary<string, object> kanriNoParameter = new Dictionary<string, object>
                            {
                                { "@KanriNo", row["KANRI_NO"].ToString() }
                            };

                            // SQL文を実行（WK_IN_MAIN_ERRORのデータをD_ERRORに）
                            bool dErrorExcuteCheck = _func.Execute(connection, transaction, dErrorStrSql, kanriNoParameter);

                            if (!dErrorExcuteCheck)
                            {
                                MessageBox.Show("D_ERRORテーブルの登録に失敗しました。", "エラー");
                                return;
                            }
                        }
                        #endregion

                        // WKテーブル初期化
                        InitWK_Table(connection, transaction);

                        // コミット
                        transaction.Commit();
                    }
                }

                // 件数初期化
                InitCount();

                MessageBox.Show("取り込みが完了しました。", "確認");
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
        private void EndButton_DataImport_Click(object sender, EventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, EXCEPTION_ERROR_TITLE);
            }
        }
        #endregion

        #region 件数初期化
        /// <summary>
        /// 件数を初期化する
        /// </summary>
        private void InitCount()
        {
            Total_Count.Text = "";
            OK_Count.Text = "";
            NG_Count.Text = "";
            Layout_Error.Text = "";
            ControlNumber_Error.Text = "";
            Imported_Error.Text = "";
            Duplication_Error.Text = "";
            ReceptionDate_Error.Text = "";
            DB_Size_Error.Text = "";

            ImportButton_DataImport.Enabled = false;
            ImportButton_DataImport.BackColor = SystemColors.ControlDark;
            ImportButton_DataImport.Cursor = Cursors.Default;
        }
        #endregion

        #region WKテーブル初期化
        /// <summary>
        /// WKテーブル初期化する
        /// </summary>
        /// <param name="connection">SqlConnection</param>
        /// <param name="transaction">SqlTransaction</param>
        private void InitWK_Table(SqlConnection connection, SqlTransaction transaction)
        {
            try
            {
                // WK_IN_MAINデリートSQL文の生成
                string wkInMainDeleteSql = _func.MakeDeleteSql(WK_MAIN);

                // WK_IN_MAINデリートSQL文を実行
                bool wkInMainDeleteExcuteCheck = _func.Execute(connection, transaction, wkInMainDeleteSql, null);

                // WK_IN_MAIN_ERRORデリートSQL文の生成
                string wkInMainErrorDeleteSql = _func.MakeDeleteSql(WK_MAIN_ERROR);

                // WK_IN_MAIN_ERRORデリートSQL文を実行
                bool wkInMainErrorDeleteExcuteCheck = _func.Execute(connection, transaction, wkInMainErrorDeleteSql, null);

                // WK_IN_MAIN_INSERT_ERRデリートSQL文の生成
                string wkInMainInsertErrDeleteSql = _func.MakeDeleteSql(WK_MAIN_INSERT_ERROR);

                // WK_IN_MAIN_INSERT_ERRデリートSQL文を実行
                bool wkInMainInsertErrDeleteExcuteCheck = _func.Execute(connection, transaction, wkInMainInsertErrDeleteSql, null);

                if (!wkInMainDeleteExcuteCheck || !wkInMainErrorDeleteExcuteCheck || !wkInMainInsertErrDeleteExcuteCheck)
                {
                    throw new Exception("WKテーブルの初期化に失敗しました。");
                }
            }
            catch (SqlException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region エラーログファイル作成
        /// <summary>
        /// エラーログファイルを作成する
        /// </summary>
        /// <param name="dataSet">DataSet</param>
        /// <param name="connection">SqlConnection</param>
        /// <param name="transaction">SqlTransaction</param>
        private void MakeErrLogFile(DataSet dataSet, SqlConnection connection, SqlTransaction transaction)
        {
            // 現在の日時を取得
            string nowDateTime = DateTime.Now.ToString("yyyyMMddHHmmss");

            // 出力先フォルダがない場合フォルダを作成する
            if (!Directory.Exists(outputFolderPath))
            {
                Directory.CreateDirectory(outputFolderPath);
            }

            // 同名エラーログファイル存在時、追加する文字
            int i = 0;

            // テキストファイルの存在チェック（存在していればファイル名に追加する文字列作成）
            while (File.Exists(Path.Combine(outputFolderPath, $"取込エラー_{nowDateTime}{_func.NumStr(i)}.txt")))
            {
                i += 1;
            }

            // WK_IN_MAIN_INSERT_ERRORテーブルの全レコードを取得SQL文の生成
            string getMainInsErrTableStrSql = $"SELECT * FROM {WK_MAIN_INSERT_ERROR}";

            _func.FillDataTable(dataSet, connection, transaction, getMainInsErrTableStrSql, null, WK_MAIN_INSERT_ERROR);

            // 各エラーレコード用リスト
            List<string> LayoutErrorList = new List<string>();
            List<string> IncorrectControlNumberErrorList = new List<string>();
            List<string> ImportedControlNumberErrorList = new List<string>();
            List<string> DuplicateControlNumberErrorList = new List<string>();
            List<string> IncorrectReceptionDateErrorList = new List<string>();
            List<string> DBSizeErrorList = new List<string>();

            // エラーごとにリストにセット
            foreach (DataRow record in dataSet.Tables[WK_MAIN_INSERT_ERROR].Rows)
            {
                switch ((int)record["ERR_NO"])
                {
                    case (int)My_Function.ErrorCd.LayoutError:
                        LayoutErrorList.Add(record["LINE_DATA"].ToString());
                        break;
                    case (int)My_Function.ErrorCd.IncorrectControlNumber:
                        IncorrectControlNumberErrorList.Add(record["LINE_DATA"].ToString());
                        break;
                    case (int)My_Function.ErrorCd.ImportedControlNumber:
                        ImportedControlNumberErrorList.Add(record["LINE_DATA"].ToString());
                        break;
                    case (int)My_Function.ErrorCd.DuplicateControlNumber:
                        DuplicateControlNumberErrorList.Add(record["LINE_DATA"].ToString());
                        break;
                    case (int)My_Function.ErrorCd.IncorrectReceptionDate:
                        IncorrectReceptionDateErrorList.Add(record["LINE_DATA"].ToString());
                        break;
                    case (int)My_Function.ErrorCd.DBSizeError:
                        DBSizeErrorList.Add(record["LINE_DATA"].ToString());
                        break;
                }
            }

            // テキストファイル作成（false:上書き）
            using (StreamWriter writeText = new StreamWriter(Path.Combine(outputFolderPath, $"取込エラー_{nowDateTime}{_func.NumStr(i)}.txt"), false, Encoding.UTF8))
            {
                // ファイルに書き込む
                if (LayoutErrorList.Count != 0)
                {
                    writeText.WriteLine("【レイアウトエラー】");
                    foreach (string errRecord in LayoutErrorList)
                    {
                        writeText.WriteLine(errRecord);
                    }
                }

                if (IncorrectControlNumberErrorList.Count != 0)
                {
                    writeText.WriteLine("【事務局管理番号不備】");
                    foreach (string errRecord in IncorrectControlNumberErrorList)
                    {
                        writeText.WriteLine(errRecord);
                    }
                }

                if (ImportedControlNumberErrorList.Count != 0)
                {
                    writeText.WriteLine("【既に取込済みの事務局管理番号】");
                    foreach (string errRecord in ImportedControlNumberErrorList)
                    {
                        writeText.WriteLine(errRecord);
                    }
                }

                if (DuplicateControlNumberErrorList.Count != 0)
                {
                    writeText.WriteLine("【事務局管理番号がファイル内で重複】");
                    foreach (string errRecord in DuplicateControlNumberErrorList)
                    {
                        writeText.WriteLine(errRecord);
                    }
                }

                if (IncorrectReceptionDateErrorList.Count != 0)
                {
                    writeText.WriteLine("【受付日が不備】");
                    foreach (string errRecord in IncorrectReceptionDateErrorList)
                    {
                        writeText.WriteLine(errRecord);
                    }
                }

                if (DBSizeErrorList.Count != 0)
                {
                    writeText.WriteLine("【DBサイズエラー】");
                    foreach (string errRecord in DBSizeErrorList)
                    {
                        writeText.WriteLine(errRecord);
                    }
                }
            }
        }
        #endregion

        #region クロージングイベント
        private void Frm0100_IN_DATA_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                // 読込処理後、取込処理が行われていない場合
                if (ImportButton_DataImport.Enabled)
                {
                    // 確認ダイアログ表示
                    DialogResult dialog = MessageBox.Show("データが出力されていません。画面を閉じてもよろしいですか？", "確認", MessageBoxButtons.YesNo);

                    // いいえを選択したら処理終了
                    if (dialog == DialogResult.No)
                    {
                        e.Cancel = true;
                        return;
                    }

                    // 終了する場合は、閉じる前にWKテーブルを初期化する
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        // トランザクション開始
                        using (SqlTransaction transaction = connection.BeginTransaction())
                        {
                            // WKテーブル初期化
                            InitWK_Table(connection, transaction);

                            // コミット
                            transaction.Commit();
                        }
                    }
                }
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
    }
}
