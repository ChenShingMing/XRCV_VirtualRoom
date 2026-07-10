using UnityEngine;

// P1: Quest 已改為 OpenXR 路徑（見 PlatformBuilder.SwitchToQuestForAndroid），
// 此 InputHandler 不再使用。保留類別殼以避免場景中 Input_OVR (inactive) 的 Missing Script 警告。
// 待確認 Input_OVR GameObject 可從場景移除後，整個檔案可一併刪除。
[System.Obsolete("Quest 已使用 InputHandler_OpenXR，此類別不再有效")]
public class InputHandler_OVR : InputHandler
{
    public override Vector3 GetInputPointerOnGazeSphere() => throw new System.NotSupportedException();
    public override void InputHandle() => throw new System.NotSupportedException();
}
