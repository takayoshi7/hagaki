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
using Microsoft.VisualBasic;

namespace hagaki
{
    public partial class Frm0400_SEARCH : Form
    {
        #region メンバ変数
        private string connectionString = string.Empty;   // 接続文字列
        private My_Function _func;                        // My_Functionを使えるように
        private DataTable searchResultTable;              // 検索結果格納テーブル
        private DataTable nothingTable = new DataTable(); // 検索結果が0だった場合の表示テーブル
        #endregion

        #region 定数
        private const string D_MAIN = "D_MAIN";
        private const string JYOTAI = "M_JYOTAI";
        private const string OUT = "M_OUT";
        private const string EXCEPTION_ERROR_TITLE = "例外エラー";
        #endregion

        #region コンストラクタ
        public Frm0400_SEARCH()
        {
            InitializeComponent();
        }

        public Frm0400_SEARCH(string connestStr)
        {
            InitializeComponent();

            connectionString = connestStr;
        }
        #endregion

        #region ロードイベント
        private void Frm0400_SEARCH_Load(object sender, EventArgs e)
        {
            try
            {
                // My_Functionクラスをインスタンス化
                _func = new My_Function();

                // コンボボックス用リストを対象テーブルから取得する
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // DataSetを作成
                    DataSet dataSet = new DataSet();

                    #region **********状態区分選択コンボボックス用リスト作成**********
                    // M_JYOTAIテーブルデータを取得SQL文の生成
                    string jyotaiSqlStr = $"SELECT * FROM {JYOTAI}";

                    // データを取得してDataSetに追加
                    _func.FillDataTable(dataSet, connection, null, jyotaiSqlStr, null, JYOTAI);

                    // コンボボックスに設定
                    JyotaiKb.DataSource = dataSet.Tables[JYOTAI];
                    JyotaiKb.ValueMember = "JYOTAI_KB";
                    JyotaiKb.DisplayMember = "JYOTAI_NAME";

                    // 初期選択肢を空にする（選択なし）
                    JyotaiKb.SelectedIndex = -1;
                    #endregion

                    #region **********出力区分選択コンボボックス用リスト作成**********
                    // M_OUTテーブルデータを取得SQL文の生成
                    string outSqlStr = $"SELECT * FROM {OUT}";

                    // データを取得してDataSetに追加
                    _func.FillDataTable(dataSet, connection, null, outSqlStr, null, OUT);

                    // コンボボックスに設定
                    NgOutKb.DataSource = dataSet.Tables[OUT];
                    NgOutKb.ValueMember = "OUT_KB";
                    NgOutKb.DisplayMember = "OUT_NAME";
                    DataTable copyOutTable = dataSet.Tables[OUT].Copy();
                    HisoOutKb.DataSource = copyOutTable;
                    HisoOutKb.ValueMember = "OUT_KB";
                    HisoOutKb.DisplayMember = "OUT_NAME";

                    // 初期選択肢を空にする（選択なし）
                    NgOutKb.SelectedIndex = -1;
                    HisoOutKb.SelectedIndex = -1;
                    #endregion
                }

                #region 検索結果のデータが0だった場合の表示用テーブル作成
                // カラム名配列
                string[] columnNames = new[]
                {
                    "KANRI_NO", "UKE_DATE", "ZIP_CD", "ADD_ALL", "NAME_SEI", "NAME_MEI",
                    "TEL_NO", "ANK_1", "ANK_2", "ANK_3", "JYOTAI_KB", "NG_OUT_KB",
                    "NG_OUT_DATETIME", "NG_OUT_LOGINID", "HISO_OUT_DATETIME",
                    "HISO_OUT_LOGINID", "UPDATE_DATETIME", "UPDATE_LOGINID"
                };

                // テーブルにカラムを追加
                foreach (string columnName in columnNames)
                {
                    nothingTable.Columns.Add(columnName);
                }

                // 行作成
                DataRow dtrow = nothingTable.NewRow();
                dtrow["KANRI_NO"] = "データがありません";

                // テーブルに行を追加
                nothingTable.Rows.Add(dtrow);
                #endregion

                // 奇数行（表示上は偶数行）を水色にする
                DataGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.LightSteelBlue;

                // 数値のみの入力制御
                BeforeKanriNo.KeyPress += _func.NumTextKeyPress;
                AfterKanriNo.KeyPress += _func.NumTextKeyPress;
                ZipCd.KeyPress += _func.NumTextKeyPress;
                TelNo.KeyPress += _func.NumTextKeyPress;

                // フォーカス外れたときの強制半角制御
                BeforeKanriNo.Leave += NumText_Leave;
                AfterKanriNo.Leave += NumText_Leave;
                ZipCd.Leave += NumText_Leave;
                TelNo.Leave += NumText_Leave;
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

        #region 検索
        private void SearchButton_Click(object sender, EventArgs e)
        {
            try
            {
                // マウスカーソルを砂時計にする
                Cursor = Cursors.WaitCursor;

                // 入力値を取得
                string beforeK_No = BeforeKanriNo.Text;
                string afterK_No = AfterKanriNo.Text;
                DateTime beforeUke_Date = BeforeUkeDate.Value;
                DateTime afterUke_Date = AfterUkeDate.Value;
                string zip_Cd = ZipCd.Text;
                string add_All = Address.Text;
                string name_Sei = Sei.Text;
                string name_Mei = Mei.Text;
                string tel_No = TelNo.Text;
                string jyotai_Kb = JyotaiKb.SelectedValue != null ? JyotaiKb.SelectedValue.ToString() : "";
                string ng_Out = NgOutKb.SelectedValue != null ? NgOutKb.SelectedValue.ToString() : "";
                string hiso_Out = HisoOutKb.SelectedValue != null ? HisoOutKb.SelectedValue.ToString() : "";

                // 管理番号入力チェック
                if (!string.IsNullOrWhiteSpace(beforeK_No) && !string.IsNullOrWhiteSpace(afterK_No))
                {
                    // 事務局管理番号の後の値が前の値より小さければエラー
                    if (int.Parse(beforeK_No) > int.Parse(afterK_No))
                    {
                        MessageBox.Show("事務局管理番号の範囲が正しくありません。", "エラー");
                        return;
                    }
                }

                // 受付日の後の値が前の値より小さければエラー
                if (beforeUke_Date > afterUke_Date)
                {
                    MessageBox.Show("受付日の範囲が正しくありません。", "エラー");
                    return;
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    // DBを開く
                    connection.Open();

                    // DataSetを作成
                    DataSet dataSet = new DataSet();

                    // SQL文作成用
                    StringBuilder getDmain = new StringBuilder();
                    getDmain.AppendLine("SELECT *, " +
                                        "CONCAT(ADD_1, ADD_2, ADD_3, ' ', ADD_4) AS ADD_ALL " +
                                        $"FROM {D_MAIN} WHERE ");

                    // パラメータ作成用
                    Dictionary<string, object> parameters = new Dictionary<string, object>();

                    // 検索条件設定
                    if (!string.IsNullOrWhiteSpace(beforeK_No))
                    {
                        getDmain.AppendLine("KANRI_NO >= @BeforeKariNo AND ");
                        parameters.Add("@BeforeKariNo", beforeK_No);
                    }

                    if (!string.IsNullOrWhiteSpace(afterK_No))
                    {
                        getDmain.AppendLine("KANRI_NO <= @AfterKariNo AND ");
                        parameters.Add("@AfterKariNo", afterK_No);
                    }

                    getDmain.AppendLine("UKE_DATE >= @UkeDateStart AND UKE_DATE <= @UkeDateEnd");
                    parameters.Add("@UkeDateStart", beforeUke_Date.ToString("yyyyMMdd"));
                    parameters.Add("@UkeDateEnd", afterUke_Date.ToString("yyyyMMdd"));

                    if (!string.IsNullOrWhiteSpace(zip_Cd))
                    {
                        getDmain.AppendLine(" AND ZIP_CD = @ZipCd");
                        parameters.Add("@ZipCd", zip_Cd);
                    }

                    if (!string.IsNullOrWhiteSpace(add_All))
                    {
                        getDmain.AppendLine(" AND CONCAT(ADD_1, ADD_2, ADD_3, ' ', ADD_4) LIKE @Address");
                        parameters.Add("@Address", $"%{add_All}%");
                    }

                    if (!string.IsNullOrWhiteSpace(name_Sei))
                    {
                        getDmain.AppendLine(" AND NAME_SEI LIKE @Sei");
                        parameters.Add("@Sei", $"%{name_Sei}%");
                    }

                    if (!string.IsNullOrWhiteSpace(name_Mei))
                    {
                        getDmain.AppendLine(" AND NAME_MEI LIKE @Mei");
                        parameters.Add("@Mei", $"%{name_Mei}%");
                    }

                    if (!string.IsNullOrWhiteSpace(tel_No))
                    {
                        getDmain.AppendLine(" AND TEL_NO LIKE @Tel");
                        parameters.Add("@Tel", $"%{tel_No}%");
                    }

                    if (!string.IsNullOrWhiteSpace(jyotai_Kb))
                    {
                        getDmain.AppendLine(" AND JYOTAI_KB = @JotaiKb");
                        parameters.Add("@JotaiKb", jyotai_Kb);
                    }

                    if (!string.IsNullOrWhiteSpace(ng_Out))
                    {
                        getDmain.AppendLine(" AND NG_OUT_KB = @NgOut");
                        parameters.Add("@NgOut", ng_Out);
                    }

                    if (!string.IsNullOrWhiteSpace(hiso_Out))
                    {
                        getDmain.AppendLine(" AND HISO_OUT_KB = @HisoOut");
                        parameters.Add("@HisoOut", hiso_Out);
                    }

                    // 検索結果取得
                    _func.FillDataTable(dataSet, connection, null, getDmain.ToString(), parameters, D_MAIN);

                    // 絞り込んだD_MAINテーブル取得
                        searchResultTable = dataSet.Tables[D_MAIN];

                    // 検索結果による分岐処理
                    if (searchResultTable.Rows.Count > 0)
                    {
                        // 検索結果あり
                        DataGridView.DataSource = searchResultTable;

                        // ADD_1、ADD_2、ADD_3、ADD_4カラムを非表示にする
                        DataGridView.Columns["ADD_1"].Visible = false;
                        DataGridView.Columns["ADD_2"].Visible = false;
                        DataGridView.Columns["ADD_3"].Visible = false;
                        DataGridView.Columns["ADD_4"].Visible = false;
                    }
                    else
                    {
                        // 検索結果なし
                        DataGridView.DataSource = nothingTable;
                    }

                    // マウスカーソルを元に戻す
                    Cursor = Cursors.Default;
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

        #region 戻る
        private void BackButton_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion

        #region 検索条件リセット
        /// <summary>
        /// 入力された検索条件をリセットする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetButton_Click(object sender, EventArgs e)
        {
            BeforeKanriNo.Text = string.Empty;
            AfterKanriNo.Text = string.Empty;
            BeforeUkeDate.Text = DateTime.Parse("2022/01/01").ToString();
            AfterUkeDate.Text = DateTime.Now.ToString();
            ZipCd.Text = string.Empty;
            Address.Text = string.Empty;
            Sei.Text = string.Empty;
            Mei.Text = string.Empty;
            TelNo.Text = string.Empty;
            JyotaiKb.SelectedIndex = -1;
            NgOutKb.SelectedIndex = -1;
            HisoOutKb.SelectedIndex = -1;
        }
        #endregion

        #region フォーカスが外れたときの処理
        /// <summary>
        /// 数値のみテキストボックスのフォーカスが外れたときに強制半角変換
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumText_Leave(object sender, EventArgs e)
        {
            // フォーカスを外されたテキストボックスName
            string outForcus = ((Control)sender).Name;

            // 半角に変換
            switch (outForcus)
            {
                case "BeforeKanriNo":
                    BeforeKanriNo.Text = StCls_Function.VbStrConv(BeforeKanriNo.Text, (VbStrConv)8);
                    break;
                case "AfterKanriNo":
                    AfterKanriNo.Text = StCls_Function.VbStrConv(AfterKanriNo.Text, (VbStrConv)8);
                    break;
                case "ZipCd":
                    ZipCd.Text = StCls_Function.VbStrConv(ZipCd.Text, (VbStrConv)8);
                    break;
                case "TelNo":
                    TelNo.Text = StCls_Function.VbStrConv(TelNo.Text, (VbStrConv)8);
                    break;
            }
        }
        #endregion

        #region ダブルクリック処理
        /// <summary>
        /// 検索結果の表でダブルクリックをした時に、選択されたデータのメンテナンス画面を開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // DataGridViewのダブルクリックした行の事務局管理番号を取得
            string selectedKanriNo = DataGridView.CurrentRow.Cells[0].Value.ToString();

            // 絞り込まれたデータの管理番号のみのリスト
            List<string> searchResultKanriNoList = new List<string>();
            foreach (DataRow rows in searchResultTable.Rows)
            {
                searchResultKanriNoList.Add(rows["KANRI_NO"].ToString());
            }

            // メンテナンス画面に選択した管理番号を渡す
            Frm0500_MAINTENANCE Frm0500_MAINTENANCE = new Frm0500_MAINTENANCE(connectionString, selectedKanriNo, searchResultKanriNoList);

            // メンテナンス画面を開く
            Frm0500_MAINTENANCE.ShowDialog();
        }
        #endregion
    }
}
