using UnityEngine;
using TMPro; // 引入 TextMeshPro 命名空間
using UnityEngine.EventSystems;
using Microsoft.MixedReality.Toolkit.Experimental.UI;

public class OpenXRInputFieldKeyboardListener : MonoBehaviour
{
    public float distance = 0.5f;
    public float verticalOffset = -0.5f;
    
    private Transform positionSource;
    private TMP_InputField lastInputField;

    // 當腳本開始時，自動查找所有的 InputField 並進行監聽
    void Start()
    {
        positionSource = Camera.main.transform;

        // 獲取場景中的所有 InputField
        TMP_InputField[] inputFields = FindObjectsOfType<TMP_InputField>(true);

        foreach (TMP_InputField inputField in inputFields)
        {
            inputField.shouldHideSoftKeyboard = true;
            // 為每個 InputField 註冊 OnSelect 事件，當被選擇時呼叫 ShowVirtualKeyboard
            inputField.onSelect.AddListener(x=> OpenKeyboard(inputField));
        }
    }

    // 顯示虛擬鍵盤的方法
    void OpenKeyboard(TMP_InputField tmpInputField)
    {
        NonNativeKeyboard.Instance.InputField = tmpInputField;
        NonNativeKeyboard.Instance.PresentKeyboard(tmpInputField.text);

        Vector3 direction = positionSource.forward;
        direction.y = 0;
        direction.Normalize();

        Vector3 targetPosision = positionSource.position + direction * distance + Vector3.up * verticalOffset;
        NonNativeKeyboard.Instance.RepositionKeyboard(targetPosision);

        SetCaretColorAlpha(tmpInputField, 1);
        NonNativeKeyboard.Instance.OnClosed += Instance_OnClosed;
        lastInputField = tmpInputField;
    }

    private void Instance_OnClosed(object sender, System.EventArgs e)
    {
        ResetCaretColorAlpha();
        NonNativeKeyboard.Instance.OnClosed -= Instance_OnClosed;
    }


    private void SetCaretColorAlpha(TMP_InputField tmpInputField, float value)
    {
        tmpInputField.customCaretColor = true;
        Color caretColor = tmpInputField.caretColor;
        caretColor.a = value;
        tmpInputField.caretColor = caretColor;
    }

    private void ResetCaretColorAlpha()
    {
        if (lastInputField == null) return;
        SetCaretColorAlpha(lastInputField, 0);
        lastInputField = null;
    }
}