using System;

// ---------------------------------------------
//  クラス名   : StCls_GaijiCheck
//  概要　　　 : 外字チェック関係
//  作成日　　 : 2025/02/05
//  作成者　　 : 高橋
//  最終更新日 : 
//  最終更新者 : 
// ---------------------------------------------

namespace hagaki.StaticClass
{
    public static class StCls_GaijiCheck
    {
        private const string pb_GaijiFile = "外字定義.csv";  // 外字定義ファイル名
        private const string strSep = ",";                   // 外字定義ファイル区切り文字
        private const string strEncod = "Shift-JIS";         // 外字定義ファイル読込エンコード

        // フラグ初期化
        private static bool ReadFlg = false;

        // 配列
        public static bool[] CodeFlag = new bool[65536];  // C#は配列のサイズが1から始まるため、65535に対応するためには65536要素

        #region 列挙体

        // CSVファイル列
        public enum CSV_COLUM
        {
            RANGE_NAME = 0,  // 範囲名
            START_CD,        // 開始文字コード
            END_CD,          // 終了文字コード
            USED,            // 使用フラグ
            COMMENT,         // コメント

            MaxCol           // 最大列数
        }

        // 関数戻り値
        public enum CODE_RET
        {
            NONE = 0,    // 正常
            UN_FILE,     // ファイル不明
            WARNING      // エラーあり警告
        }

        // 使用フラグ
        public enum USE_CODE
        {
            NOCHECK = 0,  // 検査無効
            CHECK,        // 検査有効
            ERR_NULL,     // NULL値
            ERR_VARUE     // エラー
        }

        #endregion

        //**************************************************
        // 関数
        //**************************************************

        #region 関数：外字文字初期化
        //*************************************************************************************************************
        // 処理名        : 外字文字初期化
        // 処理概要      : 外字ファイルを読み込み、値を保持する
        // 引数          : なし
        // 戻り値        : True     成功
        //                 False    失敗
        // 備考          : 外字定義ファイルはINIファイルと同じ場所に置く事！！
        //*************************************************************************************************************
        public static bool Get_GaijiFileData()
        {
            // 外字定義ファイルが読み込めない場合はエラーにする
            ReadFlg = false;
            if (Code_Init(StCls_Public.pb_XmlPath + pb_GaijiFile) == (int)CODE_RET.NONE)
            {
                ReadFlg = true;
                return true;
            }
            else
            {
                ReadFlg = false;
                throw new Exception("外字定義ファイルの取得に失敗しました。");
            }
        }
        #endregion

        #region "関数：外字ファイルを読込"
        //*************************************************************************************************************
        // 処理名        : 外字ファイルを読込
        // 処理概要      : 外字ファイルを読み込み、値を保持する
        // 引数          : strPath 　　　　　定義ファイルパス
        // 戻り値        : CODE_RET.NONE     正常
        //                 CODE_RET.UN_FILE  定義ファイル不明
        //                 CODE_RET.WARNING  エラーあり
        // 備考          : 各行の詳細エラーはCODEINFOのメンバ変数use_flgに格納
        //                 USE_CODE.ERR_NULL     内容無し
        //                 USE_CODE.ERR_VARUE    内容エラー
        //*************************************************************************************************************
        private static int Code_Init(string strPath)
        {
            long lCount;
            string Buff;
            string[] Colum;

            bool flg;  // 検査フラグ

            // パス検査
            if (!StCls_File.FF_IsFile(strPath))
            {
                return (int)CODE_RET.UN_FILE;
            }

            // 行数カウント(空行は無視)
            lCount = StCls_File.GetTextLine(strPath);

            // 読み込み
            using (System.IO.StreamReader hReader = new System.IO.StreamReader(strPath, System.Text.Encoding.GetEncoding(strEncod)))
            {
                while (hReader.Peek() >= 0)
                {
                    Buff = hReader.ReadLine();
                    Colum = Buff.Split(new char[] { strSep[0] });

                    // カラム数不足
                    if (Colum.Length != (int)CSV_COLUM.MaxCol)
                    {
                        return (int)CODE_RET.WARNING;
                    }
                    else
                    {
                        // トリミング
                        for (int intCol = 0; intCol < Colum.Length; intCol++)
                        {
                            Colum[intCol] = Colum[intCol].Trim();
                            // 半角変換
                            switch (intCol)
                            {
                                case (int)CSV_COLUM.START_CD:
                                case (int)CSV_COLUM.END_CD:
                                case (int)CSV_COLUM.USED:
                                    Colum[intCol] = StCls_Function.VbStrConv(Colum[intCol], Microsoft.VisualBasic.VbStrConv.Narrow);
                                    break;
                            }
                        }

                        // 開始文字コード未設定、終了文字コード→開始文字コード
                        if (string.IsNullOrEmpty(Colum[(int)CSV_COLUM.START_CD]))
                        {
                            Colum[(int)CSV_COLUM.START_CD] = Colum[(int)CSV_COLUM.END_CD];
                        }
                        // 終了文字コード未設定、開始文字コード→終了文字コード
                        else if (string.IsNullOrEmpty(Colum[(int)CSV_COLUM.END_CD]))
                        {
                            Colum[(int)CSV_COLUM.END_CD] = Colum[(int)CSV_COLUM.START_CD];
                        }

                        // エラー検査
                        // 開始、終了文字コードが未設定
                        if (string.IsNullOrEmpty(Colum[(int)CSV_COLUM.START_CD]) && string.IsNullOrEmpty(Colum[(int)CSV_COLUM.END_CD]))
                        {
                            return (int)CODE_RET.WARNING;
                        }
                        else if (!int.TryParse(Colum[(int)CSV_COLUM.START_CD], System.Globalization.NumberStyles.HexNumber, null, out _) ||
                                !int.TryParse(Colum[(int)CSV_COLUM.END_CD], System.Globalization.NumberStyles.HexNumber, null, out _))
                        {
                            return (int)CODE_RET.WARNING;
                        }
                        else if (!StCls_Function.IsAllowChar(Colum[(int)CSV_COLUM.USED], "01", false))
                        {
                            return (int)CODE_RET.WARNING;
                        }
                    }

                    // 使用フラグが1の場合
                    flg = StCls_Function.CastInt(Colum[(int)CSV_COLUM.USED]) == (int)USE_CODE.CHECK;

                    // チェックフラグを格納
                    for (long lngCnt = Convert.ToInt32("&H" + Colum[(int)CSV_COLUM.START_CD]);
                         lngCnt <= Convert.ToInt32("&H" + Colum[(int)CSV_COLUM.END_CD]); lngCnt++)
                    {
                        CodeFlag[(int)lngCnt] = flg;
                    }
                }
            }

            return (int)CODE_RET.NONE;
        }
        #endregion

        #region "関数：外字文字検査"
        /// <summary>
        /// 外字文字検査
        /// </summary>
        /// <param name="data">検査文字列</param>
        /// <returns>正常の場合は空文字、使用不可文字がある場合はその文字が戻り値に格納される</returns>
        public static string Code_Check(string data)
        {
            if (ReadFlg)
            {
                //後続処理続行
            }
            else
            {
                throw new Exception("外字定義ファイルが読み込まれていません！");
            }

            int moji;  // 文字整数値
            string strChar = string.Empty;

            // 文字列の各文字を順番に処理
            foreach (char character in data)
            {
                // 文字コード取得
                moji = Convert.ToInt32(character.ToString(), 16);

                // チェックフラグが立っている場合
                if (CodeFlag[moji])
                {
                    // 文字列にその文字が存在しない場合のみ追加
                    if (!strChar.Contains(((char)moji).ToString()))
                    {
                        strChar += ((char)moji).ToString();
                    }
                }
            }

            return strChar;
        }
        #endregion
    }
}
