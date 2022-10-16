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

	// Use this for initialization
	void Start()
	{
		loadingUI.SetActive(false);
		loadingSlider.value = 0.0f;
	}

	// Update is called once per frame
	void Update()
	{
		if (isFinish) return;

		targetValue = operation.progress;

		if (operation.progress >= 0.9f)
		{
			//operation.progress的值最大為0.9
			targetValue = 1.0f;
		}

		if (targetValue != loadingSlider.value)
		{
			//插值運算
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
		loadingUI.SetActive(true);
		StartCoroutine(AsyncLoading());
	}

	public void AllowSceneActivation()
    {
		if (!isFinish) return;
		operation.allowSceneActivation = true;
	}

	IEnumerator AsyncLoading()
	{
		operation = SceneManager.LoadSceneAsync(sceneName);

		//阻止當載入完成自動切換
		operation.allowSceneActivation = false;

		yield return operation;
	}
}
