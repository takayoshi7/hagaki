using System;
using System.Runtime.InteropServices;

namespace hagaki.StaticClass
{
    public static class StCls_Animation
    {
        // デフォルト時間
        public static int DefaultTime = 500;

        /// <summary>
        /// 列挙体：アニメーションフラグ
        /// </summary>
        private enum AnimateWindowFlags
        {
            AW_HOR_POSITIVE = 0x1,
            AW_HOR_NEGATIVE = 0x2,
            AW_VER_POSITIVE = 0x4,
            AW_VER_NEGATIVE = 0x8,
            AW_CENTER = 0x10,
            AW_HIDE = 0x10000,
            AW_ACTIVATE = 0x20000,
            AW_SLIDE = 0x40000,
            AW_BLEND = 0x80000
        }

        /// <summary>
        /// アニメーションプリセット
        /// </summary>
        public enum AnimatePreset
        {
            //=====================================================
            // 開く時のアクション
            //=====================================================

            // 中央徐々に表示
            OPEN_Blend = AnimateWindowFlags.AW_ACTIVATE | AnimateWindowFlags.AW_BLEND,

            // 中央から表示
            OPEN_Center = AnimateWindowFlags.AW_ACTIVATE | AnimateWindowFlags.AW_CENTER,

            // 上からスライド
            OPEN_UpperSlide = AnimateWindowFlags.AW_ACTIVATE | AnimateWindowFlags.AW_VER_POSITIVE,

            // 下からスライド
            OPEN_DownSlide = AnimateWindowFlags.AW_ACTIVATE | AnimateWindowFlags.AW_VER_NEGATIVE,

            // 左からスライド
            OPEN_LeftSlide = AnimateWindowFlags.AW_ACTIVATE | AnimateWindowFlags.AW_HOR_POSITIVE,

            // 右からスライド
            OPEN_RightSlide = AnimateWindowFlags.AW_ACTIVATE | AnimateWindowFlags.AW_HOR_NEGATIVE,

            // 左上からスライド
            OPEN_UpperLeftSlide = AnimateWindowFlags.AW_ACTIVATE | AnimateWindowFlags.AW_HOR_POSITIVE | AnimateWindowFlags.AW_VER_POSITIVE,

            // 右下からスライド
            OPEN_DownRightSlide = AnimateWindowFlags.AW_ACTIVATE | AnimateWindowFlags.AW_HOR_NEGATIVE | AnimateWindowFlags.AW_VER_NEGATIVE,

            // 左下からスライド
            OPEN_DownLeftSlide = AnimateWindowFlags.AW_ACTIVATE | AnimateWindowFlags.AW_HOR_POSITIVE | AnimateWindowFlags.AW_VER_NEGATIVE,

            // 右上からスライド
            OPEN_UpperRightSlide = AnimateWindowFlags.AW_ACTIVATE | AnimateWindowFlags.AW_HOR_NEGATIVE | AnimateWindowFlags.AW_VER_POSITIVE,

            //=====================================================
            // 閉じる時のアクション
            //=====================================================

            // 中央徐々に閉じる
            CLOSE_Blend = AnimateWindowFlags.AW_HIDE | AnimateWindowFlags.AW_BLEND,

            // 中央から閉じる
            CLOSE_Center = AnimateWindowFlags.AW_HIDE | AnimateWindowFlags.AW_CENTER,

            // 上から閉じる
            CLOSE_UpperSide = AnimateWindowFlags.AW_HIDE | AnimateWindowFlags.AW_VER_POSITIVE,

            // 下から閉じる
            CLOSE_DownSide = AnimateWindowFlags.AW_HIDE | AnimateWindowFlags.AW_VER_NEGATIVE,

            // 左から閉じる
            CLOSE_LeftSide = AnimateWindowFlags.AW_HIDE | AnimateWindowFlags.AW_HOR_POSITIVE,

            // 右から閉じる
            CLOSE_RightSide = AnimateWindowFlags.AW_HIDE | AnimateWindowFlags.AW_HOR_NEGATIVE,

            // 左上から閉じる
            CLOSE_UpperLeftSide = AnimateWindowFlags.AW_HIDE | AnimateWindowFlags.AW_HOR_POSITIVE | AnimateWindowFlags.AW_VER_POSITIVE,

            // 右上から閉じる
            CLOSE_UpperRightSide = AnimateWindowFlags.AW_HIDE | AnimateWindowFlags.AW_HOR_NEGATIVE | AnimateWindowFlags.AW_VER_POSITIVE,

            // 左下から閉じる
            CLOSE_DownLeftSide = AnimateWindowFlags.AW_HIDE | AnimateWindowFlags.AW_HOR_POSITIVE | AnimateWindowFlags.AW_VER_NEGATIVE,

            // 右下から閉じる
            CLOSE_DownRightSide = AnimateWindowFlags.AW_HIDE | AnimateWindowFlags.AW_HOR_NEGATIVE | AnimateWindowFlags.AW_VER_NEGATIVE
        }

        [DllImport("user32.dll")]
        public static extern bool AnimateWindow(IntPtr hwnd, int time, AnimatePreset flags);
    }
}
