using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Data.SqlClient;
using System.Net;
using System.Xml.Linq;

namespace hagaki
{
    public partial class Frm0000_MENU : Form
    {
        #region メンバ変数
        private string connectionString = string.Empty;      // DB接続文字列
        #endregion

        #region 定数
        private const string OPERATION_XML = "KensyuSys.xml"; // DB接続情報取得xmlファイル名
        #endregion

        #region コンストラクタ
        public Frm0000_MENU()
        {
            InitializeComponent();
        }
        #endregion

        #region ロードイベント
        private void Frm0000_MENU_Load(object sender, EventArgs e)
        {
            // メニューボタンクリック時制御
            ImportData_Menu.Click         += new EventHandler(SelectTopMenu);
            OutputNG_Menu.Click           += new EventHandler(SelectTopMenu);
            OutputDeliveryData_Menu.Click += new EventHandler(SelectTopMenu);
            Search_Menu.Click             += new EventHandler(SelectTopMenu);
            OutputReport_Menu.Click       += new EventHandler(SelectTopMenu);
            End_Menu.Click                += new EventHandler(SelectTopMenu);

            // XMLファイルを読込
            XElement xEle = XElement.Load(OPERATION_XML);

            // 接続文字列作成
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = Dns.GetHostName();
            builder.InitialCatalog = xEle.Element("DB")?.Element("DATABASE")?.Value ?? "";
            builder.UserID = xEle.Element("DB")?.Element("ID")?.Value ?? "";
            builder.Password = xEle.Element("DB")?.Element("PASSWORD")?.Value ?? "";
            builder.IntegratedSecurity = false;

            connectionString = builder.ConnectionString;
        }
        #endregion

        #region メニュー画面選択
        private void SelectTopMenu(object sender, EventArgs e)
        {
            try
            {
                // 押されたボタンのName取得
                string selectButtonName = ((Button)sender).Name;

                // 終了ボタンが押されたらアプリケーション終了
                if (selectButtonName == "End_Menu")
                {
                    DialogResult dialog = MessageBox.Show("終了します。よろしいでしょうか？", "確認", MessageBoxButtons.YesNo);

                    if (dialog == DialogResult.No)
                    {
                        return;
                    }

                    // アプリケーション終了
                    Close();
                }

                // メニュー画面を非表示
                Hide();

                // 押されたボタンの画面を開く
                switch (selectButtonName)
                {
                    case "ImportData_Menu":
                        Frm0100_IN_DATA Frm0100_IN_DATA = new Frm0100_IN_DATA(connectionString);
                        Frm0100_IN_DATA.ShowDialog();
                        break;
                    case "OutputNG_Menu":
                        Frm0200_OUT_NG_DATA Frm0200_OUT_NG_DATA = new Frm0200_OUT_NG_DATA();
                        Frm0200_OUT_NG_DATA.ShowDialog();
                        break;
                    case "OutputDeliveryData_Menu":
                        Frm0300_OUT_HISO_DATA Frm0300_OUT_HISO_DATA = new Frm0300_OUT_HISO_DATA();
                        Frm0300_OUT_HISO_DATA.ShowDialog();
                        break;
                    case "Search_Menu":
                        Frm0400_SEARCH Frm0400_SEARCH = new Frm0400_SEARCH();
                        Frm0400_SEARCH.ShowDialog();
                        break;
                    case "OutputReport_Menu":
                        Frm0600_OUT_REPORT Frm0600_OUT_REPORT = new Frm0600_OUT_REPORT();
                        Frm0600_OUT_REPORT.ShowDialog();
                        break;
                }

                // メニュー画面を表示
                Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "エラー");
            }
        }
        #endregion
    }
}
