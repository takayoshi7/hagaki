using System;
using System.Security.Permissions;
using System.Xml;

// ---------------------------------------------
//  クラス名   : Cls_Xml
//  概要       : XMLファイル操作
//  作成日　　 : 2025/02/05
//  作成者　　 : 高橋
//  最終更新日 : 
//  最終更新者 : 
// ---------------------------------------------

namespace hagaki.Class
{
    internal class Cls_Xml
    {
        // XMLの実体
        // Newでセットされます。
        // New失敗でも、何かしらの値で初期化されてしまいます。（ファイルパス間違ってても空初期化確認）
        // IsInitializedプロパティで正常に初期化されたか判定してください。
        private XmlDocument _xmlDoc;

        // 正常に初期化されたかの判定用
        private bool _isInitialize;

        /// <summary>
        /// 正常に初期化されたかを判定する（正常なXMLが読み込まれたか）
        /// </summary>
        public bool IsInitialized
        {
            get { return _isInitialize; }
        }

        #region 　　+　Sub：　コンストラクタ
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="xmlFilePath">読み込むXMLファイルのパス</param>
        public Cls_Xml(string xmlFilePath)
        {
            try
            {
                // XMLファイル読み込み
                FileIOPermission f = new FileIOPermission(PermissionState.None);
                f.AllLocalFiles = FileIOPermissionAccess.Read;

                _xmlDoc = new XmlDocument();
                _xmlDoc.Load(xmlFilePath);

                // New成功
                _isInitialize = true;
            }
            catch (Exception)
            {
                // FIXME 2014/10/06 matsuda Throw Ex したほうがいいかも？
                // ※現状では開発者がIsInitialized参照してエラー処理できるようにしてる。

                // New失敗（呼び出し元はIsInitializedプロパティで判定）
                // エラー例、ファイルがない、Xmlとして読み込めない　etc
                _isInitialize = false;
            }
        }
        #endregion

        #region 　　+　Function：　指定タグ間のテキストを取得する
        /// <summary>
        /// 指定タグ間のテキストを取得する
        /// </summary>
        /// <param name="tagName">タグ名</param>
        /// <returns>タグあり：読み込んだテキスト値　　タグなし：ブランク</returns>
        public string ReadXmlText(string tagName)
        {
            // タグがなければブランクをかえす
            if (!ExistsXmlTag(tagName))
                return string.Empty;

            return _xmlDoc.SelectSingleNode(tagName).InnerText;
        }
        #endregion

        #region 　　+　Function：　指定タグ間のテキストを取得する（複数同一タグ→配列取得）
        /// <summary>
        /// 指定タグ間のテキストを取得する（同一タグ→配列取得）
        /// </summary>
        /// <param name="tagName">タグ名</param>
        /// <returns>タグあり：読み込んだテキスト値（配列）　　タグなし：null</returns>
        public string[] ReadXmlTextToArray(string tagName)
        {
            // タグがなければnull
            if (!ExistsXmlTag(tagName))
                return null;

            System.Collections.Generic.List<string> result = new System.Collections.Generic.List<string>();

            // 指定タグと一致するテキストを配列にセット
            foreach (XmlNode node in _xmlDoc.SelectNodes(tagName))
            {
                result.Add(node.InnerText);
            }

            return result.ToArray();
        }
        #endregion

        #region 　　+　Function：　指定タグが存在するか確認する
        /// <summary>
        /// 指定タグが存在するか確認する
        /// </summary>
        /// <param name="tagName">タグ名</param>
        /// <returns>存在する：true　　存在しない：false</returns>
        public bool ExistsXmlTag(string tagName)
        {
            return _xmlDoc.SelectSingleNode(tagName) != null;
        }
        #endregion
    }
}
