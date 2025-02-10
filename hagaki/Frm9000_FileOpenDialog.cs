using hagaki.StaticClass;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

// ***************************************************
// * 機能名:    ファイル参照ダイアログ
// ***************************************************

namespace hagaki
{

    public partial class Frm9000_FileOpenDialog : Form
    {
        // ★★★　呼び出し元との連携用変数　★★★
        // 　「決定イベント」で値セットされます。
        // 　呼び出し元はこの変数から値取得してください。
        // 
        // 　ファイル選択：　選択したファイルパスを返す
        // 　フォルダ選択：　選択したフォルダパスを返す
        // 　※MultiSelect時は、複数選択したパスをタブ区切りで返す。
        public string ResultPath = string.Empty;

        // ダイアログタイプ列挙体
        public enum DialogType
        {
            Folder = 0,  // フォルダ選択ダイアログ
            File         // ファイル選択ダイアログ
        }

        // 拡張子フィルタ列挙体（フィルタ設定を追加したい場合は２のべき乗で定数を追加）
        public enum ExtensionFilter
        {
            All = 1,     // 全て
            Text = 2,    // テキスト
            Csv = 4,     // CSV
            Excel = 8,   // エクセル
            Tsv = 16,    // テキスト（tsv）

            Job = 32,    // ジョブ（job）
            Tpl = 64     // テンプレート（tpl）
        }

        public Frm9000_FileOpenDialog()
        {
            InitializeComponent();
        }

        #region プライベートメンバ

        // ファイルアイコンのキャッシュ key: ファイル拡張子, value: Icon型
        // ※サーバー表示ではアイコン取得が低速。アイコンは一度のみ取得して使いまわす。
        private static Dictionary<string, Icon> _iconCache = new Dictionary<string, Icon>();

        // 機能設定パラメータ
        private static DialogType _dialogType;             // 選択ダイアログ
        private static string _effectiveDir;               // 有効パス
        private static string _initDir;                    // ディレクトリパス
        private static ExtensionFilter _filterExtension;   // フィルター値
        private static bool _multiSelect;                  // マルチ選択値

        // コンボ用　拡張子フィルタ
        private const string EXT_ALL = "*.*";
        private const string EXT_TXT = "*.txt";
        private const string EXT_CSV = "*.csv";
        private const string EXT_XLS = "*.xls";
        private const string EXT_TSV = "*.tsv";
        private const string EXT_JOB = "*.job";
        private const string EXT_TPL = "*.tpl";

        // リストビュー項目
        private enum emLst_Col
        {
            FileName = 0,      // フォルダ・ファイル名称
            FolderOrFile,      // フォルダかファイルか
            FullName           // フルパス
        }

        #endregion

        #region API：SHGetFileInfo関係

        // SHGetFileInfo関数の宣言
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SHGetFileInfo(
            string pszPath,
            int dwFileAttributes,
            ref SHFILEINFO psfi,
            int cbFileInfo,
            int uFlags);

        // SHGetFileInfo関数で使用するフラグ
        public const int SHGFI_ICON = 0x100;         // アイコン・リソースの取得
        public const int SHGFI_LARGEICON = 0x0;      // 大きいアイコン
        public const int SHGFI_SMALLICON = 0x1;      // 小さいアイコン（フォルダの場合はこっちの方が良い）

        // SHGetFileInfo関数で使用する構造体
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SHFILEINFO
        {
            public IntPtr hIcon;
            public IntPtr iIcon;
            public int dwAttributes;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        #endregion

        // **************************************************
        // 　Form呼び出し用関数
        // **************************************************

        #region  =　+ Function：　ファイル参照ダイアログを開く
        /// <summary>
        /// ファイル参照ダイアログを開く
        /// </summary>
        /// <param name="effectiveDir">有効ディレクトリパス</param>
        /// <param name="initDir">初期フォルダパス</param>
        /// <param name="extensionFilter">拡張子のフィルタ</param>
        /// <returns></returns>
        public static string OpenDialog(string effectiveDir,
                                        string initDir = "",
                                        ExtensionFilter extensionFilter = ExtensionFilter.All)
        {
            string result = string.Empty;

            try
            {
                // フォームをモーダルで開く
                using (Frm9000_FileOpenDialog frm = new Frm9000_FileOpenDialog(effectiveDir, DialogType.File, initDir, extensionFilter, false))
                {
                    // フォームが破棄されていなければダイアログを表示
                    if (!frm.IsDisposed)
                    {
                        frm.ShowDialog();
                        result = frm.ResultPath; // 結果パスを取得
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex; // 例外処理
            }

            return result;
        }
        #endregion

        #region  =　+ Function：　ファイル参照ダイアログを開く（複数項目選択）
        /// <summary>
        /// ファイル参照ダイアログを開く（複数項目選択）
        /// </summary>
        /// <param name="effectiveDir">有効ディレクトリパス</param>
        /// <param name="initDir">初期フォルダパス</param>
        /// <param name="extensionFilter">拡張子のフィルタ</param>
        /// <returns></returns>
        public static string[] OpenDialogMultiSelect(string effectiveDir,
                                                    string initDir = "",
                                                    ExtensionFilter extensionFilter = ExtensionFilter.All)
        {
            string result = string.Empty;

            try
            {
                // フォームをモーダルで開く
                using (Frm9000_FileOpenDialog frm = new Frm9000_FileOpenDialog(effectiveDir, DialogType.File, initDir, extensionFilter, true))
                {
                    // フォームが破棄されていなければダイアログを表示
                    if (!frm.IsDisposed)
                    {
                        frm.ShowDialog();
                        result = frm.ResultPath;  // 結果パスを取得
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex; // 例外処理
            }

            // タブ区切りで分割して返す
            return result.Split(new[] { '\t' }, StringSplitOptions.None);
        }
        #endregion

        #region  =　+ Function：　フォルダ参照ダイアログを開く
        /// <summary>
        /// フォルダ参照ダイアログを開く
        /// </summary>
        /// <param name="effectiveDir">有効ディレクトリパス</param>
        /// <param name="initDir">初期フォルダパス</param>
        /// <param name="extensionFilter">拡張子のフィルタ</param>
        /// <returns></returns>
        public static string OpenDialogFolder(string effectiveDir,
                                             string initDir = "",
                                             ExtensionFilter extensionFilter = ExtensionFilter.All)
        {
            string result = string.Empty;

            try
            {
                // フォルダ参照ダイアログを開く
                using (Frm9000_FileOpenDialog frm = new Frm9000_FileOpenDialog(effectiveDir, DialogType.Folder, initDir, extensionFilter, false))
                {
                    // フォームが破棄されていなければダイアログを表示
                    if (!frm.IsDisposed)
                    {
                        frm.ShowDialog();
                        result = frm.ResultPath;  // 選択したフォルダのパスを取得
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex; // 例外を再スロー
            }

            return result;
        }
        #endregion

        // **************************************************
        // 　Form
        // **************************************************

        #region 　　-　Constructor：New
        public Frm9000_FileOpenDialog(string effectiveDir,
                                      DialogType dialogType,
                                      string initDir = "",
                                      ExtensionFilter extensionFilter = ExtensionFilter.All,
                                      bool multiSelect = false)
        {
            InitializeComponent();

            try
            {
                // プライベートメンバの初期化
                _effectiveDir = effectiveDir;
                _dialogType = dialogType;
                _initDir = initDir;
                _filterExtension = extensionFilter;
                _multiSelect = multiSelect;

                if (!_effectiveDir.EndsWith("\\"))
                    _effectiveDir += "\\";
                if (!_initDir.EndsWith("\\"))
                    _initDir += "\\";

                // 不正なパスが指定された場合
                if (!Directory.Exists(_effectiveDir))
                    throw new Exception("指定された有効ディレクトリは存在しないかアクセスできません！");
                if (!Directory.Exists(_initDir))
                    throw new Exception("指定された初期選択フォルダは存在しないかアクセスできません！");

                // 初期フォルダが有効ディレクトリ外
                if (!_initDir.Contains(_effectiveDir))
                    throw new Exception("指定された初期選択フォルダは、有効ディレクトリの範囲外です！");

                // アイコンキャッシュの初期化
                _iconCache = new Dictionary<string, Icon>();

                // 戻り値初期化
                ResultPath = "";
            }
            catch (Exception ex)
            {
                ErrMsgHelper(ex);
                Close();
            }
        }
        #endregion

        #region  - Sub: Form_Load
        private void Frm9000_FileOpenDialog_Load(object sender, EventArgs e)
        {
            try
            {
                // --------------------------------------------------------
                // 基本設定
                // --------------------------------------------------------

                // デザイナで設定済み（デザイナで設定しておかないとうまく表示されなかった為）
                // フォルダ表示ツリービューとファイル表示リストビューにイメージリストを設定
                // TVw_Folder.ImageList = Img_TVw_Folder;
                // LVw_File.SmallImageList = Img_LVw_File;

                // フォームレイアウト
                // ファイル選択：全コントロール表示
                // フォルダ選択：ファイル一覧は非表示
                ChangeLayout(_dialogType);

                if (_dialogType == DialogType.File)
                {
                    Text = "ファイル選択ダイアログ";
                    LVw_File.Columns[(int)emLst_Col.FileName].Text = "ファイル名";
                }
                else
                {
                    Text = "フォルダ選択ダイアログ";
                    LVw_File.Columns[(int)emLst_Col.FileName].Text = "フォルダ名";

                    // フォルダ選択ダイアログの場合のみコンテキストメニュー表示
                    TVw_Folder.ContextMenuStrip = ContextMenuStrip1;
                }

                // --------------------------------------------------------
                // フォルダアイコン設定
                // --------------------------------------------------------

                string iconPath = @"C:\Windows";   // OSに関係なくまずあるはず？？
                Icon folderIcon = null;

                if (Directory.Exists(iconPath))
                {
                    folderIcon = GetIcon_ByShInfo(iconPath);

                    // 取得成功：ImageListを置き換える
                    // 取得失敗：ImageListをそのまま使用（アイコンがちょっとダサい）
                    if (folderIcon != null)
                    {
                        TVw_Folder.ImageList.Images.Clear();
                        TVw_Folder.ImageList.Images.Add(folderIcon);
                        TVw_Folder.SelectedImageKey = "";
                    }
                }

                // --------------------------------------------------------
                // 表示ファイル制御
                // --------------------------------------------------------

                // コンボには常にひとつだけセットされる！！
                switch (_filterExtension)
                {
                    case ExtensionFilter.All:
                        Cmb_Filter.Items.Add("全てのファイル(" + EXT_ALL + ")");
                        break;

                    case ExtensionFilter.Text:
                        Cmb_Filter.Items.Add("テキストファイル(" + EXT_TXT + ")");
                        break;

                    case ExtensionFilter.Csv:
                        Cmb_Filter.Items.Add("CSVファイル(" + EXT_CSV + ")");
                        break;

                    case ExtensionFilter.Excel:
                        Cmb_Filter.Items.Add("EXCELファイル(" + EXT_XLS + ")");
                        break;

                    case ExtensionFilter.Tsv:
                        Cmb_Filter.Items.Add("テキストファイル(" + EXT_TSV + ")");
                        break;

                    case ExtensionFilter.Job:
                        Cmb_Filter.Items.Add("ジョブファイル(" + EXT_JOB + ")");
                        break;

                    case ExtensionFilter.Tpl:
                        Cmb_Filter.Items.Add("テンプレートファイル(" + EXT_TPL + ")");
                        break;

                    default:
                        throw new Exception("不正な拡張子が指定されました。管理者にご確認ください。");
                }

                Cmb_Filter.SelectedIndex = 0;

                // --------------------------------------------------------
                // ツリービュー・リストビュー初期設定
                // --------------------------------------------------------

                // 列の非表示
                LVw_File.Columns[(int)emLst_Col.FolderOrFile].Width = 0;
                LVw_File.Columns[(int)emLst_Col.FullName].Width = 0;

                // マルチセレクト設定
                LVw_File.MultiSelect = _multiSelect;

                Init_TreeView(_effectiveDir, _initDir);
            }
            catch (Exception ex)
            {
                ErrMsgHelper(ex);
                Close();
            }
        }
        #endregion

        #region  - Sub: フォームレイアウト変更（ファイル選択/フォルダ選択）
        private void ChangeLayout(DialogType dialogType)
        {
            if (dialogType == Frm9000_FileOpenDialog.DialogType.File)
            {
                // 【ファイル選択】
                Size = new Size(795, Size.Height);

                TVw_Folder.Size = new Size(336, TVw_Folder.Size.Height);
                Btn_OK.Location = new Point(587, Btn_OK.Location.Y);
                Btn_Cancel.Location = new Point(690, Btn_Cancel.Location.Y);

                LVw_File.TabStop = true;
                Cmb_Filter.TabStop = true;
            }
            else
            {
                // 【フォルダ選択】
                Size = new Size(359, Size.Height);

                TVw_Folder.Size = new Size(324, TVw_Folder.Size.Height);
                Btn_OK.Location = new Point(150, Btn_OK.Location.Y);
                Btn_Cancel.Location = new Point(253, Btn_Cancel.Location.Y);

                // Tab移動防止
                LVw_File.TabStop = false;
                Cmb_Filter.TabStop = false;
            }
        }
        #endregion

        #region  フォーム右上の閉じるボタンを無効化
        protected override CreateParams CreateParams
        {
            get
            {
                const int CS_NOCLOSE = 0x200;

                CreateParams createParams1 = base.CreateParams;
                createParams1.ClassStyle |= CS_NOCLOSE;

                return createParams1;
            }
        }
        #endregion

        // **************************************************
        // 　決定イベント
        // **************************************************

        #region  - Sub: Button_Click OK
        private void Btn_OK_Click(object sender, EventArgs e)
        {
            List<string> fileCollection = new List<string>();
            string currentPath = string.Empty;

            try
            {
                if (_dialogType == DialogType.File)
                {
                    // 【ファイル選択】 右リストビューから取得
                    foreach (ListViewItem item in LVw_File.SelectedItems)
                    {
                        currentPath = item.SubItems[(int)emLst_Col.FullName].Text;

                        // 有効ディレクトリ検査
                        if (!Chk_IsEffectiveDir(currentPath)) return;

                        // 有効時のみ格納
                        fileCollection.Add(currentPath);
                    }
                }
                else
                {
                    // 【フォルダ選択】 左ツリービューから取得
                    if (TVw_Folder.SelectedNode != null)
                    {
                        currentPath = ((DirectoryInfo)TVw_Folder.SelectedNode.Tag).FullName + @"\";

                        fileCollection.Add(currentPath);
                    }
                }

                // 選択されているパスを返す（複数選択時はタブ区切り）
                ResultPath = string.Join("\t", fileCollection.ToArray());
                Close();
            }
            catch (Exception ex)
            {
                ErrMsgHelper(ex);
            }
        }
        #endregion

        #region  - Sub: Button_Click キャンセル
        private void Btn_Cancel_Click(object sender, EventArgs e)
        {
            try
            {
                // ブランクを返す
                ResultPath = string.Empty;
                Close();
            }
            catch (Exception ex)
            {
                ErrMsgHelper(ex);
            }
        }
        #endregion

        #region  - Sub: ContextMenuStrip_Click ツリービュー右クリック（フォルダ選択時のみ）
        private void ContextMenuStrip1_Click(object sender, EventArgs e)
        {
            // フォルダ選択：イベント発生します
            // ファイル選択：フォルダツリー右クリック自体できません

            string headDirectory = string.Empty;

            try
            {
                if (TVw_Folder.SelectedNode == null) return;

                // SelectedNode.FullPathとの重複箇所を削る
                // 例.
                // \\10.10.168.30\開発・インフラ\ → \\10.10.168.30\
                headDirectory = _effectiveDir.TrimEnd('\\');
                headDirectory = headDirectory.Substring(0, headDirectory.LastIndexOf('\\') + 1);

                // 選択されているパスを返す
                ResultPath = headDirectory + TVw_Folder.SelectedNode.FullPath;
                Close();
            }
            catch (Exception ex)
            {
                ErrMsgHelper(ex);
            }
        }
        #endregion

        #region  - Sub: ListView_DoubleClick ファイル一覧ダブルクリック
        private void LVw_File_DoubleClick(object sender, EventArgs e)
        {
            string selectedPath = string.Empty;

            try
            {
                selectedPath = LVw_File.SelectedItems[0].SubItems[(int)emLst_Col.FullName].Text;

                // 有効ディレクトリ検査
                if (!Chk_IsEffectiveDir(selectedPath)) return;

                // 選択されているパスを返す
                ResultPath = selectedPath;
                Close();
            }
            catch (Exception ex)
            {
                ErrMsgHelper(ex);
            }
        }
        #endregion

        // **************************************************
        // 　通常イベント
        // **************************************************

        #region  - Sub: Button_Click ルート選択
        private void Btn_RootSelect_Click(object sender, EventArgs e)
        {
            string rootDirectoryPath = string.Empty;

            try
            {
                rootDirectoryPath = StCls_File.FF_FolderDialog();

                // Me.LVw_File.Items.Clear() // 必要に応じて追加

                // 有効ディレクトリ検査
                if (!Chk_IsEffectiveDir(rootDirectoryPath)) return;

                // 戻り値がブランク以外ならフルパスセット＆各コントロール初期化
                if (!string.IsNullOrEmpty(rootDirectoryPath))
                {
                    // ツリービュー・リストビュー初期設定（ルートを選択）
                    rootDirectoryPath += @"\";
                    Init_TreeView(rootDirectoryPath, rootDirectoryPath);

                    // リストにファイルを表示
                    TVw_Folder.SelectedNode = TVw_Folder.Nodes[0];
                    Show_Files(TVw_Folder.SelectedNode);
                }
            }
            catch (Exception ex)
            {
                ErrMsgHelper(ex);
            }
        }
        #endregion

        #region  - Sub: TreeView_NodeMouseDoubleClick ノードを展開します
        // ADDBY 2014/07/01 matsuda
        private void TVw_Folder_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            try
            {
                // 左リスト：選択フォルダに含まれる子フォルダを表示
                // 右リスト：選択フォルダに含まれるファイルを表示

                if (!e.Node.IsExpanded)
                {
                    // 選択フォルダをアクティブにしたいため、選択フォルダと同階層のフォルダはすべて閉じる
                    if (e.Node.Parent != null)
                    {
                        foreach (TreeNode childNode in e.Node.Parent.Nodes)
                        {
                            childNode.Collapse();
                        }
                    }

                    // 選択フォルダの中身を表示
                    if (e.Node.Nodes == null || e.Node.Nodes.Count == 0)
                    {
                        // フォルダの表示
                        Add_Nodes_BySubDirectory(e.Node);

                        // ファイルの表示
                        Show_Files(e.Node);
                    }
                }

                // リスト表示の都合、選択フォルダは常に展開しておく
                e.Node.Expand();
            }
            catch (Exception ex)
            {
                ErrMsgHelper(ex);
            }
        }
        #endregion

        #region  - Sub: TreeView_BeforeSelect ノード展開前　子ノードを追加します
        // ADDBY 2014/07/01 matsuda
        private void TreeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            try
            {
                // 選択フォルダの中身を表示
                if (e.Node.Nodes == null || e.Node.Nodes.Count == 0)
                {
                    // フォルダの表示
                    Add_Nodes_BySubDirectory(e.Node);

                    // ファイルの表示
                    Show_Files(e.Node);
                }
            }
            catch (Exception ex)
            {
                ErrMsgHelper(ex);
            }
        }
        #endregion

        #region  - Sub: TreeView_AfterCollapse ノード縮小後　子ノードを削除します
        // ADDBY 2014/07/01 matsuda
        private void TVw_Folder_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            try
            {
                // 子ノードをすべて削除　（消さないと残ったまま）
                e.Node.Nodes.Clear();
            }
            catch (Exception ex)
            {
                ErrMsgHelper(ex);
            }
        }
        #endregion

        // **************************************************
        // 　関数
        // **************************************************

        #region  - Sub: ErrorMessage
        private void ErrMsgHelper(Exception ex)
        {
            if (ex is UnauthorizedAccessException)
            {
                // アクセスエラー
                MessageBox.Show("選択されたフォルダにはアクセスできません。", "アクセスエラー", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                // 上記以外
                MessageBox.Show("参照ダイアログの表示に失敗しました！\n\n内容：\n" + ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region  - Sub: TreeView 初期設定 / エントリ
        /** 
         * TreeView 初期設定 / 初期フォルダ選択
         * @param effectiveDirectoryPath 有効フォルダパス（ルート）
         * @param initDirectoryPath 初期フォルダパス（ここを展開した状態にする）
         */
        private void Init_TreeView(string effectiveDirectoryPath, string initDirectoryPath)
        {
            TreeNode rootNode = null;
            DirectoryInfo rootDi = null;
            DirectoryInfo[] baseDiList = null;

            DirectoryInfo initDi = null;
            TreeNode initNode = null;

            try
            {
                // 完成したツリー
                //   C（開始）
                //    |- Hoge\
                //         |- Foo\（初期フォルダ）
                //         |- aaa\（初期フォルダと同階は表示）
                //         |- bbb\（初期フォルダと同階は表示）

                TVw_Folder.Nodes.Clear();
                TVw_Folder.ShowPlusMinus = false;

                // GUARD：不正なディレクトリ
                if (!Directory.Exists(effectiveDirectoryPath)) return;
                if (!Directory.Exists(initDirectoryPath)) return;

                // ---------------------------------------------------------
                // ツリーの作成
                // ---------------------------------------------------------

                // ディレクトリ情報
                rootDi = new DirectoryInfo(effectiveDirectoryPath);
                initDi = new DirectoryInfo(initDirectoryPath);

                // ここを基準として以下処理する
                rootNode = Create_Node_ByDirectoryInfo(rootDi);

                // 初期フォルダまでの階層を配列取得
                baseDiList = Get_Directories_RootToInit(rootDi, initDi);

                // ツリーに表示
                Add_Nodes_ByDirectories(rootNode, baseDiList, 0);
                TVw_Folder.Nodes.Add(rootNode);

                // ---------------------------------------------------------
                // 初期フォルダ表示
                // ---------------------------------------------------------

                // ツリーから初期フォルダを取得
                initNode = Find_MatchedNode_ByDirectory(rootNode, initDi);

                LVw_File.Items.Clear();

                if (initNode != null)
                {
                    // ノード選択
                    TVw_Folder.SelectedNode = initNode;

                    // 初期フォルダに含まれるノードを表示
                    // ex.
                    //   C(0)
                    //    |-Hoge(1)
                    //       |-Foo(2 = LAST)
                    //           |-Test1 ←
                    //           |-Test2 ←
                    //           |-Test3 ←
                    Add_Nodes_BySubDirectory(initNode);

                    // リストにファイルを表示　これがやりたい！！
                    Show_Files(initNode);

                    // 子ノード展開
                    TVw_Folder.SelectedNode.Expand();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region  - Sub: TreeView 指定ディレクトリ階層をツリーに追加します（再帰前提）
        /** 
         * 指定ディレクトリ階層をツリーに追加します（再帰前提）
         * @param rootNode 追加元となるノード
         * @param baseDirectories ツリーに追加するディレクトリ配列
         * @param baseDirectoryIndex 配列の現在位置（初回コール時=0/再帰毎+=1）
         */
        private void Add_Nodes_ByDirectories(TreeNode rootNode, DirectoryInfo[] baseDirectories, int baseDirectoryIndex)
        {
            // GUARD
            if (baseDirectories == null) return;
            if (baseDirectories.Length - 1 < baseDirectoryIndex) return;

            DirectoryInfo currentDirectory = null;
            TreeNode currentNode = null;

            // baseDirectories 
            // ex.
            //   ↑親  (0)　c:\　　　　　　　　
            //       　(1)　c:\Hoge\
            //   ↓子  (2)　c:\Hoge\Foo\

            // 現在位置
            currentDirectory = baseDirectories[baseDirectoryIndex];

            if (baseDirectories.Length - 1 == baseDirectoryIndex)
            {
                // 【Index = LAST】

                // 終端です。
                // この場合のみサブディレクトリを全て表示します。

                // ex.
                //   C(0)
                //    |-Hoge(1)
                //       |-Foo(2 = LAST)
                //           |-Test1 ←
                //           |-Test2 ←
                //           |-Test3 ←
                Add_Nodes_BySubDirectory(rootNode);
            }
            else
            {
                // 【Index = 0～】

                // 終端になるまでは、
                // baseDirectoriesの次Indexのフォルダのみを表示します。

                // ex.
                //   C(0)
                //    |-Hoge(1)
                //       |-Foo(2)
                currentNode = Create_Node_ByDirectoryInfo(currentDirectory);

                rootNode.Nodes.Add(currentNode);

                // 再帰処理 (0)→(1)→(2)
                Add_Nodes_ByDirectories(currentNode, baseDirectories, baseDirectoryIndex + 1);
            }
        }
        #endregion

        #region  - Sub: TreeView 指定ディレクトリの子階層をツリーに追加する
        /** 
         * TreeViewに指定ディレクトリの子階層を追加する
         * @param targetNode このTreeNode（DirectoryInfo）の子階層を処理する
         */
        private void Add_Nodes_BySubDirectory(TreeNode targetNode)
        {
            DirectoryInfo targetDirectory = null;
            TreeNode childNode = null;

            try
            {
                // ex.
                //   C\:Hoge\
                //   C\:Hoge\test\   ※孫以降は無視
                //   C\:Foo\
                //   C\:Bar\
                // ↓　
                //   C
                //    |-Hoge
                //    |-Foo
                //    |-Bar

                // Tag = DirectoryInfo
                targetDirectory = targetNode.Tag as DirectoryInfo;

                // GUARD: サブディレクトリがなければ終了
                DirectoryInfo[] childDirectories = targetDirectory.GetDirectories();
                if (childDirectories == null || childDirectories.Length == 0)
                    return;

                // サブディレクトリを処理
                foreach (DirectoryInfo childDirectory in childDirectories)
                {
                    // ディレクトリ情報からノード作成
                    childNode = Create_Node_ByDirectoryInfo(childDirectory);

                    // 既にルートに存在する場合はスキップ
                    if (targetNode.Nodes.IndexOfKey(childNode.Name) > -1)
                        continue;

                    // ルートに追加
                    targetNode.Nodes.Add(childNode);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region  - Sub: ListView 選択フォルダに含まれるファイルを表示する
        /** 
         * ListViewに選択フォルダに含まれるファイルを表示する
         * @param targetNode 処理対象のノード
         */
        private void Show_Files(TreeNode targetNode)
        {
            DirectoryInfo targetDirectory = null;
            ListViewItem item = null;
            ListViewItem.ListViewSubItem[] subItems = null;

            LVw_File.Items.Clear();

            targetDirectory = targetNode.Tag as DirectoryInfo;

            // フォルダ選択かファイル選択か？
            switch (_dialogType)
            {
                case DialogType.Folder:
                    // ---------------------------------------------------------
                    // フォルダ表示ツリービュー フォルダ列挙
                    // ---------------------------------------------------------
                    foreach (DirectoryInfo directory in targetDirectory.GetDirectories())
                    {
                        item = new ListViewItem(directory.Name, 0);
                        subItems = new ListViewItem.ListViewSubItem[]
                        {
                    new ListViewItem.ListViewSubItem(item, "Directory"),
                    new ListViewItem.ListViewSubItem(item, directory.FullName)
                        };

                        item.SubItems.AddRange(subItems);
                        LVw_File.Items.Add(item);
                    }
                    break;

                case DialogType.File:
                    // ---------------------------------------------------------
                    // ファイル表示リストビュー ファイル列挙
                    // ---------------------------------------------------------
                    string searchExt = "";
                    Icon appIcon = null;

                    // コンボで指定された拡張子のみ検索
                    searchExt = GetSearchPattern(Cmb_Filter);

                    foreach (FileInfo file in targetDirectory.GetFiles(searchExt))
                    {
                        // リスト項目を作成
                        item = new ListViewItem(file.Name, 1);
                        subItems = new ListViewItem.ListViewSubItem[]
                        {
                    new ListViewItem.ListViewSubItem(item, "File"),
                    new ListViewItem.ListViewSubItem(item, file.FullName),
                    new ListViewItem.ListViewSubItem(item, file.DirectoryName)
                        };

                        // リストに追加
                        item.SubItems.AddRange(subItems);
                        LVw_File.Items.Add(item);

                        // アイコン取得と設定
                        appIcon = GetIcon(file, _iconCache);

                        // 万が一アイコン取得できなければアイコン表示しません。次ループへ。
                        if (appIcon == null) continue;

                        // イメージリストにアイコンを追加
                        // イメージインデックスを設定しないと全部同じアイコンが使用される
                        LVw_File.Items[LVw_File.Items.Count - 1].ImageIndex = Img_LVw_File.Images.Count;
                        Img_LVw_File.Images.Add(file.Extension, appIcon);
                    }
                    break;
            }

            // 列幅の自動調整
            // LVw_File.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize); // 全ての列
            LVw_File.AutoResizeColumn((int)emLst_Col.FileName, ColumnHeaderAutoResizeStyle.HeaderSize); // 列指定

            // 列の非表示
            LVw_File.Columns[(int)emLst_Col.FolderOrFile].Width = 0;
            LVw_File.Columns[(int)emLst_Col.FullName].Width = 0;
        }
        #endregion

        #region  - Function: DirectoryInfo() 初期選択位置→ルートまでのディレクトリ階層を取得します
        /** 
         * 初期選択位置からルートまでのディレクトリ階層を取得します
         * @param effectiveDirectory ルートディレクトリ
         * @param initDirectory 初期選択ディレクトリ
         * @returns DirectoryInfo配列 初期選択位置→ルートまで
         */
        private DirectoryInfo[] Get_Directories_RootToInit(DirectoryInfo effectiveDirectory, DirectoryInfo initDirectory)
        {
            // GUARD：対象が不正なら終了
            if (effectiveDirectory == null || initDirectory == null)
                return null;

            List<DirectoryInfo> result = new List<DirectoryInfo>();
            DirectoryInfo currentDirectory = null;

            try
            {
                // 以下、昇順でツリー階層を設定します
                // 例. DirectoryInfo(0) c:\
                //     DirectoryInfo(1) c:\Windows\
                //     DirectoryInfo(2) c:\Windows\hoge\ 初期選択ディレクトリ

                // 初期選択ディレクトリ
                // ルートと一致するならチェック終了
                if (Chk_EqualsDirectoryPath(effectiveDirectory.FullName, initDirectory.FullName))
                    return null;

                result.Insert(0, initDirectory);

                // 下記チェックの準備
                currentDirectory = initDirectory;

                // 親→親→親→...
                while (currentDirectory.Parent != null)
                {
                    currentDirectory = currentDirectory.Parent;

                    // ルートと一致するなら、以降調べる必要なし
                    if (Chk_EqualsDirectoryPath(effectiveDirectory.FullName, currentDirectory.FullName))
                        break;

                    result.Insert(0, currentDirectory);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result.ToArray();
        }
        #endregion

        #region  - Function: TreeNode 子ノードを検索し、指定ディレクトリと一致するノードを取得します
        /** 
         * 子ノードを検索し、指定ディレクトリと一致するノードを取得します
         * @param rootNode 検索元ノード
         * @param searchDirectory 検索するディレクトリ情報
         * @returns 一致あり：TreeNode　一致なし：null
         */
        private TreeNode Find_MatchedNode_ByDirectory(TreeNode rootNode, DirectoryInfo searchDirectory)
        {
            TreeNode result = null;
            TreeNode[] matchedNodes = null;

            // ノードの絞込み
            // TreeNode.Name値を元に、一致するすべてのノードを取得します（一致なければLength=0）
            matchedNodes = rootNode.Nodes.Find(searchDirectory.Name, true);

            // ディレクトリパスが完全一致するノードを取得
            foreach (TreeNode node in matchedNodes)
            {
                if (Chk_EqualsDirectoryPath(((DirectoryInfo)node.Tag).FullName, searchDirectory.FullName))
                {
                    result = node;
                    break;
                }
            }

            return result;
        }
        #endregion

        #region  - Function: TreeNode DirectoryInfoからノードを作成します
        /** 
         * DirectoryInfoからノードを作成します
         * @param di ノードの元となるDirectoryInfo
         * @returns TreeNode
         */
        private TreeNode Create_Node_ByDirectoryInfo(DirectoryInfo di)
        {
            TreeNode node = new TreeNode();

            // Nodes.Find用にNameを設定
            node.Name = di.Name;
            node.Text = di.Name;

            // 他処理でノードからDirectoryInfoを参照できるようTagにセット
            node.Tag = di;

            node.ImageKey = "folder";

            return node;
        }
        #endregion

        #region  - Function: ComboBox 選択項目からファイルSearchPatternを取得します
        /** 
         * ComboBoxの選択項目からファイルの検索パターンを取得します
         * @param cmd ComboBox コントロール
         * @returns 検索パターン（拡張子）
         */
        private string GetSearchPattern(ComboBox cmd)
        {
            // コンボ項目は常に1つです。いずれの拡張子でフィルタするか識別します。
            string selectedText = cmd.Items[0].ToString();

            if (selectedText.Contains(EXT_ALL))
            {
                return EXT_ALL;
            }
            else if (selectedText.Contains(EXT_TXT))
            {
                return EXT_TXT;
            }
            else if (selectedText.Contains(EXT_CSV))
            {
                return EXT_CSV;
            }
            else if (selectedText.Contains(EXT_XLS))
            {
                return EXT_XLS;
            }
            else if (selectedText.Contains(EXT_TSV))
            {
                return EXT_TSV;
            }
            else if (selectedText.Contains(EXT_JOB))
            {
                return EXT_JOB;
            }
            else if (selectedText.Contains(EXT_TPL))
            {
                return EXT_TPL;
            }
            else
            {
                throw new Exception("不正な拡張子が指定されました。管理者にご確認ください。");
            }
        }
        #endregion

        #region  - Function: Icon FileInfoから対応するアイコンを取得（Me._iconCacheも参照）
        /** 
         * FileInfoから対応するアイコンを取得し、キャッシュも参照します。
         * @param file FileInfo ファイル情報
         * @param iconCache アイコンキャッシュ
         * @returns アイコン
         */
        private Icon GetIcon(FileInfo file, Dictionary<string, Icon> iconCache)
        {
            Icon result = null;
            Uri uriAdd = new Uri(file.FullName);

            // アイコンキャッシュがnullであれば新しく作成
            if (iconCache == null)
            {
                iconCache = new Dictionary<string, Icon>();
            }

            // アイコンの取得
            if (iconCache.ContainsKey(file.Extension))
            {
                // キャッシュ参照
                result = iconCache[file.Extension];
            }
            else
            {
                if (uriAdd.IsUnc)
                {
                    // UNCパスの場合
                    result = GetIcon_ByShInfo(file.FullName);
                }
                else
                {
                    // ローカルフォルダ（こちらが速い）
                    result = Icon.ExtractAssociatedIcon(file.FullName);
                }

                // キャッシュに登録
                if (result != null)
                {
                    iconCache.Add(file.Extension, result);
                }
            }

            return result;
        }

        /** 
         * SHFILEINFOを使用してUNCパスからアイコンを取得
         * @param iconPath アイコンのパス
         * @returns アイコン
         */
        private Icon GetIcon_ByShInfo(string iconPath)
        {
            Icon result = null;

            // SHFILEINFOを使用してフォルダアイコンを取得
            SHFILEINFO shInfo = new SHFILEINFO();
            IntPtr hSuccess = SHGetFileInfo(iconPath, 0, ref shInfo, Marshal.SizeOf(shInfo), SHGFI_ICON | SHGFI_SMALLICON);

            if (!hSuccess.Equals(IntPtr.Zero))
            {
                result = Icon.FromHandle(shInfo.hIcon);
            }

            return result;
        }
        #endregion

        #region  - Function: Check 2つのディレクトリパスが一致するか調べます
        /** 
         * 2つのディレクトリパスが一致するか調べます
         * @param directoryPath1 チェック対象1
         * @param directoryPath2 チェック対象2
         * @returns 一致する：TRUE 一致しない：FALSE
         */
        private bool Chk_EqualsDirectoryPath(string directoryPath1, string directoryPath2)
        {
            string tempPath1 = directoryPath1.ToLower();
            string tempPath2 = directoryPath2.ToLower();

            // DirectoryInfo.Nameではパス末尾に \ がないため付与する
            if (!tempPath1.EndsWith("\\"))
            {
                tempPath1 += "\\";
            }
            if (!tempPath2.EndsWith("\\"))
            {
                tempPath2 += "\\";
            }

            return (tempPath1 == tempPath2);
        }
        #endregion

        #region  - Function: Check パスが有効ディレクトリに含まれるか調べます
        /** 
         * パスが有効ディレクトリに含まれるか調べます
         * @param targetPath 対象パス（ディレクトリ/ファイルどちらでも可）
         * @returns 有効ディレクトリに含まれていればtrue、それ以外はfalse
         */
        private bool Chk_IsEffectiveDir(string targetPath)
        {
            string strPath = targetPath.ToLower();

            // 有効ディレクトリパスが含まれているかどうか
            if (strPath.IndexOf(_effectiveDir.ToLower()) == -1)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("有効ディレクトリ外のフォルダが選択されました。");
                sb.AppendLine("ルートを変更できません。");
                sb.AppendLine();
                sb.AppendLine("有効ディレクトリ：　" + _effectiveDir);

                MessageBox.Show(sb.ToString(), "確認", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }

            return true;
        }
        #endregion
    }
}
