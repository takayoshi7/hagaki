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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace hagaki
{
    public partial class ProgressDialog: Form
    {
        #region　メンバ変数
        private BackgroundWorker backgroundWorker; // BackgroundWorkerのインスタンス
        #endregion

        #region コンストラクタ
        public ProgressDialog()
        {
            InitializeComponent();
        }

        /// <param name="workHandler">進捗管理する処理</param>
        /// <param name="tuple">進捗管理する処理に渡すSqlConnectionとSqlTransaction</param>
        public ProgressDialog(DoWorkEventHandler workHandler, Tuple<SqlConnection, SqlTransaction> tuple)
        {
            InitializeComponent();

            #region BackgroundWorkerの設定
            // BackgroundWorkerオブジェクトを作成
            backgroundWorker = new BackgroundWorker();
            // 進捗報告をサポートする設定（進捗報告を行うためtrueに設定）
            backgroundWorker.WorkerReportsProgress = true;
            // キャンセルをサポートする設定（キャンセルを行うためtrueに設定）
            backgroundWorker.WorkerSupportsCancellation = true;
            // バックグラウンドで実行する（進捗管理する）処理を設定
            backgroundWorker.DoWork += workHandler;
            // 進捗状況の更新を処理するためのイベント
            backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
            // 処理が完了した後の処理をするためのイベント
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
            // バックグラウンド作業を非同期に開始（進捗管理する処理（Frm0300_OUT_HISO_DATA.ProgressDialog_DoWork）にSqlConnectionとSqlTransactionを渡す）
            backgroundWorker.RunWorkerAsync(tuple);
            #endregion
        }
        #endregion

        #region プログレスバーを更新
        /// <summary>
        /// プログレスバーの進捗表示を更新する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // プログレスバーとラベルの更新
            ProgressBar1.Value = e.ProgressPercentage;
            MessageLabel.Text = $"{e.ProgressPercentage}% / 100%";
        }
        #endregion

        #region 処理完了後
        /// <summary>
        /// 処理が完了した後の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // 処理結果による分岐
            if (e.Cancelled)
            {
                DialogResult = DialogResult.Cancel;
                MessageBox.Show("処理がキャンセルされました", "キャンセル");
            }
            else if (e.Error != null)
            {
                DialogResult = DialogResult.Abort;
                MessageBox.Show("処理にエラーが発生しました。", "エラー");
            }
            else if (e.Result is Exception ex)
            {
                DialogResult = DialogResult.Abort;
                MessageBox.Show(ex.Message, "エラー");
            }
            else
            {
                DialogResult = DialogResult.OK;
            }

            // キャンセルボタンを無効にする
            CancelButton.Enabled = false;
            CancelButton.BackColor = SystemColors.ControlDark;
            CancelButton.Cursor = Cursors.Default;
        }
        #endregion

        #region キャンセルボタン
        /// <summary>
        /// キャンセルボタンがクリックされた時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, EventArgs e)
        {
            // バックグラウンド作業が実行中の場合
            if (backgroundWorker.IsBusy)
            {
                // バックグラウンド作業をキャンセルする
                backgroundWorker.CancelAsync();
            }

            // ダイアログを閉じる
            Close();
        }
        #endregion
    }
}
