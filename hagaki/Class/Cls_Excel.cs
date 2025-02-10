using System;
using System.Collections.Generic;
using Excel = Microsoft.Office.Interop.Excel;

// ---------------------------------------------
//  クラス名   : Cls_Excel
//  概要       : エクセル関係
//  作成日　　 : 2025/02/05
//  作成者　　 : 高橋
//  最終更新日 : 
//  最終更新者 : 
// ---------------------------------------------
// アクセス　 : +Public　-Private　#Protected　
// キーワード : @MustInherits/MustOverride/Overridable　!Inherits/Implements/Overrides　*Overloads　?Shadows
// ---------------------------------------------

namespace hagaki.Class
{
    internal class Cls_Excel : IDisposable
    {
        // オブジェクト参照変数
        private Excel.Application xlApp; // エクセルアプリケーション
        private Excel.Workbooks xlBooks; // エクセルワークブックのコレクション
        private Excel.Workbook xlBook;   // エクセルワークブック
        private Excel.Sheets xlSheets;   // エクセルシートのコレクション
        private Excel.Worksheet xlSheet; // エクセルシート
        private Excel.Range xlRange;     // エクセル範囲操作用（クラス内で使いまわします！）

        // COMオブジェクト開放処理で、カウンタを強制的に0にするかのフラグ
        private const bool RELEASE_FORCED = true;

        #region ＊＊＊ 使用上の注意：excel.rangeオブジェクト ＊＊＊

        // --------------------------------------
        // ● excel.rangeオブジェクトの注意点 ●
        // --------------------------------------
        // excel.rangeオブジェクトを変数に代入した際は、
        // 必ず「Release_ComObject」メソッドでプロセスを解放してください！
        // --------------------------------------
        // 同様に、xlSheet.Cells、xlSheet.Rows、xlSheet.Columnsなど、
        // 戻り値がexcel.Rangeであるコレクションへの参照もプロセスが残りますので注意してください。
        // 仮に function が return xlSheet.Cells(r,c) であれば確実にプロセスが残ります。
        // --------------------------------------
        // 例外的に return xlSheet.Range の場合のみ、プロセスの解放は不要のようです。
        // ex. function: return xlSheet.Range(r1, c1, r2, c2)
        // --------------------------------------

        // 【注意】プロセス解放しないとメモリリークが発生する可能性があり、
        // Excelの動作が重くなったり、アプリケーションが応答しなくなることがあります。
        // 特に、Excelを複数回操作するような場合に注意が必要です。

        // 【対策】
        // 使用後は必ず「Release_ComObject」で解放することを忘れないでください。
        // 例えば、以下のようにRangeオブジェクトを操作した後に解放することが推奨されます。

        // 例:
        // Excel.Range xlRange = xlSheet.Cells[1, 1];
        // Release_ComObject(xlRange);
        // --------------------------------------

        #endregion

        // **************************************************
        // メンバ
        // **************************************************

        #region □　-　デリゲート
        // シート取得メソッドを呼び出します
        private delegate Excel.Worksheet DelegGetSheet(object index);

        // 実処理を呼び出します
        private delegate void DelegRunAction();

        // プロパティ変更処理を呼び出します
        private delegate void DelegRunSetting(XlsRangeProperty value);
        #endregion

        #region ■　-　クラス　data　stragege：　操作範囲を管理します（Excel.Range汎用）　XlsRangeAddress
        //--------------------------------------
        // 範囲の指定はXlsRangeAddressクラスを使うことに統一することでソースの再利用性を高めます！
        // 具体的には、解放処理を1箇所に集中させることが目的です。
        //--------------------------------------
        private class XlsRangeAddress
        {
            public int RowStart { get; set; }  // 開始行
            public int RowEnd { get; set; }    // 終了行
            public int ColStart { get; set; }  // 開始列
            public int ColEnd { get; set; }    // 終了列

            protected Excel.Range _xlRangeRef;    // Excel.Range参照の基点とするオブジェクト
            protected Excel.Range _xlRangeStart;  // 操作開始範囲のExcel.Range
            protected Excel.Range _xlRangeEnd;    // 操作終了範囲のExcel.Range

            #region  + method: コンストラクタ（セル指定）
            // コンストラクタ（セル指定）
            public XlsRangeAddress(int row, int col)
            {
                // セルの指定
                RowStart = row;
                RowEnd = row;
                ColStart = col;
                ColEnd = col;

                // Excel.Range参照変数を初期化します
                _xlRangeRef = null;
                _xlRangeStart = null;
                _xlRangeEnd = null;
            }
            #endregion

            #region  + method: コンストラクタ（範囲指定）
            // コンストラクタ（範囲指定）
            public XlsRangeAddress(int rowStart, int colStart, int rowEnd, int colEnd)
            {
                // 範囲の指定
                RowStart = rowStart;
                RowEnd = rowEnd;
                ColStart = colStart;
                ColEnd = colEnd;

                // Excel.Range参照変数を初期化します
                _xlRangeRef = null;
                _xlRangeStart = null;
                _xlRangeEnd = null;
            }
            #endregion

            #region  + method: 捜査範囲のExcel.Rangeへの参照を解放します
            // 捜査範囲のExcel.Rangeへの参照を解放します
            public void ReleaseReference(Cls_Excel excel)
            {
                // xls_excelのプライベート変数：lRangeを解放します
                excel.Xls_ReleaseRange();

                // Excel.Range参照変数を解放します
                excel.Xls_Release(_xlRangeEnd);
                excel.Xls_Release(_xlRangeStart);
                excel.Xls_Release(_xlRangeRef);
            }
            #endregion

            #region  + method: 操作範囲のExcel.Rangeオブジェクトを取得します
            // 操作範囲のExcel.Rangeオブジェクトを取得します
            public virtual Excel.Range CreateRange(Cls_Excel excel)
            {
                // あらかじめ参照変数を初期化しておきます
                ReleaseReference(excel);

                // 参照変数に値を設定します
                SetRange(excel);

                // 参照変数をもとに、操作範囲を返して上げます
                return excel.xlSheet.Range[_xlRangeStart, _xlRangeEnd];
            }
            #endregion

            #region  + method: 操作範囲のExcel.Rangeへの参照をプライベート変数に設定します
            // 操作範囲のExcel.Rangeへの参照をプライベート変数に設定します
            protected virtual void SetRange(Cls_Excel excel)
            {
                // Cellsへの参照を基点とします
                _xlRangeRef = excel.xlSheet.Cells;
                _xlRangeStart = (Excel.Range)_xlRangeRef[RowStart, ColStart];
                _xlRangeEnd = (Excel.Range)_xlRangeRef[RowEnd, ColEnd];
            }
            #endregion
        }
        #endregion

        #region ■　-　クラス　data　stragege：　操作範囲を管理します（Excel.Rows用）　XlsRangeAddressRow
        private class XlsRangeAddressRow : XlsRangeAddress
        {
            #region 　　+　sub：　コンストラクタ（単一行指定）
            public XlsRangeAddressRow(int row)
                : base(row, 0, row, 0) { }
            #endregion

            #region 　　+　sub：　コンストラクタ（複数行指定）
            public XlsRangeAddressRow(int rowStart, int rowEnd)
                : base(rowStart, 0, rowEnd, 0) { }
            #endregion

            #region 　　# !　sub：　操作範囲のExcel.Rangeへの参照をプライベート変数に設定します
            protected override void SetRange(Cls_Excel excel)
            {
                // Rowsへの参照を基点とします
                _xlRangeRef = excel.xlSheet.Rows;
                _xlRangeStart = (Excel.Range)_xlRangeRef[RowStart];
                _xlRangeEnd = (Excel.Range)_xlRangeRef[RowEnd];
            }
            #endregion
        }
        #endregion

        #region ■　-　クラス　data　stragege：　操作範囲を管理します（Excel.Columns用）　XlsRangeAddressCol
        private class XlsRangeAddressCol : XlsRangeAddress
        {
            #region 　　+　sub：　コンストラクタ（単一列指定）
            public XlsRangeAddressCol(int col)
                : base(0, col, 0, col) { }
            #endregion

            #region 　　+　sub：　コンストラクタ（複数列指定）
            public XlsRangeAddressCol(int colStart, int colEnd)
                : base(0, colStart, 0, colEnd) { }
            #endregion

            #region 　　# !　sub：　操作範囲のExcel.Rangeへの参照をプライベート変数に設定します
            protected override void SetRange(Cls_Excel excel)
            {
                // Columnsへの参照を基点とします
                _xlRangeRef = excel.xlSheet.Columns;
                _xlRangeStart = (Excel.Range)_xlRangeRef[ColStart];
                _xlRangeEnd = (Excel.Range)_xlRangeRef[ColEnd];
            }
            #endregion
        }
        #endregion


        #region ■　-　クラス　data　stragege：　操作範囲を管理します（Address用）　XlsRangeAddressStr
        private class XlsRangeAddressStr : XlsRangeAddress
        {
            public string Address { get; set; }  // 範囲アドレス

            #region 　　+　sub：　コンストラクタ（A1形式文字列指定）
            public XlsRangeAddressStr(string address)
                : base(0, 0)  // 0で初期化します
            {
                Address = address;  // 範囲をアドレスで指定
            }
            #endregion

            #region 　　# !　sub：　操作範囲のExcel.Rangeへの参照をプライベート変数に設定します
            protected override void SetRange(Cls_Excel excel)
            {
                // Cellsへの参照を基点とします
                _xlRangeRef = excel.xlSheet.Cells;
            }
            #endregion

            #region 　　+ !　function：　操作範囲のExcel.Rangeオブジェクトを取得します
            public override Excel.Range CreateRange(Cls_Excel excel)
            {
                // あらかじめ参照変数を初期化しておきます
                ReleaseReference(excel);
                // 参照変数をもとに、操作範囲を返して上げます
                return excel.xlSheet.Range[Address];
            }
            #endregion
        }
        #endregion

        #region ■　-　クラス　data：　操作範囲の各種プロパティ設定値を管理します　XlsRangeProperty
        private class XlsRangeProperty
        {
            // 操作対象のRange
            public object WriteValue { get; set; }

            // セル属性
            public Excel.XlPattern Pattern { get; set; }
            public Excel.XlLineStyle LineStyle { get; set; }
            public Excel.XlBordersIndex BordersIndex { get; set; }
            public int Height { get; set; }
            public string BackgroundColor { get; set; }
            public bool Hidden { get; set; }

            // フォント属性
            public int FontSize { get; set; }
            public bool FontBold { get; set; }
            public bool FontUnderline { get; set; }
            public bool FontItalic { get; set; }
            public bool FontStrikethrough { get; set; }

            // 文字指定
            public int CharactersStartIndex { get; set; }
            public int CharactersTargetLength { get; set; }
        }
        #endregion

        // **************************************************
        // イベント
        // **************************************************

        #region 　　+　sub：　コンストラクタ
        /// <summary>
        /// 初期化する
        /// </summary>
        /// <remarks></remarks>
        public Cls_Excel()
        {
            // 初期化
            Xls_Dispose();
            xlApp = null;
            xlBooks = null;
            xlBook = null;
            xlSheets = null;
            xlSheet = null;
            xlRange = null;
        }
        #endregion

        // **************************************************
        // ファイル操作
        // **************************************************

        #region 　　+　sub：　ファイルを開きます

        /// <summary>
        /// Excelファイルを開く
        /// </summary>
        /// <param name="FilePath">開くファイルパス</param>
        /// <param name="SheetName">開くシート名</param>
        /// <param name="password">エクセルのパスワード（パスワードなしならブランク）</param>
        /// <remarks></remarks>
        public void Xls_Open(string FilePath, string SheetName, string password = "")
        {
            // シート名を指定してファイルを開きます
            Helper_XlsOpen(FilePath, SheetName, password, Xls_GetSheetByName);
        }

        /// <summary>
        /// Excelファイルを開く
        /// </summary>
        /// <param name="FilePath">開くファイルパス</param>
        /// <param name="Index">開くシートインデックス</param>
        /// <param name="password">エクセルのパスワード（パスワードなしならブランク）</param>
        /// <remarks></remarks>
        public void Xls_Open(string FilePath, int Index, string password = "")
        {
            // シートインデックスを指定してファイルを開きます
            Helper_XlsOpen(FilePath, Index, password, Xls_GetSheetByIndex);
        }

        /// <summary>
        /// 新規にExcelファイルを開く
        /// </summary>
        /// <param name="Index">開くシートインデックス（既定値は１シート目）</param>
        /// <remarks></remarks>
        public void Xls_NewOpen(int Index = 1)
        {
            // 新規のExcelファイルを開きます
            Helper_XlsOpen(string.Empty, Index, "", Xls_GetSheetByName);
        }

        private void Helper_XlsOpen(string filePath, object index, string password, DelegGetSheet getSheet)
        {
            try
            {
                // プライベート変数に各エクセルオブジェクトへの参照を設定します
                xlApp = new Excel.Application();
                xlBooks = xlApp.Workbooks;

                if (filePath.Length == 0)
                {
                    // 新規のファイルを開く場合
                    xlBook = xlBooks.Add();
                }
                else
                {
                    // 既存のファイルを開く場合
                    xlBook = xlBooks.Open(filePath, Type.Missing, Type.Missing, Type.Missing, password);
                }

                xlSheets = xlBook.Worksheets;

                // IndexはObject型です。実際の型によって処理が異なります。
                // = Integer 該当するIndexを開く
                // = String 該当するシート名を開く
                xlSheet = getSheet.Invoke(index);
            }
            catch (Exception ex)
            {
                // Com参照の削除
                Xls_Close();
                throw ex;
            }
        }

        #endregion

        #region 　　+　sub：　ファイルを上書き保存します
        /// <summary>
        /// Excelファイルを上書き保存する
        /// </summary>
        /// <remarks>保存先フォルダ・ファイルへのアクセス権限が必要です</remarks>
        public void Xls_Save()
        {
            // アラート非表示
            Xls_Alerts(false);

            // 上書き保存
            xlBook.Save();

            // アラート表示
            Xls_Alerts(true);
        }
        #endregion

        #region 　　+　sub：　ファイルを名前をつけて保存します
        /// <summary>
        /// ファイルを名前をつけて保存する
        /// </summary>
        /// <param name="SaveName">保存後のファイル名</param>
        /// <param name="Password">パスワード</param>
        /// <remarks>保存先フォルダ・ファイルへのアクセス権限が必要です</remarks>
        public void Xls_SaveAs(string SaveName, string Password = null)
        {
            // アラート非表示
            Xls_Alerts(false);

            // 名前をつけて保存
            xlBook.SaveAs(SaveName, Type.Missing, Password);

            // アラート表示
            Xls_Alerts(true);
        }
        #endregion

        //**************************************************
        //シート操作
        //**************************************************

        #region 　　-　sub：　シート操作の制御メソッド
        private void Xls_RunActionSheet(object index, DelegGetSheet getSheet, DelegRunAction runAction)
        {
            // 参照変数xlSheetへの値の代入を分散させたくなかったため、
            // シート操作時は極力このメソッドを通すようにしてください。

            // ※DelegRunActionが引数なしのメソッドをポイントするため、
            // 実処理で値の設定等が必要な場合は、本メソッドの呼び出し元で必要な処理を実装してください。

            // シートの参照を取得します
            xlSheet = getSheet.Invoke(index);

            // 指定された処理を実行します
            runAction.Invoke();
        }
        #endregion

        #region 　　-　function：　シートを取得します（シート名で指定）
        private Excel.Worksheet Xls_GetSheetByName(object index)
        {
            Xls_CloseSheet();   // xlSheetの初期化処理
            return (Excel.Worksheet)xlSheets.Item[index];
        }
        #endregion

        #region    -   function：　シートを取得します（インデックスで指定）
        /// <summary>
        /// シートを取得します（インデックスで指定）
        /// </summary>
        /// <param name="index">インデックスで指定されたシート</param>
        /// <returns>指定されたインデックスのシート</returns>
        public Excel.Worksheet Xls_GetSheetByIndex(object index)
        {
            Xls_CloseSheet();   // xlSheetの初期化処理
            return (Excel.Worksheet)xlSheets.Item[index];
        }
        #endregion

        #region 　　+　function：　シート名を取得する
        /// <summary>
        /// シート名を取得する
        /// </summary>
        /// <remarks></remarks>
        public string Xls_ShtName()
        {
            return xlSheet.Name;
        }
        #endregion

        #region 　　+　function：　シート数を取得する
        /// <summary>
        /// シート数を取得する
        /// </summary>
        /// <remarks></remarks>
        public int Xls_ShtCount()
        {
            return xlSheets.Count;
        }
        #endregion

        #region 　　+　function：　シート名の存在チェックをする
        /// <summary>
        /// シート名の存在チェックをする
        /// </summary>
        /// <param name="SheetName">チェックする文字列</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool Xls_ShtNameContains(string SheetName)
        {
            bool result = false;

            foreach (Excel.Worksheet sheet in xlSheets)
            {
                // シート名が一致するかチェックします
                if (sheet.Name.ToLower() == SheetName.ToLower())
                {
                    result = true;
                }

                // プライベート変数xlSheetの参照先と異なるシートである場合のみプロセスを解放します
                // ※xlSheetと同じ参照先まで開放してしますと、xlSheetへのアクセスでエラーになります！！
                if (!object.Equals(xlSheet, sheet))
                {
                    Xls_Release(sheet);
                }

                if (result)
                {
                    break;
                }
            }

            return result;
        }
        #endregion

        #region 　　+　sub：　シートをコピーする
        /// <summary>
        /// Excelシートをコピーする
        /// </summary>
        /// <param name="sheetName">コピー元のExcelシート名</param>
        /// <param name="copyName">コピー先のExcelシート名</param>
        /// <remarks></remarks>
        public void Xls_ShtCopy(string sheetName, string copyName, bool activeCopySheet = false, bool moveLast = false)
        {
            Xls_RunActionSheet(sheetName, Xls_GetSheetByName, Xls_Run_ShtCopy);
            Helper_ShtCopy(copyName, activeCopySheet, moveLast);
        }

        /// <summary>
        /// Excelシートをコピーする
        /// </summary>
        /// <param name="Index">コピー元のExcelシートのインデックス</param>
        /// <param name="copyName">コピー先のExcelシート名</param>
        /// <remarks></remarks>
        public void Xls_ShtCopy(int index, string copyName, bool activeCopySheet = false, bool moveLast = false)
        {
            Xls_RunActionSheet(index, Xls_GetSheetByName, Xls_Run_ShtCopy);
            Helper_ShtCopy(copyName, activeCopySheet, moveLast);
        }

        private void Helper_ShtCopy(string copyName, bool activeCopySheet, bool moveLast)
        {
            string originalName = xlSheet.Name;

            // コピー先のシートをアクティブにします
            Xls_ShtActive(xlSheet.Name + " (2)");

            // コピー先のシート名を変更します
            Xls_ShtReName(copyName);

            // コピー先をアクティブにしないのであれば、コピー元をアクティブにします
            if (!activeCopySheet)
            {
                Xls_ShtActive(originalName);
            }

            // コピー先シートを末尾に移動させる必要があれば、実行します
            if (moveLast)
            {
                Xls_ShtMoveLast(copyName);
            }
        }
        #endregion

        #region 　　+　sub：　シートを削除する
        /// <summary>
        /// Excelシートを削除する
        /// </summary>
        /// <param name="SheetName">削除するExcelシート名</param>
        /// <remarks></remarks>
        public void Xls_ShtDelete(string sheetName)
        {
            Xls_RunActionSheet(sheetName, Xls_GetSheetByName, Xls_Run_ShtDelete);
        }

        /// <summary>
        /// Excelシートを削除する
        /// </summary>
        /// <param name="Index">削除するExcelシートのインデックス</param>
        /// <remarks></remarks>
        public void Xls_ShtDelete(int index)
        {
            Xls_RunActionSheet(index, Xls_GetSheetByIndex, Xls_Run_ShtDelete);
        }
        #endregion

        #region 　　+　sub：　シートをアクティブにする
        /// <summary>
        /// Excelシートをアクティブにする
        /// </summary>
        /// <param name="SheetName">アクティブにするシート名</param>
        /// <remarks></remarks>
        public void Xls_ShtActive(string sheetName)
        {
            Xls_RunActionSheet(sheetName, Xls_GetSheetByIndex, Xls_Run_ShtActive);
        }

        /// <summary>
        /// Excelシートをアクティブにする
        /// </summary>
        /// <param name="Index">アクティブにするシートのインデックス</param>
        /// <remarks></remarks>
        public void Xls_ShtActive(int index)
        {
            Xls_RunActionSheet(index, Xls_GetSheetByIndex, Xls_Run_ShtActive);
        }
        #endregion

        #region 　　+　sub：　シートを末尾に移動する
        /// <summary>
        /// Excelシートを末尾に移動する
        /// </summary>
        /// <param name="SheetName">移動するシート名</param>
        /// <remarks></remarks>
        public void Xls_ShtMoveLast(string sheetName)
        {
            Xls_RunActionSheet(sheetName, Xls_GetSheetByName, Xls_Run_ShtMoveLast);
        }

        /// <summary>
        /// Excelシートを末尾に移動する
        /// </summary>
        /// <param name="Index">移動するシートのインデックス</param>
        /// <remarks></remarks>
        public void Xls_ShtMoveLast(int index)
        {
            Xls_RunActionSheet(index, Xls_GetSheetByIndex, Xls_Run_ShtMoveLast);
        }
        #endregion

        #region 　　+　sub：　シート名を変更する
        /// <summary>
        /// 指定したExcelシート名を変更する
        /// </summary>
        /// <param name="NewName">変更後のシート名</param>
        /// <remarks></remarks>
        public void Xls_ShtReName(string newName)
        {
            if (!Xls_ShtNameContains(newName))
            {
                xlSheet.Name = newName;
            }
            else
            {
                Xls_ShtReName(newName, 2);
            }
        }

        /// <summary>
        /// 重複するシート名が存在する場合の処理
        /// </summary>
        private void Xls_ShtReName(string newName, int sheetCount)
        {
            string strNewName = newName + "_" + sheetCount;

            if (!Xls_ShtNameContains(strNewName))
            {
                xlSheet.Name = strNewName;
            }
            else
            {
                Xls_ShtReName(newName, sheetCount + 1);
            }
        }
        #endregion

        #region 　　+　function：　指定した文字列が存在するセルの列番号と行番号を取得する
        /// <summary>
        /// 指定した文字列が存在するセルの列番号と行番号を取得する
        /// </summary>
        /// <param name="stringVal">検索対象文字列</param>
        /// <param name="col">セルの列番号</param>
        /// <param name="row">セルの行番号</param>
        /// <returns>true…取得成功  false…失敗（boolean）</returns>
        /// <remarks></remarks>
        public bool Xls_GetColRowCell(string stringVal, out long col, out long row)
        {
            Excel.Range xlCells = null;
            Excel.Range xlFound = null;

            try
            {
                bool returnVal = true;
                long wkCol = 0;
                long wkRow = 0;

                // 指定した文字列が存在するセルの列番号と行番号を取得します
                wkCol = xlSheet.Cells.Find(stringVal).Column;
                wkRow = xlSheet.Cells.Find(stringVal).Row;

                xlCells = xlSheet.Cells;
                xlFound = xlCells.Find(stringVal);

                wkCol = xlFound.Column;
                wkRow = xlFound.Row;

                if (wkCol == 0 || wkRow == 0)
                {
                    // 取得失敗
                    col = 0;
                    row = 0;
                    returnVal = false;
                }
                else
                {
                    // 取得成功
                    col = wkCol;
                    row = wkRow;
                }

                return returnVal;
            }
            catch (Exception)
            {
                col = 0;
                row = 0;
                return false;
            }
            finally
            {
                Xls_Release(xlCells);
                Xls_Release(xlFound);
            }
        }
        #endregion

        // **************************************************
        // 　Excel.Range操作の汎用制御メソッド
        // **************************************************

        #region 　　-　method：　操作範囲に対して指定された処理を実行します
        // 操作範囲に対して指定された処理を実行します
        private void RunActionRange(XlsRangeAddress address, DelegRunAction runAction)
        {
            // 参照変数xlRangeへの値の代入を分散させたくなかったため、
            // シート操作時は極力このメソッドを通すようにしてください。

            // 開始と終了のアドレスを指定して、操作範囲への参照を取得します
            xlRange = address.CreateRange(this);

            // 指定された処理を実行します
            runAction.Invoke();

            // 参照の解放をおこないます
            address.ReleaseReference(this);
        }
        #endregion

        #region 　　-　method：　操作範囲に対して指定されたプロパティ設定を実行します
        // 操作範囲に対して指定されたプロパティ設定を実行します
        private void RunSettingRange(XlsRangeAddress address, XlsRangeProperty rangeProperty, DelegRunSetting runSetting)
        {
            // 参照変数xlRangeへの値の代入を分散させたくなかったため、
            // シート操作時は極力このメソッドを通すようにしてください。

            // 開始と終了のアドレスを指定して、操作範囲への参照を取得します
            xlRange = address.CreateRange(this);

            // プロパティの設定を実行します
            runSetting.Invoke(rangeProperty);

            // 参照の解放をおこないます
            address.ReleaseReference(this);
        }
        #endregion

        //**************************************************
        //セル・範囲操作
        //**************************************************

        #region 　　+　sub：　指定したセルを選択する
        /// <summary>
        /// 指定したセルを選択する
        /// </summary>
        /// <param name="Row">指定セル（行）</param>
        /// <param name="Col">指定セル（列）</param>
        /// <remarks></remarks>
        public void Xls_SelectCell(int Row, int Col)
        {
            // 戻り値
            XlsRangeAddress address = new XlsRangeAddress(Row, Col);
            RunActionRange(address, runSelect);
        }
        #endregion

        #region 　　+　function：　指定したセルの値を取得する
        /// <summary>
        /// 指定したセルの値を取得する
        /// </summary>
        /// <param name="Row">指定セル（行）</param>
        /// <param name="Col">指定セル（列）</param>
        /// <returns>セルの値（String）</returns>
        /// <remarks></remarks>
        public string Xls_ReadCell(int row, int col)
        {
            XlsRangeAddress address = new XlsRangeAddress(row, col);

            // 指定セルへの参照を取得します
            xlRange = address.CreateRange(this);

            // 指定セルの値を取得します
            string result = xlRange.Value.ToString();

            // 参照変数を解放します
            address.ReleaseReference(this);

            return result;
        }
        #endregion

        #region 　　+　function：　指定範囲の値をstring配列として取得する
        /// <summary>
        /// 指定範囲の値をstring配列として取得する
        /// </summary>
        /// <param name="sRow">読み込み開始行</param>
        /// <param name="sCol">読み込み開始列</param>
        /// <param name="eRow">読み込み終了行</param>
        /// <param name="eCol">読み込み終了列</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public string[,] Xls_ReadRange(int sRow, int sCol, int eRow, int eCol)
        {
            // 列と行のインデックスを取得する
            int iRow = eRow - sRow;
            int iCol = eCol - sCol;

            XlsRangeAddress address = new XlsRangeAddress(sRow, sCol, eRow, eCol);

            xlRange = address.CreateRange(this);

            // 範囲の値を取得します
            object[,] buff = (object[,])xlRange.Value;

            // 値を文字列に変換して再格納
            string[,] result = new string[iRow + 1, iCol + 1];

            // 範囲取得した配列のインデックスが１から始まるため、インデックスを指定する変数も加算する
            for (int intRow = 1; intRow <= iRow + 1; intRow++)
            {
                for (int intCol = 1; intCol <= iCol + 1; intCol++)
                {
                    if (string.IsNullOrEmpty(buff[intRow, intCol]?.ToString()))
                    {
                        result[intRow - 1, intCol - 1] = "";
                    }
                    else
                    {
                        result[intRow - 1, intCol - 1] = buff[intRow, intCol].ToString();
                    }
                }
            }

            // 参照の解放を行います
            address.ReleaseReference(this);

            // 戻り値
            return result;
        }
        #endregion

        #region 　　+　sub：　セル操作　指定セルに値を書込む
        /// <summary>
        /// 指定したセルに値を書き込む
        /// </summary>
        /// <param name="row">指定セル（行）</param>
        /// <param name="col">指定セル（列）</param>
        /// <param name="writeValue">指定したセルに書き込む値</param>
        /// <remarks></remarks>
        public void Xls_WriteCell(int row, int col, string writeValue)
        {
            XlsRangeAddress address = new XlsRangeAddress(row, col, row, col);

            // 配列の値をプロパティ変数にセットします
            XlsRangeProperty value = new XlsRangeProperty();
            value.WriteValue = writeValue;

            // 配列書き込みメソッドをコールバックします
            RunSettingRange(address, value, runWrite);
        }

        /// <summary>
        /// 指定したセルに値を書き込む
        /// </summary>
        /// <param name="cellAddress">セルの座標（例："A1"）、またはセル名</param>
        /// <param name="writeValue">指定したセルに書き込む値</param>
        /// <remarks></remarks>
        public void Xls_WriteCell_ByAddress(string cellAddress, string writeValue)
        {
            XlsRangeAddressStr address = new XlsRangeAddressStr(cellAddress);

            // 配列の値をプロパティ変数にセットします
            XlsRangeProperty value = new XlsRangeProperty();
            value.WriteValue = writeValue;

            // 配列書き込みメソッドをコールバックします
            RunSettingRange(address, value, runWrite);
        }
        #endregion

        #region 　　+　sub：　指定範囲に配列の値を書込む
        /// <summary>
        /// 指定範囲に配列の値を書込む
        /// </summary>
        /// <param name="sRow">範囲開始セル（行）</param>
        /// <param name="sCol">範囲開始セル（列）</param>
        /// <param name="eRow">範囲終了セル（行）</param>
        /// <param name="eCol">範囲終了セル（列）</param>
        /// <param name="writeArray">指定した範囲に書き込む配列</param>
        /// <remarks></remarks>
        public void Xls_WriteRange(int sRow, int sCol, int eRow, int eCol, Array writeArray)
        {
            XlsRangeAddress address = new XlsRangeAddress(sRow, sCol, eRow, eCol);

            // 配列の値をプロパティ変数にセットします
            XlsRangeProperty value = new XlsRangeProperty();
            value.WriteValue = writeArray;

            // 配列書き込みメソッドをコールバックします
            RunSettingRange(address, value, runWriteArray);
        }
        #endregion

        #region 　　+　sub：　指定範囲に数式配列の値を書き込みます
        public void Xls_WriteFormula(int sRow, int sCol, int eRow, int eCol, Array writeArray)
        {
            // 書き込みメソッドをコールバックします
            XlsRangeAddress address = new XlsRangeAddress(sRow, sCol, eRow, eCol);

            // 配列の値をプロパティ変数にセットします
            XlsRangeProperty value = new XlsRangeProperty();
            value.WriteValue = writeArray;

            RunSettingRange(address, value, runWriteFormulaArray);
        }
        #endregion

        #region 　　+　sub：　指定範囲を削除する
        public void Xls_RangeDelete(int row, int col)
        {
            Xls_RangeDelete(row, col, row, col);
        }

        public void Xls_RangeDelete(int sRow, int sCol, int eRow, int eCol)
        {
            // 削除メソッドを呼び出します
            XlsRangeAddress address = new XlsRangeAddress(sRow, sCol, eRow, eCol);
            RunActionRange(address, runDelete);
        }
        #endregion

        #region 　　+　sub：　指定範囲をコピーする
        public void Xls_RangeCopy(int row, int col)
        {
            Xls_RangeCopy(row, col, row, col);
        }

        public void Xls_RangeCopy(int sRow, int sCol, int eRow, int eCol)
        {
            // コピーメソッドを呼び出します
            XlsRangeAddress address = new XlsRangeAddress(sRow, sCol, eRow, eCol);
            RunActionRange(address, runCopy);
        }
        #endregion

        #region 　　+　sub：　コピーした範囲を挿入する
        public void Xls_RangeInsert(int row, int col)
        {
            Xls_RangeInsert(row, col, row, col);
        }

        public void Xls_RangeInsert(int sRow, int sCol, int eRow, int eCol)
        {
            // 挿入メソッドを呼び出します
            XlsRangeAddress address = new XlsRangeAddress(sRow, sCol, eRow, eCol);
            RunActionRange(address, runInsert);
        }
        #endregion

        #region 　　+　sub：　指定範囲を結合する
        /// <summary>
        /// 指定範囲を結合する
        /// </summary>
        /// <param name="sRow">開始行番号</param>
        /// <param name="sCol">開始列番号</param>
        /// <param name="eRow">終了行番号</param>
        /// <param name="eCol">終了列番号</param>
        /// <remarks></remarks>
        public void Xls_RangeMerge(int sRow, int sCol, int eRow, int eCol)
        {
            // 【注意】値ありの複数セルを結合する場合「いいですか？」のExcelメッセージが表示される。
            // 　　　　上記は事前にXls_Alert（False）で回避可能！

            XlsRangeAddress address = new XlsRangeAddress(sRow, sCol, eRow, eCol);
            XlsRangeProperty value = new XlsRangeProperty(); // こいつは空でOK。コールバック先で使用していない

            RunSettingRange(address, value, runMerge);
        }
        #endregion

        // **************************************************
        // 行操作
        // **************************************************

        #region 　　+　function：　指定行の値をstring配列として取得します
        /// <summary>
        /// 指定行の値をstring配列として取得する
        /// </summary>
        /// <param name="row">読み込み行</param>
        /// <param name="sCol">読み込み開始列</param>
        /// <param name="eCol">読み込み終了列</param>
        /// <returns>指定範囲の値を含む文字列配列</returns>
        public string[] Xls_ReadRow(int row, int sCol, int eCol)
        {
            // 指定範囲の値を2次元配列として読み込みます
            string[,] buff = Xls_ReadRange(row, sCol, row, eCol);

            // 1次元配列に変換して返します
            return Xls_ArrayRankDown(buff);
        }
        #endregion

        #region 　　+　sub：　指定行に配列の値を書込みます
        /// <summary>
        /// 指定行に配列の値を書き込む
        /// </summary>
        /// <param name="row">書き込み行</param>
        /// <param name="sCol">範囲開始セル（列）</param>
        /// <param name="eCol">範囲終了セル（列）</param>
        /// <param name="writeArray">指定した行に書き込む配列</param>
        public void Xls_WriteRow(int row, int sCol, int eCol, Array writeArray)
        {
            // Xls_WriteRange メソッドに処理を委譲します
            Xls_WriteRange(row, sCol, row, eCol, writeArray);
        }
        #endregion

        #region 　　+　sub：　指定行に行を挿入します
        public void Xls_RowInsert(int row)
        {
            // 挿入メソッドをコールバックします
            XlsRangeAddressRow address = new XlsRangeAddressRow(row);
            RunActionRange(address, runInsert);
        }
        #endregion

        #region    +   sub：　指定行をコピーします
        public void Xls_RowCopy(int row)
        {
            Xls_RowCopy(row, row);
        }

        public void Xls_RowCopy(int sRow, int eRow)
        {
            // コピーメソッドをコールバックします
            XlsRangeAddressRow address = new XlsRangeAddressRow(sRow, eRow);
            RunActionRange(address, runCopy);
        }
        #endregion

        #region 　　+　method：　指定行を削除します
        // 指定行を削除します
        public void Xls_RowDelete(int row)
        {
            Xls_RowDelete(row, row);
        }

        public void Xls_RowDelete(int sRow, int eRow)
        {
            // 削除メソッドをコールバックします
            XlsRangeAddressRow address = new XlsRangeAddressRow(sRow, eRow);
            RunActionRange(address, runDelete);
        }
        #endregion

        #region 　　+　method：　指定行の高さを設定します
        // 指定行の高さを設定します
        public void Xls_RowHeight(int row, int height)
        {
            Xls_RowHeight(row, row, height);
        }

        public void Xls_RowHeight(int sRow, int eRow, int height)
        {
            // 高さ変更メソッドをコールバックします
            XlsRangeAddressRow address = new XlsRangeAddressRow(sRow, eRow);

            XlsRangeProperty value = new XlsRangeProperty
            {
                Height = height
            };

            RunSettingRange(address, value, runSetRowHeight);
        }
        #endregion

        #region 　　+　method：　指定行の表示/非表示を設定します
        // 指定行の表示/非表示を設定します
        public void Xls_RowHidden(int row, bool hidden)
        {
            Xls_RowHidden(row, row, hidden);
        }

        public void Xls_RowHidden(int sRow, int eRow, bool hidden)
        {
            // 表示/非表示変更メソッドをコールバックします
            XlsRangeAddressRow address = new XlsRangeAddressRow(sRow, eRow);

            XlsRangeProperty value = new XlsRangeProperty
            {
                Hidden = hidden
            };

            RunSettingRange(address, value, runHidden);
        }
        #endregion

        // **************************************************
        // 　列操作
        // **************************************************

        #region function: 指定列の値をstring配列として取得します
        /// <summary>
        /// 指定列の値をstring配列として取得する
        /// </summary>
        /// <param name="sRow">読み込み開始行</param>
        /// <param name="eRow">読み込み終了行</param>
        /// <param name="col">読み込み列</param>
        /// <returns></returns>
        public string[] Xls_ReadCol(int sRow, int eRow, int col)
        {
            // 指定範囲の値を配列に読み込みます
            string[,] buff = Xls_ReadRange(sRow, col, eRow, col);

            // 戻り値（1次元配列に変換して返します）
            return Xls_ArrayRankDown(buff);
        }
        #endregion

        #region sub: 指定列に配列の値を書込みます
        /// <summary>
        /// 指定列に配列の値を書込む
        /// </summary>
        /// <param name="sRow">範囲開始セル（行）</param>
        /// <param name="eRow">範囲終了セル（行）</param>
        /// <param name="col">書き込み列</param>
        /// <param name="writeArray">指定した範囲に書き込む配列</param>
        public void Xls_WriteCol(int sRow, int eRow, int col, Array writeArray)
        {
            // Xls_WriteRangeメソッドに処理を委譲します
            Xls_WriteRange(sRow, col, eRow, col, writeArray);
        }
        #endregion

        #region sub: 指定列に列を挿入します
        public void Xls_ColInsert(int col)
        {
            // 挿入メソッドをコールバックします
            XlsRangeAddressCol address = new XlsRangeAddressCol(col);
            RunActionRange(address, runInsert);
        }
        #endregion

        #region sub: 指定列をコピーします
        public void Xls_ColCopy(int col)
        {
            Xls_ColCopy(col, col);
        }

        public void Xls_ColCopy(int sCol, int eCol)
        {
            // コピーメソッドをコールバックします
            XlsRangeAddressCol address = new XlsRangeAddressCol(sCol, eCol);
            RunActionRange(address, runCopy);
        }
        #endregion

        #region sub: 指定列を削除します
        public void Xls_ColDelete(int col)
        {
            Xls_ColDelete(col, col);
        }

        public void Xls_ColDelete(int sCol, int eCol)
        {
            // 削除メソッドをコールバックします
            XlsRangeAddressCol address = new XlsRangeAddressCol(sCol, eCol);
            RunActionRange(address, runDelete);
        }
        #endregion

        #region sub: 指定列の表示/非表示を設定します
        public void Xls_ColHidden(int col, bool hidden)
        {
            Xls_ColHidden(col, hidden);
        }

        public void Xls_ColHidden(int sCol, int eCol, bool hidden)
        {
            // 表示/非表示変更メソッドをコールバックします
            XlsRangeAddressCol address = new XlsRangeAddressCol(sCol, eCol);

            XlsRangeProperty value = new XlsRangeProperty
            {
                Hidden = hidden
            };

            RunSettingRange(address, value, runHidden);
        }
        #endregion

        //**************************************************
        //プロパティ操作
        //**************************************************

        #region + function: 指定セル・指定範囲の背景色を取得する
        public string Xls_GetColor(int row, int col)
        {
            return Xls_GetColor(row, col, row, col);
        }

        public string Xls_GetColor(int sRow, int sCol, int eRow, int eCol)
        {
            // 捜査範囲を取得します
            XlsRangeAddress address = new XlsRangeAddress(sRow, sCol, eRow, eCol);

            Excel.Range xlRange = address.CreateRange(this);

            // Excel.Interiorプロパティ操作の準備を行います
            Excel.Interior xlInterior = xlRange.Interior;

            // 背景色を取得します
            System.Drawing.Color value = System.Drawing.ColorTranslator.FromOle((int)xlInterior.Color);

            address.ReleaseReference(this);
            Xls_Release(xlInterior);

            // HTMLカラー形式に変換して返します
            return System.Drawing.ColorTranslator.ToHtml(value);
        }
        #endregion

        #region + function: 指定セル・指定範囲の罫線スタイルを取得する
        public Excel.XlLineStyle Xls_GetBorderStyle(int row, int col, Excel.XlBordersIndex bordersIndex)
        {
            return Xls_GetBorderStyle(row, col, row, col, bordersIndex);
        }

        public Excel.XlLineStyle Xls_GetBorderStyle(int sRow, int sCol, int eRow, int eCol, Excel.XlBordersIndex bordersIndex)
        {
            // 捜査範囲を取得します
            XlsRangeAddress address = new XlsRangeAddress(sRow, sCol, eRow, eCol);

            Excel.Range xlRange = address.CreateRange(this);

            // Excel.Bordersプロパティ操作の準備を行います
            Excel.Borders xlBorders = xlRange.Borders;
            Excel.Border xlBorder = xlBorders.Item[bordersIndex];

            // 罫線スタイルを取得します
            Excel.XlLineStyle result = (Excel.XlLineStyle)xlBorder.LineStyle;

            // エクセルプロセスを解放します
            address.ReleaseReference(this);
            Xls_Release(xlBorders);
            Xls_Release(xlBorder);

            // Excel.XlLineStyle を返します
            return result;
        }
        #endregion

        #region + function: 指定セル・指定範囲の罫線色を取得する
        public string Xls_GetBorderColor(int row, int col, Excel.XlBordersIndex bordersIndex)
        {
            return Xls_GetBorderColor(row, col, row, col, bordersIndex);
        }

        public string Xls_GetBorderColor(int sRow, int sCol, int eRow, int eCol, Excel.XlBordersIndex bordersIndex)
        {
            // 捜査範囲を取得します
            XlsRangeAddress address = new XlsRangeAddress(sRow, sCol, eRow, eCol);

            Excel.Range xlRange = address.CreateRange(this);

            // Excel.Bordersプロパティ操作の準備を行います
            Excel.Borders xlBorders = xlRange.Borders;
            Excel.Border xlBorder = xlBorders.Item[bordersIndex];

            // 罫線色を取得します
            System.Drawing.Color result = System.Drawing.ColorTranslator.FromOle((int)xlBorder.Color);

            address.ReleaseReference(this);
            Xls_Release(xlBorders);
            Xls_Release(xlBorder);

            // HTMLカラー形式に変換して返します
            return System.Drawing.ColorTranslator.ToHtml(result);
        }
        #endregion

        #region + sub: 指定セル・指定範囲の背景色を変更する
        public void Xls_BackColor(int Row, int Col, string htmlColor)
        {
            Xls_BackColor(Row, Col, Row, Col, htmlColor);
        }

        public void Xls_BackColor(int sRow, int sCol, int eRow, int eCol, string htmlColor)
        {
            XlsRangeAddress address = new XlsRangeAddress(sRow, sCol, eRow, eCol);

            XlsRangeProperty value = new XlsRangeProperty
            {
                BackgroundColor = htmlColor
            };

            RunSettingRange(address, value, runSetBackColor);
        }

        public void Xls_BackColor(string strAddress, string htmlColor)
        {
            XlsRangeAddressStr address = new XlsRangeAddressStr(strAddress);

            XlsRangeProperty value = new XlsRangeProperty
            {
                BackgroundColor = htmlColor
            };

            RunSettingRange(address, value, runSetBackColor);
        }
        #endregion

        #region + sub: 指定セル・指定範囲の背景パターンを変更する
        public void Xls_BackPattern(int row, int col, Excel.XlPattern pattern)
        {
            Xls_BackPattern(row, col, row, col, pattern);
        }

        public void Xls_BackPattern(int sRow, int sCol, int eRow, int eCol, Excel.XlPattern pattern)
        {
            XlsRangeAddress address = new XlsRangeAddress(sRow, sCol, eRow, eCol);

            XlsRangeProperty value = new XlsRangeProperty
            {
                Pattern = pattern
            };

            RunSettingRange(address, value, runSetPattern);
        }

        public void Xls_BackPattern(string strAddress, Excel.XlPattern pattern)
        {
            XlsRangeAddressStr address = new XlsRangeAddressStr(strAddress);

            XlsRangeProperty value = new XlsRangeProperty
            {
                Pattern = pattern
            };

            RunSettingRange(address, value, runSetPattern);
        }
        #endregion

        #region sub: 指定セル・指定範囲の罫線を変更する
        public void Xls_Border(int row, int col, Excel.XlLineStyle lineStyle, Excel.XlBordersIndex bordersIndex = (Excel.XlBordersIndex)(-1))
        {
            Xls_Border(row, col, lineStyle, bordersIndex);
        }

        public void Xls_Border(int sRow, int sCol, int eRow, int eCol, Excel.XlLineStyle lineStyle, Excel.XlBordersIndex bordersIndex = (Excel.XlBordersIndex)(-1))
        {
            XlsRangeAddress address = new XlsRangeAddress(sRow, sCol, eRow, eCol);

            XlsRangeProperty value = new XlsRangeProperty
            {
                LineStyle = lineStyle,
                BordersIndex = bordersIndex
            };

            RunSettingRange(address, value, runSetBorder);
        }
        #endregion

        #region + sub: 指定セル・指定範囲のフォントサイズを変更します
        public void Xls_FontSize(int row, int col, int fontSize)
        {
            Xls_FontSize(row, col, row, col, fontSize);
        }

        public void Xls_FontSize(int sRow, int sCol, int eRow, int eCol, int fontSize)
        {
            XlsRangeAddress address = new XlsRangeAddress(sRow, sCol, eRow, eCol);

            XlsRangeProperty value = new XlsRangeProperty
            {
                FontSize = fontSize
            };

            RunSettingRange(address, value, runSetFontSize);
        }
        #endregion

        #region + sub: 指定セル・指定範囲のフォント太字を変更します
        public void Xls_FontBold(int row, int col, bool fontBold)
        {
            Xls_FontBold(row, col, row, col, fontBold);
        }

        public void Xls_FontBold(int sRow, int sCol, int eRow, int eCol, bool fontBold)
        {
            XlsRangeAddress address = new XlsRangeAddress(sRow, sCol, eRow, eCol);

            XlsRangeProperty value = new XlsRangeProperty
            {
                FontBold = fontBold
            };

            RunSettingRange(address, value, runSetFontBold);
        }
        #endregion

        #region + sub: セルの書式を文字列に設定します
        public void Xls_CellType_String()
        {
            Excel.Range xlCells = xlSheet.Cells;

            xlCells.Select();
            xlCells.NumberFormatLocal = "@";

            // 参照変数を解放します
            Xls_Release(xlCells);
        }
        #endregion

        #region + sub: 列の幅や行の高さを内容に合わせて調節します
        public void Xls_AutoFit()
        {
            Excel.Range xlCells = xlSheet.Cells;

            xlCells.Select();
            xlCells.EntireColumn.AutoFit();

            // 参照変数を解放します
            Xls_Release(xlCells);
        }
        #endregion

        #region + sub: セル内文字のスタイルを設定します
        /// <summary>
        /// セル内文字のスタイルを設定します
        /// </summary>
        /// <param name="row">行番号</param>
        /// <param name="col">列番号</param>
        /// <param name="charactersStartIndex">セル内文字の操作開始インデックス（0～）</param>
        /// <param name="charactersTargetLength">セル内文字の操作文字数（1～）</param>
        /// <param name="bold">太字</param>
        /// <param name="underline">下線</param>
        /// <param name="italic">斜体</param>
        /// <param name="strikethrough">打ち消し線</param>
        /// <remarks></remarks>
        public void Xls_CharactersStyle(int row, int col, int charactersStartIndex, int charactersTargetLength,
                                        bool bold = false, bool underline = false, bool italic = false, bool strikethrough = false)
        {
            XlsRangeAddress address = new XlsRangeAddress(row, col, row, col);

            XlsRangeProperty value = new XlsRangeProperty
            {
                // セル内文字
                CharactersStartIndex = charactersStartIndex,
                CharactersTargetLength = charactersTargetLength,

                // 文字スタイル
                FontBold = bold,
                FontUnderline = underline,
                FontItalic = italic,
                FontStrikethrough = strikethrough
            };

            RunSettingRange(address, value, runSetCharacterStyle);
        }
        #endregion

        //**************************************************
        //アドレス関係
        //**************************************************

        #region + function: エクセルが認識する最終行の行番号を取得する
        public int Xls_GetLastRowNo()
        {
            // 行末尾への参照を取得します
            Excel.Range xlCells = xlSheet.Range["$A$65536"];

            // 行末尾からさかのぼり、エクセルが認識する最終行の参照を取得します
            Excel.Range xlEnd = xlCells.End[Excel.XlDirection.xlUp];

            // 最終行の行番号を取得します
            int result = xlEnd.Row;

            // 参照の解放を行います
            Xls_Release(xlCells);
            Xls_Release(xlEnd);

            return result;
        }
        #endregion

        #region 　　+　function：　エクセルが認識する最終列の列番号を取得する
        public int Xls_GetLastColNo()
        {
            // 最終列への参照を取得します
            Excel.Range xlCells = xlSheet.Cells;
            Excel.Range xlEnd = xlCells.SpecialCells(Excel.XlCellType.xlCellTypeLastCell);

            // 最終列の列番号を取得します
            int result = xlEnd.Column;

            // 参照の解放をおこないます
            Release_ComObject(ref xlCells, false);
            Release_ComObject(ref xlEnd, false);

            return result;
        }
        #endregion

        #region - function: 指定範囲を取得する（★封印します！★)
        //----------------------------------------------------------
        // Excel.Rangeの作成・解放を安全に行うためこのメソッドは使いません！
        //----------------------------------------------------------
        // ★　Excel.Rangeの安全な作り方　★
        //
        // ① まずはCells、Rowsなどのコレクションへの参照を変数に代入します（一次参照） ex. xlCells = xlSheet.Cells
        // ② Row、Colで値を取得する際も、つど変数に代入してください（二次参照）   ex. startRange = xlCells[row,col]
        // ③ このタイミングでお目当てのExcel.Rangeが取得できると思います（三次参照）　ex. xlRange = xlCells[startRange,endRange]
        // ④ たとえば一次参照が解放された状態で、二次・三次参照にアクセスするとエラーになります。
        //     変数の解放は、　三次参照 →　二次参照 → 一次参照　の順でおこなうようにしてください.
        //
        // 以上、めんどくさいですが気をつけましょう！
        //----------------------------------------------------------

        // 以下のメソッドは使用しません！
        //
        // // GetRange（単一セルの取得）
        // private Excel.Range GetRange(int row, int col)
        // {
        //     // セルの参照を取得します
        //     return GetRange(row, col, row, col);
        // }
        //
        // // GetRange（範囲の取得）
        // private Excel.Range GetRange(int sRow, int sCol, int eRow, int eCol)
        // {
        //     // セル範囲の参照を取得します
        //     return xlSheet.Range[Xls_GetAddress(sRow, sCol), Xls_GetAddress(eRow, eCol)];
        // }
        //
        // // GetRange（文字列アドレスから範囲を取得）
        // private Excel.Range GetRange(string address)
        // {
        //     // 指定アドレスの参照を取得します
        //     return xlSheet.Range[address];
        // }
        //
        // // GetRange（XlsRangeAddressから範囲を取得）
        // private Excel.Range GetRange(XlsRangeAddress rangeAddress)
        // {
        //     if (rangeAddress.IsString)
        //     {
        //         return GetRange(rangeAddress.Address);
        //     }
        //     else
        //     {
        //         return GetRange(rangeAddress.RowStart, rangeAddress.ColStart, rangeAddress.RowEnd, rangeAddress.ColEnd);
        //     }
        // }
        #endregion

        #region + function: A1形式のアドレスを取得する
        public string Xls_GetAddress(int row, int col)
        {
            return Xls_GetAddress(row, col, row, col);
        }

        public string Xls_GetAddress(int sRow, int sCol, int eRow, int eCol)
        {
            string result = "";

            // アドレスオブジェクトを作成します
            XlsRangeAddress address = new XlsRangeAddress(sRow, sCol, eRow, eCol);

            // 操作範囲を取得します
            xlRange = address.CreateRange(this);

            // A1形式のアドレスを取得します
            result = xlRange.Address;

            // オブジェクトを解放します
            address.ReleaseReference(this);

            return result;
        }
        #endregion

        // **************************************************
        // 　処理実行（基本的にコールバックで呼び出します）
        // **************************************************

        #region sub callback: xlSheetを削除します
        private void Xls_Run_ShtDelete()
        {
            // 削除に備えて、アラートを非表示にします
            Xls_Alerts(false);

            // シートの削除
            xlSheet.Delete();

            // アラート表示をもとに戻します
            Xls_Alerts(true);
        }
        #endregion

        #region sub callback: xlSheetを選択します
        private void Xls_Run_ShtActive()
        {
            // 現在のシートをアクティブにします
            xlSheet.Activate();
        }
        #endregion

        #region sub callback: xlSheetを末尾に移動します
        private void Xls_Run_ShtMoveLast()
        {
            // シートを末尾に移動します
            xlSheet.Move(Type.Missing, xlSheets.Item[xlSheets.Count]);
        }
        #endregion

        #region sub callback: xlSheetをコピーします
        private void Xls_Run_ShtCopy()
        {
            // シートをコピーします
            xlSheet.Copy(xlSheet);
        }
        #endregion

        #region sub callback: xlRangeに値を書き込みます
        private void runWrite(XlsRangeProperty value)
        {
            // 値を書き込む
            xlRange.Value = value.WriteValue;
        }
        #endregion

        #region sub callback: xlRangeに配列を書き込みます
        private void runWriteArray(XlsRangeProperty value)
        {
            Array aryValue = (Array)value.WriteValue;

            // 値を書き込む
            if (aryValue.Rank == 1)
            {
                xlRange.Value = value.WriteValue;
            }
            else
            {
                // 行数と列数を取得
                int rowCount = aryValue.GetLength(0);  // 行数
                int colCount = aryValue.GetLength(1);  // 列数

                // Resizeメソッドを使って範囲を拡張
                Excel.Range xlResize = xlRange.Resize[rowCount, colCount];  // 行数、列数を指定
                xlResize.Value = value.WriteValue;  // 値を書き込む
                Xls_Release(xlResize);  // 範囲の解放
            }
        }
        #endregion

        #region sub callback: xlRangeに数式を書き込みます
        private void runWriteFormula(XlsRangeProperty value)
        {
            // 数式を書き込む
            xlRange.Formula = value.WriteValue;
        }
        #endregion

        #region sub callback: xlRangeに数式配列を書き込みます
        private void runWriteFormulaArray(XlsRangeProperty value)
        {
            Array aryValue = (Array)value.WriteValue;

            // 数式を書き込む
            if (aryValue.Rank == 1)
            {
                xlRange.FormulaArray = value;
            }
            else
            {
                int rowCount = aryValue.GetLength(0);
                int colCount = aryValue.GetLength(1);

                Excel.Range xlResize = xlRange.Resize[rowCount, colCount];
                xlResize.FormulaArray = aryValue;
                Xls_Release(xlResize);
            }
        }
        #endregion

        #region sub callback: xlRangeを削除します
        private void runDelete()
        {
            // 削除する
            xlRange.Delete();
        }
        #endregion

        #region sub callback: xlRangeをコピーします
        private void runCopy()
        {
            // コピーする
            xlRange.Copy();
        }
        #endregion

        #region sub callback: xlRangeを挿入します
        private void runInsert()
        {
            // 挿入する
            xlRange.Insert();
        }
        #endregion

        #region sub callback: xlRangeを選択します
        private void runSelect()
        {
            // 選択する
            xlRange.Select();
        }
        #endregion

        #region sub callback: xlRangeの表示/非表示を設定します
        private void runHidden(XlsRangeProperty value)
        {
            // 削除する
            xlRange.Hidden = value.Hidden;
        }
        #endregion

        #region sub callback: xlRangeの行の高さを設定します
        private void runSetRowHeight(XlsRangeProperty value)
        {
            xlRange.RowHeight = value.Height;
        }
        #endregion

        #region sub callback: xlRangeの罫線を設定します
        private void runSetBorder(XlsRangeProperty value)
        {
            // 操作範囲の内部情報への参照を取得します
            Excel.Borders xlBorders = xlRange.Borders;
            Excel.Border xlBorder = null;

            // 罫線ポジションの指定があれば、指定された箇所のみ設定します
            if ((int)value.BordersIndex == -1)
            {
                xlBorders.LineStyle = value.LineStyle;
            }
            else
            {
                xlBorder = xlBorders[value.BordersIndex];
                xlBorder.LineStyle = value.LineStyle;
            }

            // 参照変数を解放します
            Xls_Release(xlBorders);
            Xls_Release(xlBorder);
        }
        #endregion

        #region sub callback: xlRangeのフォントサイズを設定します
        private void runSetFontSize(XlsRangeProperty value)
        {
            // 操作範囲の内部情報への参照を取得します
            Excel.Font xlFont = xlRange.Font;

            // フォントサイズを変更します
            xlFont.Size = value.FontSize;

            // 参照変数を解放します
            Xls_Release(xlFont);
        }
        #endregion

        #region sub callback: xlRangeのフォント太さを設定します
        private void runSetFontBold(XlsRangeProperty value)
        {
            // 操作範囲の内部情報への参照を取得します
            Excel.Font xlFont = xlRange.Font;

            // フォント太さを変更します
            xlFont.Bold = value.FontBold;

            // 参照変数を解放します
            Xls_Release(xlFont);
        }
        #endregion

        #region sub callback: xlRangeのセルパターンを設定します
        private void runSetPattern(XlsRangeProperty value)
        {
            // 操作範囲の内部情報への参照を取得します
            Excel.Interior xlInterior = xlRange.Interior;

            // 背景パターンを変更します
            xlInterior.Pattern = (Excel.XlPattern)value.Pattern;

            // 参照変数を解放します
            Xls_Release(xlInterior);
        }
        #endregion

        #region sub callback: xlRangeの背景色を設定します
        private void runSetBackColor(XlsRangeProperty value)
        {
            // HTML指定された色を取得します
            System.Drawing.Color sysColor = System.Drawing.ColorTranslator.FromHtml((string)value.BackgroundColor);

            // 操作範囲の内部情報への参照を取得します
            Excel.Interior xlInterior = xlRange.Interior;

            // 背景色を変更します
            xlInterior.Color = System.Drawing.ColorTranslator.ToOle(sysColor);

            // 参照変数を解放します
            Xls_Release(xlInterior);
        }
        #endregion

        #region sub callback: xlRange.Charactersを設定します
        private void runSetCharacterStyle(XlsRangeProperty value)
        {
            int startIndex = value.CharactersStartIndex;
            int length = value.CharactersTargetLength;

            // 操作範囲の内部情報への参照を取得します
            Excel.Characters xlCharacters = xlRange.Characters[startIndex, length];
            Excel.Font xlFont = xlCharacters.Font;

            xlFont.Bold = value.FontBold;
            xlFont.Underline = value.FontUnderline;
            xlFont.Italic = value.FontItalic;
            xlFont.Strikethrough = value.FontStrikethrough;

            // 参照変数を解放します
            Xls_Release(xlFont);
            Xls_Release(xlCharacters);
        }
        #endregion

        #region sub callback: xlRangeを結合します
        private void runMerge(XlsRangeProperty value)
        {
            // 結合する
            xlRange.Merge();
        }
        #endregion

        // **************************************************
        // 　設定
        // **************************************************

        #region sub: 設定 アプリケーション表示設定をする
        /// <summary>
        /// Excelアプリケーションを画面に表示するかどうか
        /// </summary>
        /// <param name="isVisible">true=表示　false=非表示</param>
        /// <remarks></remarks>
        public void Xls_Visible(bool isVisible)
        {
            xlApp.Visible = isVisible;
        }
        #endregion

        #region sub: 設定 システムメッセージの表示設定をする
        /// <summary>
        /// Excelシステムメッセージを画面に表示するかどうか
        /// </summary>
        /// <param name="isAlerts">true=表示　false=非表示</param>
        /// <remarks></remarks>
        public void Xls_Alerts(bool isAlerts)
        {
            xlApp.DisplayAlerts = isAlerts;
        }
        #endregion

        // **************************************************
        // 　ユーティリティ
        // **************************************************

        #region function: 2次元配列を1次元配列に変換して返します
        private string[] Xls_ArrayRankDown(string[,] buff)
        {
            List<string> result = new List<string>(buff.GetLength(1));

            foreach (string value in buff)
            {
                result.Add(value);
            }

            // 戻り値
            return result.ToArray();
        }
        #endregion

        // **************************************************
        // 　COMオブジェクトのプロセス解放制御
        // **************************************************

        #region sub: アプリケーションを解放する
        /// <summary>
        /// Excelアプリケーションを解放する
        /// </summary>
        /// <remarks></remarks>
        public void Xls_Close()
        {
            // エクセルを閉じる
            Xls_ReleaseRange();
            Xls_CloseSheets();
            Xls_CloseBooks();
            Xls_CloseApp();
        }
        #endregion

        #region sub: オブジェクトを解放する
        /// <summary>
        /// Excelオブジェクトを解放する
        /// </summary>
        /// <remarks></remarks>
        public void Xls_Dispose()
        {
            // 全オブジェクトの解放
            Xls_Release(xlRange);
            Xls_Release(xlSheet);
            Xls_Release(xlSheets);
            Xls_Release(xlBook);
            Xls_Release(xlBooks);
            Xls_Release(xlApp);
        }
        #endregion

        #region sub: アプリケーション（xlApp）を閉じる
        // UPDBY 2014/10/28 matsuda Try句を追加
        private void Xls_CloseApp()
        {
            try
            {
                // アプリケーションを閉じる
                xlApp.Quit();
            }
            catch (Exception)
            {
                // xlAppのCom解放された後Nothingにならない。
                // つねにxlApp.Quit実行するしかないため、Catchで対応。
            }

            // プロセスを解放
            Xls_Release(xlApp);
        }
        #endregion

        #region sub: エクセルブック（xlBooks）を閉じる
        private void Xls_CloseBooks()
        {
            // プロセスを解放する
            Xls_CloseBook();
            Xls_Release(xlBooks);
        }
        #endregion

        #region sub: エクセルブック（xlBook）を閉じる
        private void Xls_CloseBook()
        {
            try
            {
                // ブックを閉じる
                xlBook.Close(false);
            }
            catch (Exception)
            {
                // エラーハンドリング（特に処理なし）
            }

            // プロセスを解放する
            Xls_Release(xlBook);
        }
        #endregion

        #region sub: エクセルシート（xlSheets）を閉じる
        private void Xls_CloseSheets()
        {
            Xls_CloseSheet();
            // プロセスを解放する
            Xls_Release(xlSheets);
        }
        #endregion

        #region sub: エクセルシート（xlSheet）を閉じる
        private void Xls_CloseSheet()
        {
            // プロセスを解放する
            Xls_Release(xlSheet);
        }
        #endregion

        #region sub: 操作範囲（xlRange）を解放します
        private void Xls_ReleaseRange()
        {
            // プロセスを解放する
            Xls_Release(xlRange);
        }
        #endregion

        #region sub: 解放制御のヘルパーメソッド
        private void Xls_Release(object obj)
        {
            Release_ComObject(ref obj, RELEASE_FORCED);
        }
        #endregion

        #region sub: COM オブジェクトのプロセスを解放
        private void Release_ComObject<T>(ref T objCom, bool force = false) where T : class
        {
            // オブジェクトの存在チェック
            if (objCom == null)
            {
                return;
            }

            try
            {
                // プロセスの解放処理
                if (System.Runtime.InteropServices.Marshal.IsComObject(objCom))
                {
                    if (force)
                    {
                        // 参照カウントを0に変更
                        System.Runtime.InteropServices.Marshal.FinalReleaseComObject(objCom);
                    }
                    else
                    {
                        int count = System.Runtime.InteropServices.Marshal.ReleaseComObject(objCom);
                        // Debug.WriteLine(count);  // 0以上が表示されたら、解放されていない分がある
                    }
                }
            }
            finally
            {
                objCom = null;
            }
        }
        #endregion

        // **************************************************
        // 　IDisposable（Dispose時に参照全解放）
        // **************************************************

        /// <summary>
        /// アプリケーションをCloseしてDisposeする。
        /// </summary>
        /// <remarks></remarks>
        public void Dispose()
        {
            // IDisposable.Disposeの実装
            Xls_Close();
            Xls_Dispose();
        }

        // **************************************************
        // 　バーコードコントロール操作
        // **************************************************

        #region  + sub: バーコードコントロール貼り付け

        // ※※注意事項※※※※※※※※※※※※※※※※※※※※※※※※
        // ※　既にバーコードが存在するシートに処理を行う場合、
        // ※　Excelが一時的に立ち上がりますが、バグではありません。
        // ※※※※※※※※※※※※※※※※※※※※※※※※※※※※※※

        /// <summary>
        /// バーコードコントロール貼り付け
        /// 　※Excel2010、Excel2002 動作確認済み
        /// </summary>
        /// <param name="BarCodeLinkedCell">リンクセルアドレス 例："A1"</param>
        /// <param name="BarCodeStyle">バーコードスタイル(0～10)　2:JAN-13 6:Code-39 7:Code-128 </param>
        /// <param name="BarCodeLeft">バーコード貼り付け位置 (左から)</param>
        /// <param name="BarCodeTop">バーコード貼り付け位置 (上から)</param>
        /// <param name="BarCodeWidth">バーコードの幅</param>
        /// <param name="BarCodeHeight">バーコードの高さ</param>
        /// <param name="BarCodeShowData">データ表示　0: 無し(デフォルト値), 1: 有り</param>
        /// <param name="BarCodeLineWeight">線の太さ　0:極細～3:標準(デフォルト値)～7:超極太</param>
        /// <remarks>
        /// バーコードスタイル一覧
        /// 0 - UPC-A
        /// 1 - UPC-E
        /// 2 - JAN-13
        /// 3 - JAN-8
        /// 4 - Casecode
        /// 5 - NW-7
        /// 6 - Code-39
        /// 7 - Code-128
        /// 8 - US Postnet
        /// 9 - US Postal FIM
        /// 10 - カスタマバーコード
        /// </remarks>
        public void PasteBarCodeCtrl(string BarCodeLinkedCell,
                                      int BarCodeStyle,
                                      int BarCodeLeft,
                                      int BarCodeTop,
                                      int BarCodeWidth,
                                      int BarCodeHeight,
                                      int BarCodeShowData = 0,
                                      int BarCodeLineWeight = 3)
        {
            Excel.OLEObject objOLEObj = null;
            Excel.OLEObjects objOLEObjs = null;

            try
            {
                objOLEObjs = xlSheet.OLEObjects() as Excel.OLEObjects;

                // Addメソッドを使ってOLEオブジェクトを追加
                objOLEObj = objOLEObjs.Add(
                    ClassType: "BARCODE.BarCodeCtrl.1",     // ClassType
                    Filename: Type.Missing,                 // Filename (指定しない場合は Missing を使用)
                    Link: false,                            // Link
                    DisplayAsIcon: false,                   // DisplayAsIcon
                    IconFileName: Type.Missing,             // IconFileName (指定しない場合は Missing を使用)
                    IconIndex: Type.Missing,                // IconIndex (指定しない場合は Missing を使用)
                    IconLabel: Type.Missing,                // IconLabel (指定しない場合は Missing を使用)
                    Left: BarCodeLeft,                      // Left
                    Top: BarCodeTop,                        // Top
                    Width: BarCodeWidth,                    // Width
                    Height: BarCodeHeight                   // Height
                );

                // Excelを表示しない
                xlApp.Visible = false;
                xlApp.ScreenUpdating = false;

                dynamic barcodeObject = objOLEObj.Object;
                barcodeObject.Style = BarCodeStyle;           // スタイル
                barcodeObject.SubStyle = 0;                   // サブスタイル
                barcodeObject.Validation = 0;                 // データの確認
                barcodeObject.LineWeight = BarCodeLineWeight; // 線の太さ
                barcodeObject.Direction = 0;                  // バーコードの向き
                barcodeObject.ShowData = BarCodeShowData;     // データの表示
                barcodeObject.ForeColor = 0;                  // 前景色
                barcodeObject.BackColor = 16777215;           // 背景色
                barcodeObject.Refresh();                      // 最描写

                objOLEObj.Left = BarCodeLeft;                             // 左から出力位置(ポイント指定)
                objOLEObj.Top = BarCodeTop;                               // 上から出力位置(ポイント指定)
                objOLEObj.Width = BarCodeWidth;                           // 出力幅指定
                objOLEObj.Height = BarCodeHeight;                         // 出力高さ指定
                objOLEObj.LinkedCell = BarCodeLinkedCell;                 // データ取得セル設定
                objOLEObj.Placement = Excel.XlPlacement.xlMoveAndSize;    // 描画後の「セル高」変更に対し「バーコード高」が連動しない可能性があるので念の為
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Release_ComObject(ref objOLEObj);
                Release_ComObject(ref objOLEObjs);
            }
        }

        #endregion

        // **************************************************
        // 　エクセル印刷
        // **************************************************

        #region  + function: エクセル印刷（ブック単位）
        /// <summary>
        /// エクセル印刷（ブック単位）
        /// </summary>
        /// <param name="intFrom">印刷開始ページ番号 デフォルト値:最初のページ(1～)</param>
        /// <param name="intTo">印刷終了ページ番号 デフォルト値:最後のページ</param>
        /// <param name="copies">印刷部数 デフォルト値:1</param>
        /// <param name="preview">印刷プレビュー有無 デフォルト値:False</param>
        /// <param name="printer">プリンタの指定 デフォルト値:アクティブなプリンタ</param>
        /// <returns>戻り値：String　ブランク:正常　その他:エラー</returns>
        /// <remarks>
        /// ※印刷終了ページ番号
        /// 最大ページ数を超えた場合も最後のページまで印刷されます。
        /// ※プリンタの指定
        /// 存在しないプリンタが指定されてもエラーは発生しません。
        /// </remarks>
        public string BookPrint(int intFrom = 0,
                                int intTo = 0,
                                int copies = 1,
                                bool preview = false,
                                string printer = "")
        {
            try
            {
                if (!xlApp.Visible && preview)
                {
                    // プレビュー有の場合Excel表示
                    xlApp.Visible = true;
                }
                else
                {
                    // Excelを表示しない
                    xlApp.Visible = false;
                    xlApp.ScreenUpdating = false;
                }

                // プリンタの設定
                if (string.IsNullOrEmpty(printer))
                {
                    printer = xlApp.ActivePrinter;
                }

                // 印刷
                if (intFrom != 0 && intTo == 0)
                {
                    // Fromのみ指定あり
                    xlBook.PrintOut(intFrom, Type.Missing, copies, preview, printer);
                }
                else if (intFrom == 0 && intTo != 0)
                {
                    // Toのみ指定あり
                    xlBook.PrintOut(Type.Missing, intTo, copies, preview, printer);
                }
                else if (intFrom != 0 && intTo != 0)
                {
                    // From～To指定あり
                    xlBook.PrintOut(intFrom, intTo, copies, preview, printer);
                }
                else
                {
                    // 全て印刷
                    xlBook.PrintOut(Type.Missing, Type.Missing, copies, preview, printer);
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        #endregion

        #region  + function: エクセル印刷（シート名指定）
        /// <summary>
        /// エクセル印刷（シート名指定）
        /// </summary>
        /// <param name="sheetName">印刷するシート名orシート番号(1～)</param>
        /// <param name="copies">印刷部数 デフォルト値:1</param>
        /// <param name="preview">印刷プレビュー有無 デフォルト値:False</param>
        /// <param name="printer">プリンタの指定 デフォルト値:アクティブなプリンタ</param>
        /// <returns>戻り値：String　ブランク:正常　その他:エラー</returns>
        /// <remarks>
        /// ※プリンタの指定
        /// 存在しないプリンタが指定されてもエラーは発生しません。
        /// </remarks>
        public string SheetPrint(object sheetName,
                                 int copies = 1,
                                 bool preview = false,
                                 string printer = "")
        {
            try
            {
                if (!xlApp.Visible && preview)
                {
                    // プレビュー有の場合Excel表示
                    xlApp.Visible = true;
                }
                else
                {
                    // Excelを表示しない
                    xlApp.Visible = false;
                    xlApp.ScreenUpdating = false;
                }

                // プリンタの設定
                if (string.IsNullOrEmpty(printer))
                {
                    printer = xlApp.ActivePrinter;
                }

                // 印刷
                xlBook.Worksheets[sheetName].PrintOut(Type.Missing, Type.Missing, copies, preview, printer);

                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        #endregion
    }
}
