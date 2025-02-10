using hagaki.StaticClass;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

// ---------------------------------------------
//  クラス名   : Cls_FormColor
//  概要　　　 : ＤＢ操作関係
//  作成日　　 : 2025/02/05
//  作成者　　 : 高橋
//  最終更新日 : 
//  最終更新者 : 
// ---------------------------------------------

// ------------------------------
// SetFormColor: 背景色一括変更
// ------------------------------

// ------------------------------
// private関数
// ------------------------------
// SetGroupBoxColor ：GroupBox内の背景色一括変更
// SetControl       ：Tagプロパティで色を変更するかどうかの判断

namespace hagaki.Class
{
    public class Cls_FormColor
    {
        #region 背景色一括変更
        /// <summary>
        /// 背景色一括変更
        /// </summary>
        /// <param name="frm">フォームオブジェクト</param>
        /// <param name="formColor">背景色値</param>
        /// <param name="foreColor">文字色値</param>
        /// <param name="groupFlg">True→GroupBoxオブジェクトの色変更を反映させる(デフォルト)
        ///                     False→GroupBoxオブジェクトの色変更を反映させない</param>
        /// <param name="labelFlg">True→Labelオブジェクトの色変更を反映させる(デフォルト)
        ///                     False→Labelオブジェクトの色変更を反映させない</param>
        /// <param name="checkFlg">True→CheckBoxオブジェクトの色変更を反映させる(デフォルト)
        ///                     False→CheckBoxオブジェクトの色変更を反映させない</param>
        /// <param name="radioFlg">True→RadioButtonオブジェクトの色変更を反映させる(デフォルト)
        ///                     False→RadioButtonオブジェクトの色変更を反映させない</param>
        /// <param name="tabFlg">True→TabControlオブジェクトの色変更を反映させる(デフォルト)
        ///                     False→TabControlオブジェクトの色変更を反映させない</param>
        /// <param name="pictureFlg">True→PictureBoxボックスオブジェクトの色変更を反映させる(デフォルト)
        ///                     False→PictureBoxボックスオブジェクトの色変更を反映させない</param>
        /// <param name="splitFlg">True→SplitContainerオブジェクトの色変更を反映させる(デフォルト)
        ///                     False→SplitContainerオブジェクトの色変更を反映させない</param>
        /// <param name="panelFlg">True→Panelオブジェクトの色変更を反映させる(デフォルト)
        ///                     False→Panelオブジェクトの色変更を反映させない</param>
        /// <remarks>各オブジェクトのTagプロパティに1をセットすると、一括変更の対象外になるので、
        ///          ピンポイントでの色変更が可能</remarks>
        public void SetFormColor(ref Form frm,
                                  int formColor,
                                  int foreColor = 0,
                                  bool groupFlg = true,
                                  bool labelFlg = true,
                                  bool checkFlg = true,
                                  bool radioFlg = true,
                                  bool tabFlg = true,
                                  bool pictureFlg = true,
                                  bool splitFlg = true,
                                  bool panelFlg = true)
        {
            // 文字色設定（追加の関数）
            SetColorType(frm, foreColor);

            // フォームの背景色を設定
            frm.BackColor = ColorTranslator.FromWin32(formColor);

            // フォーム内の全てのコントロールに対して背景色を設定
            System.Collections.Generic.List<Control> controls = StCls_Function.GetAllControls(frm).ToList();  // 全コントロールを取得

            // for ループを使用して ref を使えるようにする
            for (int i = 0; i < controls.Count; i++)
            {
                Control ctrl = controls[i];

                if (ctrl is GroupBox && groupFlg)
                {
                    GroupBox groupBox = (GroupBox)ctrl;

                    if (SetControl(ref ctrl))
                    {
                        ctrl.BackColor = ColorTranslator.FromWin32(formColor);
                    }

                    // GroupBox内のコントロールも対象として色変更
                    SetGroupBoxColor(ref groupBox, formColor, groupFlg, labelFlg, checkFlg, radioFlg, tabFlg, pictureFlg);
                }
                else if (ctrl is Label && labelFlg)
                {
                    if (SetControl(ref ctrl))
                    {
                        ctrl.BackColor = ColorTranslator.FromWin32(formColor);
                    }
                }
                else if (ctrl is CheckBox && checkFlg)
                {
                    if (SetControl(ref ctrl))
                    {
                        ctrl.BackColor = ColorTranslator.FromWin32(formColor);
                    }
                }
                else if (ctrl is RadioButton && radioFlg)
                {
                    if (SetControl(ref ctrl))
                    {
                        ctrl.BackColor = ColorTranslator.FromWin32(formColor);
                    }
                }
                else if (ctrl is TabControl && tabFlg)
                {
                    if (SetControl(ref ctrl))
                    {
                        ctrl.BackColor = ColorTranslator.FromWin32(formColor);
                    }
                }
                else if (ctrl is PictureBox && pictureFlg)
                {
                    if (SetControl(ref ctrl))
                    {
                        ctrl.BackColor = ColorTranslator.FromWin32(formColor);
                    }
                }
                else if (ctrl is TabPage && tabFlg)
                {
                    if (SetControl(ref ctrl))
                    {
                        ctrl.BackColor = ColorTranslator.FromWin32(formColor);
                    }
                }
                else if (ctrl is Panel && panelFlg)
                {
                    if (SetControl(ref ctrl))
                    {
                        ctrl.BackColor = ColorTranslator.FromWin32(formColor);
                    }
                }
            }
        }
        #endregion

        #region GroupBox内の背景色一括変更
        /// <summary>
        /// GroupBox内の背景色一括変更
        /// </summary>
        /// <param name="grp">GroupBoxオブジェクト</param>
        /// <param name="formColor">色値</param>
        /// <param name="groupFlg">True→GroupBoxオブジェクトの色変更を反映させる</param>
        /// <param name="labelFlg">True→Labelオブジェクトの色変更を反映させる</param>
        /// <param name="checkFlg">True→CheckBoxオブジェクトの色変更を反映させる</param>
        /// <param name="radioFlg">True→RadioButtonオブジェクトの色変更を反映させる</param>
        /// <param name="tabFlg">True→TabControlオブジェクトの色変更を反映させる</param>
        /// <param name="pictureFlg">True→PictureBoxオブジェクトの色変更を反映させる</param>
        /// <remarks>各オブジェクトのTagプロパティに1をセットすると、一括変更の対象外になるので、
        ///          ピンポイントでの色変更が可能</remarks>
        public void SetGroupBoxColor(ref GroupBox grp,
                                     int formColor,
                                     bool groupFlg,
                                     bool labelFlg,
                                     bool checkFlg,
                                     bool radioFlg,
                                     bool tabFlg,
                                     bool pictureFlg)
        {
            // grp.Controls をリストに変換して、インデックスでアクセス
            for (int i = 0; i < grp.Controls.Count; i++)
            {
                Control ctrl = grp.Controls[i];  // インデックスを使ってコントロールを取得

                if (ctrl is GroupBox && groupFlg)
                {
                    // GroupBox 型にキャスト
                    GroupBox groupBox = (GroupBox)ctrl;  // GroupBox 型にキャスト

                    // SetControl を呼び出すときに ref を渡す
                    if (SetControl(ref ctrl))  // ref で渡す
                    {
                        groupBox.BackColor = ColorTranslator.FromWin32(formColor);
                    }

                    // GroupBox 内の GroupBox 内の背景色変更
                    SetGroupBoxColor(ref groupBox, formColor, groupFlg, labelFlg, checkFlg, radioFlg, tabFlg, pictureFlg);
                }
                else if (ctrl is Label && labelFlg)
                {
                    if (SetControl(ref ctrl))  // ref ではなく通常の渡し
                    {
                        ctrl.BackColor = ColorTranslator.FromWin32(formColor);
                    }
                }
                else if (ctrl is CheckBox && checkFlg)
                {
                    if (SetControl(ref ctrl))  // ref ではなく通常の渡し
                    {
                        ctrl.BackColor = ColorTranslator.FromWin32(formColor);
                    }
                }
                else if (ctrl is RadioButton && radioFlg)
                {
                    if (SetControl(ref ctrl))  // ref ではなく通常の渡し
                    {
                        ctrl.BackColor = ColorTranslator.FromWin32(formColor);
                    }
                }
                else if (ctrl is TabControl && tabFlg)
                {
                    if (SetControl(ref ctrl))  // ref ではなく通常の渡し
                    {
                        ctrl.BackColor = ColorTranslator.FromWin32(formColor);
                    }
                }
                else if (ctrl is PictureBox && pictureFlg)
                {
                    if (SetControl(ref ctrl))  // ref ではなく通常の渡し
                    {
                        ctrl.BackColor = ColorTranslator.FromWin32(formColor);
                    }
                }
            }
        }
        #endregion

        #region Tagプロパティで色変更するかどうかの判断
        /// <summary>
        /// Tagプロパティで色変更するかどうかの判断
        /// </summary>
        /// <param name="ctrl">コントロールオブジェクト</param>
        /// <returns>色変更を行う場合は true、行わない場合は false</returns>
        public bool SetControl(ref Control ctrl)
        {
            switch (ctrl.Tag as string)
            {
                case "0":
                    return true;
                case "1":
                    return false;
                default:
                    return true;
            }
        }
        #endregion

        #region 背景／文字色の追加設定
        /// <summary>
        /// 背景色および文字色の設定
        /// </summary>
        /// <param name="ctrl">コントロールオブジェクト</param>
        /// <param name="foreColor">文字色（整数型で色指定）</param>
        public void SetColorType(Control ctrl, int foreColor)
        {
            foreach (Control ctrlItem in ctrl.Controls)
            {
                if (ctrlItem is Button)
                {
                    // ボタンの背景色上書き（上書きしてるだけなので個別に背景色を変えていても問題ない）
                    ctrlItem.BackColor = ctrlItem.BackColor;

                    // 文字色は設定しないコメント部分をそのまま保留
                    // もし、文字色を設定したい場合は以下のコードを使用
                    // ctrlItem.ForeColor = ColorTranslator.FromWin32(foreColor);
                }

                // 階層構造コントロールの再帰処理
                if (ctrlItem.Controls.Count > 0)
                {
                    SetColorType(ctrlItem, foreColor);
                }
            }
        }
        #endregion
    }
}
