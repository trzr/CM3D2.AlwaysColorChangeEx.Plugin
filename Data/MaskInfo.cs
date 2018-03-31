
namespace CM3D2.AlwaysColorChangeEx.Plugin.Data {
    public class MaskInfo {
        public readonly SlotInfo slotInfo;
        public TBodySkin slot;
        public SlotState state;
        public bool value;
        public MaskInfo(SlotInfo si, TBodySkin slot) {
            slotInfo = si;
            this.slot = slot;
        }
        public void UpdateState() {
            if (slot.obj == null) {
                state = SlotState.NotLoaded;
            } else if (!slot.boVisible) {
                state = SlotState.Masked;
            } else {
                state = SlotState.Displayed;
            }
        }

        public string Name(bool useDisplayName) {
            return useDisplayName ? slotInfo.DisplayName : slotInfo.Name;
        }
    }
}