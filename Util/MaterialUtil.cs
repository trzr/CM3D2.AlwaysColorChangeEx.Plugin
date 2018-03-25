/*
 * マテリアル関連のユーティリティクラス
 */
using System;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable 0168
namespace CM3D2.AlwaysColorChangeEx.Plugin.Util {

    public static class MaterialUtil {
        public static float GetRenderQueue(string matName) {

            try {
                var priorityMaterials = 
                    PrivateAccessor.Get<Dictionary<int, KeyValuePair<string, float>>>(typeof(ImportCM), "m_hashPriorityMaterials");
                KeyValuePair<string, float> kvPair;
                var hashCode = matName.GetHashCode();
                if (priorityMaterials == null || !priorityMaterials.TryGetValue(hashCode, out kvPair)) return -1f;
                if (kvPair.Key == matName) return kvPair.Value;
                return -1f;
                
            } catch(Exception e) {
                LogUtil.Error("failed to get pmat field.", e);
                return 0f;
            }
        }
    }

}
