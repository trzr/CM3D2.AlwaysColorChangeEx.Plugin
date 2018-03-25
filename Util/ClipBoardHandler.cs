
using System;
using System.IO;
using System.Reflection;
using CM3D2.AlwaysColorChangeEx.Plugin.Data;
using UnityEngine;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Util {
    /// <summary>
    /// クリップボードハンドラ
    /// 現版では、マテリアル情報用
    /// </summary>
    public class ClipBoardHandler {
        // クリップボードを監視する際のサイズ範囲
        private const int MIN_LENGTH = 20;
        private const int MAX_LENGTH = 3333;

        private static readonly ClipBoardHandler INSTANCE = new ClipBoardHandler();
        public static ClipBoardHandler Instance {
            get { return INSTANCE;  }
        }
        
        public string mateText;
        public bool isMateText;
        private int prevLength;

        private ClipBoardHandler() {
            // unity 5-
            if (!ClipboardCHelper.IsSupport()) {
                LogUtil.Debug("ClipboardCHelper disabled. using direct GUIUtility.systemCopyBuffer");
                GetClipboard = () =>  GUIUtility.systemCopyBuffer ;
                SetClipboard = (text) => {
                    GUIUtility.systemCopyBuffer = text;
                };
            } else {
                LogUtil.Debug("ClipboardCHelper enabled.");
                GetClipboard = () => ClipboardCHelper.clipBoard;
                SetClipboard = (text) => {
                    ClipboardCHelper.clipBoard = text;
                };
            }
        }
        
        public Func<string> GetClipboard;
        public Action<string> SetClipboard;
        // クリップボードを再読み込みし、データであるか判定する
        public void Reload() {
            var clip = GetClipboard();
            
            if (clip.Length < MIN_LENGTH || clip.Length > MAX_LENGTH) {
                mateText = null;
                isMateText = false;
                prevLength = 0;
                return;
            }

            // 負荷軽減のため、文字列チェック無し：長さが変わったときにのみチェック
            if (prevLength == clip.Length) return;
            //if (prevLength == clip.Length) {
            //     // 前回とフラグ変更なし
            //     if (clip == mateText) return;
            //}

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