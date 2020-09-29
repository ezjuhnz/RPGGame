using UnityEngine;
using System.Collections;

public class Combo : MonoBehaviour {
    private static Combo _instance;
    private UISprite comboSprite;
    private UILabel comboLabel;

    private int comboCount; //连击数
    private float comboTimer = 2; //连击时间,超过该时间仍未收到攻击,连击数归零
    private float peaceTimer = 0; //如果该时间超过comboTimer,则连击数归零

    public static Combo Instance
    {
        get { return _instance; }
    }
    void Awake()
    {
        _instance = this;
        comboLabel = transform.Find("Label").GetComponent<UILabel>();
        this.gameObject.SetActive(false);
    }

	void Start () {
       
	}
	
	// Update is called once per frame
	void Update () {
        peaceTimer += Time.deltaTime;
        if(peaceTimer >= comboTimer)
        {
            this.gameObject.SetActive(false);
            comboCount = 0;
        }
    }

    public void ComboPlus() //增加连击数
    {
        peaceTimer = 0;
        comboCount++;
        this.gameObject.SetActive(true);
        comboLabel.text = comboCount + "";
        comboLabel.transform.localScale = Vector3.one; //还原大小
        iTween.ScaleTo(comboLabel.gameObject, new Vector3(1.5f, 1.5f, 1.5f), 0.1f);
        iTween.ShakePosition(comboLabel.gameObject, new Vector3(0.1f, 0.1f, 0.1f), 0.2f);
    }
}
