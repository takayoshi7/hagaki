using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

// ---------------------------------------------
//  クラス名   : StCls_File
//  概要　　　 : ファイル操作関係
//  作成日　　 : 2025/02/05
//  作成者　　 : 高橋
//  最終更新日 : 
//  最終更新者 : 
// ---------------------------------------------

namespace hagaki.StaticClass
{
    public static class StCls_File
    {
        #region API
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern int ShellExecute(
        IntPtr hwnd,
        string lpOperation,
        string lpFile,
        string lpParameters,
        string lpDirectory,
        int nShowCmd);
        #endregion

        // **************************************************
        // 　フォルダ関係
        // **************************************************

        #region フォルダ存在検査
        public static bool FF_IsFolder(string strPath)
        {
            if (Directory.Exists(strPath))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region フォルダ作成
        public static bool FF_CreateFolder(string strPath)
        {
            try
            {
                Directory.CreateDirectory(strPath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region フォルダコピー
        /// <summary>
        /// フォルダ（ディレクトリにあるすべてのファイル）をコピーする
        /// </summary>
        /// <param name="strPath_From">コピー元フォルダパス</param>
        /// <param name="strPath_To">コピー先フォルダパス</param>
        /// <param name="bOverwrite">上書き指定（True：上書き、False：ファイルが存在する場合は上書きしない）</param>
        /// <returns>成功 True / 失敗 False</returns>
        public static bool FF_CopyFolder(string strPath_From, string strPath_To, bool bOverwrite = false)
        {
            try
            {
                // コピー先のディレクトリがなければ作成する
                if (!FF_IsFolder(strPath_To))
                {
                    FF_CreateFolder(strPath_To);
                    File.SetAttributes(strPath_To, File.GetAttributes(strPath_From));
                    bOverwrite = true;
                }

                // コピー元のディレクトリにあるすべてのファイルをコピーする
                if (bOverwrite)
                {
                    foreach (string stCopyFrom in Directory.GetFiles(strPath_From))
                    {
                        string stCopyTo = Path.Combine(strPath_To, Path.GetFileName(stCopyFrom));
                        FF_CopyFile(stCopyFrom, stCopyTo, true);
                    }
                }
                else
                {
                    // 上書き不可能な場合は存在しない時のみコピーする
                    foreach (string stCopyFrom in Directory.GetFiles(strPath_From))
                    {
                        string stCopyTo = Path.Combine(strPath_To, Path.GetFileName(stCopyFrom));

                        if (!FF_IsFile(stCopyTo))
                        {
                            FF_CopyFile(stCopyFrom, stCopyTo, false);
                        }
                    }
                }

                // コピー元のディレクトリをすべてコピーする (再帰)
                foreach (string stCopyFrom in Directory.GetDirectories(strPath_From))
                {
                    string stCopyTo = Path.Combine(strPath_To, Path.GetFileName(stCopyFrom));
                    FF_CopyFolder(stCopyFrom, stCopyTo, bOverwrite);
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
        #endregion

        #region フォルダ移動
        /// <summary>
        /// フォルダを移動する
        /// </summary>
        /// <param name="strPath_From">移動元フォルダパス</param>
        /// <param name="strPath_To">移動先フォルダパス</param>
        /// <returns>成功 True / 失敗 False</returns>
        public static bool FF_MoveFolder(string strPath_From, string strPath_To)
        {
            try
            {
                Directory.Move(strPath_From, strPath_To);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region フォルダ削除
        /// <summary>
        /// フォルダを削除する（サブフォルダも含む）
        /// </summary>
        /// <param name="strPath">削除するフォルダパス</param>
        /// <returns>成功 True / 失敗 False</returns>
        public static bool FF_DeleteFolder(string strPath)
        {
            try
            {
                Directory.Delete(strPath, true); // 第二引数trueでサブフォルダ、ファイルも削除
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region フォルダパスの取得
        /// <summary>
        /// 指定したパスからディレクトリ名を取得する
        /// </summary>
        /// <param name="strPath">パス</param>
        /// <returns>ディレクトリ名</returns>
        public static string FF_GetDirectoryName(string strPath)
        {
            try
            {
                // 指定したファイルが存在しなくても例外はスローされない
                return Path.GetDirectoryName(strPath);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        #endregion

        #region 読込専用フォルダ判定
        /// <summary>
        /// フォルダの属性から読み取り専用か判断する
        /// </summary>
        /// <param name="strPath">フォルダパス</param>
        /// <returns>True: 読み取り専用, False: 読み取り専用以外</returns>
        public static bool FF_CheckReadOnlyFolder(string strPath)
        {
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(Path.GetDirectoryName(strPath));

                // 読み取り専用属性が設定されているか確認
                if ((dirInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    return true;
                }
            }
            catch (Exception)
            {
                // 例外が発生した場合は読み取り専用ではないとみなす
                return false;
            }

            return false;
        }
        #endregion

        #region フォルダ指定ダイアログ
        /// <summary>
        /// フォルダを指定するダイアログを表示する
        /// </summary>
        /// <param name="strDescription">表示する説明文</param>
        /// <returns>選択したフォルダのフルパス（失敗時は空文字列）</returns>
        public static string FF_FolderDialog(string strDescription = "")
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                // 上部に表示する説明テキストを指定する
                if (string.IsNullOrEmpty(strDescription))
                {
                    fbd.Description = "フォルダを指定してください。";
                }
                else
                {
                    fbd.Description = strDescription;
                }

                // ルートフォルダを指定する（デフォルトでDesktop）
                fbd.RootFolder = Environment.SpecialFolder.Desktop;

                // 最初に選択するフォルダを指定する（RootFolder以下でなければならない）
                fbd.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                // ダイアログを表示し、戻り値が [OK] の場合は選択したフォルダを返す
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    return fbd.SelectedPath;
                }
                else
                {
                    return string.Empty;
                }
            }
        }
        #endregion

        #region 指定されたパスのウインドウを開く
        /// <summary>
        /// 指定されたパスのウインドウを開く
        /// </summary>
        /// <param name="path">対象となるパス</param>
        /// <returns>正常に開けた場合はTrue、異常またはフォルダが無い場合はFalse</returns>
        /// <remarks>フォルダが存在しない場合、メッセージボックスが表示される</remarks>
        public static bool WindowOpen(string path)
        {
            try
            {
                // フォルダ存在検査
                if (!Directory.Exists(path))
                {
                    return false;
                }

                // フォルダを開く
                Process.Start("rundll32.exe", $"url.dll,FileProtocolHandler {path}");
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
        #endregion


        // **************************************************
        // 　ファイル関係
        // **************************************************

        #region ファイル存在検査
        /// <summary>
        /// ファイルが存在するかを確認する
        /// </summary>
        /// <param name="path">ファイルのパス</param>
        /// <returns>ファイルが存在する場合はTrue、それ以外はFalse</returns>
        public static bool FF_IsFile(string path)
        {
            return File.Exists(path);
        }
        #endregion

        #region ファイルコピー
        /// <summary>
        /// ファイルをコピーする
        /// </summary>
        /// <param name="fromPath">コピー元ファイルのパス</param>
        /// <param name="toPath">コピー先ファイルのパス</param>
        /// <param name="overwrite">上書きオプション（Trueの場合、既存ファイルを上書き）</param>
        /// <returns>成功した場合はTrue、それ以外はFalse</returns>
        public static bool FF_CopyFile(string fromPath, string toPath, bool overwrite = false)
        {
            try
            {
                // コピー先ファイルがすでに存在する場合は上書きするかどうかのオプションを指定
                File.Copy(fromPath, toPath, overwrite);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
        #endregion

        #region ファイル移動
        /// <summary>
        /// ファイルを移動する
        /// </summary>
        /// <param name="fromPath">移動元ファイルのパス</param>
        /// <param name="toPath">移動先ファイルのパス</param>
        /// <returns>成功した場合はTrue、それ以外はFalse</returns>
        public static bool FF_MoveFile(string fromPath, string toPath)
        {
            try
            {
                // ファイルを移動する
                File.Move(fromPath, toPath);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
        #endregion

        #region ファイル削除
        /// <summary>
        /// ファイルを削除する
        /// </summary>
        /// <param name="path">削除するファイルのパス</param>
        /// <returns>成功した場合はTrue、それ以外はFalse</returns>
        public static bool FF_DeleteFile(string path)
        {
            try
            {
                // ファイルを削除する
                File.Delete(path);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
        #endregion

        #region 拡張子取得
        /// <summary>
        /// 指定したパスの拡張子を取得する
        /// </summary>
        /// <param name="path">ファイルパス</param>
        /// <returns>拡張子 (例: .txt), エラー時は空文字列</returns>
        public static string FF_GetExtension(string path)
        {
            try
            {
                // 指定したパスの拡張子を取得する
                return Path.GetExtension(path);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        #endregion

        #region ファイル名（拡張子あり）の取得
        /// <summary>
        /// 指定したパスのファイル名（拡張子あり）を取得する
        /// </summary>
        /// <param name="path">ファイルまたはフォルダのパス</param>
        /// <returns>ファイル名（拡張子あり）、エラー時は空文字列</returns>
        public static string FF_GetFileName(string path)
        {
            try
            {
                // 指定したパスのファイル名（拡張子あり）を取得する
                return Path.GetFileName(path);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        #endregion

        #region ファイル名（拡張子なし）の取得
        /// <summary>
        /// 指定したパスのファイル名（拡張子なし）を取得する
        /// </summary>
        /// <param name="path">ファイルパス</param>
        /// <returns>ファイル名（拡張子なし）、エラー時は空文字列</returns>
        public static string FF_GetFileNameWithoutExtension(string path)
        {
            try
            {
                // 指定したパスのファイル名（拡張子なし）を取得する
                return Path.GetFileNameWithoutExtension(path);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        #endregion

        #region ファイル名に使用できない文字を含んでいるかチェックする
        /// <summary>
        /// ファイル名に使用できない文字を含んでいるかチェックする
        /// </summary>
        /// <param name="path">ファイルパス</param>
        /// <returns>使用できない文字が含まれている場合は False、それ以外は True</returns>
        public static bool FF_GetInvalidFileNameChars(string path)
        {
            try
            {
                string fileName = FF_GetFileName(path);
                if (string.IsNullOrEmpty(fileName))
                {
                    return false;
                }

                char[] invalidFileChars = Path.GetInvalidFileNameChars();

                // 使用できない文字がファイル名に含まれているかチェックする
                foreach (char invalidChar in invalidFileChars)
                {
                    if (fileName.IndexOf(invalidChar) > -1)
                    {
                        return false; // 使用できない文字が見つかった
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true; // 使用できない文字は含まれていない
        }
        #endregion

        #region 読込専用ファイル判定
        /// <summary>
        /// ファイルの属性から読み取り専用か判断する
        /// </summary>
        /// <param name="path">ファイルのパス</param>
        /// <returns>読み取り専用の場合は true、そうでない場合は false</returns>
        public static bool FF_CheckReadOnlyFile(string path)
        {
            try
            {
                // ファイルの属性を取得
                FileAttributes fileAttr = File.GetAttributes(path);

                // 読み取り専用属性が設定されているか確認
                if ((fileAttr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    return true; // 読み取り専用
                }

                return false; // 読み取り専用以外
            }
            catch (Exception)
            {
                return false; // エラーが発生した場合は false を返す
            }
        }
        #endregion

        #region ファイル指定ダイアログ
        /// <summary>
        /// ファイルを指定するダイアログを表示する
        /// </summary>
        /// <param name="title">ダイアログのタイトル</param>
        /// <param name="initialDirectory">初期表示されるフォルダ</param>
        /// <param name="initialFileName">初期表示するファイル名</param>
        /// <param name="filter">ファイルの種類フィルター</param>
        /// <param name="filterIndex">ファイルの種類インデックス（複数指定している場合）</param>
        /// <returns>選択したファイルのフルパス（失敗した場合は空文字列）</returns>
        public static string FF_FileDialog(string title, string initialDirectory, string initialFileName, string filter, int filterIndex = 1)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                // タイトルを設定
                if (string.IsNullOrEmpty(title))
                {
                    ofd.Title = "ファイルを選択してください";
                }
                else
                {
                    ofd.Title = title;
                }

                // 初期表示されるフォルダを指定
                if (string.IsNullOrEmpty(initialDirectory))
                {
                    ofd.InitialDirectory = "C:\\";
                }
                else
                {
                    ofd.InitialDirectory = initialDirectory;
                }

                // 初期ファイル名を設定
                ofd.FileName = initialFileName;

                // フィルタ設定
                ofd.Filter = filter;

                // 初期フィルタインデックス設定
                ofd.FilterIndex = filterIndex;

                // ダイアログを表示
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    return ofd.FileName; // 選択したファイルのフルパスを返す
                }
                else
                {
                    return string.Empty; // キャンセルまたは閉じた場合は空文字列を返す
                }
            }
        }
        #endregion

        #region テキストファイルの行数を取得
        /// <summary>
        /// テキストファイルの行数を取得
        /// </summary>
        /// <param name="filePath">ファイルのパス</param>
        /// <returns>行数 (Long)</returns>
        public static long GetTextLine(string filePath)
        {
            long lineCount = 0;

            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    while (reader.Peek() >= 0)
                    {
                        reader.ReadLine();
                        lineCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                // エラーハンドリング（必要に応じて）
                Console.WriteLine($"Error: {ex.Message}");
                return -1;  // エラー発生時に -1 を返す（または適切なエラーハンドリングを実施）
            }

            return lineCount;
        }
        #endregion

        #region テキストファイルに行を追加する
        /// <summary>
        /// テキストデータの末尾にラインデータを出力する
        /// </summary>
        /// <param name="outPath">出力先ファイルパス</param>
        /// <param name="strLineData">出力文字列</param>
        /// <returns>成功時に true、失敗時に false</returns>
        public static bool SetTextLine(string outPath, string strLineData)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(outPath, true)) // true: 追記モード
                {
                    // 書き込み（改行付き）
                    sw.WriteLine(strLineData);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region テキストファイルに行を追加する（Shift-JIS）
        /// <summary>
        /// テキストデータの末尾にラインデータを出力する（Shift-JIS）
        /// </summary>
        /// <param name="outPath">出力先ファイルパス</param>
        /// <param name="strLineData">出力文字列</param>
        /// <param name="append">追記するかどうか。Trueで追記、Falseで上書き（初期値: True）</param>
        /// <returns>成功時に true、失敗時に false</returns>
        public static bool SetTextLineSJIS(string outPath, string strLineData, bool append = true)
        {
            try
            {
                // Shift-JISエンコーディングでStreamWriterを使用
                using (StreamWriter sw = new StreamWriter(outPath, append, Encoding.GetEncoding("shift_jis")))
                {
                    // 書き込み（改行付き）
                    sw.WriteLine(strLineData);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion
    }
}
