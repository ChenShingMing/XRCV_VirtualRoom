using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class UpdateUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject panel;

    [Header("UI Elements")]
    public Text    titleText;
    public Text    noteText;
    public Slider  progressSlider;
    public Text    statusText;
    public Button  skipButton;

    private System.Action _onSkip;
    private bool _skipEnabled;
    private bool _prevTrigger;

    private void Awake()
    {
        if (panel != null) panel.SetActive(false);
    }

    // Quest 沒有 Ray Interactor，用控制器 Trigger / A 鍵觸發略過
    private void Update()
    {
        if (!_skipEnabled || _onSkip == null) return;

        bool trigger = false;
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.Controller, devices);

        foreach (var dev in devices)
        {
            bool val;
            if (dev.TryGetFeatureValue(CommonUsages.triggerButton, out val) && val) { trigger = true; break; }
            if (dev.TryGetFeatureValue(CommonUsages.primaryButton,  out val) && val) { trigger = true; break; }
        }

        if (trigger && !_prevTrigger)
        {
            _onSkip();
            _skipEnabled = false;
        }
        _prevTrigger = trigger;
    }

    public void ShowUpdatePrompt(string releaseNote, bool forceUpdate, System.Action onSkip)
    {
        if (panel != null)
        {
            // LoadingManager.Start() 會把父 Canvas 關掉，需要先把它打開
            var parentCanvas = panel.GetComponentInParent<Canvas>(true);
            if (parentCanvas != null) parentCanvas.gameObject.SetActive(true);
            panel.SetActive(true);
        }
        if (titleText      != null) titleText.text = "發現新版本";
        if (noteText       != null) noteText.text  = releaseNote;
        if (progressSlider != null) progressSlider.value = 0f;
        if (statusText     != null) statusText.text = "準備下載…";

        _onSkip = onSkip;
        _skipEnabled = !forceUpdate;

        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(!forceUpdate);
            skipButton.onClick.RemoveAllListeners();
            if (onSkip != null)
                skipButton.onClick.AddListener(() => onSkip());
        }
    }

    public void SetProgress(float progress)
    {
        if (progressSlider != null) progressSlider.value = progress;
        if (statusText     != null) statusText.text = $"下載中… {(int)(progress * 100)}%";
    }

    public void SetVerifying()
    {
        if (statusText != null) statusText.text = "驗證檔案中…";
        _skipEnabled = false;
        if (skipButton != null) skipButton.gameObject.SetActive(false);
    }

    public void ShowComplete()
    {
        if (statusText != null) statusText.text = "下載完成";
        if (skipButton != null) skipButton.gameObject.SetActive(false);
    }

    public void ShowError(string error)
    {
        if (statusText != null) statusText.text = $"下載失敗：{error}";
        _skipEnabled = true;
        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(true);
            var label = skipButton.GetComponentInChildren<Text>();
            if (label != null) label.text = "略過更新";
        }
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
    }
}
