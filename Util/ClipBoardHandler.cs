
using System;
using System.IO;
using CM3D2.AlwaysColorChangeEx.Plugin.Data;
using UnityEngine;

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
                            
            string clip = GUIUtility.systemCopyBuffer;
            //ClipboardHelper.clipBoard;
            
            if (clip.Length < MIN_LENGTH || clip.Length > MAX_LENGTH) {
                mateText = null;
                isMateText = false;
                prevLength = 0;
                return;
            }
                
            // 負荷軽減のため、文字列チェック無し：長さが変わったときにのみチェック
            if (prevLength == clip.Length) return;
                // if (clip == mateText) return;
                // 前回とフラグ変更なし
            //}

            prevLength = clip.Length;
            mateText = clip;
            isMateText = MateHandler.IsParsable(clip);
        }
        
        public void SetClipBoard(string text) {
            GUIUtility.systemCopyBuffer = text;
//            ClipboardHelper.clipBoard = text;
        }
        
        public void Clear() {
            mateText = null;
            prevLength = 0;
            isMateText = false;
        }
    }
}