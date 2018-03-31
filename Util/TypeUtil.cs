
namespace CM3D2.AlwaysColorChangeEx.Plugin.Util {
    /// <summary>
    /// </summary>
    public static class TypeUtil {
        public const int BODY_START = (int)MPN_TYPE_RANGE.BODY_START;
        public const int BODY_END   = (int)MPN_TYPE_RANGE.BODY_END;
        public const int WEAR_START = (int)MPN_TYPE_RANGE.WEAR_START;
        public const int WEAR_END   = (int)MPN_TYPE_RANGE.WEAR_END;
        public const int PARTSCOLOR_START = (int)MaidParts.PARTS_COLOR.NONE + 1;
        public const int PARTSCOLOR_END   = (int)MaidParts.PARTS_COLOR.MAX - 1;

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
