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
    internal class MyClass
    {
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
                    try
                    {
                        // DataTableにデータを取得し、DataSetに追加
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
    }
}
