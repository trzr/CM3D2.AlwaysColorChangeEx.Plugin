
using System;
using System.IO;
using CM3D2.AlwaysColorChangeEx.Plugin.Data;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Util
{
    /// <summary>
    /// </summary>
    public class ClipBoardHandler
    {
        // クリップボードを監視する際のサイズ範囲
        private const int MIN_LENGTH = 20;
        private const int MAX_LENGTH = 3333;

        private ClipBoardHandler() { }
        private static ClipBoardHandler instance = new ClipBoardHandler();        
        public static ClipBoardHandler Instance {
            get { return instance;  }
        }
        
        public string mateText;
        public bool isMateText;
        private int prevLength;
        
        
        // クリップボードを再読み込みし、データであるか判定する
        public void Reload() {
            string clip = ClipboardHelper.clipBoard;
            
            if (clip.Length < MIN_LENGTH || clip.Length > MAX_LENGTH) {
                mateText = null;
                isMateText = false;
                prevLength = 0;
                return;
            }
                
            if (prevLength == clip.Length) {
                //if (mateHeader != null && clip == mateHeader.text) {
                if (clip == mateText) return;
                // 前回とフラグ変更なし
            }
            prevLength = clip.Length;
            mateText = clip;
            isMateText = MateHandler.IsParsable(clip);
        }
        
        public void Clear() {
            mateText = null;
            prevLength = 0;
            isMateText = false;
        }
    }
}