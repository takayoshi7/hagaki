using hagaki.Class;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace hagaki
{
    public partial class Frm0500_MAINTENANCE : Form
    {
        #region メンバ変数
        private string connectionString = string.Empty;   // 接続文字列
        private My_Function _func;                        // My_Functionを使えるように
        private Cls_DBConn _conn;                         // Cls_DBConnを使えるように
        private string searchSelectedKanriNo = string.Empty;    // 検索画面で選択された管理番号
        private DataTable searchResultData = new DataTable();   // 検索画面で絞り込まれた全データ

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

        public Frm0500_MAINTENANCE(string connestStr, string kanriNo, DataTable searchResultTable)
        {
            InitializeComponent();

            connectionString = connestStr;
            searchSelectedKanriNo = kanriNo;
            searchResultData = searchResultTable;
        }
        #endregion

        #region ロードイベント
        private void Frm0500_MAINTENANCE_Load(object sender, EventArgs e)
        {
            try
            {
                // My_Functionクラスをインスタンス化
                _func = new My_Function();

                // Cls_DBConnクラスをインスタンス化
                _conn = new Cls_DBConn(connectionString);

                // 初期フォーカスを郵便番号欄にする
                ActiveControl = ZipCdText;





            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, EXCEPTION_ERROR_TITLE);
            }
        }
        #endregion





        #region 各項目欄に表示
        /// <summary>
        /// 選択された事務局管理番号の情報を表示する
        /// </summary>
        /// <param name="kanriNo">選択されている事務局管理番号</param>
        public void SetTextBox(string kanriNo)
        {
            // DB接続
            SqlConnection connect = _conn.SetDBConnction();

            try
            {
                // エラー項目の表をクリア
                ErrorDataGridView.Rows.Clear();

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

                // 選択された事務局管理番号のデータを取得SQL文の生成
                string getSelectedData = $"SELECT * FROM {D_MAIN} WHERE KANRI_NO = '{kanriNo}'";

                // データセット作成
                DataSet dset = _conn.SetDataSet(connect, getSelectedData, "selectedData");

                // selectedDataテーブル取得
                DataTable selectedTable = dset.Tables["selectedData"];

                // 選択されたレコード情報取得
                DataRow rowData = selectedTable.Rows[0];

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

                switch (rowData["JYOTAI_KB"].ToString())
                {
                    case "0":
                        OK_RadioButton.Checked = true;
                        break;
                    case "1":
                        NG_RadioButton.Checked = true;
                        break;
                    case "2":
                        KEEP_RadioButton.Checked = true;
                        break;
                    case "3":
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

                // レコードのエラーコード取得SQL文の生成
                string getErrorData = $"SELECT * FROM {D_ERROR} WHERE KANRI_NO = '{kanriNo}' ORDER BY ERR_CD ASC";

                // データセット作成
                dset = _conn.SetDataSet(connect, getErrorData, "errorData");

                // errorDataテーブル取得
                DataTable errorTable = dset.Tables["errorData"];

                // エラーがなければ処理終了
                if (errorTable.Rows.Count == 0)
                {
                    return;
                }

                // エラー文言取得SQL文の生成
                string getErrorMongon = $"SELECT * FROM {M_ERROR}";

                // データセット作成
                dset = _conn.SetDataSet(connect, getErrorMongon, "M_ERROR");

                // M_ERRORテーブル取得
                DataTable mErrorTable = dset.Tables["M_ERROR"];

                // エラーコードごとに繰り返し
                foreach (DataRow errorRow in errorTable.Rows)
                {
                    // エラーコード取得
                    string errCd = errorRow["ERR_CD"].ToString();

                    // エラー内容
                    string errContents = "";

                    // エラーコードの文言を取得
                    foreach (DataRow mongon in mErrorTable.Rows)
                    {
                        if (mongon["ERR_CD"].ToString() == errCd)
                        {
                            errContents = mongon["ERR_MONGON"].ToString();
                        }
                    }

                    // エラー表示
                    ErrorDataGridView.Rows.Add(errCd, errContents);

                    // 追加設定（仕様書に無い）
                    // データ表示時にエラーの項目は背景を赤に
                    switch (errCd)
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, EXCEPTION_ERROR_TITLE);
            }
        }
        #endregion
    }
}
