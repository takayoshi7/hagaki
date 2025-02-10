using hagaki.StaticClass;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

// ---------------------------------------------
//  クラス名   : Cls_DBConn
//  概要　　　 : ＤＢ操作関係
//  作成日　　 : 2025/02/05
//  作成者　　 : 高橋
//  最終更新日 : 
//  最終更新者 : 
// ---------------------------------------------

namespace hagaki.Class
{
    internal class Cls_DBConn
    {
        private ConnectDB Connect = new ConnectDB();  // DB初期設定クラス

        private bool _tranSts;                        // トランザクション状態
        private SqlCommand _tranCmd;                  // トランザクション用コマンド
        private int _cmdTimeoutSec;                   // SQLコマンド実行時タイムアウト時間（秒）【v1.0.2】

        public string Descrionption;                  // エラーメッセージ格納

        public const int DEFAULT_TIME_OUT_SEC = 30;   // SQLコマンド実行時タイムアウト時間（秒）既定値=30秒

        #region コンストラクタ
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="ConString">接続文字列</param>
        /// <param name="cmdTimeoutSec">SQLコマンド実行時タイムアウト時間 / 指定なしなら規定値</param>
        /// <remarks></remarks>
        public Cls_DBConn(string ConString, int cmdTimeoutSec = DEFAULT_TIME_OUT_SEC)
        {
            // 接続文字列取得
            Connect.SystemCoonectString = ConString;

            // SQLコマンド実行時タイムアウト時間
            _cmdTimeoutSec = cmdTimeoutSec;
        }
        #endregion

        #region 接続文字列
        // DB接続情報
        public class ConnectDB
        {
            private string mServer;        // Server
            private string mDataBase;      // Database
            private string mUser;          // User
            private string mPassword;      // Password
            private string mSystemCoonectString;

            // Server
            public string Server
            {
                get { return mServer; }
                set { mServer = value; }
            }

            // Database
            public string Database
            {
                get { return mDataBase; }
                set { mDataBase = value; }
            }

            // User
            public string User
            {
                get { return mUser; }
                set { mUser = value; }
            }

            // Password
            public string Password
            {
                get { return mPassword; }
                set { mPassword = value; }
            }

            public string SystemCoonectString
            {
                get { return mSystemCoonectString; }
                set { mSystemCoonectString = value; }
            }
        }
        #endregion

        #region DB接続
        // 機能　： DB接続(MySQL)
        // 戻り値： SqlConnection
        public SqlConnection SetDBConnction()
        {
            // 接続文字列
            SqlConnection cn = new SqlConnection();
            cn.ConnectionString = Connect.SystemCoonectString;

            try
            {
                cn.Open();
                return cn;
            }
            catch (Exception)
            {
                return null;
            }
        }
        #endregion

        #region DB切断
        // 機能　： DB切断
        // 引数　： ref SqlConnection Conn - コネクション
        // 戻り値： なし
        public void DisConnect(ref SqlConnection Conn)
        {
            if (Conn != null && Conn.State == ConnectionState.Open)
            {
                Conn.Close();
                Conn.Dispose();
            }
        }
        #endregion

        #region サーバー日時を取得
        /// <summary>
        /// 指定のデータを取得
        /// </summary>
        /// <returns>String データ</returns>
        public string GetTime()
        {
            string result = string.Empty;

            // DB接続
            using (SqlConnection mConn = SetDBConnction())
            {
                // DB接続確認
                if (mConn == null)
                {
                    return string.Empty;
                }

                using (SqlCommand sCmd = mConn.CreateCommand())
                {
                    // SQLクエリ設定
                    sCmd.CommandText = "SELECT CONVERT(VARCHAR,getdate(),120) as NOWTIME";

                    // SQLコマンド実行時タイムアウト時間【v1.0.3】
                    sCmd.CommandTimeout = _cmdTimeoutSec;

                    using (SqlDataReader sDR = sCmd.ExecuteReader())
                    {
                        if (sDR.Read())
                        {
                            return sDR["NOWTIME"].ToString();
                        }
                        // データリーダ閉じる
                        sDR.Close();
                    }
                    // リソース開放
                    sCmd.Dispose();
                }
            }
            return result;
        }
        #endregion

        #region 指定のデータを取得
        /// <summary>
        /// 指定のデータを取得
        /// </summary>
        /// <param name="Conn">コネクション</param>
        /// <param name="Field">フィールド名</param>
        /// <param name="Table">テーブル名</param>
        /// <param name="Filter">WHERE句</param>
        /// <param name="Func">集計関数</param>
        /// <returns></returns>
        public string FuncSQL(ref SqlConnection Conn, string Field, string Table, string Filter, string Func = "")
        {
            using (SqlCommand sCmd = Conn.CreateCommand())
            {
                string result = string.Empty;

                StringBuilder sb = new StringBuilder();
                sb.Append("SELECT ");
                sb.Append(string.IsNullOrWhiteSpace(Func) ? string.Empty : Func.Trim());
                sb.Append("(");
                sb.Append(Field.Trim());
                sb.Append(") as Result FROM ");
                sb.Append(Table.Trim());

                if (Filter.Length != 0)
                {
                    sb.Append(" WHERE ");
                    sb.Append(Filter.Trim());
                }

                sCmd.CommandText = sb.ToString();

                // SQLコマンド実行時タイムアウト時間【v1.0.3】
                sCmd.CommandTimeout = _cmdTimeoutSec;

                // トランザクション中ならSqlTransactionセット【v1.0.1】
                sCmd.Transaction = GetCorrectTransaction(Conn);

                using (SqlDataReader sDR = sCmd.ExecuteReader())
                {
                    if (sDR.Read())
                    {
                        return sDR["Result"].ToString();
                    }

                    // データリーダ閉じる
                    sDR.Close();
                }

                // リソース開放
                sCmd.Dispose();

                return result;
            }
        }
        #endregion

        #region DB値を取得(DLookup)
        /// <summary>
        /// DB値を取得する
        /// </summary>
        /// <param name="Conn">コネクション</param>
        /// <param name="Field">フィールド名</param>
        /// <param name="Table">テーブル名</param>
        /// <param name="Filter">集計関数</param>
        /// <returns>成功 DB値, 失敗 Null</returns>
        public string DLookup(ref SqlConnection Conn, string Field, string Table, string Filter = "")
        {
            return FuncSQL(ref Conn, Field, Table, Filter);
        }
        #endregion

        #region レコード数を取得(DCount)
        // 機能　： レコード数を取得する
        // 引数　： ref SqlConnection Conn - コネクション
        //         : string Field - フィールド名
        //         : string Table - テーブル名
        //         : string Filter - WHERE句
        //         : string Func - 集計関数
        // 戻り値： 成功 レコード数, 失敗 0
        public int DCount(ref SqlConnection Conn, string Field, string Table, string Filter = "")
        {
            return int.Parse(FuncSQL(ref Conn, Field, Table, Filter, "Count"));
        }
        #endregion

        #region 最大値を取得(DMax)
        /// <summary>
        /// 最大値を取得する
        /// </summary>
        /// <param name="Conn">コネクション</param>
        /// <param name="Field">フィールド名</param>
        /// <param name="Table">テーブル名</param>
        /// <param name="Filter">WHERE句</param>
        /// <returns>成功 最大値, 失敗 Null</returns>
        public string DMax(ref SqlConnection Conn, string Field, string Table, string Filter = "")
        {
            return FuncSQL(ref Conn, Field, Table, Filter, "Max");
        }
        #endregion

        #region 最小値を取得(DMin)
        /// <summary>
        /// 最小値を取得する
        /// </summary>
        /// <param name="Conn">コネクション</param>
        /// <param name="Field">フィールド名</param>
        /// <param name="Table">テーブル名</param>
        /// <param name="Filter">WHERE句</param>
        /// <returns>成功 最小値, 失敗 Null</returns>
        public string DMin(ref SqlConnection Conn, string Field, string Table, string Filter = "")
        {
            return FuncSQL(ref Conn, Field, Table, Filter, "Min");
        }
        #endregion

        #region 合計値を取得(DSum)
        /// <summary>
        /// 合計値を取得する
        /// </summary>
        /// <param name="Conn">コネクション</param>
        /// <param name="Field">フィールド名</param>
        /// <param name="Table">テーブル名</param>
        /// <param name="Filter">WHERE句</param>
        /// <returns>成功 合計値, 失敗 Null</returns>
        public string DSum(ref SqlConnection Conn, string Field, string Table, string Filter = "")
        {
            // フィールドを数値型に変換＆数値のみ抽出
            SetNumOnly(ref Field, ref Filter);
            return FuncSQL(ref Conn, Field, Table, Filter, "Sum");
        }
        #endregion

        #region 数値集計関数の準備
        /// <summary>
        /// 数値集計関数の準備(数値型変換＆数値のみ抽出)
        /// </summary>
        /// <param name="Field">フィールド名</param>
        /// <param name="Filter">WHERE句</param>
        private void SetNumOnly(ref string Field, ref string Filter)
        {
            string strAnd = string.Empty;

            Field = Field.Trim();

            if (!string.IsNullOrEmpty(Filter))
            {
                strAnd = " And ";
            }

            // 数値のみ評価対象
            Filter = Filter.Trim() + strAnd + "IsNumeric(" + Field + ") = 1 "; // MySQLにIsNumeric関数はないので修正要
            // bigint型に変換
            Field = "CAST(" + Field + " AS bigint)";
        }
        #endregion

        #region データリーダをセット
        /// <summary>
        /// 指定のSQL結果をデータリーダにセットする
        /// </summary>
        /// <param name="Conn">コネクション</param>
        /// <param name="strSQL">SQL文</param>
        /// <returns>成功 SqlDataReader, 失敗 null</returns>
        public SqlDataReader SetDataReader(SqlConnection Conn, string strSQL)
        {
            using (SqlCommand sCmd = Conn.CreateCommand())
            {
                // UPDBY 2017/02/24 matsuda 重複列名対応 解放処理追加【v1.0.7】
                SqlDataReader result = null;

                try
                {
                    sCmd.CommandText = strSQL;

                    // SQLコマンド実行時タイムアウト時間【v1.0.3】
                    sCmd.CommandTimeout = _cmdTimeoutSec;

                    // ADDBY 2014/11/14 matsuda トランザクション中ならSqlTransactionセット【v1.0.1】
                    sCmd.Transaction = GetCorrectTransaction(Conn);

                    // UPDBY 2017/02/06 matsuda 重複列名対応【v1.0.6】
                    result = sCmd.ExecuteReader();

                    // ADDBY 2017/02/06 matsuda 重複列名対応【v1.0.6】
                    ChkDupColumnName_DataReader(result);

                    // UPDBY 2017/02/06 matsuda 重複列名対応【v1.0.6】
                    return result;
                }
                catch (Exception ex)
                {
                    if (StCls_Public.pb_SQLLogFlg)
                    {
                        StCls_File.SetTextLineSJIS(StCls_Public.pb_SQLLogPath, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "\t" + strSQL.Replace("\r\n", ""));
                        StCls_File.SetTextLineSJIS(StCls_Public.pb_SQLLogPath, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "\t" + ex.Message);
                    }
                    Descrionption = ex.Message;

                    // ADDBY 2017/02/24 matsuda 重複列名対応 解放処理追加【v1.0.7】
                    if (result != null)
                    {
                        result.Close();
                        result.Dispose();
                    }
                    else
                    {
                        // 処理なし
                    }

                    return null;
                }
                finally
                {
                    // 明示的に開放
                    sCmd.Dispose();
                }
            }
        }
        #endregion

        #region データセットを取得
        /// <summary>
        /// 指定のSQL結果をデータセットにセットする
        /// </summary>
        /// <param name="Conn">コネクション</param>
        /// <param name="strSQL">SQL文</param>
        /// <param name="TableName">データセットのテーブル名</param>
        /// <returns>成功 DataSet, 失敗 Nothing</returns>
        public DataSet SetDataSet(SqlConnection Conn, string strSQL, string TableName = "")
        {
            DataSet ds = new DataSet();
            SqlCommand cmd;

            // SQL文
            cmd = new SqlCommand();
            cmd.Connection = Conn;
            cmd.CommandText = strSQL;

            // SQLコマンド実行時タイムアウト時間【v1.0.3】
            cmd.CommandTimeout = _cmdTimeoutSec;

            // ADDBY 2014/11/14 matsuda トランザクション中ならSqlTransactionセット【v1.0.1】
            cmd.Transaction = GetCorrectTransaction(Conn);

            // SqlDataAdapter
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                try
                {
                    Descrionption = string.Empty;

                    // ADDBY 2017/02/06 matsuda 重複列名対応【v1.0.6】
                    ChkDupColumnName_Sql(ref Conn, strSQL);

                    // DataSetに格納
                    if (string.IsNullOrEmpty(TableName))
                    {
                        da.Fill(ds);
                    }
                    else
                    {
                        da.Fill(ds, TableName);
                    }

                    return ds;
                }
                catch (Exception ex)
                {
                    // エラーメッセージ格納
                    if (StCls_Public.pb_SQLLogFlg)
                    {
                        StCls_File.SetTextLineSJIS(StCls_Public.pb_SQLLogPath, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "\t" + strSQL.Replace("\r\n", ""));
                        StCls_File.SetTextLineSJIS(StCls_Public.pb_SQLLogPath, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "\t" + ex.Message);
                    }
                    Descrionption = ex.Message;
                    return null;
                }
            }
        }
        #endregion

        #region トランザクションの開始
        /// <summary>
        /// トランザクションを開始する
        /// </summary>
        /// <param name="Conn">コネクション</param>
        /// <returns>成功 SqlTransaction, 失敗 null</returns>
        public SqlTransaction Begin(ref SqlConnection Conn)
        {
            SqlTransaction tranCn = Conn.BeginTransaction();

            _tranCmd = Conn.CreateCommand();
            _tranCmd.Transaction = tranCn;

            // トランザクション状態
            _tranSts = true;

            return tranCn;
        }
        #endregion

        #region トランザクションのコミット
        /// <summary>
        /// トランザクションをコミットする
        /// </summary>
        /// <param name="Tran">トランザクション</param>
        public void Commit(ref SqlTransaction Tran)
        {
            // トランザクション中でなければ終了
            if (!_tranSts) return;

            Tran.Commit();

            _tranCmd.Dispose();
            _tranSts = false;
        }
        #endregion

        #region トランザクションのロールバック
        // --------------------------------------------------------------------------
        // 機能　： トランザクションをロールバックする
        // 引数　： ref SqlTransaction Tran - トランザクション
        // 戻り値： なし
        // --------------------------------------------------------------------------
        public void RollBack(ref SqlTransaction Tran)
        {
            if (!_tranSts) return;

            Tran.Rollback();

            _tranCmd.Dispose();
            _tranSts = false;
        }
        #endregion

        #region アクションクエリの実行
        /// <summary>
        /// アクションクエリを実行する
        /// </summary>
        /// <param name="Conn">コネクション</param>
        /// <param name="strSQL">SQL文</param>
        /// <param name="AfterRecode">更新件数</param>
        /// <returns>成功 True, 失敗 False</returns>
        public bool Execute(SqlConnection Conn, string strSQL, long AfterRecode = 0)
        {
            if (StCls_Public.pb_SQLLogFlg)
            {
                StCls_File.SetTextLine(StCls_Public.pb_SQLLogPath, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "\t" + strSQL.Replace("\r\n", ""));
            }

            using (SqlCommand sCmd = Conn.CreateCommand())
            {
                sCmd.CommandText = strSQL;

                // SQLコマンド実行時タイムアウト時間【v1.0.2】
                sCmd.CommandTimeout = _cmdTimeoutSec;

                // ADDBY 2014/11/14 matsuda トランザクション中ならSqlTransactionセット【v1.0.1】
                sCmd.Transaction = GetCorrectTransaction(Conn);

                try
                {
                    Descrionption = string.Empty;
                    AfterRecode = sCmd.ExecuteNonQuery();
                    return true;
                }
                catch (Exception ex)
                {
                    if (StCls_Public.pb_SQLLogFlg)
                    {
                        StCls_File.SetTextLineSJIS(StCls_Public.pb_SQLLogPath, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "\t" + strSQL.Replace("\r\n", ""));
                        StCls_File.SetTextLineSJIS(StCls_Public.pb_SQLLogPath, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "\t" + ex.Message);
                    }
                    Descrionption = ex.Message; // エラーメッセージ格納
                    return false;
                }
                finally
                {
                    // 明示的に開放
                    sCmd.Dispose();
                }
            }
        }
        #endregion

        #region 適切なSqlTransactionを取得する
        /// <summary>
        /// 適切なSqlTransactionを取得する
        /// </summary>
        /// <param name="conn">コネクション</param>
        /// <returns>SqlTransaction - 適切なトランザクション、またはnull</returns>
        private SqlTransaction GetCorrectTransaction(SqlConnection conn)
        {
            // SqlCommandの不具合対応。
            // トランザクション中の場合、SqlCommand.Transactionに実行中のSqlTransactionをセットする必要がある。
            // このメソッド内でトランザクション中か判断し、中であればSqlTransactionを返す。
            // トランザクション中でなければnullを設定しても不都合なし。

            if (_tranSts)
            {
                if (object.Equals(_tranCmd.Transaction.Connection, conn))
                {
                    // connに紐づくSqlTransactionを返す
                    return _tranCmd.Transaction;
                }
                else
                {
                    // たとえば Conn1でトランザクション中、Conn2はトランザクションしていないので nullを返す
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region バルクインサート実行
        /// <summary>
        /// バルクインサート実行
        /// </summary>
        /// <param name="conn">コネクション</param>
        /// <param name="wkTableName">バルクインサート対象テーブル名</param>
        /// <param name="wkFilePath">バルクインサート元ファイルパス（SQLserverへファイルが置かれている事）</param>
        /// <param name="wkDATAFILETYPE">指定したデータ ファイルの型　※基本charで文字列なのでこのまま</param>
        /// <param name="wkFIELDTERMINATOR">区切り文字（デフォルトタブ区切り）</param>
        /// <param name="wkROWTERMINATOR">行区切り文字（デフォルト改行コード）</param>
        /// <param name="wkFIRSTROW">読込開始行　（デフォルト１行目から）</param>
        /// <param name="wkFORMATFILE">フォーマットファイル使用時のフォーマットファイルパス</param>
        /// <returns>成功：ブランク, 失敗：エラーメッセージ</returns>
        /// <remarks>DBの権限が必要 [SQLServer]-[セキュリティ]-[ログイン]-[該当ユーザ]-[サーバーロール] に [bulkadmin] を付ける。</remarks>
        public string RunBulkInsert(ref SqlConnection conn, string wkTableName, string wkFilePath,
                                    string wkDATAFILETYPE = "char", string wkFIELDTERMINATOR = "\t",
                                    string wkROWTERMINATOR = "\n", long wkFIRSTROW = 1, string wkFORMATFILE = "")
        {
            StringBuilder sb = new System.Text.StringBuilder();

            sb.Clear();
            sb.Append("BULK INSERT ");
            sb.Append(wkTableName);
            sb.Append(" ");
            sb.Append("FROM ");
            sb.Append("'");
            sb.Append(wkFilePath);
            sb.Append("' ");
            sb.Append("WITH ");
            sb.Append("(");
            sb.Append("DATAFILETYPE = '");
            sb.Append(wkDATAFILETYPE);
            sb.Append("' ,");
            sb.Append("FIELDTERMINATOR ='");
            sb.Append(wkFIELDTERMINATOR);
            sb.Append("' ,");
            sb.Append("ROWTERMINATOR = '");
            sb.Append(wkROWTERMINATOR);
            sb.Append("' ,");
            sb.Append("FIRSTROW = ");
            sb.Append(wkFIRSTROW);
            sb.Append(" ");

            if (!string.IsNullOrEmpty(wkFORMATFILE))
            {
                sb.Append(" , ");
                sb.Append("FORMATFILE ='");
                sb.Append(wkFORMATFILE);
                sb.Append("' ");
            }

            sb.Append(")");

            if (!Execute(conn, sb.ToString()))
            {
                return Descrionption;
            }

            return string.Empty;
        }
        #endregion

        #region バルクコピー実行
        /// <summary>
        /// バルクコピー実行
        /// </summary>
        /// <param name="conn">コネクション</param>
        /// <param name="tableName">テーブル名</param>
        /// <param name="dt">データテーブル</param>
        public void RunBulkCopy(SqlConnection conn, string tableName, DataTable dt)
        {
            try
            {
                using (SqlBulkCopy sBC = new SqlBulkCopy(conn))
                {
                    // コピー先テーブル名の設定
                    // ------------------------------
                    sBC.DestinationTableName = tableName;

                    // バルクコピー
                    // ------------------------------
                    sBC.WriteToServer(dt);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 重複列名チェック(DataReader版)
        /// <summary>
        /// 重複列名チェック
        /// </summary>
        /// <param name="sdr">チェック対象(DataReader)</param>
        /// <remarks>
        /// ADDBY 2017/02/06 matsuda 重複列名対応【v1.0.6】
        /// </remarks>
        public void ChkDupColumnName_DataReader(SqlDataReader sdr)
        {
            try
            {
                List<string> fieldNameList = new List<string>();
                List<string> dupNameList = new List<string>();

                // 各列名を確認
                for (int i = 0; i < sdr.FieldCount; i++)
                {
                    string fieldName = sdr.GetName(i).ToUpper();

                    if (string.IsNullOrEmpty(fieldName))
                    {
                        // 列名なし
                        // ※カラム結合等で列名が指定されていない項目が複数存在する場合、
                        // 　列名がブランクの項目が重複し例外へ飛ばされてしまう
                        // 　その回避策としてブランクの場合処理を飛ばして無視する
                        // ※列名にブランクを指定して値を取得する場合は考慮しない
                        // 正常：SELECT (ADD1 + ADD2) , ADD3 , ADD4  FROM D_ADDRESS
                        // 例外：SELECT (ADD1 + ADD2) ,(ADD3 + ADD4) FROM D_ADDRESS

                        System.Diagnostics.Debug.WriteLine($"列名指定なし　Index：{i}");
                        continue;
                    }
                    else if (fieldNameList.Contains(fieldName))
                    {
                        // 現Indexまでで、列名に重複あり
                        dupNameList.Add(fieldName);
                        break;
                    }
                    else
                    {
                        // 現Indexまでで、列名に重複なし
                    }

                    fieldNameList.Add(fieldName);
                }

                if (dupNameList.Count == 0)
                {
                    // 正しいSQLでした
                }
                else
                {
                    // 列名に重複あり

                    StringBuilder msg = new StringBuilder();

                    msg.AppendLine("【SQLエラー】");
                    msg.AppendLine("SELECT句の列名に重複があります。");
                    msg.AppendLine();

                    foreach (string dupName in dupNameList)
                    {
                        msg.AppendLine($"・{dupName}");
                    }

                    throw new Exception(msg.ToString());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 重複列名チェック(Sql版)
        /// <summary>
        /// 重複列名チェック
        /// ※引数のSQLにTOP1を差し込むことで処理時間を短縮し、関数「ChkDupColumnName_DataReader」でチェックを行う
        /// ※TOP1の差し込みに失敗した場合、チェックは問題なく行われるが、大幅に処理速度が落ちる恐れがあります
        /// </summary>
        public void ChkDupColumnName_Sql(ref SqlConnection conn, string strSQL)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                using (SqlCommand sCmd = conn.CreateCommand())
                {
                    strSQL = strSQL.ToUpper();

                    // 第二引数[RemoveEmptyEntries]・・・配列の空の要素を削除した配列を返す
                    // ※これにより空白以外の文字の配列を作成
                    string[] chkSqlArray = strSQL.Split(new[] { " ", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                    if (chkSqlArray[0] != "SELECT")
                    {
                        // 一番最初の項目が"SELECT"以外の場合処理中止
                        return;
                    }

                    // SELECTの位置を保持する変数
                    int selectIndex = 0;

                    //##########################################################################################
                    //#①最初のSELECT句に対しTOP1を挿入
                    //#②[UNION]または[UNION ALL]が存在した場合は、続くSELECT句に対しTOP1を挿入
                    //#③SqlDataReaderにより重複列名チェック
                    //#④重複列名が存在した場合は例外が発生する
                    //#※TOP1を挿入する関数(ReplaceSql)内で[変換不能なSQL(*1へ)]の場合は、
                    //#  引数のSQLがほぼそのまま使用されるため、
                    //#  大幅に処理速度が落ちる恐れがあります。
                    //#  その場合は、各案件のSQLを修正するという対応をお願い致します。
                    //#
                    //#*1変換不能なsql
                    //#１、先頭が[SELECT]以外の文字列
                    //#
                    //#２、[UNION]の対象を両方または片方()で囲んでいる場合
                    //#　( SELECT ??? FROM ??? WHERE ??? ) UNION ( SELECT ??? FROM ??? WHERE ??? )
                    //#　  SELECT ??? FROM ??? WHERE ???   UNION ( SELECT ??? FROM ??? WHERE ??? )
                    //#
                    //#３、上記以外で[SELECT]～[項目名]の間に[DISTINCT][TOP ???]以外の文字列が含まれている場合
                    //#　正常動作が確認できる　：対応不要とする
                    //#　正常動作が確認できない：Cls_DBConnの関数(ReplaceSql)内を修正
                    //##########################################################################################

                    while (true)
                    {
                        // SQL配列置換処理　※第一引数ByRef
                        ReplaceSql(ref chkSqlArray, selectIndex);

                        // UNIONの位置を取得
                        selectIndex = Array.IndexOf(chkSqlArray, "UNION", selectIndex + 1);

                        if (selectIndex == -1)
                        {
                            break;
                        }
                        else if (chkSqlArray[selectIndex + 1] == "ALL")
                        {
                            // SELECT句の位置を取得 UNION ALLの次の為+2
                            selectIndex += 2;
                        }
                        else
                        {
                            // SELECT句の位置を取得 UNIONの次の為+1
                            selectIndex += 1;
                        }
                    }

                    sCmd.CommandText = string.Join(" ", chkSqlArray);

                    sCmd.CommandTimeout = _cmdTimeoutSec;

                    sCmd.Transaction = GetCorrectTransaction(conn);

                    using (SqlDataReader sDR = sCmd.ExecuteReader())
                    {
                        ChkDupColumnName_DataReader(sDR);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region 重複列名チェックSql版補助関数
        /// <summary>
        /// 重複列名チェックSql版補助関数
        /// SELECT句にTOP1を追加する
        /// </summary>
        /// <param name="sqlArray">対称SQL配列</param>
        /// <param name="selectIndex">SELECTの位置</param>
        private void ReplaceSql(ref string[] sqlArray, int selectIndex)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("");
                System.Diagnostics.Debug.WriteLine("関数：ReplaceSql");
                System.Diagnostics.Debug.WriteLine("置換予定SELECT位置：" + selectIndex.ToString());

                if (sqlArray[selectIndex] == "SELECT" && sqlArray[selectIndex + 1] == "DISTINCT"
                                                      && sqlArray[selectIndex + 2] == "TOP"
                                                      && StCls_Check.CHF_Decimal(sqlArray[selectIndex + 3].Replace("(", "").Replace(")", "")) == (long)StCls_Check.ERRCODE.ERR_NONE)
                {
                    // "SELECT DISTINCT TOP ???" の場合 ??? を 1 に置換
                    sqlArray[selectIndex + 3] = "1";
                    System.Diagnostics.Debug.WriteLine("処理結果：成功");
                }
                else if (sqlArray[selectIndex] == "SELECT" && sqlArray[selectIndex + 1] == "TOP"
                                                           && StCls_Check.CHF_Decimal(sqlArray[selectIndex + 2].Replace("(", "").Replace(")", "")) == (long)StCls_Check.ERRCODE.ERR_NONE)
                {
                    // "SELECT TOP ???" の場合 ??? を 1 に置換
                    sqlArray[selectIndex + 2] = "1";
                    System.Diagnostics.Debug.WriteLine("処理結果：成功");
                }
                else if (sqlArray[selectIndex] == "SELECT" && sqlArray[selectIndex + 1] == "DISTINCT")
                {
                    // "SELECT DISTINCT"の場合 "DISTINCT" を "DISTINCT TOP 1" に置換
                    sqlArray[selectIndex + 1] = "DISTINCT TOP 1";
                    System.Diagnostics.Debug.WriteLine("処理結果：成功");
                }
                else if (sqlArray[selectIndex] == "SELECT")
                {
                    // 上記以外の場合、"SELECT" を "SELECT TOP 1" に置換
                    sqlArray[selectIndex] = "SELECT TOP 1";
                    System.Diagnostics.Debug.WriteLine("処理結果：成功");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("処理結果：失敗");
                }

                System.Diagnostics.Debug.WriteLine("SQL：");
                System.Diagnostics.Debug.WriteLine(string.Join(" ", sqlArray));
                System.Diagnostics.Debug.WriteLine("");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}
