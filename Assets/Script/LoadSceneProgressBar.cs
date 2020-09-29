using UnityEngine;
using System.Collections;

public class LoadSceneProgressBar : MonoBehaviour {

    public static LoadSceneProgressBar _instance;

    private GameObject bg;
    private UISlider progressBar;//进度条
    private bool isAsyn = false;
    private AsyncOperation ao = null;

    void Awake()
    {
        _instance = this;
        bg = this.transform.Find("BG").gameObject;
        gameObject.SetActive(false);
        progressBar = transform.Find("BG/ProgressBarBg").GetComponent<UISlider>();
    }

    void Update()
    {
        if(isAsyn)
        {
            progressBar.value = ao.progress;
        }
    }

    public void Show(AsyncOperation ao)
    {
        gameObject.SetActive(true);//显示进度条
        bg.SetActive(true);//显示背景图
        isAsyn = true;
        this.ao = ao;
    }
}
