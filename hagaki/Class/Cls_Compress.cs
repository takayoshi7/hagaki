using hagaki.StaticClass;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

// ==================================================================================================
// クラス名       ：Cls_Compress
// 概要         　：ファイル及びフォルダの圧縮
// 作成日         ：2025/02/05
// 作成者         ：高橋
// 
// ※※※※※※※※※※※※※※※※※※※※※※※※※※
// ※このクラスを使用する上での注意点
// ※
// ※ZIP圧縮の場合
// ※・「zip32.dll」ファイルと「ZIP32J.DLL」ファイルを
// ※　実行ファイルと同じ階層に保存してください。
// ※　参照の必要はありません。
// ※
// ※CAB圧縮の場合
// ※・「cab32.dll」ファイルを
// ※　実行ファイルと同じ階層に保存してください。
// ※　参照の必要はありません。
// ※・ZIPと違い一度作成したCABファイルにファイルを
// ※　追加することはできません。
// ※
// ※LZH圧縮の場合
// ※・「UNLHA32.DLL」ファイルを
// ※　実行ファイルと同じ階層に保存してください。
// ※　参照の必要はありません。
// ※※※※※※※※※※※※※※※※※※※※※※※※※※
// ==================================================================================================

namespace hagaki.Class
{
    internal class Cls_Compress
    {
        // Zip関数の宣言
        [DllImport("Zip32j", CharSet = CharSet.Auto)]
        public static extern int Zip(int hWnd, string szCmdLine, string szOutPut, int dwsize);

        // Cab関数の宣言
        [DllImport("cab32", CharSet = CharSet.Auto)]
        public static extern int Cab(int hWnd, string szCmdLine, string szOutPut, int dwsize);

        // Unlha関数の宣言
        [DllImport("unlha32", CharSet = CharSet.Auto)]
        public static extern int Unlha(int hWnd, string szCmdLine, string szOutPut, int dwsize);

        // 下記詳細説明はZIP圧縮です。コマンド以外はLZH圧縮、CAB圧縮ともにZIPと同じです。
        // ##############################################################################################
        // 概要      :指定ファイルを圧縮処理します。
        // 
        // 引数
        //          hWnd：ZIP32J.DLL を呼び出すアプリのウィンドウ・ハンドル。
        //                ZIP32J.DLL は実行時にこのウィンドウに対して EnableWin-
        //                dow() を実行しウィンドウの動作を抑制します。ウィンドウ
        //                が存在しないコンソールアプリの場合や，指定する必要のな
        //                い場合は NULL を渡します。
        // 
        //     szCmdLine：ZIP32J.DLL に渡すコマンド文字列。
        //    
        //            　例： [-<options>...] <archive_file_name>[.zip] [<directory_name>\] [<filespec>...]
        //    
        //                さしあたって
        //    
        //                     -r <archive file> <source file> ...
        //     
        //                   イメージとしてはこのような形
        //                     "-r C:\test\zip_test.zip C:\test\ZipFolder001"
        //                     "-r C:\test\zip_test.zip C:\test\ZipFile001.txt"
        //    
        //                でディレクトリ付きで再帰的に圧縮できます。
        //                他のオプションはあまりテストしていません。
        //               「@FILENAME」によるレスポンスファイルの利用もできます。 
        // 
        //              【主に使用するコマンド】
        //               -r:再帰的検索。   -j:ファイル名のパスを保存しない。  -q:処理を静かにする。
        //               ※このコマンド以外に必要になった場合、各自で確認をお願いします。
        // 
        //      szOutPut：ZIP32J.DLL が結果を返すためのバッファ。グローバルメモリー
        //    	        等の場合はロックされている必要があります。
        // 
        //        dwSize：バッファのサイズ。結果が指定サイズを越える場合は、この
        //    	        サイズに切り詰められます。
        //    	        結果がこのサイズより小さい場合は、最後に NULL 文字が付
        //    	        加されます。（最低１文字のみが保証される）
        //    	        バッファのサイズいっぱいの場合等、NULL 文字がどこにも
        //    	        ない可能性がある点に留意のこと。
        // 
        // 戻り値    :正常終了の時		    0
        //            エラーが発生した場合	0 以外の数
        // ##############################################################################################

        // CAB圧縮　制限事項
        // =====================
        // 
        //   (1) コマンドラインパラメタ数
        //       256個のコマンドラインでのパラメタ数を最大限許します。これを超えるパラメタ
        //       を指定するには、レスポンスファイルを利用してください。レスポンスファイル
        //       を利用すれば、DLLが扱えるメモリ許容量までパラメタを指定できます。
        //       １つの書庫に格納できるファイル数には制限はありません。
        // 
        //   (2) 作成可能なキャビネットサイズ
        //       作成できるキャビネットの最大サイズは、(2ギガ - 1)バイトまでです。
        //       圧縮して格納されたサイズがこのサイズを超えると、マルチボリューム指定され
        //       ていなくても自動的にマルチボリューム形式に変更されます。
        // 
        //   (3) 空のフォルダの格納
        //       キャビネットファイルの仕様では、空のフォルダを格納できません。
        // 
        // 　■レスポンスファイル注意事項
        // 　・１行に複数のファイルを納める場合にはスペース区切りで記入
        // 　・パス名自体に空白が含まれる場合には""（ダブルクォーテーション）で必ず括らなくてはなりません
        // 　・１行に記述できるファイルのリストは、文字列長で1024バイト未満、ファイル数で128個まで
        // 　・行数に制限はなし

        #region メソッド

        #region ZIP圧縮

        #region "１つのファイルを圧縮"
        /// <summary>
        /// 指定した１つのファイル／フォルダをZipファイルに圧縮する
        /// </summary>
        /// <param name="zipPath">"圧縮後のファイル名(絶対パス).zip"</param>
        /// <param name="file">圧縮するファイルパス</param>
        /// <param name="pwd">パスワード</param>
        /// <remarks>1つのZipファイルのみ作成する</remarks>
        /// <returns>戻り値は空文字("")が正常。それ以外は文言を返す。</returns>
        public string CreateOneFileZip(string zipPath, string file, string pwd = "")
        {
            try
            {
                string parentDir = "";  //指定ファイルの親ディレクトリ
                string currentDir = Directory.GetCurrentDirectory();   //元のカレントディレクトリ
                string cmdPws = "";      // ZIPコマンドライン(work)

                if (!string.IsNullOrEmpty(pwd))
                {
                    cmdPws = " -P " + pwd;
                }

                //ファイル存在チェック（上書き確認）
                if (StCls_File.FF_IsFile(zipPath))
                {
                    if (MessageBox.Show(zipPath + Environment.NewLine + "この書庫は既に存在します。上書きしますか？", "ファイル操作", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                    {
                        if (!StCls_File.FF_DeleteFile(zipPath))
                        {
                            return "既存ファイルの削除に失敗しました。";
                        }
                    }
                    else
                    {
                        return "処理を中断しました。";
                    }
                }

                //同名ファイルなし　正常
                if (!StCls_File.FF_CreateFolder(Path.GetDirectoryName(zipPath)))
                {
                    return "出力先フォルダ作成に失敗しました。";
                }

                //末尾のバックスラッシュを削除
                file = file.TrimEnd('\\');

                //指定ファイルの親ディレクトリの絶対パスを文字列で取得
                parentDir = Path.GetDirectoryName(file);

                //カレントディレクトリを指定ファイルの親ディレクトリに設定
                Directory.SetCurrentDirectory(parentDir);

                //指定ファイルパスを絶対パスから相対パスに変換
                file = file.Replace(parentDir, ".");
                //パスをダブルクォーテーションで囲む
                file = "\"" + file + "\"";

                //圧縮処理の戻り値が0以外の場合例外処理
                if (Zip(0, "-r -q" + cmdPws + " \"" + zipPath + "\" " + file, "", 0) != 0)
                {
                    return "圧縮に失敗しました。";
                }

                //カレントディレクトリを元に戻す
                Directory.SetCurrentDirectory(currentDir);

                return "";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region "複数ファイルを圧縮"
        /// <summary>
        /// 指定した複数のファイル／フォルダをZipファイルに圧縮する
        /// </summary>
        /// <param name="zipPath">"圧縮後のファイル名(絶対パス).zip"</param>
        /// <param name="files">圧縮するファイルパスの配列</param>
        /// <param name="pwd">パスワード</param>
        /// <remarks>複数のファイル／フォルダを指定した場合でも、1つのZipファイルのみ作成する</remarks>
        /// <returns>戻り値は空文字("")が正常。それ以外は文言を返す。</returns>
        public string CreateMultiFileZip(string zipPath, string[] files, string pwd = "")
        {
            try
            {
                string parentDir = "";  //指定ファイルの親ディレクトリ
                string pressFiles = ""; //指定ファイルパスをスペースで連結
                string currentDir = Directory.GetCurrentDirectory();   //元のカレントディレクトリ
                string cmdPws = "";      // ZIPコマンドライン(work)

                if (!string.IsNullOrEmpty(pwd))
                {
                    cmdPws = " -P " + pwd;
                }

                //ファイル存在チェック（上書き確認）
                if (StCls_File.FF_IsFile(zipPath))
                {
                    if (MessageBox.Show(zipPath + Environment.NewLine + "この書庫は既に存在します。上書きしますか？", "ファイル操作", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                    {
                        if (!StCls_File.FF_DeleteFile(zipPath))
                        {
                            return "既存ファイルの削除に失敗しました。";
                        }
                    }
                    else
                    {
                        return "処理を中断しました。";
                    }
                }

                //同名ファイルなし　正常
                if (!StCls_File.FF_CreateFolder(Path.GetDirectoryName(zipPath)))
                {
                    return "出力先フォルダ作成に失敗しました。";
                }

                foreach (string file in files)
                {
                    string currentFile = file.TrimEnd('\\');

                    //指定ファイルの親ディレクトリの絶対パスを文字列で取得
                    parentDir = Path.GetDirectoryName(currentFile);

                    //カレントディレクトリを指定ファイルの親ディレクトリに設定
                    Directory.SetCurrentDirectory(parentDir);

                    //指定ファイルパスを絶対パスから相対パスに変換
                    currentFile = currentFile.Replace(parentDir, ".");
                    //全ての指定ファイルをpressFilesにスペース区切りで格納
                    pressFiles = pressFiles + "\"" + currentFile + "\" ";
                }

                //圧縮処理の戻り値が0以外の場合例外処理
                if (Zip(0, "-r -q" + cmdPws + " \"" + zipPath + "\" " + pressFiles, "", 0) != 0)
                {
                    return "圧縮に失敗しました。";
                }

                //カレントディレクトリを元に戻す
                Directory.SetCurrentDirectory(currentDir);

                return "";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #endregion

        #region "CAB圧縮"
        /// <summary>
        /// 指定した複数のファイル／フォルダをCabファイルに圧縮する
        /// </summary>
        /// <param name="cabPath">"圧縮後のファイル名(絶対パス).cab"</param>
        /// <param name="files">圧縮するファイルパスの配列</param>
        /// <remarks>複数のファイル／フォルダを指定した場合でも、1つのCabファイルのみ作成する</remarks>
        /// <returns>戻り値は空文字("")が正常。それ以外はエラー文言を返す。</returns>
        public string CreateMultiFileCab(string cabPath, string[] files)
        {
            string resFile = ""; // レスポンスファイル
            string currentDir = Directory.GetCurrentDirectory(); // 元のカレントディレクトリ

            try
            {
                string parentDir = ""; // 指定ファイルの親ディレクトリ
                string pressFiles = ""; // 指定ファイルパスをスペースで連結
                int cnt = 0;
                DateTime startTime = DateTime.Now; // Get_ServerTime()に相当

                // ファイル存在チェック（上書き確認）
                if (StCls_File.FF_IsFile(cabPath))
                {
                    if (MessageBox.Show(cabPath + Environment.NewLine + "この書庫は既に存在します。上書きしますか？", "ファイル操作", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                    {
                        if (!StCls_File.FF_DeleteFile(cabPath))
                        {
                            return "既存ファイルの削除に失敗しました。";
                        }
                    }
                    else
                    {
                        return "処理を中断しました。";
                    }
                }

                // 同名ファイルなし　正常
                if (!StCls_File.FF_CreateFolder(Path.GetDirectoryName(cabPath)))
                {
                    return "出力先フォルダ作成に失敗しました。";
                }

                foreach (string file in files)
                {
                    cnt += 1;

                    string currentFile = file.TrimEnd('\\');

                    // 指定ファイルの親ディレクトリの絶対パスを文字列で取得
                    parentDir = Path.GetDirectoryName(currentFile);

                    if (StCls_File.FF_IsFolder(file))
                    {
                        // フォルダ内の全てのファイルを指定
                        currentFile = file + "\\*.*";
                    }
                    else
                    {
                        if (!StCls_File.FF_IsFile(file))
                        {
                            return "パスが存在しません。";
                        }
                    }

                    // カレントディレクトリを指定ファイルの親ディレクトリに設定
                    Directory.SetCurrentDirectory(parentDir);

                    // 指定ファイルパスを絶対パスから相対パスに変換
                    currentFile = currentFile.Replace(parentDir, ".");
                    // 全ての指定ファイルをpressFilesにスペース区切りで格納
                    pressFiles += "\"" + currentFile + "\" ";

                    // 改行を挿入
                    pressFiles += Environment.NewLine;
                }

                // レスポンスファイルはアプリケーションのフォルダに作成
                // ファイル名が被らないようにファイル名にPIDを使用
                resFile = Path.Combine(Application.StartupPath, StCls_Public.pb_User.Login + "_" + Process.GetCurrentProcess().Id.ToString() + "_rp.lst");

                // 排他制御
                // ※ファイル名が被った場合で同時に以下のWhile文を実行されると排他は行われません。
                // あくまで時間差で来た場合の補助です。
                // ※3件以上呼び出された場合の順番は保障されていません。
                while (StCls_File.FF_IsFile(resFile))
                {
                    System.Threading.Thread.Sleep(10000);

                    // １時間(60分)待機
                    if ((DateTime.Now - startTime).TotalMinutes > 60)
                    {
                        return "排他待機時間を超過したため処理を中断しました。";
                    }
                }

                // レスポンスファイル作成
                File.WriteAllText(resFile, pressFiles, System.Text.Encoding.GetEncoding("shift_jis"));

                // 圧縮処理の戻り値が0以外の場合例外処理
                if (Cab(0, "-a -r -mz \"" + cabPath + "\" \"" + "@" + resFile + "\"", "", 0) != 0)
                {
                    throw new Exception("圧縮に失敗しました。");
                }

                // レスポンスファイル削除
                if (!StCls_File.FF_DeleteFile(resFile))
                {
                    throw new Exception("レスポンスファイル削除失敗");
                }

                // カレントディレクトリを元に戻す
                Directory.SetCurrentDirectory(currentDir);

                return "";
            }
            catch (Exception ex)
            {
                if (StCls_File.FF_IsFile(resFile))
                {
                    // レスポンスファイル削除
                    StCls_File.FF_DeleteFile(resFile);
                }

                // カレントディレクトリを元に戻す
                Directory.SetCurrentDirectory(currentDir);

                throw ex;
            }
        }
        #endregion

        #region "LZH圧縮"
        /// <summary>
        /// 指定した複数のファイル／フォルダをLzhファイルに圧縮する
        /// </summary>
        /// <param name="lzhPath">"圧縮後のファイル名(絶対パス).lzh"</param>
        /// <param name="files">圧縮するファイルパスの配列</param>
        /// <remarks>複数のファイル／フォルダを指定した場合でも、1つのlzhファイルのみ作成する</remarks>
        /// <returns>戻り値は空文字("")が正常。それ以外はエラー文言を返す。</returns>
        public string CreateMultiFileLzh(string lzhPath, string[] files)
        {
            try
            {
                string parentDir = ""; // 指定ファイルの親ディレクトリ
                string pressFiles = ""; // 指定ファイルパスをスペースで連結
                string currentDir = Directory.GetCurrentDirectory(); // 元のカレントディレクトリ

                // ファイル存在チェック（上書き確認）
                if (StCls_File.FF_IsFile(lzhPath))
                {
                    if (MessageBox.Show(lzhPath + Environment.NewLine + "この書庫は既に存在します。上書きしますか？", "ファイル操作", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                    {
                        if (!StCls_File.FF_DeleteFile(lzhPath))
                        {
                            return "既存ファイルの削除に失敗しました。";
                        }
                    }
                    else
                    {
                        return "処理を中断しました。";
                    }
                }

                // 同名ファイルなし　正常
                if (!StCls_File.FF_CreateFolder(Path.GetDirectoryName(lzhPath)))
                {
                    return "出力先フォルダ作成に失敗しました。";
                }

                foreach (string file in files)
                {
                    string currentFile = file.TrimEnd('\\');

                    // 指定ファイルの親ディレクトリの絶対パスを文字列で取得
                    parentDir = Path.GetDirectoryName(currentFile);

                    // カレントディレクトリを指定ファイルの親ディレクトリに設定
                    Directory.SetCurrentDirectory(parentDir);

                    // 指定ファイルパスを絶対パスから相対パスに変換
                    currentFile = currentFile.Replace(parentDir, ".");
                    // 全ての指定ファイルをpressFilesにスペース区切りで格納
                    pressFiles += "\"" + currentFile + "\" ";
                }

                // 圧縮処理の戻り値が0以外の場合例外処理
                if (Unlha(0, "a -q \"" + lzhPath + "\" " + pressFiles, "", 0) != 0)
                {
                    return "圧縮に失敗しました。";
                }

                // カレントディレクトリを元に戻す
                Directory.SetCurrentDirectory(currentDir);

                return "";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        public string GetDirectoryMemberSplit(string fileName, ref string makeDir, ref string makeFile)
        {
            int z = 0;            // 圧縮ファイル名の文字操作に利用
            int p = 0;            // 圧縮ファイル名の文字操作に利用
            string fn = "";       // 圧縮ファイルのあるパス名
            string fl = "";       // 圧縮ファイル名

            // 初期化
            makeDir = "";
            makeFile = "";
            fn = "";
            fl = "";
            p = 0;
            z = 0;

            // 末尾の "\" を除去
            if (fileName.EndsWith("\\"))
            {
                fileName = fileName.Substring(0, fileName.Length - 1);
            }

            // "\" の位置を取得し、位置を変数に格納
            while (fileName[fileName.Length - z - 1] != '\\' && fileName.Length > z)
            {
                z++;
                if (fileName[fileName.Length - z - 1] == '.' && p == 0)
                {
                    // "." の位置を取得し、位置を変数に格納
                    p = z + 1;
                }
            }

            // フォルダ名を取得
            fl = fileName.Substring(0, fileName.Length - z);
            // ファイル名を取得
            fn = fileName.Substring(fileName.Length - z);

            // 引数と格納バッファに格納
            makeFile = fn;

            makeDir = fn.Substring(0, fn.Length - p);
            return fl;
        }

        #endregion
    }
}
