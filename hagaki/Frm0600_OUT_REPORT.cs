using hagaki.StaticClass;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Excel = Microsoft.Office.Interop.Excel;

namespace hagaki
{
    public partial class Frm0600_OUT_REPORT : Form
    {
        #region メンバ変数
        private string connectionString = string.Empty;    // 接続文字列
        private My_Function _func;                         // My_Functionを使えるように
        private string outReportFolderPath = string.Empty; // 報告書データ出力先フォルダパス
        #endregion

        #region 定数
        private const string D_MAIN = "D_MAIN";                                   // メインテーブル
        private const string OPERATION_XML = "KensyuSys.xml";                     // XMLファイル名
        private const string OUT_REPORT_PATH_NODE = "DIR/OUT_REPORT_FLDPATH";     // ノード
        private const string HINA_REPORT_FILE_PATH = "アンケート報告書_雛型.xlsx"; // 雛型になる報告書ファイルのパス
        private const string EXCEPTION_ERROR_TITLE = "例外エラー";                // 例外時表示メッセージタイトル
        #endregion

        #region コンストラクタ
        public Frm0600_OUT_REPORT()
        {
            InitializeComponent();
        }

        public Frm0600_OUT_REPORT(string connestStr)
        {
            InitializeComponent();

            connectionString = connestStr;
        }
        #endregion

        #region ロードイベント
        private void Frm0600_OUT_REPORT_Load(object sender, EventArgs e)
        {
            try
            {
                // XMLファイルを読込
                XElement xEle = XElement.Load(OPERATION_XML);

                // パスを分解
                string[] pathParts = OUT_REPORT_PATH_NODE.Split('/');

                // DIR要素を取得
                XElement dirElement = xEle.Element(pathParts[0]);

                // 値を取得
                outReportFolderPath = dirElement?.Element(pathParts[1])?.Value ?? "";

                // エラーファイル出力先パス表示
                OutReportPathLabel.Text = outReportFolderPath;

                // My_Functionクラスをインスタンス化
                _func = new My_Function();
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

        #region 報告書データ出力先のフォルダを開く
        private void OutputDirButton_Click(object sender, EventArgs e)
        {
            try
            {
                // 出力先フォルダの存在チェック
                if (!Directory.Exists(outReportFolderPath))
                {
                    // フォルダがない場合作成する
                    Directory.CreateDirectory(outReportFolderPath);
                }

                // フォルダを開く
                bool openFileCheck = StCls_File.WindowOpen(outReportFolderPath);

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

        #region 報告書エクセル出力
        private void OutHoukokuButton_Click(object sender, EventArgs e)
        {
            // エクセルを使えるように
            Excel.Application excelApp = new Excel.Application();

            try
            {
                // 出力する受付日の範囲を取得
                DateTime beforeUkeDate = BeforeUkeDate.Value;
                DateTime afterUkeDate = AfterUkeDate.Value;

                // 日付を文字列に変換
                string beforeReportUkeDate = beforeUkeDate.ToString("yyyyMMdd");
                string afterReportUkeDate = afterUkeDate.ToString("yyyyMMdd");

                // 受付日の後の値が前の値より小さければエラー
                if (beforeUkeDate > afterUkeDate)
                {
                    MessageBox.Show("受付日の範囲が正しくありません。", "エラー");
                    return;
                }

                // 確認ダイアログ表示
                DialogResult result = MessageBox.Show("出力してもよろしいでしょうか？", "確認", MessageBoxButtons.YesNo);

                // いいえを選択したら出力キャンセル
                if (result == DialogResult.No)
                {
                    return;
                }

                // 雛型エクセルファイル存在確認
                if (!File.Exists(HINA_REPORT_FILE_PATH))
                {
                    MessageBox.Show("雛型エクセルファイルが見つかりませんでした。", "エラー");
                    return;
                }

                // 出力先フォルダがない場合フォルダを作成する
                if (!Directory.Exists(outReportFolderPath))
                {
                    Directory.CreateDirectory(outReportFolderPath);
                }

                int i = 0;

                // コピー先エクセルファイルの存在チェック（存在していればファイル名に追加する文字列作成）
                while (File.Exists(outReportFolderPath + "アンケート報告書_" + beforeReportUkeDate + "_" + afterReportUkeDate + _func.NumStr(i) + ".xlsx"))
                {
                    i++;
                }

                // コピー先ファイルパス
                string copyFilePath = outReportFolderPath + "アンケート報告書_" + beforeReportUkeDate + "_" + afterReportUkeDate + _func.NumStr(i) + ".xlsx";

                // 雛型エクセルファイルをコピーする
                File.Copy(HINA_REPORT_FILE_PATH, copyFilePath);

                #region 報告書出力用データ取得
                // 報告書データ出力対象データテーブル
                DataTable reportTable = new DataTable();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    // DBを開く
                    connection.Open();

                    // DataSetを作成
                    DataSet dataSet = new DataSet();

                    // SQL文作成用
                    StringBuilder getReportDMain = new StringBuilder();
                    getReportDMain.AppendLine($"SELECT KANRI_NO, UKE_DATE, ANK_1, ANK_2, ANK_3 FROM {D_MAIN}");
                    getReportDMain.AppendLine(" WHERE UKE_DATE >= @beforeUkeDate AND UKE_DATE <= @afterUkeDate");
                    getReportDMain.AppendLine(" ORDER BY UKE_DATE ASC");

                    // パラメータ作成用
                    Dictionary<string, object> parameters = new Dictionary<string, object>()
                    {
                        { "@beforeUkeDate", beforeReportUkeDate },
                        { "@afterUkeDate", afterReportUkeDate }
                    };

                    // D_MAIN_REPORTテーブル取得
                    _func.FillDataTable(dataSet, connection, null, getReportDMain.ToString(), parameters, D_MAIN + "_REPORT");

                    // 報告書データ出力対象（D_MAIN_REPORTテーブル）取得
                    reportTable = dataSet.Tables[D_MAIN + "_REPORT"];
                }
                #endregion

                // 警告メッセージを非表示にする
                excelApp.Application.DisplayAlerts = false;

                // エクセルファイルを開く
                Excel.Workbook excelBook = excelApp.Workbooks.Open(copyFilePath);

                // 各シートを指定
                Excel.Worksheet sexSheet = (Excel.Worksheet)excelBook.Worksheets["性別"];
                Excel.Worksheet ageSheet = (Excel.Worksheet)excelBook.Worksheets["年齢"];
                Excel.Worksheet jobSheet = (Excel.Worksheet)excelBook.Worksheets["職業"];

                // 範囲内の日付リスト
                List<DateTime> dateList = new List<DateTime>();

                // 最初の日から1日ずつ足して終わりの日までをリストに追加
                while (beforeUkeDate <= afterUkeDate)
                {
                    dateList.Add(beforeUkeDate);
                    beforeUkeDate = beforeUkeDate.AddDays(1);
                }

                // 表示するデータの数を取得
                int countData = dateList.Count;

                // 選択範囲による分岐処理
                if (countData == 1)
                {
                    // 範囲が1日なら不要な行削除
                    sexSheet.Rows["9"].Delete(Excel.XlDirection.xlUp);
                    ageSheet.Rows["9"].Delete(Excel.XlDirection.xlUp);
                    jobSheet.Rows["9"].Delete(Excel.XlDirection.xlUp);
                }
                else if (countData == 2)
                {
                    // 範囲が2日なら行を追加も削除もしない
                }
                else
                {
                    // 範囲が3日以上
                    // 追加する行数
                    int insertRowsCount = countData - 2;

                    // 各シートで8行目をコピーして挿入
                    sexSheet.Rows[8].Copy();
                    sexSheet.Rows[$"9:{8 + insertRowsCount}"].Insert(Excel.XlDirection.xlDown);

                    ageSheet.Rows[8].Copy();
                    ageSheet.Rows[$"9:{8 + insertRowsCount}"].Insert(Excel.XlDirection.xlDown);

                    jobSheet.Rows[8].Copy();
                    jobSheet.Rows[$"9:{8 + insertRowsCount}"].Insert(Excel.XlDirection.xlDown);
                }

                // エクセル貼り付けデータ用2次元配列
                object[,] arr_changeSexData = new object[countData + 1, 5];
                object[,] arr_changeAgeData = new object[countData + 1, 7];
                object[,] arr_changeJobData = new object[countData + 1, 8];

                // 性別シート合計取得用
                int totalMan = 0, totalWoman = 0, totalGenderUnknown = 0, totalSexSubtotal = 0;
                // 年齢シート合計取得用
                int totalUnder20 = 0, total20to40 = 0, total40to60 = 0, totalOver60 = 0, totalAgeUnknown = 0, totalAgeSubtotal = 0;
                // 職業シート合計取得用
                int totalSelfEmp = 0, totalCompanyEmp = 0, totalPart = 0, totalStudent = 0, totalOthers = 0, totalJobUnknown = 0, totalJobSubtotal = 0;

                // エクセル貼り付け用データを作成（合計行まで）
                for (int j = 0; j <= countData; j++)
                {
                    // 合計行かどうか
                    if (j != countData)
                    {
                        #region *****性別シートのデータ*****
                        // 対象の件数取得
                        int man = reportTable.Select($"ANK_1 = 1 and UKE_DATE = '{dateList[j]}'").Length;
                        int woman = reportTable.Select($"ANK_1 = 2 and UKE_DATE = '{dateList[j]}'").Length;
                        int genderUnknown = reportTable.Select($"ANK_1 = 9 and UKE_DATE = '{dateList[j]}'").Length;
                        int sexSubtotal = man + woman + genderUnknown;

                        // 配列にセット
                        arr_changeSexData[j, 0] = dateList[j];
                        arr_changeSexData[j, 1] = man;
                        arr_changeSexData[j, 2] = woman;
                        arr_changeSexData[j, 3] = genderUnknown;
                        arr_changeSexData[j, 4] = sexSubtotal;

                        // 合計に足す
                        totalMan += man;
                        totalWoman += woman;
                        totalGenderUnknown += genderUnknown;
                        totalSexSubtotal += sexSubtotal;
                        #endregion

                        #region *****年齢シートのデータ*****
                        // 対象の件数取得
                        int under20 = reportTable.Select($"ANK_2 = 1 and UKE_DATE = '{dateList[j]}'").Length;
                        int from20to40 = reportTable.Select($"ANK_2 = 2 and UKE_DATE = '{dateList[j]}'").Length;
                        int from40to60 = reportTable.Select($"ANK_2 = 3 and UKE_DATE = '{dateList[j]}'").Length;
                        int over60 = reportTable.Select($"ANK_2 = 4 and UKE_DATE = '{dateList[j]}'").Length;
                        int ageUnknown = reportTable.Select($"ANK_2 = 9 and UKE_DATE = '{dateList[j]}'").Length;
                        int ageSubtotal = under20 + from20to40 + from40to60 + over60 + ageUnknown;

                        arr_changeAgeData[j, 0] = dateList[j];
                        arr_changeAgeData[j, 1] = under20;
                        arr_changeAgeData[j, 2] = from20to40;
                        arr_changeAgeData[j, 3] = from40to60;
                        arr_changeAgeData[j, 4] = over60;
                        arr_changeAgeData[j, 5] = ageUnknown;
                        arr_changeAgeData[j, 6] = ageSubtotal;

                        totalUnder20 += under20;
                        total20to40 += from20to40;
                        total40to60 += from40to60;
                        totalOver60 += over60;
                        totalAgeUnknown += ageUnknown;
                        totalAgeSubtotal += ageSubtotal;
                        #endregion

                        #region *****職業シートのデータ*****
                        // 対象の件数取得
                        int selfEmp = reportTable.Select($"ANK_3 = 1 and UKE_DATE = '{dateList[j]}'").Length;
                        int companyEmp = reportTable.Select($"ANK_3 = 2 and UKE_DATE = '{dateList[j]}'").Length;
                        int part = reportTable.Select($"ANK_3 = 3 and UKE_DATE = '{dateList[j]}'").Length;
                        int student = reportTable.Select($"ANK_3 = 4 and UKE_DATE = '{dateList[j]}'").Length;
                        int others = reportTable.Select($"ANK_3 = 5 and UKE_DATE = '{dateList[j]}'").Length;
                        int jobUnknown = reportTable.Select($"ANK_3 = 9 and UKE_DATE = '{dateList[j]}'").Length;
                        int jobSubtotal = selfEmp + companyEmp + part + student + others + jobUnknown;

                        arr_changeJobData[j, 0] = dateList[j];
                        arr_changeJobData[j, 1] = selfEmp;
                        arr_changeJobData[j, 2] = companyEmp;
                        arr_changeJobData[j, 3] = part;
                        arr_changeJobData[j, 4] = student;
                        arr_changeJobData[j, 5] = others;
                        arr_changeJobData[j, 6] = jobUnknown;
                        arr_changeJobData[j, 7] = jobSubtotal;

                        totalSelfEmp += selfEmp;
                        totalCompanyEmp += companyEmp;
                        totalPart += part;
                        totalStudent += student;
                        totalOthers += others;
                        totalJobUnknown += jobUnknown;
                        totalJobSubtotal += jobSubtotal;
                        #endregion
                    }
                    else
                    {
                        arr_changeSexData[j, 0] = "合計";
                        arr_changeSexData[j, 1] = totalMan;
                        arr_changeSexData[j, 2] = totalWoman;
                        arr_changeSexData[j, 3] = totalGenderUnknown;
                        arr_changeSexData[j, 4] = totalSexSubtotal;

                        arr_changeAgeData[j, 0] = "合計";
                        arr_changeAgeData[j, 1] = totalUnder20;
                        arr_changeAgeData[j, 2] = total20to40;
                        arr_changeAgeData[j, 3] = total40to60;
                        arr_changeAgeData[j, 4] = totalOver60;
                        arr_changeAgeData[j, 5] = totalAgeUnknown;
                        arr_changeAgeData[j, 6] = totalAgeSubtotal;

                        arr_changeJobData[j, 0] = "合計";
                        arr_changeJobData[j, 1] = totalSelfEmp;
                        arr_changeJobData[j, 2] = totalCompanyEmp;
                        arr_changeJobData[j, 3] = totalPart;
                        arr_changeJobData[j, 4] = totalStudent;
                        arr_changeJobData[j, 5] = totalOthers;
                        arr_changeJobData[j, 6] = totalJobUnknown;
                        arr_changeJobData[j, 7] = totalJobSubtotal;
                    }
                }

                // 性別シートの貼り付け範囲指定
                Excel.Range sexRange = sexSheet.Range[sexSheet.Cells[8, 2], sexSheet.Cells[countData + 8, 6]];
                sexRange.Value = arr_changeSexData;

                // 年齢シートの貼り付け範囲指定
                Excel.Range ageRange = ageSheet.Range[ageSheet.Cells[8, 2], ageSheet.Cells[countData + 8, 8]];
                ageRange.Value = arr_changeAgeData;

                // 職業シートの貼り付け範囲指定
                Excel.Range jobRange = jobSheet.Range[jobSheet.Cells[8, 2], jobSheet.Cells[countData + 8, 9]];
                jobRange.Value = arr_changeJobData;

                // ファイルを保存して閉じる
                excelBook.Save();
                excelBook.Close();

                MessageBox.Show("報告書データを出力しました。", "確認");
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
    }
}
