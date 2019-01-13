
namespace CM3D2.AlwaysColorChangeEx.Plugin.Util {
    /// <summary>
    /// </summary>
    public static class TypeUtil {
        static TypeUtil() {
            BODY_START = PrivateAccessor.Get<int>(typeof(MPN_TYPE_RANGE), "BODY_START");
            BODY_END   = PrivateAccessor.Get<int>(typeof(MPN_TYPE_RANGE), "BODY_END");
            FOLDER_BODY_START = PrivateAccessor.Get<int>(typeof(MPN_TYPE_RANGE), "FOLDER_BODY_START");
            FOLDER_BODY_END   = PrivateAccessor.Get<int>(typeof(MPN_TYPE_RANGE), "FOLDER_BODY_END");
            WEAR_START = PrivateAccessor.Get<int>(typeof(MPN_TYPE_RANGE), "WEAR_START");
            WEAR_END   = PrivateAccessor.Get<int>(typeof(MPN_TYPE_RANGE), "WEAR_END");
            SET_START  = PrivateAccessor.Get<int>(typeof(MPN_TYPE_RANGE), "SET_START");
            SET_END    = PrivateAccessor.Get<int>(typeof(MPN_TYPE_RANGE), "SET_END");
            PARTS_COLOR_START = PrivateAccessor.Get<MaidParts.PARTS_COLOR>(typeof(MaidParts.PARTS_COLOR), "NONE") + 1;
            PARTS_COLOR_END   = PrivateAccessor.Get<MaidParts.PARTS_COLOR>(typeof(MaidParts.PARTS_COLOR), "MAX") - 1;
        }
        public static readonly int BODY_START;
        public static readonly int BODY_END;
        public static readonly int FOLDER_BODY_START;
        public static readonly int FOLDER_BODY_END;
        public static readonly int WEAR_START;
        public static readonly int WEAR_END;
        public static readonly int SET_START;
        public static readonly int SET_END;
        public static readonly MaidParts.PARTS_COLOR PARTS_COLOR_START;
        public static readonly MaidParts.PARTS_COLOR PARTS_COLOR_END;

        public static bool IsBody(MPN mpn) {
            var mpnNo = (int)mpn;
            return  (BODY_START <= mpnNo && mpnNo <= BODY_END);
        }

        public static bool IsWear(MPN mpn) {
            var mpnNo = (int)mpn;
            return  (WEAR_START <= mpnNo && mpnNo <= WEAR_END);
        }
    }
}
