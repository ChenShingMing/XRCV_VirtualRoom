using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using HutongGames.PlayMaker;
using Sirenix.OdinInspector;
using UnityEngine.Events;

public class LoadingManager : MonoBehaviour
{
	public static string nextSceneName;

	public string sceneName;
	public GameObject loadingUI;
	public Slider loadingSlider;
	public Text loadingText;

	public UnityEvent OnLoadFinish;

	private float loadingSpeed = 1;
	private float targetValue;

	private AsyncOperation operation;


	bool isFinish = true;

	void Start()
	{
		if (loadingUI != null) loadingUI.SetActive(false);
		if (loadingSlider != null) loadingSlider.value = 0.0f;
	}

	// Update is called once per frame
	void Update()
	{
		if (isFinish) return;
		if (operation == null) return;

		targetValue = operation.progress;

		if (operation.progress >= 0.9f)
		{
			//operation.progress���ȳ̤j��0.9
			targetValue = 1.0f;
		}

		if (targetValue != loadingSlider.value)
		{
			//���ȹB��
			loadingSlider.value = Mathf.Lerp(loadingSlider.value, targetValue, Time.deltaTime * loadingSpeed);
			if (Mathf.Abs(loadingSlider.value - targetValue) < 0.01f)
			{
				loadingSlider.value = targetValue;
			}
		}

		loadingText.text = ((int)(loadingSlider.value * 100)).ToString() + "%";

		if ((int)(loadingSlider.value * 100) == 100)
		{
			isFinish = true;
			loadingUI.SetActive(false);

			if (OnLoadFinish != null)
            {
				OnLoadFinish.Invoke();
			}
		}
	}

	public void StartLoadScene()
	{
		if (!isFinish) return;

		isFinish = false;
		if (loadingUI != null) loadingUI.SetActive(true);
		StartCoroutine(AsyncLoading());
	}

	public void AllowSceneActivation()
    {
		if (!isFinish) return;
		operation.allowSceneActivation = true;
	}

	IEnumerator AsyncLoading()
	{
		string target = !string.IsNullOrEmpty(nextSceneName) ? nextSceneName
		              : !string.IsNullOrEmpty(sceneName)     ? sceneName
		              : null;
		if (target == null)
		{
			Debug.LogWarning("[LoadingManager] No scene name set — aborting load");
			isFinish = true;
			yield break;
		}
		operation = SceneManager.LoadSceneAsync(target);

		//��������J�����۰ʤ���
		operation.allowSceneActivation = false;

		yield return operation;
	}
}
