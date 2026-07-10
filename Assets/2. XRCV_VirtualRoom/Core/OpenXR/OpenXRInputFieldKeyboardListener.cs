using UnityEngine;
using TMPro; // �ޤJ TextMeshPro �R�W�Ŷ�
using UnityEngine.EventSystems;
using Microsoft.MixedReality.Toolkit.Experimental.UI;

public class OpenXRInputFieldKeyboardListener : MonoBehaviour
{
    public float distance = 0.5f;
    public float verticalOffset = -0.5f;
    
    private Transform positionSource;
    private TMP_InputField lastInputField;

    // ���}���}�l�ɡA�۰ʬd��Ҧ��� InputField �öi���ť
    void Start()
    {
        positionSource = Camera.main.transform;

        // ������������Ҧ� InputField
        TMP_InputField[] inputFields = FindObjectsByType<TMP_InputField>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (TMP_InputField inputField in inputFields)
        {
            // ���C�� InputField ���U OnSelect �ƥ�A���Q��ܮɩI�s ShowVirtualKeyboard
            inputField.onSelect.AddListener(x=> OpenKeyboard(inputField));
        }
    }

    // ��ܵ�����L����k
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