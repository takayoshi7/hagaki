using hagaki.Class;
using hagaki.StaticClass;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static hagaki.My_Function;
using static System.Net.WebRequestMethods;

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
        private Cls_DBConn _conn;                       // Cls_DBConnを使えるように
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

                // Cls_DBConnクラスをインスタンス化
                _conn = new Cls_DBConn(connectionString);

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
                InputDataPathLabel.Text = selectedFilePath;
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
            SqlTransaction transaction = null;

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
                    connection.Open();  // 接続を開く

                    using (SqlCommand command = connection.CreateCommand())
                    {
                        // トランザクション開始
                        transaction = connection.BeginTransaction();

                        // WKテーブル初期化
                        InitWK_Table(connection, transaction);

                        // 行番号
                        int lineNo = 1;

                        // WK_IN_MAINテーブルのカラムのサイズを取得SQL文の生成（DBサイズエラーチェック用）
                        string getColmunSizeSqlStr = $"SELECT COLUMN_NAME, CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{WK_MAIN}'";

                        // DataSetを作成
                        DataSet dataSet = new DataSet();

                        // パラメータを辞書で管理
                        //var parameters = new Dictionary<string, object>
                        //{
                        //    { "@Condition", "someValue" }
                        //};

                        // データを取得してDataSetに追加
                        _func.FillDataTable(dataSet, connection, transaction, getColmunSizeSqlStr, null, WK_MAIN + "_SIZE");

                        // カラムのサイズ用リスト
                        List<int> sizeList = new List<int>();

                        // 13項目のサイズをリストに入れる
                        for (int i = 0; i <= 12; i++)
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
                                // １行取り出し
                                string line = sr.ReadLine();

                                // タブで区切って配列に入れる
                                string[] dataArray = line.Split('\t');

                                #region 取り込み不可エラーチェック
                                #region **********項目数（レイアウトエラー）チェック**********
                                // エラーだった場合、WK_IN_MAIN_INSERT_ERRテーブルに登録
                                if (!_func.ItemsNumCheck(dataArray))
                                {
                                    // SQL文の生成
                                    string mainInsErrorSql = _func.MakeInsertSql(WK_MAIN_INSERT_ERROR, line, lineNo, (int)My_Function.ErrorCd.LayoutError);

                                    // SQL文を実行
                                    bool mainInsErrorExcuteCheck = _func.Execute(connection, transaction, mainInsErrorSql, null);

                                    // 次の繰り返し処理へ
                                    if (mainInsErrorExcuteCheck)
                                    {
                                        lineNo += 1;
                                        continue; // 次の繰り返しへ
                                    }
                                    else
                                    {
                                        throw new Exception("WK_IN_MAIN_INSERT_ERRテーブルに登録できませんでした。");
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
                                        string mainInsErrorSql = _func.MakeInsertSql(WK_MAIN_INSERT_ERROR, line, lineNo, (int)My_Function.ErrorCd.LayoutError);

                                        // SQL文を実行
                                        bool mainInsErrorExcuteCheck = _func.Execute(connection, transaction, mainInsErrorSql, null);

                                        // 次の繰り返し処理へ
                                        if (mainInsErrorExcuteCheck)
                                        {
                                            lineNo += 1;
                                            continue; // 次の繰り返しへ
                                        }
                                        else
                                        {
                                            throw new Exception("WK_IN_MAIN_INSERT_ERRテーブルに登録できませんでした。");
                                        }
                                    }
                                }
                                #endregion

                                #region 取り込み可能エラーチェック
                                // エラーコードによる状態区分
                                int errorCdKb = 0;

                                // 取込不可エラーが無い場合、エラーコードのエラーチェック
                                List<int> errorCdList = _func.ErrorCheck(dataArray);

                                foreach (int errorCd in errorCdList)
                                {
                                    // WK_IN_MAIN_ERRORテーブルに登録が必要なエラーコード（エラーレベル1または2）か
                                    if (errorCd != 116)
                                    {
                                        // SQL文の生成
                                        string mainErrorSql = _func.MakeInsertSql(WK_MAIN_ERROR, line, lineNo, errorCd);

                                        // SQL文を実行
                                        bool mainErrorExcuteCheck = _func.Execute(connection, transaction,  mainErrorSql, null);

                                        if (mainErrorExcuteCheck)
                                        {
                                            // エラーレベルが2であれば状態区分を1（NG）に
                                            if (errorCd != 102)
                                            {
                                                errorCdKb = 1;
                                            }
                                        }
                                        else
                                        {
                                            throw new Exception("WK_IN_MAIN_ERRORテーブルに登録できませんでした。");
                                        }
                                    }
                                }

                                // WK_IN_MAINテーブルに登録
                                // SQL文の生成
                                string mainStrSql = _func.MakeInsertSql(WK_MAIN, line, lineNo, errorCdKb);

                                // SQL文を実行
                                bool mainExcuteCheck = _func.Execute(connection, transaction, mainStrSql, null);

                                if (!mainExcuteCheck)
                                {
                                    throw new Exception("WK_IN_MAINテーブルに登録できませんでした。");
                                }
                                #endregion

                                // 次の繰り返し処理へ
                                lineNo += 1;
                            }
                        }










                        transaction.Commit();
                    }
                }


            }
            catch (SqlException sqlex)
            {
                // nullでなければロールバック
                transaction?.Rollback();

                MessageBox.Show(sqlex.Message, EXCEPTION_ERROR_TITLE);
            }
            catch (Exception ex)
            {
                // nullでなければロールバック
                transaction?.Rollback();

                MessageBox.Show(ex.Message, EXCEPTION_ERROR_TITLE);
            }
        }
        #endregion

        #region 取込
        private void ImportButton_DataImport_Click(object sender, EventArgs e)
        {
            try
            {

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
        /// <param name="connection">DB接続</param>
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
