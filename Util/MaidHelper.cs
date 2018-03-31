namespace CM3D2.AlwaysColorChangeEx.Plugin.Util {
    public static class MaidHelper {

        public static string GetName(Maid maid) {
#if COM3D2
            return maid.status.fullNameJpStyle;
#else
            return maid.Param.status.last_name + " " + maid.Param.status.first_name;
#endif
        }

        public static string GetGuid(Maid maid) {
#if COM3D2
            return maid.status.guid;
#else
            return maid.Param.status.guid;
#endif
        }

    }
}