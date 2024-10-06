using UnityEngine;
using TMPro; // 引入 TextMeshPro 命名空間
using UnityEngine.EventSystems;

public class OpenXRInputFieldKeyboardListener : MonoBehaviour
{
    private GameObject lastSelectedObject; // 記錄上次選中的物件

    void Update()
    {
        // 獲取目前被選中的物件
        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;

        // 檢查是否選中了一個新的 TMP_InputField
        if (selectedObject != null && selectedObject != lastSelectedObject)
        {
            TMP_InputField tmpInputField = selectedObject.GetComponent<TMP_InputField>();

            if (tmpInputField != null)
            {
                // 如果是新的 TMP_InputField，彈出虛擬鍵盤
                ShowVirtualKeyboard(tmpInputField);
            }

            // 更新 lastSelectedObject 為當前選中的物件
            lastSelectedObject = selectedObject;
        }
    }

    // 顯示虛擬鍵盤的方法
    void ShowVirtualKeyboard(TMP_InputField tmpInputField)
    {
        // 彈出虛擬鍵盤的邏輯
        Debug.Log("Showing virtual keyboard for input field: " + tmpInputField.name);

        // 使用 Unity 的 TouchScreenKeyboard 彈出鍵盤
        TouchScreenKeyboard.Open(tmpInputField.text, TouchScreenKeyboardType.Default);
    }
}