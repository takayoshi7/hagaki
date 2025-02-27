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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
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
        private My_Function _func;                               // My_Functionを使えるように
        private string outNgFolderPath = string.Empty;           // NG票出力先フォルダパス
        private DataTable noOutNgTable;                          // NG票出力対象データテーブル
        private List<string> ngKanriNoList = new List<string>(); // 事務局管理番号のみのリスト
        #endregion

        #region 定数
        private const string D_MAIN = "D_MAIN";                       // メインテーブル
        private const string D_ERROR = "D_ERROR";                     // エラーテーブル
        private const string M_ERROR = "M_ERROR";                     // エラーコードテーブル
        private const string OPERATION_XML = "KensyuSys.xml";         // XMLファイル名
        private const string OUT_NG_PATH_NODE = "DIR/OUT_NG_FLDPATH"; // ノード
        private const string HINA_EXCEL_FILE_PATH = "NG票_雛型.xlsx";  // 雛型になるエクセルファイルのパス
        private const string EXCEPTION_ERROR_TITLE = "例外エラー";    // 例外時表示メッセージタイトル
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
                XElement xEle = XElement.Load(OPERATION_XML);

                // パスを分解
                string[] pathParts = OUT_NG_PATH_NODE.Split('/');

                // DIR要素を取得
                XElement dirElement = xEle.Element(pathParts[0]);

                // 値を取得
                outNgFolderPath = dirElement?.Element(pathParts[1])?.Value ?? "";

                // エラーファイル出力先パス表示
                OutNgPathLabel.Text = outNgFolderPath;

                // My_Functionクラスをインスタンス化
                _func = new My_Function();

                // 件数初期化
                InitNgCount();
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

        #region エラーファイル出力先のフォルダを開く
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
                MessageBox.Show(ex.Message, EXCEPTION_ERROR_TITLE);
            }
        }
        #endregion

        #region 件数確認
        private void CheckNumCaseButton_Click(object sender, EventArgs e)
        {
            // トランザクション
            SqlTransaction transaction = null;

            try
            {
                // 件数初期化
                InitNgCount();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    // 接続を開く
                    connection.Open();

                    using (SqlCommand command = connection.CreateCommand())
                    {
                        // トランザクション開始
                        transaction = connection.BeginTransaction();

                        // DataSetを作成
                        DataSet dataSet = new DataSet();

                        // D_MAINテーブルのNG票出力対象データを取得SQL文の生成
                        StringBuilder ngSubjectDMainSqlStr = new StringBuilder();
                        ngSubjectDMainSqlStr.AppendLine("SELECT");
                        ngSubjectDMainSqlStr.AppendLine(" DM.KANRI_NO,");
                        ngSubjectDMainSqlStr.AppendLine(" UKE_DATE,");
                        ngSubjectDMainSqlStr.AppendLine(" ZIP_CD,");
                        ngSubjectDMainSqlStr.AppendLine(" ADD_1,");
                        ngSubjectDMainSqlStr.AppendLine(" ADD_2,");
                        ngSubjectDMainSqlStr.AppendLine(" ADD_3,");
                        ngSubjectDMainSqlStr.AppendLine(" ADD_4,");
                        ngSubjectDMainSqlStr.AppendLine(" NAME_SEI,");
                        ngSubjectDMainSqlStr.AppendLine(" NAME_MEI,");
                        ngSubjectDMainSqlStr.AppendLine(" TEL_NO,");
                        ngSubjectDMainSqlStr.AppendLine(" ANK_1,");
                        ngSubjectDMainSqlStr.AppendLine(" ANK_2,");
                        ngSubjectDMainSqlStr.AppendLine(" ANK_3,");
                        ngSubjectDMainSqlStr.AppendLine(" DE.ERR_CD,");
                        ngSubjectDMainSqlStr.AppendLine(" ERR_MONGON");
                        ngSubjectDMainSqlStr.AppendLine($" FROM {D_MAIN} AS DM");
                        ngSubjectDMainSqlStr.AppendLine($" INNER JOIN {D_ERROR} AS DE ON DM.KANRI_NO = DE.KANRI_NO");
                        ngSubjectDMainSqlStr.AppendLine($" INNER JOIN {M_ERROR} AS ME ON DE.ERR_CD = ME.ERR_CD");
                        ngSubjectDMainSqlStr.AppendLine(" WHERE JYOTAI_KB = 1 AND NG_OUT_KB = 0");
                        ngSubjectDMainSqlStr.AppendLine(" ORDER BY DE.ERR_CD ASC, DM.KANRI_NO ASC");

                        // データを取得してDataSetに追加
                        _func.FillDataTable(dataSet, connection, transaction, ngSubjectDMainSqlStr.ToString(), null, D_MAIN + "_NG");

                        // D_MAIN_NGテーブル取得
                        noOutNgTable = dataSet.Tables[D_MAIN + "_NG"];

                        // 事務局管理番号だけのリスト作成
                        foreach (DataRow record in noOutNgTable.Rows)
                        {
                            ngKanriNoList.Add(record["KANRI_NO"].ToString());
                        }

                        // 重複削除して昇順に並び替え
                        ngKanriNoList = ngKanriNoList.Distinct().OrderBy(x => x).ToList();

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

        #region エクセル出力
        private void OutputButton_Click(object sender, EventArgs e)
        {
            // エクセルを使えるように
            Excel.Application excelApp = new Excel.Application();

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
                }

                // 現在の日時を取得
                DateTime nowDateTime = DateTime.Now;

                // 現在の日時を変換（ファイル名用）
                string nowDateTime_file = nowDateTime.ToString("yyyyMMddHHmmss");

                // 現在の日時を変換（エクセル表示＆DB用）
                string nowDateTime_DB = nowDateTime.ToString("yyyy/MM/dd HH:mm:ss");

                // 出力先フォルダがない場合フォルダを作成する
                if (!Directory.Exists(outNgFolderPath))
                {
                    Directory.CreateDirectory(outNgFolderPath);
                }

                int i = 0;

                // コピー先エクセルファイルの存在チェック（存在していればファイル名に追加する文字列作成）
                while (File.Exists(outNgFolderPath + "NG票_" + nowDateTime_file + _func.NumStr(i) + ".xlsx"))
                {
                    i++;
                }

                // コピー先ファイルパス
                string copyFilePath = outNgFolderPath + "NG票_" + nowDateTime_file + _func.NumStr(i) + ".xlsx";

                // 雛型エクセルファイルをコピーする
                File.Copy(HINA_EXCEL_FILE_PATH, copyFilePath);

                // 警告メッセージを非表示にする
                excelApp.Application.DisplayAlerts = false;

                // エクセルファイルを開く
                Excel.Workbook excelBook = excelApp.Workbooks.Open(copyFilePath);

                // 雛型シート取得
                Excel.Worksheet originalSheet = excelBook.Sheets["雛型"];

                // エクセルにデータをセット
                for (int j = 0; j < ngKanriNoList.Count; j++)
                {
                    // 取得したレコードの事務局管理番号
                    string getRecordKnum = ngKanriNoList[j];

                    // 雛型シートを後ろにコピー
                    originalSheet.Copy(After: excelBook.Sheets[excelBook.Sheets.Count]);

                    // コピーされたシート取得
                    Excel.Worksheet sheet = (Excel.Worksheet)excelBook.Sheets[excelBook.Sheets.Count];

                    // シート名を対象の事務局管理番号にする
                    sheet.Name = getRecordKnum;

                    // エクセルに出力日をセット
                    sheet.Cells[4, 25] = nowDateTime_DB;

                    // エクセルファイル用メインデータ2次元配列作成
                    object[,] setData = new object[13, 1];
                    setData[0, 0] = getRecordKnum;
                    setData[1, 0] = noOutNgTable.Rows[j]["UKE_DATE"].ToString().Insert(6, "/").Insert(4, "/");
                    setData[2, 0] = noOutNgTable.Rows[j]["ZIP_CD"];
                    setData[3, 0] = noOutNgTable.Rows[j]["ADD_1"];
                    setData[4, 0] = noOutNgTable.Rows[j]["ADD_2"];
                    setData[5, 0] = noOutNgTable.Rows[j]["ADD_3"];
                    setData[6, 0] = noOutNgTable.Rows[j]["ADD_4"];
                    setData[7, 0] = noOutNgTable.Rows[j]["NAME_SEI"];
                    setData[8, 0] = noOutNgTable.Rows[j]["NAME_MEI"];
                    setData[9, 0] = noOutNgTable.Rows[j]["TEL_NO"];
                    setData[10, 0] = noOutNgTable.Rows[j]["ANK_1"];
                    setData[11, 0] = noOutNgTable.Rows[j]["ANK_2"];
                    setData[12, 0] = noOutNgTable.Rows[j]["ANK_3"];

                    // エクセルファイルの貼り付け範囲指定
                    Excel.Range range = sheet.Range[sheet.Cells[8, 9], sheet.Cells[20, 9]];

                    // エクセルファイルに貼り付け
                    range.Value = setData;

                    // エクセルファイル貼り付け用NG内容
                    string setError = "";

                    foreach (DataRow rows in noOutNgTable.Rows)
                    {
                        // 対象の事務局管理番号でなければ次のレコードに
                        if (rows["KANRI_NO"].ToString() != getRecordKnum)
                        {
                            continue;
                        }

                        // NG内容取得
                        setError += "・" + rows["ERR_MONGON"].ToString() + Environment.NewLine;
                    }

                    // エクセルにNG内容をセット
                    sheet.Cells[7, 16] = setError;
                }

                // 雛型シートを削除
                originalSheet.Delete();

                // ファイルを保存して閉じる
                excelBook.Save();
                excelBook.Close();

                // ログインユーザー名取得
                string loginID = StCls_Function.GetUser();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    // 接続を開く
                    connection.Open();

                    // トランザクション開始
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        using (SqlCommand command = connection.CreateCommand())
                        {
                            // D_MAINを更新
                            foreach (string kanriNo in ngKanriNoList)
                            {
                                // D_MAINアップデートSQL文の作成
                                StringBuilder dMainNgOutUpdateSql = new StringBuilder();
                                dMainNgOutUpdateSql.AppendLine($"UPDATE {D_MAIN} SET");
                                dMainNgOutUpdateSql.AppendLine($" NG_OUT_KB = '1',");
                                dMainNgOutUpdateSql.AppendLine($" NG_OUT_DATETIME = '{nowDateTime_DB}',");
                                dMainNgOutUpdateSql.AppendLine($" NG_OUT_LOGINID = '{loginID}',");
                                dMainNgOutUpdateSql.AppendLine($" UPDATE_DATETIME = '{nowDateTime_DB}',");
                                dMainNgOutUpdateSql.AppendLine($" UPDATE_LOGINID = '{loginID}'");
                                dMainNgOutUpdateSql.AppendLine($" WHERE KANRI_NO = @KanriNo");

                                // パラメータ
                                Dictionary<string, object> kanriNoParameter = new Dictionary<string, object>
                                {
                                    { "@KanriNo", kanriNo }
                                };

                                // D_MAINアップデートSQL文を実行
                                bool dMainOutExcuteCheck = _func.Execute(connection, transaction, dMainNgOutUpdateSql.ToString(), kanriNoParameter);

                                if (!dMainOutExcuteCheck)
                                {
                                    MessageBox.Show("D_MAINテーブルを更新できませんでした。", "エラー");
                                }
                            }

                            // コミット
                            transaction.Commit();
                        }
                    }
                }

                // 件数初期化
                InitNgCount();

                MessageBox.Show("NG票を出力しました。", "確認");
            }
            catch (SqlException sqlex)
            {
                MessageBox.Show(sqlex.Message, EXCEPTION_ERROR_TITLE);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, EXCEPTION_ERROR_TITLE);
            }
            finally
            {
                // ファイルクローズ
                excelApp.Quit();

                // COMオブジェクト開放
                Marshal.ReleaseComObject(excelApp);
                excelApp = null;
            }
        }
        #endregion

        #region 戻る
        private void BackButton_Click(object sender, EventArgs e)
        {
            Close();
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
