using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

// ---------------------------------------------
//  クラス名   : Cls_Log
//  概要       : ログ書き込みクラス
//  作成日　　 : 2025/02/05
//  作成者　　 : 高橋
//  最終更新日 : 
//  最終更新者 : 
// ---------------------------------------------

namespace hagaki.Class
{
    internal class Cls_Log
    {
        /// <summary>ログ書込みの閾値</summary>
        /// <remarks></remarks>
        public enum LogLv
        {
            Low = 0,
            Mid,
            High
        }

        #region  + class: 抽象クラス（ログクラスの基本機能）
        public abstract class LogWriter
        {
            private const string FILE_EXT = ".log";   // ログファイル拡張子の規定値
            private bool _kugiriZumiFlg;              // 初回書込み区切り文字の出力有無
            protected LogLv _writeLogLv;

            // プロパティ代用
            protected List<string> SkipMethodName;    // 無視するトレース名（LogWriter継承先で設定※主にSqlLog）
            public bool UseDebugModeOnly { get; set; } // デバッグ時のみ情報出力するか
            public bool UseOutputLogErr { get; set; }  // ログ書込み失敗時の情報出力するか
            public LogLv WriteTraceLogLv { get; set; } // 実行するかの閾値
            public LogLv WriteParamLogLv { get; set; } // 実行するかの閾値
            public LogLv WriteExLogLv { get; set; }    // 実行するかの閾値
            public string OutputDirectoryPath { get; set; } // ログ出力先パス
            public string OutputFileName { get; set; }     // ログ出力ファイル名
            public string LogErrDirectoryPath { get; set; } // 失敗情報出力先パス
            public string LogErrFileName { get; set; }      // 失敗情報出力ファイル名

            // 継承先で実装するメソッド
            public abstract void WriteTrace(string message = "");
            public abstract void WriteParam(params object[] param);
            public abstract void WriteEx(Exception ex);

            public LogWriter(string directoryPath, string fileName)
            {
                // フラグ
                // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                _kugiriZumiFlg = false;  // 初回書込み区切り文字 = 未出力

                // プロパティ（代用）
                // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                UseDebugModeOnly = false; // デバッグ時のみ情報出力 = 通常時/デバッグ時両方出力する
                UseOutputLogErr = true;   // ログ書込み失敗時の情報出力 = 出力する

                WriteLogLv = LogLv.Low;   // Low以上（すべて）のログを書き込む

                WriteTraceLogLv = LogLv.Low;
                WriteParamLogLv = LogLv.Mid;
                WriteExLogLv = LogLv.High;

                // 無視するトレース名
                // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                SkipMethodName = new List<string>
                {
                    "WriteTrace",
                    "WriteParam",
                    "WriteEx",
                    "OutputFailure"
                };

                // ログ出力先
                // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                string outputDirectoryPath = directoryPath;
                string outputFileName = fileName;

                // フォルダパスの正規化
                if (!outputDirectoryPath.EndsWith("\\"))
                {
                    outputDirectoryPath += "\\";
                }

                // ファイル名の拡張子は削除する
                if (outputFileName.ToLower().EndsWith(FILE_EXT))
                {
                    outputFileName = outputFileName.Substring(0, outputFileName.Length - FILE_EXT.Length);
                }

                // 出力先設定（指定パス以下に年月フォルダ作成）
                // ディレクトリ 例： C:\201412\01\
                // ファイル名 例： log_toppan.taro_JH-IT15-ASK_20141201.log
                OutputDirectoryPath = string.Format("{0}\\{1}\\{2}\\", outputDirectoryPath, DateTime.Now.ToString("yyyyMM"), DateTime.Now.ToString("dd"));
                OutputFileName = string.Format("{0}_{1}_{2}_{3}{4}", outputFileName, Environment.UserName, Environment.MachineName, DateTime.Now.ToString("yyyyMMdd"), FILE_EXT);

                // 失敗情報出力先
                // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
                // 出力先設定（EXE実行パスに出力）
                // ディレクトリ 例： C:\SystemLog\201412\01\
                // ファイル名 例： LogFailure_toppan.taro_JH-IT15-ASK_20141201.log
                LogErrDirectoryPath = string.Format("{0}\\{1}\\{2}\\", Application.StartupPath + "\\SystemLog", DateTime.Now.ToString("yyyyMM"), DateTime.Now.ToString("dd"));
                LogErrFileName = string.Format("{0}_{1}_{2}_{3}{4}", "Failure", Environment.UserName, Environment.MachineName, DateTime.Now.ToString("yyyyMMdd"), FILE_EXT);

                // 失敗情報をログ出力先と同じにしたければ、 呼び出し元で次を設定
                // 　Me.LogErrDirectoryPath = Me.OutputDirectoryPath
                // 　Me.LogErrFileName = Me.OutputFileName
            }

            #region 公開メンバ
            /// <summary>
            /// ログ出力する閾値を取得/設定します。（例. Value = LogLv.Low なら、Low以上のレベルを出力）
            /// </summary>
            public LogLv WriteLogLv
            {
                get { return _writeLogLv; }
                set
                {
                    if (Enum.IsDefined(typeof(LogLv), value))
                    {
                        _writeLogLv = value; // 定義値ならそのまま
                    }
                    else
                    {
                        _writeLogLv = LogLv.Low; // 定義外の値なら Low で設定する
                    }
                }
            }
            #endregion

            #region 非公開メンバ（継承先からの呼び出しのみ）

            /// <summary>
            /// 現モードがログ書込みできるか
            /// </summary>
            protected bool ChkCanWrite(LogLv currentLv)
            {
                bool lvOk = _writeLogLv <= currentLv; // 閾値
                bool modeOk = true; // モード

                if (UseDebugModeOnly)
                {
#if DEBUG
                    modeOk = true;
#else
            modeOk = false;
#endif
                }
                else
                {
                    modeOk = true;
                }

                return lvOk && modeOk;
            }

            /// <summary>
            /// ログを出力します（継承先からの呼び出しのみ）
            /// </summary>
            protected void OutputLog(params string[] message)
            {
                try
                {
                    if (!Directory.Exists(OutputDirectoryPath))
                    {
                        Directory.CreateDirectory(OutputDirectoryPath);
                    }

                    using (StreamWriter sw = new StreamWriter(OutputDirectoryPath + OutputFileName, true))
                    {
                        // 初回書き込み：ログを見やすくするために区切り線を挿入
                        if (!_kugiriZumiFlg)
                        {
                            sw.WriteLine("-----------------");
                            _kugiriZumiFlg = true;
                        }

                        // 書込み（改行除去）
                        sw.WriteLine(string.Join("\t", message).Replace(Environment.NewLine, ""));
                    }
                }
                catch (Exception ex)
                {
                    // ログ書込みエラーは継承先で処理
                    throw ex;
                }
            }

            /// <summary>
            /// ログ書込み失敗時の情報を出力します（継承先からの呼び出しのみ）
            /// </summary>
            /// <param name="ex">失敗原因のException</param>
            /// <param name="t">実行したLogWriter</param>
            protected void OutputLogErr(Exception ex, Type t)
            {
                // GUARD：　出力不要なら終了
                if (!UseOutputLogErr) return;

                try
                {
                    // 例外からトレース情報を取得
                    StackTrace stEx = new StackTrace(ex);  // この二つなくても情報足りてる
                    StackFrame sfEx = GetValidStackFrame(stEx, SkipMethodName);
                    string categoryName = "LogErr";

                    // 出力形式：日時 ユーザー名 端末名 ログ種別 トレース情報 例外情報
                    string[] message = new string[]
                    {
                        DateTime.Now.ToString(),
                        Environment.UserName,
                        Environment.MachineName,
                        categoryName,
                        "Type：" + t.Name,
                        GetExecptionMsg(sfEx, ex)
                    };

                    if (!Directory.Exists(LogErrDirectoryPath))
                    {
                        Directory.CreateDirectory(LogErrDirectoryPath);
                    }

                    using (StreamWriter sw = new StreamWriter(LogErrDirectoryPath + LogErrFileName, true))
                    {
                        // 書込み（改行除去）
                        sw.WriteLine(string.Join("\t", message).Replace(Environment.NewLine, ""));
                    }
                }
                catch (Exception)
                {
                    // エラーは無視
                }
            }

            #endregion
        }
        #endregion

        #region     + class: 汎用ログ
        public class TraceLogWriter : LogWriter
        {
            public TraceLogWriter(string directoryPath, string fileName) : base(directoryPath, fileName)
            {
            }

            /// <summary>
            /// ログにトレース情報を書き込む
            /// </summary>
            /// <param name="message">備考（引数値とかのせるとき使う）</param>
            /// <remarks></remarks>
            public override void WriteTrace(string message = "")
            {
                try
                {
                    if (!ChkCanWrite(WriteTraceLogLv)) return;

                    // トレース情報を取得
                    StackTrace st = new StackTrace();
                    StackFrame sf = GetValidStackFrame(st, SkipMethodName);
                    string categoryName = "Trace";

                    // 出力形式：　日付　ユーザー名　端末名　カテゴリ　トレース情報　備考
                    OutputLog(DateTime.Now.ToString(), Environment.UserName, Environment.MachineName, categoryName, GetTraceMsg(sf), message);
                }
                catch (Exception ex)
                {
                    // 書込み失敗の情報を出力
                    OutputLogErr(ex, GetType());
                }
            }

            /// <summary>
            /// ログにトレース情報＆引数情報を書き込む
            /// </summary>
            /// <param name="param">引数値（WriteParam実行箇所の引数をすべて渡してください）</param>
            /// <remarks></remarks>
            public override void WriteParam(params object[] param)
            {
                try
                {
                    if (!ChkCanWrite(WriteParamLogLv)) return;

                    // トレース情報を取得
                    StackTrace st = new StackTrace();
                    StackFrame sf = GetValidStackFrame(st, SkipMethodName);
                    string categoryName = "Param";

                    // 出力形式：　日付　ユーザー名　端末名　カテゴリ　トレース情報　引数情報
                    OutputLog(DateTime.Now.ToString(), Environment.UserName, Environment.MachineName, categoryName, GetTraceMsg(sf), GetParamMsg(sf, param));
                }
                catch (Exception ex)
                {
                    // 書込み失敗の情報を出力
                    OutputLogErr(ex, GetType());
                }
            }

            /// <summary>
            /// ログにトレース情報＆例外情報を書き込む
            /// </summary>
            /// <param name="ex">発生した例外</param>
            /// <remarks></remarks>
            public override void WriteEx(Exception ex)
            {
                try
                {
                    if (!ChkCanWrite(WriteExLogLv)) return;

                    // トレース情報を取得
                    StackTrace st = new StackTrace();
                    StackFrame sf = GetValidStackFrame(st, SkipMethodName);

                    // 例外からトレース情報を取得
                    StackTrace stEx = new StackTrace(ex);
                    StackFrame sfEx = GetValidStackFrame(stEx, SkipMethodName);
                    string categoryName = "Ex";

                    // 出力形式：　日付　ユーザー名　端末名　カテゴリ　トレース情報　例外情報
                    OutputLog(DateTime.Now.ToString(), Environment.UserName, Environment.MachineName, categoryName, GetTraceMsg(sf), GetExecptionMsg(sfEx, ex));
                }
                catch (Exception ex2)
                {
                    // 書込み失敗の情報を出力
                    OutputLogErr(ex2, GetType());
                }
            }
        }
        #endregion

        #region   + class: SQLログ
        public class SqlLogWriter : TraceLogWriter
        {
            public SqlLogWriter(string directoryPath, string fileName)
                : base(directoryPath, fileName)
            {
                // 例）　次の順番で実行されたなら、呼び出し元は「Upd_D_Main」とログに出力される。
                //　Button_Click > Upd_D_Main > Execute

                // 次のメソッド名は必ず通るためトレースチェック時にスキップします
                // →【Execute】WriteParamでは引数が定義と一致せず、引数値を書き出せないので注意。
                base.SkipMethodName.Add("Execute");

                // 閾値既定：　すべてのログを書き込む
                base.WriteTraceLogLv = LogLv.High;
                base.WriteParamLogLv = LogLv.High;
                base.WriteExLogLv = LogLv.High;
            }

            /// <summary>
            /// ログに処理情報を書き込む（SQL用）
            /// </summary>
            /// <param name="message">備考（引数値とかのせるとき使う）</param>
            public override void WriteTrace(string message = "")
            {
                try
                {
                    if (!ChkCanWrite(WriteTraceLogLv)) return;

                    // トレース情報を取得
                    StackTrace st = new StackTrace();
                    StackFrame sf = GetValidStackFrame(st, SkipMethodName);
                    string categoryName = "Sql";

                    // 出力形式：　日付　ユーザー名　端末名　カテゴリ　トレース情報　備考
                    OutputLog(DateTime.Now.ToString(), Environment.UserName, Environment.MachineName, categoryName, GetTraceMsg(sf), message);
                }
                catch (Exception ex)
                {
                    // 書込み失敗の情報を出力
                    OutputLogErr(ex, GetType());
                }
            }
        }
        #endregion

        #region     + class: ダミーログ
        public class DummyLogWriter : TraceLogWriter
        {
            public DummyLogWriter(string directoryPath, string fileName)
                : base(directoryPath, fileName)
            {
                // 既に各画面にLogWriter実装済みの場合、
                // 型にDummyLogWriterを指定するとコード編集することなくログ出力機能をオフにできる
            }

            public override void WriteTrace(string message = "")
            {
                // ダミーのため書き込まない
                return;
            }

            public override void WriteParam(params object[] param)
            {
                // ダミーのため書き込まない
                return;
            }

            public override void WriteEx(Exception ex)
            {
                // ダミーのため書き込まない
                return;
            }
        }
        #endregion

        #region   = + Function: 生成メソッド
        public static LogWriter CreateLogWriter(string typeValue, string directoryPath, string fileName)
        {
            // typeValue が不正なら汎用ログを使用
            if (typeValue == null) return new TraceLogWriter(directoryPath, fileName);

            LogWriter result = null;

            string[] target = typeValue.Split(',');
            string targetType = "";
            string targetLv = "";
            string targetMode = "";
            LogLv targetLvEnum = LogLv.Low;

            // 初期化値の判定
            switch (target.Length)
            {
                case 0:
                case 1:
                    targetType = typeValue;
                    targetLv = "";
                    targetLv = "";
                    break;

                case 2:
                    targetType = target[0];
                    targetLv = target[1];
                    targetLv = "";
                    break;

                default:
                    targetType = target[0];
                    targetLv = target[1];
                    targetMode = target[2];
                    break;
            }

            // LogWriter の型指定
            switch (targetType.ToLower())
            {
                case "":
                    // 未指定なら汎用ログを使用
                    result = new TraceLogWriter(directoryPath, fileName);
                    break;

                case "0":
                case "trace":
                case "tracelog":
                case "tracelogwriter":
                    result = new TraceLogWriter(directoryPath, fileName);
                    break;

                case "1":
                case "sql":
                case "sqllog":
                case "sqllogwriter":
                    result = new SqlLogWriter(directoryPath, fileName);
                    break;

                default:
                    // 上記以外はダミーを使用
                    result = new DummyLogWriter(directoryPath, fileName);
                    break;
            }

            // ログ書込みの閾値指定
            if (Enum.TryParse(targetLv, true, out targetLvEnum))
            {
                // 定義値
                result.WriteLogLv = targetLvEnum;
            }
            else
            {
                // 未定義なら全ログ書き込む
                result.WriteLogLv = LogLv.Low;
            }

            switch (targetMode.ToLower())
            {
                case "0":
                case "false":
                case "release":
                    result.UseDebugModeOnly = false;
                    break;

                default:
                    result.UseDebugModeOnly = true;
                    break;
            }

            return result;
        }
        #endregion

        #region   = - Function: 有効なトレース情報を取得する
        /// <summary>
        /// 有効なトレース情報を取得する
        /// </summary>
        /// <param name="st">参照元のトレース情報</param>
        /// <returns></returns>
        /// <remarks></remarks>
        private static StackFrame GetValidStackFrame(StackTrace st, ICollection<string> skipMethodName)
        {
            if (st == null) return null;

            // トレース履歴を新→古へたどる。有効な値を見つけたら終了。
            foreach (StackFrame sf in st.GetFrames())
            {
                // GUARD: 指定トレース名と完全一致するなら次のトレースへ
                if (skipMethodName.Contains(sf.GetMethod().Name)) continue;

                return sf;
            }

            // ForEachでヒットしない = 有効なStackFrameなし
            return null;
        }
        #endregion

        #region   = - Function: ログ書込み内容作成

        /// <summary>
        /// ログ書込み内容作成（実行箇所のトレース情報）
        /// </summary>
        /// <param name="sf">トレース情報</param>
        /// <returns></returns>
        /// <remarks></remarks>
        [DebuggerStepThrough]
        private static string GetTraceMsg(StackFrame sf)
        {
            if (sf == null) return "StackFrame Is Nothing.";

            System.Reflection.MethodBase mi = sf.GetMethod();
            if (mi == null) return "MethodInfo Is Nothing.";

            return string.Format("Type：{0}　　Name：{1}", mi.ReflectedType.Name, mi.Name).Replace("\r\n", "");
        }

        /// <summary>
        /// ログ書込み内容作成（実行箇所の引数値情報）
        /// </summary>
        /// <param name="sf">トレース情報</param>
        /// <param name="param">実行箇所の引数値</param>
        /// <returns></returns>
        /// <remarks></remarks>
        [System.Diagnostics.DebuggerStepThrough]
        private static string GetParamMsg(StackFrame sf, params object[] param)
        {
            if (sf == null) return "StackFrame Is Nothing.";
            if (param == null) return "Parameter Is Nothing.";

            System.Reflection.MethodBase mi = sf.GetMethod();
            if (mi == null) return "MethodInfo Is Nothing.";

            System.Reflection.ParameterInfo[] pi = mi.GetParameters();
            if (pi == null) return "ParameterInfo Is Nothing.";

            // 引数の項目数が定義と異なる = 不正確な内容になるため処理しない（トレース名スキップしてると起こる可能性あり　例.Execute）
            if (param.Length != pi.Length) return "Parameter Length Is Not Matched.";

            StringBuilder result = new System.Text.StringBuilder();

            for (int i = 0; i < pi.Length; i++)
            {
                if (param[i] == null)
                {
                    result.AppendFormat("{0}：{1}　　", pi[i].Name, "Nothing");
                }
                else
                {
                    result.AppendFormat("{0}：{1}　　", pi[i].Name, param[i].ToString());
                }
            }

            return result.ToString().TrimEnd('　');
        }

        /// <summary>
        /// ログ書込み内容作成（例外情報）
        /// </summary>
        /// <param name="ex">例外情報</param>
        /// <returns></returns>
        /// <remarks></remarks>
        [System.Diagnostics.DebuggerStepThrough]
        private static string GetExecptionMsg(StackFrame sf, Exception ex)
        {
            if (sf == null) return "StackFrame Is Nothing.";

            System.Reflection.MethodBase mi = sf.GetMethod();
            if (mi == null) return "MethodInfo Is Nothing.";

            if (ex == null) return "Exception Is Nothing.";

            string targetSiteName = "";
            if (ex.TargetSite != null)
            {
                targetSiteName = ex.TargetSite.Name;
            }

            string message = "";
            if (ex.Message != null)
            {
                message = ex.Message;
            }

            return string.Format("Type：{0}　　TargetSite：{1}　　Name：{2}　　Message：{3}",
                                 mi.ReflectedType.Name, targetSiteName, ex.GetType().ToString(), message)
                   .Replace("\r\n", "");
        }
        #endregion
    }
}
