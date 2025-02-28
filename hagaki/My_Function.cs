using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using hagaki.StaticClass;
using Microsoft.VisualBasic;
using System.Collections;
using System.Reflection.Emit;
using System.Windows.Forms;

namespace hagaki
{
    internal class My_Function
    {
        #region 定数
        private const string D_MAIN = "D_MAIN";                              // メインテーブル
        private const string D_ERROR = "D_ERROR";                            // エラーテーブル
        private const string WK_MAIN = "WK_IN_MAIN";                         // 取込データ登録テーブル
        private const string WK_MAIN_ERROR = "WK_IN_MAIN_ERROR";             // 取込可能エラー登録テーブル
        private const string WK_MAIN_INSERT_ERROR = "WK_IN_MAIN_INSERT_ERR"; // 取込不可エラー登録テーブル
        private const string WK_HISO = "WK_HISO";                            // 配送出力データ登録テーブル
        private Encoding SJIS = Encoding.GetEncoding("Shift_JIS");           // SJISエンコーディングが使えるように
        #endregion

        #region 列挙体
        // メインテーブル
        public enum MainTableColumn
        {
            KanriNo,
            UkeDate,
            ZipCd,
            Add1,
            Add2,
            Add3,
            Add4,
            NameSei,
            NameMei,
            TelNo,
            Ank1,
            Ank2,
            Ank3,
            JyotaiKb,
            NgOutKb,
            HisoOutKb
        }

        // エラーテーブル
        public enum ErrorTableColumn
        {
            KanriNo,
            ErrCd
        }

        // 取り込み不可エラー
        public enum ErrorCd
        {
            NoError,
            LayoutError,
            IncorrectControlNumber,
            ImportedControlNumber,
            DuplicateControlNumber,
            IncorrectReceptionDate,
            DBSizeError
        }

        // 状態区分
        public enum JyotaiKb
        {
            Ok,
            Ng,
            Hold,
            Cancel
        }

        // NG票出力区分
        public enum NgOutKb
        {
            Un,
            Done
        }

        // 配送データ出力区分
        public enum HisoOutKg
        {
            Un,
            Done
        }
        #endregion

        #region 項目数チェック
        /// <summary>
        /// 項目数が13項目すべて存在するかチェック
        /// </summary>
        /// <param name="checkData">チェックするデータ</param>
        /// <returns>true:問題なし / false:レイアウトエラー</returns>
        public bool ItemsNumCheck(string[] checkData)
        {
            if (checkData.Length == 13)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region 事務局管理番号チェック
        /// <summary>
        /// 事務局管理番号が正しい値になっているか
        /// </summary>
        /// <param name="checkData">チェックデータ</param>
        /// <returns>true:問題なし / false:事務局管理番号不備</returns>
        public bool KanriNoCheck(string checkData)
        {
            // 数値か確認（0： 問題なし）
            if (StCls_Check.CHF_Decimal(checkData) != 0)
            {
                return false;
            }

            // 5文字か確認
            if (Encoding.Default.GetBytes(checkData).Length != 5)
            {
                return false;
            }

            // 1から始まっているか確認
            if (checkData[0] != '1')
            {
                return false;
            }
                
            return true;
        }
        #endregion

        #region 受付日チェック
        /// <summary>
        /// 受付日が正しい日付になっているか
        /// </summary>
        /// <param name="checkData">チェックデータ</param>
        /// <returns>true:問題なし / false:受付日が不備</returns>
        public bool UkeDateCheck(string checkData)
        {
            // 半角に変換
            string ukeDate = StCls_Function.VbStrConv(checkData, (VbStrConv)8);

            // 日付か確認（0：問題なし）
            if (StCls_Check.CHF_Date(ukeDate) != 0)
            {
                return false;
            }

            return true;
        }
        #endregion

        #region エラーチェック
        /// <summary>
        /// テキストデータエラーチェック
        /// </summary>
        /// <param name="checkData">チェックデータ</param>
        /// <returns>エラーコードの入ったリスト</returns>
        public List<int> ErrorCheck(string[] checkData)
        {
            // エラーコードリスト
            List<int> errorCdList = new List<int>();

            // --------------------------------------------------------------------------
            // 郵便番号が数値で7バイトか確認（0：問題なし）
            int zipCheck = ZipCdCheck(checkData[(int)MainTableColumn.ZipCd]);

            if (zipCheck != 0)
            {
                errorCdList.Add(zipCheck);
            }
            // --------------------------------------------------------------------------

            // --------------------------------------------------------------------------
            // 住所1チェック
            int address1Check = Add1Check(checkData[(int)MainTableColumn.Add1]);

            if (address1Check != 0)
            {
                errorCdList.Add(address1Check);
            }
            // --------------------------------------------------------------------------

            // --------------------------------------------------------------------------
            // 住所2チェック
            int address2Check = Add2Check(checkData[(int)MainTableColumn.Add2]);

            if (address2Check != 0)
            {
                errorCdList.Add(address2Check);
            }
            // --------------------------------------------------------------------------

            // --------------------------------------------------------------------------
            // 住所3チェック
            int address3Check = Add3Check(checkData[(int)MainTableColumn.Add3]);

            if (address3Check != 0)
            {
                errorCdList.Add(address3Check);
            }
            // --------------------------------------------------------------------------

            // --------------------------------------------------------------------------
            // 住所4チェック
            int address4Check = Add4Check(checkData[(int)MainTableColumn.Add4]);

            if (address4Check != 0)
            {
                errorCdList.Add(address4Check); // 修正: address4Check を追加すべき
            }
            // --------------------------------------------------------------------------

            // --------------------------------------------------------------------------
            // 氏名（姓）チェック
            int nameSeiCheck = SeiCheck(checkData[(int)MainTableColumn.NameSei]);

            if (nameSeiCheck != 0)
            {
                errorCdList.Add(nameSeiCheck);
            }
            // --------------------------------------------------------------------------

            // --------------------------------------------------------------------------
            // 氏名（名）チェック
            int nameMeiCheck = MeiCheck(checkData[(int)MainTableColumn.NameMei]);

            if (nameMeiCheck != 0)
            {
                errorCdList.Add(nameMeiCheck);
            }
            // --------------------------------------------------------------------------

            return errorCdList;
        }
        #endregion

        #region 郵便番号チェック
        /// <summary>
        /// 郵便番号チェック
        /// </summary>
        /// <param name="zipCd">郵便番号</param>
        /// <returns>0:問題なし / 100:7バイトではない / 101:数値以外</returns>
        public int ZipCdCheck(string zipCd)
        {
            // 半角に変換
            string convZipCd = StCls_Function.VbStrConv(zipCd, (VbStrConv)8);

            // ハイフンを除去
            convZipCd = convZipCd.Replace("-", "");

            // 数値か確認（0：問題なし、1：ブランク、それ以外：エラー）
            long check = StCls_Check.CHF_Decimal(convZipCd);

            if (check == 1)
            {
                // ブランクはOK
                return 0;
            }
            else if (check == 0)
            {
                // 数値で7バイト以外はエラー
                if (SJIS.GetByteCount(convZipCd) != 7)
                {
                    return 100;
                }

                // 数値で7バイトはOK
                return 0;
            }
            else
            {
                // 数値以外はエラー
                return 101;
            }
        }
        #endregion

        #region 住所1チェック
        /// <summary>
        /// 住所1チェック
        /// </summary>
        /// <param name="Add1">住所1</param>
        /// <returns>0:問題なし / 102:ブランク / 103:判読不明 / 104:11バイト以上</returns>
        public int Add1Check(string Add1)
        {
            // 全角に変換
            string convAdd1 = StCls_Function.VbStrConv(Add1, (VbStrConv)4);

            // ブランクエラー
            if (string.IsNullOrEmpty(convAdd1))
            {
                return 102;
            }

            // ？を含んでいればエラー
            if (convAdd1.Contains("？"))
            {
                return 103;
            }

            // 11バイト以上エラー
            if (SJIS.GetByteCount(convAdd1) >= 11)
            {
                return 104;
            }

            return 0;
        }
        #endregion

        #region 住所2チェック
        /// <summary>
        /// 住所2チェック
        /// </summary>
        /// <param name="Add2">住所2</param>
        /// <returns>0:問題なし / 105:ブランク / 106:判読不明 / 107:21バイト以上</returns>
        public int Add2Check(string Add2)
        {
            // 全角に変換
            string convAdd2 = StCls_Function.VbStrConv(Add2, (VbStrConv)4);

            // ブランクエラー
            if (string.IsNullOrEmpty(convAdd2))
            {
                return 105;
            }

            // ？を含んでいればエラー
            if (convAdd2.Contains("？"))
            {
                return 106;
            }

            // 21バイト以上エラー
            if (SJIS.GetByteCount(convAdd2) >= 21)
            {
                return 107;
            }

            return 0;
        }
        #endregion

        #region 住所3チェック
        /// <summary>
        /// 住所3チェック
        /// </summary>
        /// <param name="Add3">住所3</param>
        /// <returns>0:問題なし / 108:ブランク / 109:判読不明 / 110:41バイト以上</returns>
        public int Add3Check(string Add3)
        {
            // 全角に変換
            string convAdd3 = StCls_Function.VbStrConv(Add3, (VbStrConv)4);

            // ブランクエラー
            if (string.IsNullOrEmpty(convAdd3))
            {
                return 108;
            }

            // ？を含んでいればエラー
            if (convAdd3.Contains("？"))
            {
                return 109;
            }

            // 41バイト以上エラー
            if (SJIS.GetByteCount(convAdd3) >= 41)
            {
                return 110;
            }

            return 0;
        }
        #endregion

        #region 住所4チェック
        /// <summary>
        /// 住所4チェック
        /// </summary>
        /// <param name="Add4">住所3</param>
        /// <returns>0:問題なし / 111:判読不明 / 112:41バイト以上</returns>
        public int Add4Check(string Add4)
        {
            // 全角に変換
            string convAdd4 = StCls_Function.VbStrConv(Add4, (VbStrConv)4);

            // ？を含んでいればエラー
            if (convAdd4.Contains("？"))
            {
                return 111;
            }

            // 41バイト以上エラー
            if (SJIS.GetByteCount(convAdd4) >= 41)
            {
                return 112;
            }

            return 0;
        }
        #endregion

        #region 氏名（姓）チェック
        /// <summary>
        /// 氏名（姓）チェック
        /// </summary>
        /// <param name="nameSei">氏名（姓）</param>
        /// <returns>0:問題なし / 113:ブランク / 114:判読不明 / 115:21バイト以上</returns>
        public int SeiCheck(string nameSei)
        {
            // 全角に変換
            string convSei = StCls_Function.VbStrConv(nameSei, (VbStrConv)4);

            // ブランクエラー
            if (string.IsNullOrEmpty(convSei))
            {
                return 113;
            }

            // ？を含んでいればエラー
            if (convSei.Contains("？"))
            {
                return 114;
            }

            // 21バイト以上エラー
            if (SJIS.GetByteCount(convSei) >= 21)
            {
                return 115;
            }

            return 0;
        }
        #endregion

        #region 氏名（名）チェック
        /// <summary>
        /// 氏名（名）チェック
        /// </summary>
        /// <param name="nameMei">氏名（名）</param>
        /// <returns>0:問題なし / 116:ブランク（問題なし） / 117:判読不明 / 118:21バイト以上</returns>
        public int MeiCheck(string nameMei)
        {
            // 全角に変換
            string convMei = StCls_Function.VbStrConv(nameMei, (VbStrConv)4);

            // ブランクエラー
            if (string.IsNullOrEmpty(convMei))
            {
                return 116;
            }

            // ？を含んでいればエラー
            if (convMei.Contains("？"))
            {
                return 117;
            }

            // 21バイト以上エラー
            if (SJIS.GetByteCount(convMei) >= 21)
            {
                return 118;
            }

            return 0;
        }
        #endregion

        #region 数値以外入力制限
        /// <summary>
        /// 数値以外を入力できないようにする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void NumTextKeyPress(object sender, KeyPressEventArgs e)
        {
            // バックスペースまたはデリートであれば終了してそのまま処理
            if (e.KeyChar == (char)Keys.Back || e.KeyChar == (char)Keys.Delete)
            {
                return;
            }

            // 押されたキーが数値かチェック
            long checkNum = StCls_Check.CHF_Decimal(e.KeyChar);

            // 数値でなければ入力不可
            if (checkNum != 0)
            {
                e.Handled = true;
            }
        }
        #endregion






        #region 値の強制変換
        /// <summary>
        /// 配列の値をテーブル登録用に変換する
        /// </summary>
        /// <param name="dataArray">変換前の配列</param>
        /// <returns>変換後の配列</returns>
        public string[] ForcedConversion(string[] dataArray)
        {
            dataArray[1] = StCls_Function.VbStrConv(dataArray[(int)MainTableColumn.UkeDate], (VbStrConv)8);                // 半角変換
            dataArray[2] = StCls_Function.VbStrConv(dataArray[(int)MainTableColumn.ZipCd], (VbStrConv)8).Replace("-", ""); // 半角変換+ハイフン除去
            dataArray[3] = StCls_Function.VbStrConv(dataArray[(int)MainTableColumn.Add1], (VbStrConv)4);                   // 全角変換
            dataArray[4] = StCls_Function.VbStrConv(dataArray[(int)MainTableColumn.Add2], (VbStrConv)4);                   // 全角変換
            dataArray[5] = StCls_Function.VbStrConv(dataArray[(int)MainTableColumn.Add3], (VbStrConv)4);                   // 全角変換
            dataArray[6] = StCls_Function.VbStrConv(dataArray[(int)MainTableColumn.Add4], (VbStrConv)4);                   // 全角変換
            dataArray[7] = StCls_Function.VbStrConv(dataArray[(int)MainTableColumn.NameSei], (VbStrConv)4);                // 全角変換
            dataArray[8] = StCls_Function.VbStrConv(dataArray[(int)MainTableColumn.NameMei], (VbStrConv)4);                // 全角変換
            dataArray[9] = StCls_Function.VbStrConv(dataArray[(int)MainTableColumn.TelNo], (VbStrConv)8).Replace("-", ""); // 半角変換+ハイフン除去
            dataArray[10] = (dataArray[(int)MainTableColumn.Ank1] == "1" || dataArray[(int)MainTableColumn.Ank1] == "2")
                            ? dataArray[(int)MainTableColumn.Ank1] : "9";                                                  // 値が1または2以外であれば9にする
            dataArray[11] = (dataArray[(int)MainTableColumn.Ank2] == "1" || dataArray[(int)MainTableColumn.Ank2] == "2"
                            || dataArray[(int)MainTableColumn.Ank2] == "3" || dataArray[(int)MainTableColumn.Ank2] == "4")
                            ? dataArray[(int)MainTableColumn.Ank2] : "9";                                                  // 値が1、2、3、4以外であれば9にする
            dataArray[12] = (dataArray[(int)MainTableColumn.Ank3] == "1" || dataArray[(int)MainTableColumn.Ank3] == "2"
                            || dataArray[(int)MainTableColumn.Ank3] == "3" || dataArray[(int)MainTableColumn.Ank3] == "4"
                            || dataArray[(int)MainTableColumn.Ank3] == "5")
                            ? dataArray[(int)MainTableColumn.Ank3] : "9";                                                  // 値が1、2、3、4、5以外であれば9にする

            return dataArray;
        }
        #endregion

        #region パラメータ作成
        /// <summary>
        /// SQLインジェクション対策用に渡すパラメータを作成する
        /// </summary>
        /// <param name="dataArray">値の入った配列</param>
        /// <param name="line">タブ区切りの1レコード文字列</param>
        /// <returns>キーと値がセットのパラメータ</returns>
        public Dictionary<string, object> KeyValuePairs(string[] dataArray, string line = "")
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "@kanriNo", dataArray[(int)My_Function.MainTableColumn.KanriNo] },
                { "@UkeDate", dataArray[(int)My_Function.MainTableColumn.UkeDate] },
                { "@ZipCd", dataArray[(int)My_Function.MainTableColumn.ZipCd] },
                { "@Add1", dataArray[(int)My_Function.MainTableColumn.Add1] },
                { "@Add2", dataArray[(int)My_Function.MainTableColumn.Add2] },
                { "@Add3", dataArray[(int)My_Function.MainTableColumn.Add3] },
                { "@Add4", dataArray[(int)My_Function.MainTableColumn.Add4] },
                { "@NameSei", dataArray[(int)My_Function.MainTableColumn.NameSei] },
                { "@NameMei", dataArray[(int)My_Function.MainTableColumn.NameMei] },
                { "@TelNo", dataArray[(int)My_Function.MainTableColumn.TelNo] },
                { "@Ank1", dataArray[(int)My_Function.MainTableColumn.Ank1] },
                { "@Ank2", dataArray[(int)My_Function.MainTableColumn.Ank2] },
                { "@Ank3", dataArray[(int)My_Function.MainTableColumn.Ank3] },
                { "@Line", line },
            };

            return parameters;
        }
        #endregion




        #region DataSetにDataTable作成
        /// <summary>
        /// DataSetにDataTableを作成する
        /// </summary>
        /// <param name="dataSet">DataSet</param>
        /// <param name="connection">SqlConnection</param>
        /// <param name="transaction">SqlTransaction</param>
        /// <param name="query">クエリ</param>
        /// <param name="parameters">パラメータ</param>
        /// <param name="tableName">テーブル名</param>
        public void FillDataTable(DataSet dataSet, SqlConnection connection, SqlTransaction transaction, string query, Dictionary<string, object> parameters, string tableName)
        {
            using (SqlCommand command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = query;

                // パラメータを追加
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }
                }

                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    // DataTableにデータを取得し、DataSetに追加
                    //adapter.Fill(dataSet, tableName);

                    try
                    {
                        adapter.Fill(dataSet, tableName);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Fillでエラー: " + ex.Message);
                        throw; // 例外を再スロー
                    }
                }
            }
        }
        #endregion



        #region インサートSQL文作成
        /// <summary>
        /// インサートSQL文作成
        /// </summary>
        /// <param name="tableName">テーブル名</param>
        /// <param name="offset">行番号</param>
        /// <param name="err_cd">エラーコードまたは状態</param>
        /// <returns>SQL文</returns>
        public string MakeInsertSql(string tableName, int offset = 0, int err_cd = (int)ErrorCd.NoError)
        {
            // SQL文の生成
            StringBuilder sqlStr = new StringBuilder();

            // 読込時、登録不可の場合
            if (tableName == WK_MAIN_INSERT_ERROR)
            {
                sqlStr.AppendLine($"INSERT INTO {WK_MAIN_INSERT_ERROR}");
                sqlStr.AppendLine("(OFFSET, LINE_DATA, ERR_NO) VALUES ");
                sqlStr.AppendLine($"({offset}, @Line, {err_cd})");

                return sqlStr.ToString();
            }

            // 取込ボタン押下時
            if (tableName == D_MAIN)
            {
                // 現在の日時を取得
                string nowDateTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

                // ログインユーザー名取得
                string loginID = StCls_Function.GetUser();

                sqlStr.AppendLine($"INSERT INTO {D_MAIN}(");
                sqlStr.AppendLine("KANRI_NO, UKE_DATE, ZIP_CD, ADD_1, ADD_2, ADD_3, ADD_4, ");
                sqlStr.AppendLine("NAME_SEI, NAME_MEI, TEL_NO, ");
                sqlStr.AppendLine("ANK_1, ANK_2, ANK_3, ");
                sqlStr.AppendLine("JYOTAI_KB, ");
                sqlStr.AppendLine("NG_OUT_KB, NG_OUT_DATETIME, NG_OUT_LOGINID, ");
                sqlStr.AppendLine("HISO_OUT_KB, HISO_OUT_DATETIME, HISO_OUT_LOGINID, ");
                sqlStr.AppendLine("REGIST_DATETIME, REGIST_LOGINID, ");
                sqlStr.AppendLine("UPDATE_DATETIME, UPDATE_LOGINID) VALUES (");
                sqlStr.AppendLine("@KanriNo,");
                sqlStr.AppendLine("@UkeDate,");
                sqlStr.AppendLine("@ZipCd,");
                sqlStr.AppendLine("@Add1,");
                sqlStr.AppendLine("@Add2,");
                sqlStr.AppendLine("@Add3,");
                sqlStr.AppendLine("@Add4,");
                sqlStr.AppendLine("@NameSei,");
                sqlStr.AppendLine("@NameMei,");
                sqlStr.AppendLine("@TelNo,");
                sqlStr.AppendLine("@Ank1,");
                sqlStr.AppendLine("@Ank2,");
                sqlStr.AppendLine("@Ank3,");
                sqlStr.AppendLine($"{err_cd},");
                sqlStr.AppendLine("'0',");
                sqlStr.AppendLine("'',");
                sqlStr.AppendLine("'',");
                sqlStr.AppendLine("'0',");
                sqlStr.AppendLine("'',");
                sqlStr.AppendLine("'',");
                sqlStr.AppendLine($"'{nowDateTime}',");
                sqlStr.AppendLine($"'{loginID}',");
                sqlStr.AppendLine("'',");
                sqlStr.AppendLine("'')");
            }
            else if (tableName == D_ERROR)
            {
                sqlStr.AppendLine($"INSERT INTO {D_ERROR}(");
                sqlStr.AppendLine("KANRI_NO, ERR_CD) VALUES (");
                sqlStr.AppendLine("@KanriNo,");
                sqlStr.AppendLine($"{err_cd})");
            }
            else if (tableName == WK_HISO)
            {
                sqlStr.AppendLine($"INSERT INTO {WK_HISO}(");
                sqlStr.AppendLine("KANRI_NO, ZIP_CD, ADD_1, ADD_2, ADD_3, ADD_4, NAME_SEI, NAME_MEI) VALUES (");
                sqlStr.AppendLine("@KanriNo,");
                sqlStr.AppendLine("@ZipCd,");
                sqlStr.AppendLine("@Add1,");
                sqlStr.AppendLine("@Add2,");
                sqlStr.AppendLine("@Add3,");
                sqlStr.AppendLine("@Add4,");
                sqlStr.AppendLine("@NameSei,");
                sqlStr.AppendLine("@NameMei)");
            }

            // 読込時、取込可能の場合
            if (tableName == WK_MAIN)
            {
                sqlStr.AppendLine($"INSERT INTO {WK_MAIN}(");
                sqlStr.AppendLine("KANRI_NO, UKE_DATE, ZIP_CD, " + 
                                    "ADD_1, ADD_2, ADD_3, ADD_4, " +
                                    "NAME_SEI, NAME_MEI, TEL_NO, " +
                                    "ANK_1, ANK_2, ANK_3, " +
                                    "JYOTAI_KB, OFFSET, DUPLI_FLG, LINE_DATA) VALUES(");
                sqlStr.AppendLine("@KanriNo,");
                sqlStr.AppendLine("@UkeDate,");
                sqlStr.AppendLine("@ZipCd,");
                sqlStr.AppendLine("@Add1,");
                sqlStr.AppendLine("@Add2,");
                sqlStr.AppendLine("@Add3,");
                sqlStr.AppendLine("@Add4,");
                sqlStr.AppendLine("@NameSei,");
                sqlStr.AppendLine("@NameMei,");
                sqlStr.AppendLine("@TelNo,");
                sqlStr.AppendLine("@Ank1,");
                sqlStr.AppendLine("@Ank2,");
                sqlStr.AppendLine("@Ank3,");
                sqlStr.AppendLine($"{err_cd},");
                sqlStr.AppendLine($"'{offset}',");
                sqlStr.AppendLine("'0',");
                sqlStr.AppendLine("@Line)");
            }
            else if (tableName == WK_MAIN_ERROR)
            {
                sqlStr.AppendLine($"INSERT INTO {WK_MAIN_ERROR}(");
                sqlStr.AppendLine("KANRI_NO, ERR_CD) VALUES (");
                sqlStr.AppendLine($"@KanriNo,");
                sqlStr.AppendLine($"{err_cd})");
            }

            return sqlStr.ToString();
        }
        #endregion

        #region アップデートSQL文作成
        /// <summary>
        /// アップデートSQL文作成
        /// </summary>
        /// <param name="tableName">テーブル名</param>
        /// <param name="updateData">更新用データ</param>
        /// <returns>SQL文</returns>
        public string MakeUpdateSql(string tableName)
        {
            // SQL文の生成
            StringBuilder sqlStr = new StringBuilder();

            // 現在の日時を取得
            string nowDateTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

            // ログインユーザー名取得
            string loginID = StCls_Function.GetUser();

            sqlStr.AppendLine($"UPDATE {tableName} SET");
            sqlStr.AppendLine($" ZIP_CD = @ZipCd,");
            sqlStr.AppendLine($" ADD_1 = @Add1,");
            sqlStr.AppendLine($" ADD_2 = @Add2,");
            sqlStr.AppendLine($" ADD_3 = @Add3,");
            sqlStr.AppendLine($" ADD_4 = @Add4,");
            sqlStr.AppendLine($" NAME_SEI = @NameSei,");
            sqlStr.AppendLine($" NAME_MEI = @NameMei,");
            sqlStr.AppendLine($" TEL_NO = @TelNo,");
            sqlStr.AppendLine($" ANK_1 = @Ank1,");
            sqlStr.AppendLine($" ANK_2 = @Ank2,");
            sqlStr.AppendLine($" ANK_3 = @Ank3,");
            sqlStr.AppendLine($" JYOTAI_KB = @JyotaiKb,");
            sqlStr.AppendLine($" NG_OUT_KB = @NgOutKb,");
            sqlStr.AppendLine($" HISO_OUT_KB = @HisoOutKb,");
            sqlStr.AppendLine($" UPDATE_DATETIME = '{nowDateTime}',");
            sqlStr.AppendLine($" UPDATE_LOGINID = '{loginID}'");
            sqlStr.AppendLine($" WHERE KANRI_NO = @KanriNo");

            return sqlStr.ToString();
        }
        #endregion

        #region デリートSQL文作成
        /// <summary>
        /// デリートSQL文作成
        /// </summary>
        /// <param name="tableName">テーブル名</param>
        /// <param name="kanriNo">事務局管理番号</param>
        /// <param name="offset">行番号</param>
        /// <returns>SQL文</returns>
        public string MakeDeleteSql(string tableName, string kanriNo = "", int offset = 0)
        {
            // SQL文の生成
            StringBuilder sqlStr = new StringBuilder();

            sqlStr.AppendLine($"DELETE FROM {tableName}");

            if (!string.IsNullOrEmpty(kanriNo))
            {
                sqlStr.AppendLine($" WHERE KANRI_NO = @KanriNo");
            }
            if (offset != 0)
            {
                sqlStr.AppendLine($" AND OFFSET = {offset}");
            }

            return sqlStr.ToString();
        }
        #endregion

        #region クエリ実行
        /// <summary>
        /// SQLを実行する
        /// </summary>
        /// <param name="connection">SqlConnection</param>
        /// <param name="transaction">SqlTransaction</param>
        /// <param name="query">クエリ</param>
        /// <param name="parameters">パラメータ辞書</param>
        /// <returns>true：成功 / false：失敗</returns>
        public bool Execute(SqlConnection connection, SqlTransaction transaction, string query, Dictionary<string, object> parameters)
        {
            try
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandText = query;

                    // パラメータを追加
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue(param.Key, param.Value);
                        }
                    }

                    // SQL実行
                    command.ExecuteNonQuery();

                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region レコード数取得
        public int getRecordCount(SqlConnection connection, SqlTransaction transaction, string table, string column, string filter = "")
        {
            string query = $"SELECT COUNT({column}) FROM {table}";

            if (!string.IsNullOrEmpty(filter))
            {
                query = query + $" WHERE {filter}";
            }

            using (SqlCommand command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = query;

                // 件数を返す（対象レコードが無い場合、SQL文のCOUNTは0を返すため、ExecuteScalarも0を返す）
                return (int)command.ExecuteScalar();
            }
        }
        #endregion



        #region ファイル名が重複した時の自動採番
        /// <summary>
        /// ファイル名が重複した時の自動採番
        /// </summary>
        /// <param name="intNum">採番する番号</param>
        /// <returns>空文字または採番番号</returns>
        public string NumStr(int intNum)
        {
            return intNum == 0 ? string.Empty : $" ({intNum})";
        }
        #endregion
    }
}
