using hagaki.Class;
using hagaki.StaticClass;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;

namespace hagaki
{
    public partial class Frm0200_OUT_NG_DATA : Form
    {
        #region メンバ変数
        private string connectionString = string.Empty;          // 接続文字列
        private MyClass _myClass;                                // MClassを使えるように
        private string outNgFolderPath = string.Empty;           // NG票出力先フォルダパス
        private DataTable noOutNgTable;                          // NG票出力対象データテーブル
        private List<string> ngKanriNoList = new List<string>(); // 事務局管理番号のみのリスト
        #endregion

        #region 定数
        private const string OUT_NG_PATH_NODE = "DIR/OUT_NG_FLDPATH"; // NG票出力先ノード
        private const string HINA_EXCEL_FILE_PATH = "NG票_雛型.xlsx"; // 雛型になるエクセルファイルのパス
        #endregion

        #region コンストラクタ
        public Frm0200_OUT_NG_DATA()
        {
            InitializeComponent();
        }

        public Frm0200_OUT_NG_DATA(string connestStr)
        {
            InitializeComponent();

            connectionString = connestStr;
        }
        #endregion

        #region ロードイベント
        private void Frm0200_OUT_NG_DATA_Load(object sender, EventArgs e)
        {
            try
            {
                // XMLファイルを読込
                XElement xEle = XElement.Load(MyStaticClass.OPERATION_XML);

                // パスを分解
                string[] pathParts = OUT_NG_PATH_NODE.Split('/');

                // DIR要素を取得
                XElement dirElement = xEle.Element(pathParts[0]);

                // 値を取得
                outNgFolderPath = dirElement?.Element(pathParts[1])?.Value ?? "";

                // エラーファイル出力先パス表示
                OutNgPathLabel.Text = outNgFolderPath;

                // MyClassクラスをインスタンス化
                _myClass = new MyClass();

                // 件数初期化
                InitNgCount();
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

        #region NG票出力先のフォルダを開く
        private void OutputDirButton_Click(object sender, EventArgs e)
        {
            try
            {
                // 出力先フォルダの存在チェック
                if (!Directory.Exists(outNgFolderPath))
                {
                    // フォルダがない場合作成する
                    Directory.CreateDirectory(outNgFolderPath);
                }

                // フォルダを開く
                bool openFileCheck = StCls_File.WindowOpen(outNgFolderPath);

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

        #region 件数確認
        private void CheckNumCaseButton_Click(object sender, EventArgs e)
        {
            try
            {
                // マウスカーソルを砂時計にする
                Cursor = Cursors.WaitCursor;

                // 件数初期化
                InitNgCount();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    // 接続を開く
                    connection.Open();

                    using (SqlCommand command = connection.CreateCommand())
                    {
                        // DataSetを作成
                        DataSet dataSet = new DataSet();

                        // D_MAINテーブルのNG票出力対象データを取得SQL文の生成
                        StringBuilder ngSubjectDMainSqlStr = new StringBuilder();
                        ngSubjectDMainSqlStr.AppendLine("SELECT");
                        ngSubjectDMainSqlStr.AppendLine($" {MyStaticClass.D_MAIN}.KANRI_NO,");
                        ngSubjectDMainSqlStr.AppendLine($" {MyStaticClass.D_MAIN}.UKE_DATE,");
                        ngSubjectDMainSqlStr.AppendLine($" {MyStaticClass.D_MAIN}.ZIP_CD,");
                        ngSubjectDMainSqlStr.AppendLine($" {MyStaticClass.D_MAIN}.ADD_1,");
                        ngSubjectDMainSqlStr.AppendLine($" {MyStaticClass.D_MAIN}.ADD_2,");
                        ngSubjectDMainSqlStr.AppendLine($" {MyStaticClass.D_MAIN}.ADD_3,");
                        ngSubjectDMainSqlStr.AppendLine($" {MyStaticClass.D_MAIN}.ADD_4,");
                        ngSubjectDMainSqlStr.AppendLine($" {MyStaticClass.D_MAIN}.NAME_SEI,");
                        ngSubjectDMainSqlStr.AppendLine($" {MyStaticClass.D_MAIN}.NAME_MEI,");
                        ngSubjectDMainSqlStr.AppendLine($" {MyStaticClass.D_MAIN}.TEL_NO,");
                        ngSubjectDMainSqlStr.AppendLine($" {MyStaticClass.D_MAIN}.ANK_1,");
                        ngSubjectDMainSqlStr.AppendLine($" {MyStaticClass.D_MAIN}.ANK_2,");
                        ngSubjectDMainSqlStr.AppendLine($" {MyStaticClass.D_MAIN}.ANK_3,");
                        ngSubjectDMainSqlStr.AppendLine($" STRING_AGG({MyStaticClass.M_ERROR}.ERR_MONGON, CHAR(13) + CHAR(10)) AS error");
                        ngSubjectDMainSqlStr.AppendLine($" FROM {MyStaticClass.D_MAIN}");
                        ngSubjectDMainSqlStr.AppendLine($" INNER JOIN {MyStaticClass.D_ERROR} ON {MyStaticClass.D_MAIN}.KANRI_NO = {MyStaticClass.D_ERROR}.KANRI_NO");
                        ngSubjectDMainSqlStr.AppendLine($" INNER JOIN {MyStaticClass.M_ERROR} ON {MyStaticClass.D_ERROR}.ERR_CD = {MyStaticClass.M_ERROR}.ERR_CD");
                        ngSubjectDMainSqlStr.AppendLine($" WHERE {MyStaticClass.D_MAIN}.JYOTAI_KB = '{(int)JyotaiKb.Ng}' AND {MyStaticClass.D_MAIN}.NG_OUT_KB = '{(int)NgOutKb.Un}'");
                        ngSubjectDMainSqlStr.AppendLine($" GROUP BY");
                        ngSubjectDMainSqlStr.AppendLine($" {MyStaticClass.D_MAIN}.KANRI_NO,");
                        ngSubjectDMainSqlStr.AppendLine($" {MyStaticClass.D_MAIN}.UKE_DATE,");
                        ngSubjectDMainSqlStr.AppendLine($" {MyStaticClass.D_MAIN}.ZIP_CD,");
                        ngSubjectDMainSqlStr.AppendLine($" {MyStaticClass.D_MAIN}.ADD_1,");
                        ngSubjectDMainSqlStr.AppendLine($" {MyStaticClass.D_MAIN}.ADD_2,");
                        ngSubjectDMainSqlStr.AppendLine($" {MyStaticClass.D_MAIN}.ADD_3,");
                        ngSubjectDMainSqlStr.AppendLine($" {MyStaticClass.D_MAIN}.ADD_4,");
                        ngSubjectDMainSqlStr.AppendLine($" {MyStaticClass.D_MAIN}.NAME_SEI,");
                        ngSubjectDMainSqlStr.AppendLine($" {MyStaticClass.D_MAIN}.NAME_MEI,");
                        ngSubjectDMainSqlStr.AppendLine($" {MyStaticClass.D_MAIN}.TEL_NO,");
                        ngSubjectDMainSqlStr.AppendLine($" {MyStaticClass.D_MAIN}.ANK_1,");
                        ngSubjectDMainSqlStr.AppendLine($" {MyStaticClass.D_MAIN}.ANK_2,");
                        ngSubjectDMainSqlStr.AppendLine($" {MyStaticClass.D_MAIN}.ANK_3");
                        ngSubjectDMainSqlStr.AppendLine($" ORDER BY {MyStaticClass.D_MAIN}.KANRI_NO ASC");

                        command.CommandText = ngSubjectDMainSqlStr.ToString();

                        // データを取得してDataSetに追加
                        _myClass.FillDataTable(dataSet, command, null, MyStaticClass.D_MAIN + "_NG");

                        // D_MAIN_NGテーブル取得
                        noOutNgTable = dataSet.Tables[MyStaticClass.D_MAIN + "_NG"];
                    }
                }

                // 事務局管理番号だけのリスト作成
                foreach (DataRow record in noOutNgTable.Rows)
                {
                    ngKanriNoList.Add(record["KANRI_NO"].ToString());
                }

                // 件数表示
                NgCountLabel.Text = ngKanriNoList.Count.ToString();

                // 件数が0件以上であれば
                if (int.Parse(NgCountLabel.Text) > 0)
                {
                    // 出力ボタン活性化
                    OutputButton.Enabled = true;
                    OutputButton.BackColor = SystemColors.GradientActiveCaption;
                    OutputButton.Cursor = Cursors.Hand;
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

        #region 出力イベント
        private void OutputButton_Click(object sender, EventArgs e)
        {
            try
            {
                // 確認ダイアログ表示
                DialogResult outExcelResult = MessageBox.Show("出力してもよろしいでしょうか？", "確認", MessageBoxButtons.YesNo);

                // いいえを選択したら出力キャンセル
                if (outExcelResult == DialogResult.No)
                {
                    return;
                }

                // 雛型エクセルファイル存在確認
                if (!File.Exists(HINA_EXCEL_FILE_PATH))
                {
                    MessageBox.Show("雛型エクセルファイルが見つかりませんでした。", "確認");
                    return;
                }

                // マウスカーソルを砂時計にする
                Cursor = Cursors.WaitCursor;

                // 出力先フォルダがない場合フォルダを作成する
                if (!Directory.Exists(outNgFolderPath))
                {
                    Directory.CreateDirectory(outNgFolderPath);
                }

                // エクセル出力処理
                OutputExcel();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    // 接続を開く
                    connection.Open();

                    // トランザクション開始
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        using (SqlCommand command = connection.CreateCommand())
                        {
                            command.Transaction = transaction;

                            foreach (string kanriNo in ngKanriNoList)
                            {
                                #region D_MAINを更新
                                // D_MAINアップデートSQL文の作成
                                command.CommandText = MyStaticClass.MakeUpdateSql(MyStaticClass.D_MAIN, "OUT_NG");

                                // パラメータ
                                Dictionary<string, object> kanriNoParameter = new Dictionary<string, object>
                                {
                                    { "@KanriNo", kanriNo }
                                };

                                // D_MAINアップデートSQL文を実行
                                bool dMainOutExcuteCheck = MyStaticClass.Execute(command, kanriNoParameter);

                                if (!dMainOutExcuteCheck)
                                {
                                    MessageBox.Show("D_MAINテーブルの更新に失敗しました。", "エラー");
                                    return;
                                }
                                #endregion
                            }
                        }

                        // コミット
                        transaction.Commit();
                    }
                }

                // 件数初期化
                InitNgCount();

                // マウスカーソルを元に戻す
                Cursor = Cursors.Default;

                MessageBox.Show("NG票を出力しました。", "確認");
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

        #region 戻る
        private void BackButton_Click(object sender, EventArgs e)
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

        #region エクセル出力処理
        /// <summary>
        /// エクセルファイルを作成し、データを書き込む
        /// </summary>
        private void OutputExcel()
        {
            // エクセルを使えるように
            Excel.Application excelApp = new Excel.Application();

            try
            {
                // 現在の日時を取得
                DateTime nowDateTime = DateTime.Now;

                // 現在の日時を変換（ファイル名用）
                string nowDateTime_file = nowDateTime.ToString("yyyyMMddHHmmss");

                // 現在の日時を変換（エクセルデータ用）
                string nowDateTime_data = nowDateTime.ToString("yyyy/MM/dd HH:mm:ss");

                int i = 0;

                // コピー先エクセルファイルの存在チェック（存在していればファイル名に追加する文字列作成）
                while (File.Exists(outNgFolderPath + "NG票_" + nowDateTime_file + MyStaticClass.NumStr(i) + ".xlsx"))
                {
                    i++;
                }

                // コピー先ファイルパス
                string copyFilePath = outNgFolderPath + "NG票_" + nowDateTime_file + MyStaticClass.NumStr(i) + ".xlsx";

                // 雛型エクセルファイルをコピーする
                File.Copy(HINA_EXCEL_FILE_PATH, copyFilePath);

                // エクセルを画面に表示しない
                excelApp.Visible = false;

                // 警告メッセージを表示しない
                excelApp.Application.DisplayAlerts = false;

                // エクセルファイルを開く
                Excel.Workbook excelBook = excelApp.Workbooks.Open(copyFilePath);

                // 雛型シート取得
                Excel.Worksheet originalSheet = excelBook.Sheets["雛型"];

                // エクセルにデータをセット
                foreach (DataRow rows in noOutNgTable.Rows)
                {
                    // 雛型シートを後ろにコピー
                    originalSheet.Copy(After: excelBook.Sheets[excelBook.Sheets.Count]);

                    // コピーされたシート取得
                    Excel.Worksheet sheet = (Excel.Worksheet)excelBook.Sheets[excelBook.Sheets.Count];

                    // シート名を対象の事務局管理番号にする
                    sheet.Name = rows["KANRI_NO"].ToString();

                    // エクセルに出力日をセット
                    sheet.Cells[4, 25] = nowDateTime_data;

                    // エクセルファイル用メインデータ2次元配列作成
                    object[,] setData = new object[13, 1];

                    // メインデータをセット
                    setData[0, 0] = rows["KANRI_NO"];
                    setData[1, 0] = rows["UKE_DATE"].ToString().Insert(6, "/").Insert(4, "/");
                    setData[2, 0] = rows["ZIP_CD"];
                    setData[3, 0] = rows["ADD_1"];
                    setData[4, 0] = rows["ADD_2"];
                    setData[5, 0] = rows["ADD_3"];
                    setData[6, 0] = rows["ADD_4"];
                    setData[7, 0] = rows["NAME_SEI"];
                    setData[8, 0] = rows["NAME_MEI"];
                    setData[9, 0] = rows["TEL_NO"];
                    setData[10, 0] = rows["ANK_1"];
                    setData[11, 0] = rows["ANK_2"];
                    setData[12, 0] = rows["ANK_3"];

                    // メインデータの貼り付け範囲指定
                    Excel.Range range = sheet.Range[sheet.Cells[8, 9], sheet.Cells[20, 9]];

                    // メインデータをエクセルファイルに貼り付け
                    range.Value = setData;

                    // エクセルにNG内容をセット
                    sheet.Cells[7, 16] = rows["error"]; ;
                }

                // 雛型シートを削除
                originalSheet.Delete();

                // ファイルを保存して閉じる
                excelBook.Save();
                excelBook.Close();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                // エクセルアプリケーションを終了
                excelApp.Quit();

                // COMオブジェクト開放
                Marshal.ReleaseComObject(excelApp);
                excelApp = null;
            }
        }
        #endregion

        #region 件数初期化
        /// <summary>
        /// 表示件数を初期化して、出力ボタンを非活性化
        /// </summary>
        private void InitNgCount()
        {
            NgCountLabel.Text = string.Empty;
            ngKanriNoList.Clear();

            OutputButton.Enabled = false;
            OutputButton.BackColor = SystemColors.ControlDark;
            OutputButton.Cursor = Cursors.Default;
        }
        #endregion
    }
}
