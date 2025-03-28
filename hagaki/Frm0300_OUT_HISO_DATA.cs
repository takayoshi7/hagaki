﻿using hagaki.StaticClass;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace hagaki
{
    public partial class Frm0300_OUT_HISO_DATA : Form
    {
        #region メンバ変数
        private string connectionString = string.Empty;            // 接続文字列
        private MyClass _myClass;                                  // MyClassを使えるように
        private string outHisoFolderPath = string.Empty;           // 配送データ出力先フォルダパス
        private DataTable noOutHisoTable;                          // 配送データ出力対象データテーブル
        private List<string> hisoKanriNoList = new List<string>(); // 事務局管理番号のみのリスト
        #endregion

        #region 定数
        private const string OUT_HISO_PATH_NODE = "DIR/OUT_HISO_FLDPATH"; // 配送データ出力先ノード
        #endregion

        #region コンストラクタ
        public Frm0300_OUT_HISO_DATA()
        {
            InitializeComponent();
        }

        public Frm0300_OUT_HISO_DATA(string connestStr)
        {
            InitializeComponent();

            connectionString = connestStr;
        }
        #endregion

        #region ロードイベント
        private void Frm0300_OUT_HISO_DATA_Load(object sender, EventArgs e)
        {
            try
            {
                // XMLファイルを読込
                XElement xEle = XElement.Load(MyStaticClass.OPERATION_XML);

                // パスを分解
                string[] pathParts = OUT_HISO_PATH_NODE.Split('/');

                // DIR要素を取得
                XElement dirElement = xEle.Element(pathParts[0]);

                // 値を取得
                outHisoFolderPath = dirElement?.Element(pathParts[1])?.Value ?? "";

                // エラーファイル出力先パス表示
                OutHisoPathLabel.Text = outHisoFolderPath;

                // MyClassクラスをインスタンス化
                _myClass = new MyClass();

                // 件数初期化
                InitHisoCount();
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

        #region 配送データ出力先のフォルダを開く
        private void OutputDirButton_Click(object sender, EventArgs e)
        {
            try
            {
                // 出力先フォルダの存在チェック
                if (!Directory.Exists(outHisoFolderPath))
                {
                    // フォルダがない場合作成する
                    Directory.CreateDirectory(outHisoFolderPath);
                }

                // フォルダを開く
                bool openFileCheck = StCls_File.WindowOpen(outHisoFolderPath);

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

                            // WK_HISOデリートSQL文の生成
                            command.CommandText = MyStaticClass.MakeDeleteSql(MyStaticClass.WK_HISO); ;

                            // WK_HISOデリートSQL文を実行
                            bool wkHisoDeleteCheck = MyStaticClass.Execute(command, null);
                            if (!wkHisoDeleteCheck)
                            {
                                MessageBox.Show("WK_HISOテーブルの初期化に失敗しました。", "エラー");
                                return;
                            }

                            // DataSetを作成
                            DataSet dataSet = new DataSet();

                            // D_MAINテーブルの配送データ出力対象データを取得SQL文の生成
                            StringBuilder getHisoDMainSqlStr = new StringBuilder();
                            getHisoDMainSqlStr.AppendLine("SELECT");
                            getHisoDMainSqlStr.AppendLine(" KANRI_NO,");
                            getHisoDMainSqlStr.AppendLine(" ZIP_CD,");
                            getHisoDMainSqlStr.AppendLine(" ADD_1,");
                            getHisoDMainSqlStr.AppendLine(" ADD_2,");
                            getHisoDMainSqlStr.AppendLine(" ADD_3,");
                            getHisoDMainSqlStr.AppendLine(" ADD_4,");
                            getHisoDMainSqlStr.AppendLine(" NAME_SEI,");
                            getHisoDMainSqlStr.AppendLine(" NAME_MEI,");
                            getHisoDMainSqlStr.AppendLine(" JYOTAI_KB");
                            getHisoDMainSqlStr.AppendLine(" HISO_OUT_KB");
                            getHisoDMainSqlStr.AppendLine($" FROM {MyStaticClass.D_MAIN}");
                            getHisoDMainSqlStr.AppendLine($" WHERE HISO_OUT_KB = '{(int)HisoOutKb.Un}' AND JYOTAI_KB = '{(int)JyotaiKb.Ok}'");
                            getHisoDMainSqlStr.AppendLine(" ORDER BY KANRI_NO ASC");

                            command.CommandText = getHisoDMainSqlStr.ToString();

                            // データを取得してDataSetに追加
                            _myClass.FillDataTable(dataSet, command, null, MyStaticClass.D_MAIN + "_HISO");

                            // D_MAIN_HISOテーブル取得
                            noOutHisoTable = dataSet.Tables[MyStaticClass.D_MAIN + "_HISO"];

                            // 事務局管理番号だけのリスト作成
                            foreach (DataRow record in noOutHisoTable.Rows)
                            {
                                hisoKanriNoList.Add(record["KANRI_NO"].ToString());
                            }

                            // 重複削除して昇順に並び替え
                            hisoKanriNoList = hisoKanriNoList.Distinct().OrderBy(x => x).ToList();

                            // 件数表示
                            HisoCountLabel.Text = noOutHisoTable.Rows.Count.ToString();

                            // 件数が0件以上であれば
                            if (int.Parse(HisoCountLabel.Text) > 0)
                            {
                                // WK_HISOテーブルに登録
                                foreach (DataRow hisoOutRow in noOutHisoTable.Rows)
                                {
                                    // SQL文の生成
                                    command.CommandText = MyStaticClass.MakeInsertSql(MyStaticClass.WK_HISO);

                                    // パラメータ
                                    Dictionary<string, object> parameters = new Dictionary<string, object>
                                    {
                                        { "@KanriNo", hisoOutRow["KANRI_NO"] },
                                        { "@ZipCd", hisoOutRow["ZIP_CD"] },
                                        { "@Add1", hisoOutRow["ADD_1"] },
                                        { "@Add2", hisoOutRow["ADD_2"] },
                                        { "@Add3", hisoOutRow["ADD_3"] },
                                        { "@Add4", hisoOutRow["ADD_4"] },
                                        { "@NameSei", hisoOutRow["NAME_SEI"] },
                                        { "@NameMei", hisoOutRow["NAME_MEI"] }
                                    };

                                    // SQL文を実行
                                    bool hisoExecuteCheck = MyStaticClass.Execute(command, parameters);

                                    if (!hisoExecuteCheck)
                                    {
                                        MessageBox.Show("WK_HISOテーブルの登録に失敗しました。", "エラー");
                                        return;
                                    }
                                }

                                // 出力ボタン活性化
                                OutputButton.Enabled = true;
                                OutputButton.BackColor = SystemColors.GradientActiveCaption;
                                OutputButton.Cursor = Cursors.Hand;
                            }
                        }

                        // コミット
                        transaction.Commit();
                    }
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

        #region 配送データ出力
        private void OutputButton_Click(object sender, EventArgs e)
        {
            // プログレスダイアログを作成
            ProgressDialog progressDialog = new ProgressDialog();

            try
            {
                // 確認ダイアログ表示
                DialogResult outCsvResult = MessageBox.Show("出力してもよろしいでしょうか？", "確認", MessageBoxButtons.YesNo);

                // いいえを選択したら出力キャンセル
                if (outCsvResult == DialogResult.No)
                {
                    return;
                }

                // 出力先フォルダがない場合フォルダを作成する
                if (!Directory.Exists(outHisoFolderPath))
                {
                    Directory.CreateDirectory(outHisoFolderPath);
                }

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

                            // プログレスダイアログを作成し、進捗管理する処理とTupleを使ってオブジェクトを渡す
                            ProgressDialog pd = new ProgressDialog(new DoWorkEventHandler(ProgressDialog_DoWork), command);

                            // ダイアログの結果を確認して処理
                            DialogResult result = pd.ShowDialog();

                            // キャンセルまたはエラーの場合
                            if (result == DialogResult.Cancel || result == DialogResult.Abort)
                            {
                                return;
                            }
                        }

                        // コミット
                        transaction.Commit();
                    }
                }

                // 件数初期化
                InitHisoCount();

                MessageBox.Show("配送データを出力しました。", "確認");
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

        #region 件数初期化
        /// <summary>
        /// 表示件数を初期化して、出力ボタンを非活性化
        /// </summary>
        private void InitHisoCount()
        {
            HisoCountLabel.Text = "";

            OutputButton.Enabled = false;
            OutputButton.BackColor = SystemColors.ControlDark;
            OutputButton.Cursor = Cursors.Default;
        }
        #endregion

        #region CSV用データ作成
        /// <summary>
        /// 値はダブルクォテーションで囲まれ、カンマ区切りされた1行の文字列を作成する
        /// </summary>
        /// <param name="hisoRow">1レコードのデータ</param>
        /// <returns>1行のデータ</returns>
        private string CSVExport(DataRow hisoRow)
        {
            // データ行
            string rowStr = string.Empty;
            // 事務局管理番号
            rowStr += "\"" + hisoRow["KANRI_NO"].ToString() + "\",";
            // 郵便番号
            if (MyStaticClass.SJIS.GetByteCount(hisoRow["ZIP_CD"].ToString()) == 7 )
            {
                // 7バイトならハイフンを付ける
                rowStr += "\"" + hisoRow["ZIP_CD"].ToString().Insert(3, "-") + "\",";
            }
            else
            {
                rowStr += "\"" + hisoRow["ZIP_CD"].ToString() + "\",";
            }
            // 住所1
            rowStr += "\"" + hisoRow["ADD_1"].ToString() + "\",";
            // 住所2
            rowStr += "\"" + hisoRow["ADD_2"].ToString() + "\",";
            // 住所3
            rowStr += "\"" + hisoRow["ADD_3"].ToString() + "\",";
            // 住所4
            rowStr += "\"" + hisoRow["ADD_4"].ToString() + "\",";
            // 氏名(姓)
            rowStr += "\"" + hisoRow["NAME_SEI"].ToString() + "\",";
            // 氏名(名)
            rowStr += "\"" + hisoRow["NAME_MEI"].ToString() + "\"";

            return rowStr;
        }
        #endregion

        #region CSV出力とDB更新
        /// <summary>
        /// CSV出力とDB更新を行う
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProgressDialog_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            // 現在の日時を取得
            DateTime now = DateTime.Now;

            // 現在の日時を変換（ファイル名用）
            string nowDateTime_file = now.ToString("yyyyMMddHHmmss");

            //現在の日時を変換（DB用）
            string nowDateTime_DB = now.ToString("yyyy/MM/dd HH:mm:ss");

            // ログインユーザー名取得
            string loginID = StCls_Function.GetUser();

            int i = 0;

            // 出力先CSVファイルの存在チェック（存在していればファイル名に追加する文字列作成）
            while (File.Exists(outHisoFolderPath + "hiso_" + nowDateTime_file + MyStaticClass.NumStr(i) + ".csv"))
            {
                i++;
            }

            // 出力先ファイルパス
            string outHisoFilePath = outHisoFolderPath + "hiso_" + nowDateTime_file + MyStaticClass.NumStr(i) + ".csv";

            // 出力対象の最大件数
            double maximumValue = noOutHisoTable.Rows.Count;

            // 進捗率
            int progressRatio = 0;

            // 完了したレコード数
            int count = 0;

            // CSVファイルを作成
            using (StreamWriter sw = new StreamWriter(outHisoFilePath, false, MyStaticClass.SJIS))
            {
                // SqlCommandを取得
                SqlCommand command = e.Argument as SqlCommand;

                // SqlCommandがnullの場合
                if (command == null)
                {
                    e.Result = new Exception("SqlCommandがNULLです。");
                    return;
                }

                foreach (DataRow hisoRow in noOutHisoTable.Rows)
                {
                    // CSV用データ作成
                    string rowStr = CSVExport(hisoRow);

                    // 1行書き込み
                    sw.WriteLine(rowStr);

                    #region D_MAINを更新
                    // D_MAINアップデートSQL文の作成
                    command.CommandText = MyStaticClass.MakeUpdateSql(MyStaticClass.D_MAIN, "OUT_HISO"); ;

                    // パラメータ
                    Dictionary<string, object> kanriNoParameter = new Dictionary<string, object>
                    {
                        { "@KanriNo", hisoRow["KANRI_NO"].ToString() }
                    };

                    // D_MAINアップデートSQL文を実行
                    bool dMainOutExcuteCheck = MyStaticClass.Execute(command, kanriNoParameter);

                    if (!dMainOutExcuteCheck)
                    {
                        e.Result = new Exception("D_MAINテーブルの更新に失敗しました。");
                        return;
                    }
                    #endregion

                    // 1レコードの進捗割合を足す
                    count++;
                    progressRatio = (int)Math.Round((count / maximumValue) * 100);
                    // 100%を超えないように制限
                    progressRatio = Math.Min(progressRatio, 100);

                    // キャンセルされたか確認
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }

                    // 進捗更新
                    worker.ReportProgress(progressRatio);

                    if (progressRatio == 100)
                    {
                        // 進捗率が100%になったら少し表示させたままにする
                        Thread.Sleep(150);
                    }
                }

                // WK_HISOデリートSQL文の生成
                command.CommandText = MyStaticClass.MakeDeleteSql(MyStaticClass.WK_HISO);

                // WK_HISOデリートSQL文を実行
                bool wkHisoDeleteCheck = MyStaticClass.Execute(command, null);
                if (!wkHisoDeleteCheck)
                {
                    e.Result = new Exception("WK_HISOテーブルの初期化に失敗しました。");
                    return;
                }
            }
        }
        #endregion
    }
}
