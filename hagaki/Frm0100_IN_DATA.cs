using hagaki.Class;
using hagaki.StaticClass;
using Microsoft.VisualBasic;
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
        private string selectedFilePath = string.Empty; // 選択したファイルパス
        private string outputFolderPath = string.Empty; // 出力先フォルダパス
        private string connectionString = string.Empty; // 接続文字列
        private MyClass _myClass;                       // MyClassを使えるように
        #endregion

        #region 定数
        private const string OUTPUT_PATH_NODE = "DIR/OUT_IN_ERROR_FLDPATH"; // ノード
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
                // XMLファイルを読込
                XElement xEle = XElement.Load(MyStaticClass.OPERATION_XML);

                // パスを分解
                string[] pathParts = OUTPUT_PATH_NODE.Split('/');

                // DIR要素を取得
                XElement dirElement = xEle.Element(pathParts[0]);

                // 値を取得
                outputFolderPath = dirElement?.Element(pathParts[1])?.Value ?? "";

                // エラーファイル出力先パス表示
                OutputPathLabel.Text = outputFolderPath;

                // MyClassクラスをインスタンス化
                _myClass = new MyClass();

                // 件数初期化
                InitCount();
            }
            catch (IOException ioex)
            {
                MessageBox.Show(ioex.Message, MyStaticClass.EXCEPTION_ERROR_TITLE);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, MyStaticClass.EXCEPTION_ERROR_TITLE);
            }
        }
        #endregion

        #region 参照（ファイル選択）
        private void ReferenceButton_DataImport_Click(object sender, EventArgs e)
        {
            try
            {
                // ファイル選択（デスクトップをデフォルトディレクトリに設定した）
                selectedFilePath = StCls_File.FF_FileDialog("", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "", "テキストファイル(*.txt)|*.txt", 1);

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
                MessageBox.Show(ex.Message, MyStaticClass.EXCEPTION_ERROR_TITLE);
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
                MessageBox.Show(ex.Message, MyStaticClass.EXCEPTION_ERROR_TITLE);
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

                // マウスカーソルを砂時計にする
                Cursor = Cursors.WaitCursor;

                // 件数初期化
                InitCount();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    // 接続を開く
                    connection.Open();

                    // トランザクション開始（閉じる時、コミットされていなければ自動ロールバック）
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        // WKテーブル初期化
                        InitWK_Table(connection, transaction);

                        // 行番号
                        int lineNo = 1;

                        // DataSetを作成
                        DataSet dataSet = new DataSet();

                        // WK_IN_MAINテーブルのカラムのサイズを取得SQL文の生成（DBサイズエラーチェック用）
                        string getColmunSizeSqlStr = $"SELECT COLUMN_NAME, CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{MyStaticClass.WK_MAIN}'";

                        // データを取得してDataSetに追加
                        _myClass.FillDataTable(dataSet, connection, transaction, getColmunSizeSqlStr, null, MyStaticClass.WK_MAIN + "_SIZE");

                        // カラムのサイズ用リスト
                        List<int> sizeList = new List<int>();

                        // 13項目のサイズをリストに入れる
                        for (int i = 0; i < 13; i++)
                        {
                            sizeList.Add((int)dataSet.Tables[MyStaticClass.WK_MAIN + "_SIZE"].Rows[i]["CHARACTER_MAXIMUM_LENGTH"]);
                        }

                        // D_MAINテーブルのKANRI_NOカラムの値を取得SQL文の生成（DBとの重複チェック用）
                        string getKanriNoSqlStr = $"SELECT KANRI_NO FROM {MyStaticClass.D_MAIN}";

                        _myClass.FillDataTable(dataSet, connection, transaction, getKanriNoSqlStr, null, MyStaticClass.D_MAIN);

                        DataTable mainTable = dataSet.Tables[MyStaticClass.D_MAIN];

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
                                #region **********レイアウトエラーチェック**********
                                // エラーだった場合、他エラーチェックはせずWK_IN_MAIN_INSERT_ERRテーブルに登録
                                if (!ItemsCountCheck(dataArray))
                                {
                                    // SQL文の生成
                                    string mainInsErrSql = MyStaticClass.MakeInsertSql(MyStaticClass.WK_MAIN_INSERT_ERROR, lineNo, (int)ErrorCd.LayoutError);

                                    // SQL文を実行
                                    bool mainInsErrorExcuteCheck = MyStaticClass.Execute(connection, transaction, mainInsErrSql, lineParameters);

                                    // WK_IN_MAIN_INSERT_ERRテーブルに登録できたか
                                    if (mainInsErrorExcuteCheck)
                                    {
                                        // 登録できた場合、次の繰り返し処理へ
                                        lineNo += 1;
                                        continue;
                                    }
                                    else
                                    {
                                        // 登録できなければエラーメッセージを表示して処理終了＆トランザクションロールバック
                                        MessageBox.Show("WK_IN_MAIN_INSERT_ERRテーブルの登録に失敗しました。", "エラー");
                                        return;
                                    }
                                }
                                #endregion

                                // 管理番号
                                string kanriNo = dataArray[(int)MainTableColumn.KanriNo].ToString();
                                // 受付日
                                string ukeDate = dataArray[(int)MainTableColumn.UkeDate].ToString();
                                // 登録不可エラー番号リスト
                                List<int> registrationErrorNoList = new List<int>();

                                #region **********事務局管理番号チェック**********
                                // エラーであればリストに追加
                                if (!KanriNoCheck(kanriNo))
                                {
                                    registrationErrorNoList.Add((int)ErrorCd.IncorrectControlNumber);
                                }
                                #endregion

                                #region **********既に取込済み事務局管理番号チェック**********
                                // 重複していればリストに追加
                                if (!ImportedKanriNoCheck(mainTable, kanriNo))
                                {
                                    registrationErrorNoList.Add((int)ErrorCd.ImportedControlNumber);
                                }
                                #endregion

                                #region **********受付日チェック**********
                                // エラーであればリストに追加
                                if (!UkeDateCheck(ukeDate))
                                {
                                    registrationErrorNoList.Add((int)ErrorCd.IncorrectReceptionDate);
                                }
                                #endregion

                                #region **********DBサイズチェック**********
                                // サイズエラーであればリストに追加
                                if (!DBSizeCheck(sizeList, dataArray))
                                {
                                    registrationErrorNoList.Add((int)ErrorCd.DBSizeError);
                                }
                                #endregion

                                // 登録不可エラーがあればWK_IN_MAIN_INSERT_ERRテーブルに登録
                                if (registrationErrorNoList.Count != 0)
                                {
                                    // 昇順に並び替え
                                    registrationErrorNoList.Sort();

                                    foreach (int errCd in registrationErrorNoList)
                                    {
                                        // SQL文の生成
                                        string mainInsErrorSql = MyStaticClass.MakeInsertSql(MyStaticClass.WK_MAIN_INSERT_ERROR, lineNo, errCd);

                                        // SQL文を実行
                                        bool mainInsErrorExcuteCheck = MyStaticClass.Execute(connection, transaction, mainInsErrorSql, lineParameters);

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
                                    List<int> errorCdList = MyStaticClass.ErrorCheck(dataArray);

                                    //エラーコードがある場合
                                    if (errorCdList.Count != 0)
                                    {
                                        foreach (int errorCd in errorCdList)
                                        {
                                            // WK_IN_MAIN_ERRORテーブルに登録が必要なエラーコード（エラーレベル1または2）の場合
                                            // SQL文の生成
                                            string mainErrorSql = MyStaticClass.MakeInsertSql(MyStaticClass.WK_MAIN_ERROR, lineNo, errorCd);

                                            // 1レコードのパラメータを辞書で管理
                                            Dictionary<string, object> kanriNoParameter = new Dictionary<string, object>
                                            {
                                                { "@KanriNo", kanriNo }
                                            };

                                            // SQL文を実行
                                            bool mainErrorExcuteCheck = MyStaticClass.Execute(connection, transaction, mainErrorSql, kanriNoParameter);

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
                                    dataArray = ForcedConversion(dataArray);

                                    // 項目ごとのパラメータを辞書で管理
                                    Dictionary<string, object> parameters = MyStaticClass.KeyValuePairs(dataArray, line);

                                    // WK_IN_MAINテーブルに登録
                                    // SQL文の生成
                                    string mainStrSql = MyStaticClass.MakeInsertSql(MyStaticClass.WK_MAIN, lineNo, jotaiKb);

                                    // SQL文を実行
                                    bool mainExcuteCheck = MyStaticClass.Execute(connection, transaction, mainStrSql, parameters);

                                    if (!mainExcuteCheck)
                                    {
                                        MessageBox.Show("WK_IN_MAINテーブルの登録に失敗しました。", "エラー");
                                        return;
                                    }
                                }
                                #endregion

                                // 番号を足し、次の繰り返し処理へ
                                lineNo += 1;
                            }
                        }

                        #region **********事務局管理番号ファイル内重複チェック**********
                        // 重複データがある場合
                        if (DuplicateKanriNoCheck(dataSet, connection, transaction) > 0)
                        {
                            foreach (DataRow row in dataSet.Tables[MyStaticClass.WK_MAIN + "_DUPLI"].Rows)
                            {
                                // 重複データは取込不可エラーに（WK_IN_MAIN_INSERT_ERRORテーブルに登録）
                                // インサートSQL文の生成
                                string mainInsErrorSqlStr = MyStaticClass.MakeInsertSql(MyStaticClass.WK_MAIN_INSERT_ERROR, (int)row["OFFSET"], (int)ErrorCd.DuplicateControlNumber);

                                // 1レコードのパラメータを辞書で管理
                                Dictionary<string, object> lineParameters = new Dictionary<string, object>
                                {
                                    { "@line", row["LINE_DATA"].ToString() }
                                };

                                // インサートSQL文を実行
                                bool mainInsErrorExcuteCheck = MyStaticClass.Execute(connection, transaction, mainInsErrorSqlStr, lineParameters);

                                if (!mainInsErrorExcuteCheck)
                                {
                                    MessageBox.Show("WK_IN_MAIN_INSERT_ERRテーブルの登録に失敗しました。", "エラー");
                                    return;
                                }

                                // 重複データをWK_IN_MAINテーブルから削除
                                // デリートSQL文の生成
                                string mainDeleteSqlStr = MyStaticClass.MakeDeleteSql(MyStaticClass.WK_MAIN, row["KANRI_NO"].ToString(), (int)row["OFFSET"]);

                                // 1レコードのパラメータを辞書で管理
                                Dictionary<string, object> deleteParameter = new Dictionary<string, object>
                                {
                                    { "@KanriNo", row["KANRI_NO"].ToString() }
                                };

                                // デリートSQL文を実行
                                bool mainDeleteExcuteCheck = MyStaticClass.Execute(connection, transaction, mainDeleteSqlStr, deleteParameter);

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
                        OK_Count.Text = (MyStaticClass.GetRecordCount(connection, transaction, MyStaticClass.WK_MAIN, "JYOTAI_KB", $"JYOTAI_KB = {(int)JyotaiKb.Ok}")).ToString();
                        NG_Count.Text = (MyStaticClass.GetRecordCount(connection, transaction, MyStaticClass.WK_MAIN, "JYOTAI_KB", $"JYOTAI_KB = {(int)JyotaiKb.Ng}")).ToString();
                        Layout_Error.Text = (MyStaticClass.GetRecordCount(connection, transaction, MyStaticClass.WK_MAIN_INSERT_ERROR, "ERR_NO", $"ERR_NO = {(int)ErrorCd.LayoutError}")).ToString();
                        ControlNumber_Error.Text = (MyStaticClass.GetRecordCount(connection, transaction, MyStaticClass.WK_MAIN_INSERT_ERROR, "ERR_NO", $"ERR_NO = {(int)ErrorCd.IncorrectControlNumber}")).ToString();
                        Imported_Error.Text = (MyStaticClass.GetRecordCount(connection, transaction, MyStaticClass.WK_MAIN_INSERT_ERROR, "ERR_NO", $"ERR_NO = {(int)ErrorCd.ImportedControlNumber}")).ToString();
                        Duplication_Error.Text = (MyStaticClass.GetRecordCount(connection, transaction, MyStaticClass.WK_MAIN_INSERT_ERROR, "ERR_NO", $"ERR_NO = {(int)ErrorCd.DuplicateControlNumber}")).ToString();
                        ReceptionDate_Error.Text = (MyStaticClass.GetRecordCount(connection, transaction, MyStaticClass.WK_MAIN_INSERT_ERROR, "ERR_NO", $"ERR_NO = {(int)ErrorCd.IncorrectReceptionDate}")).ToString();
                        DB_Size_Error.Text = (MyStaticClass.GetRecordCount(connection, transaction, MyStaticClass.WK_MAIN_INSERT_ERROR, "ERR_NO", $"ERR_NO =  {(int)ErrorCd.DBSizeError}")).ToString();

                        // 取り込み不可エラーがある場合
                        if (MyStaticClass.GetRecordCount(connection, transaction, MyStaticClass.WK_MAIN_INSERT_ERROR, "ERR_NO") > 0)
                        {
                            // エラーログファイル作成
                            MakeErrLogFile(dataSet, connection, transaction);

                            MessageBox.Show("エラーログファイルを作成しました。", "確認");
                        }

                        // コミット
                        transaction.Commit();
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

                // マウスカーソルを元に戻す
                Cursor = Cursors.Default;
            }
            catch (SqlException sqlex)
            {
                MessageBox.Show(sqlex.Message, MyStaticClass.EXCEPTION_ERROR_TITLE);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, MyStaticClass.EXCEPTION_ERROR_TITLE);
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

                // マウスカーソルを砂時計にする
                Cursor = Cursors.WaitCursor;

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
                        string getWkMainSqlStr = $"SELECT * FROM {MyStaticClass.WK_MAIN}";

                        // データを取得してDataSetに追加
                        _myClass.FillDataTable(dataSet, connection, transaction, getWkMainSqlStr, null, MyStaticClass.WK_MAIN);

                        // レコードごとの処理
                        foreach (DataRow row in dataSet.Tables[MyStaticClass.WK_MAIN].Rows)
                        {
                            // DataSetのテーブルから値をオブジェクト配列に取得
                            object[] itemArray = row.ItemArray;

                            // オブジェクト配列からストリング配列に変換
                            string[] dataArray = Array.ConvertAll(itemArray, item => item.ToString());

                            // D_MAINテーブルのSQL文生成
                            string dMainStrSql = MyStaticClass.MakeInsertSql(MyStaticClass.D_MAIN, 0, int.Parse(dataArray[13]));

                            // 項目ごとのパラメータを辞書で管理
                            Dictionary<string, object> parameters = MyStaticClass.KeyValuePairs(dataArray);

                            // SQL文を実行
                            bool dMainExcuteCheck = MyStaticClass.Execute(connection, transaction, dMainStrSql, parameters);

                            if (!dMainExcuteCheck)
                            {
                                MessageBox.Show("D_MAINテーブルの登録に失敗しました。", "エラー");
                                return;
                            }
                        }
                        #endregion

                        #region WK_IN_MAIN_ERRORのデータをD_ERRORにコピー
                        // WK_IN_MAIN_ERRORテーブルのデータを取得SQL文の生成
                        string getWkMainErrSqlStr = $"SELECT * FROM {MyStaticClass.WK_MAIN_ERROR}";

                        // データを取得してDataSetに追加
                        _myClass.FillDataTable(dataSet, connection, transaction, getWkMainErrSqlStr, null, MyStaticClass.WK_MAIN_ERROR);

                        // レコードごとの処理
                        foreach (DataRow row in dataSet.Tables[MyStaticClass.WK_MAIN_ERROR].Rows)
                        {
                            // D_ERRORテーブルのSQL文生成
                            string dErrorStrSql = MyStaticClass.MakeInsertSql(MyStaticClass.D_ERROR, 0, int.Parse(row["ERR_CD"].ToString()));

                            // 1レコードのパラメータを辞書で管理
                            Dictionary<string, object> kanriNoParameter = new Dictionary<string, object>
                            {
                                { "@KanriNo", row["KANRI_NO"].ToString() }
                            };

                            // SQL文を実行（WK_IN_MAIN_ERRORのデータをD_ERRORに）
                            bool dErrorExcuteCheck = MyStaticClass.Execute(connection, transaction, dErrorStrSql, kanriNoParameter);

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

                // マウスカーソルを元に戻す
                Cursor = Cursors.Default;

                MessageBox.Show("取り込みが完了しました。", "確認");
            }
            catch (SqlException sqlex)
            {
                MessageBox.Show(sqlex.Message, MyStaticClass.EXCEPTION_ERROR_TITLE);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, MyStaticClass.EXCEPTION_ERROR_TITLE);
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
                MessageBox.Show(ex.Message, MyStaticClass.EXCEPTION_ERROR_TITLE);
            }
        }
        #endregion

        #region レイアウト（項目数）チェック
        /// <summary>
        /// 項目数が13項目すべて存在するかチェック
        /// </summary>
        /// <param name="checkData">チェックするデータ</param>
        /// <returns>true:問題なし / false:レイアウトエラー</returns>
        public bool ItemsCountCheck(string[] checkData)
        {
            if (checkData.Length == 13)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region 事務局管理番号チェック
        /// <summary>
        /// 事務局管理番号が正しい値になっているか
        /// </summary>
        /// <param name="checkData">チェックデータ</param>
        /// <returns>true:問題なし / false:事務局管理番号不備</returns>
        public bool KanriNoCheck(string checkData)
        {
            // 数値か確認（0： 問題なし）
            if (StCls_Check.CHF_Decimal(checkData) != 0)
            {
                return false;
            }

            // 5文字か確認
            if (Encoding.Default.GetBytes(checkData).Length != 5)
            {
                return false;
            }

            // 1から始まっているか確認
            if (checkData[0] != '1')
            {
                return false;
            }

            return true;
        }
        #endregion

        #region 既に取込済み事務局管理番号チェック
        /// <summary>
        /// DBに既に取込済みの事務局管理番号かどうか
        /// </summary>
        /// <param name="mainTable">D_MAINに登録されている事務局管理番号のDataTable</param>
        /// <param name="kanriNo">チェックする事務局管理番号</param>
        /// <returns>true:問題なし / false:既に登録済み</returns>
        private bool ImportedKanriNoCheck(DataTable mainTable, string kanriNo)
        {
            // 重複チェック
            foreach (DataRow drow in mainTable.Rows)
            {
                if (drow["KANRI_NO"].ToString() == kanriNo)
                {
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region 事務局管理番号ファイル内重複チェック
        /// <summary>
        /// ファイル内で事務局管理番号が重複したデータ数を返す
        /// </summary>
        /// <param name="dataSet">DataSet</param>
        /// <param name="connection">SqlConnection</param>
        /// <param name="transaction">SqlTransaction</param>
        /// <returns>重複データの数</returns>
        private int DuplicateKanriNoCheck(DataSet dataSet, SqlConnection connection, SqlTransaction transaction)
        {
            // WK_IN_MAINテーブルの重複するレコードを取得SQL文の生成
            string getDupliKanriNoSqlStr = $"SELECT MAIN.KANRI_NO, MAIN.OFFSET, MAIN.LINE_DATA, COUNT(MAIN.KANRI_NO) AS COUNT FROM {MyStaticClass.WK_MAIN} AS MAIN" +
                                            $" INNER JOIN (SELECT KANRI_NO FROM {MyStaticClass.WK_MAIN}" +
                                            " GROUP BY KANRI_NO HAVING COUNT(KANRI_NO) > 1) AS DUPLI" +
                                            " ON MAIN.KANRI_NO = DUPLI.KANRI_NO" +
                                            " GROUP BY MAIN.KANRI_NO, MAIN.OFFSET, MAIN.LINE_DATA";

            // データを取得してDataSetに追加
            _myClass.FillDataTable(dataSet, connection, transaction, getDupliKanriNoSqlStr, null, MyStaticClass.WK_MAIN + "_DUPLI");

            // 重複データの数を返す
            return dataSet.Tables[MyStaticClass.WK_MAIN + "_DUPLI"].Rows.Count;
        }
        #endregion

        #region 受付日チェック
        /// <summary>
        /// 受付日が正しい日付になっているか
        /// </summary>
        /// <param name="checkData">チェックデータ</param>
        /// <returns>true:問題なし / false:受付日が不備</returns>
        public bool UkeDateCheck(string checkData)
        {
            // 半角に変換
            string ukeDate = StCls_Function.VbStrConv(checkData, (VbStrConv)8);

            // 日付か確認（0：問題なし）
            if (StCls_Check.CHF_Date(ukeDate) != 0)
            {
                return false;
            }

            return true;
        }
        #endregion

        #region DBサイズチェック
        /// <summary>
        /// 各項目のサイズがDBのサイズ以下かどうか
        /// </summary>
        /// <param name="sizeList">DBの各項目のサイズリスト</param>
        /// <param name="dataArray">チェックする項目配列</param>
        /// <returns>true:問題なし / false:サイズオーバー</returns>
        private bool DBSizeCheck(List<int> sizeList, string[] dataArray)
        {
            // 郵便番号からアンケート（職業）までのサイズチェック
            for (int i = 2; i <= 12; i++)
            {
                if (sizeList[i] < MyStaticClass.SJIS.GetByteCount(dataArray[i]))
                {
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region 値の強制変換
        /// <summary>
        /// 配列の値をテーブル登録用に変換する
        /// </summary>
        /// <param name="dataArray">変換前の配列</param>
        /// <returns>変換後の配列</returns>
        public string[] ForcedConversion(string[] dataArray)
        {
            dataArray[1] = StCls_Function.VbStrConv(dataArray[(int)MainTableColumn.UkeDate], (VbStrConv)8);                // 半角変換
            dataArray[2] = StCls_Function.VbStrConv(dataArray[(int)MainTableColumn.ZipCd], (VbStrConv)8).Replace("-", ""); // 半角変換+ハイフン除去
            dataArray[3] = StCls_Function.VbStrConv(dataArray[(int)MainTableColumn.Add1], (VbStrConv)4);                   // 全角変換
            dataArray[4] = StCls_Function.VbStrConv(dataArray[(int)MainTableColumn.Add2], (VbStrConv)4);                   // 全角変換
            dataArray[5] = StCls_Function.VbStrConv(dataArray[(int)MainTableColumn.Add3], (VbStrConv)4);                   // 全角変換
            dataArray[6] = StCls_Function.VbStrConv(dataArray[(int)MainTableColumn.Add4], (VbStrConv)4);                   // 全角変換
            dataArray[7] = StCls_Function.VbStrConv(dataArray[(int)MainTableColumn.NameSei], (VbStrConv)4);                // 全角変換
            dataArray[8] = StCls_Function.VbStrConv(dataArray[(int)MainTableColumn.NameMei], (VbStrConv)4);                // 全角変換
            dataArray[9] = StCls_Function.VbStrConv(dataArray[(int)MainTableColumn.TelNo], (VbStrConv)8).Replace("-", ""); // 半角変換+ハイフン除去
            dataArray[10] = (dataArray[(int)MainTableColumn.Ank1] == "1" || dataArray[(int)MainTableColumn.Ank1] == "2")
                            ? dataArray[(int)MainTableColumn.Ank1] : "9";                                                  // 値が1または2以外であれば9にする
            dataArray[11] = (dataArray[(int)MainTableColumn.Ank2] == "1" || dataArray[(int)MainTableColumn.Ank2] == "2"
                            || dataArray[(int)MainTableColumn.Ank2] == "3" || dataArray[(int)MainTableColumn.Ank2] == "4")
                            ? dataArray[(int)MainTableColumn.Ank2] : "9";                                                  // 値が1、2、3、4以外であれば9にする
            dataArray[12] = (dataArray[(int)MainTableColumn.Ank3] == "1" || dataArray[(int)MainTableColumn.Ank3] == "2"
                            || dataArray[(int)MainTableColumn.Ank3] == "3" || dataArray[(int)MainTableColumn.Ank3] == "4"
                            || dataArray[(int)MainTableColumn.Ank3] == "5")
                            ? dataArray[(int)MainTableColumn.Ank3] : "9";                                                  // 値が1、2、3、4、5以外であれば9にする

            return dataArray;
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
                string wkInMainDeleteSql = MyStaticClass.MakeDeleteSql(MyStaticClass.WK_MAIN);

                // WK_IN_MAINデリートSQL文を実行
                bool wkInMainDeleteExcuteCheck = MyStaticClass.Execute(connection, transaction, wkInMainDeleteSql, null);

                // WK_IN_MAIN_ERRORデリートSQL文の生成
                string wkInMainErrorDeleteSql = MyStaticClass.MakeDeleteSql(MyStaticClass.WK_MAIN_ERROR);

                // WK_IN_MAIN_ERRORデリートSQL文を実行
                bool wkInMainErrorDeleteExcuteCheck = MyStaticClass.Execute(connection, transaction, wkInMainErrorDeleteSql, null);

                // WK_IN_MAIN_INSERT_ERRデリートSQL文の生成
                string wkInMainInsertErrDeleteSql = MyStaticClass.MakeDeleteSql(MyStaticClass.WK_MAIN_INSERT_ERROR);

                // WK_IN_MAIN_INSERT_ERRデリートSQL文を実行
                bool wkInMainInsertErrDeleteExcuteCheck = MyStaticClass.Execute(connection, transaction, wkInMainInsertErrDeleteSql, null);

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
            while (File.Exists(Path.Combine(outputFolderPath, $"取込エラー_{nowDateTime}{MyStaticClass.NumStr(i)}.txt")))
            {
                i += 1;
            }

            // WK_IN_MAIN_INSERT_ERRORテーブルの全レコードを取得SQL文の生成
            string getMainInsErrTableStrSql = $"SELECT * FROM {MyStaticClass.WK_MAIN_INSERT_ERROR}";

            _myClass.FillDataTable(dataSet, connection, transaction, getMainInsErrTableStrSql, null, MyStaticClass.WK_MAIN_INSERT_ERROR);

            // 各エラーレコード用リスト
            List<string> LayoutErrorList = new List<string>();
            List<string> IncorrectControlNumberErrorList = new List<string>();
            List<string> ImportedControlNumberErrorList = new List<string>();
            List<string> DuplicateControlNumberErrorList = new List<string>();
            List<string> IncorrectReceptionDateErrorList = new List<string>();
            List<string> DBSizeErrorList = new List<string>();

            // エラーごとにリストにセット
            foreach (DataRow record in dataSet.Tables[MyStaticClass.WK_MAIN_INSERT_ERROR].Rows)
            {
                // リストにセットするエラーレコード
                string errRecord = record["LINE_DATA"].ToString();

                switch ((int)record["ERR_NO"])
                {
                    case (int)ErrorCd.LayoutError:
                        LayoutErrorList.Add(errRecord);
                        break;
                    case (int)ErrorCd.IncorrectControlNumber:
                        IncorrectControlNumberErrorList.Add(errRecord);
                        break;
                    case (int)ErrorCd.ImportedControlNumber:
                        ImportedControlNumberErrorList.Add(errRecord);
                        break;
                    case (int)ErrorCd.DuplicateControlNumber:
                        DuplicateControlNumberErrorList.Add(errRecord);
                        break;
                    case (int)ErrorCd.IncorrectReceptionDate:
                        IncorrectReceptionDateErrorList.Add(errRecord);
                        break;
                    case (int)ErrorCd.DBSizeError:
                        DBSizeErrorList.Add(errRecord);
                        break;
                }
            }

            // テキストファイル作成（false:上書き）
            using (StreamWriter writeText = new StreamWriter(Path.Combine(outputFolderPath, $"取込エラー_{nowDateTime}{MyStaticClass.NumStr(i)}.txt"), false, Encoding.UTF8))
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
                MessageBox.Show(sqlex.Message, MyStaticClass.EXCEPTION_ERROR_TITLE);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, MyStaticClass.EXCEPTION_ERROR_TITLE);
            }
        }
        #endregion
    }
}
