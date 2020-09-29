using UnityEngine;
using System.Collections;

public class BloodScreen : MonoBehaviour {
    private static BloodScreen _instance;
    private  UISprite sprite;
    private  TweenAlpha alpha;

    public static BloodScreen Instance
    {
        get { return _instance; }
    }
	void Start () {
        _instance = this;
        sprite = GetComponent<UISprite>();
        alpha = GetComponent<TweenAlpha>();
    }

    public void ShowBloodScreen()
    {
        sprite.alpha = 1; //默认是0,不显示出血效果
        alpha.ResetToBeginning();//reset to 1,显示出血效果
        alpha.PlayForward();     //播放到0,不显示出血效果
    }
}
