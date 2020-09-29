using UnityEngine;
using System.Collections;

public class HpBarController : MonoBehaviour {
    private static HpBarController _instance;
    private UISlider hpSlider;
    private UILabel nameLabel;

    private GameObject hpBarGo;
    private GameObject bossHpBarGo;


    public static HpBarController Instance
    {
        get { return _instance; }
    }
    void Awake()
    {
        hpBarGo = transform.root.Find("EnermyHpBar").gameObject;
        bossHpBarGo = transform.root.Find("BossHpBar").gameObject;
        _instance = this;
        this.gameObject.SetActive(false);//Boss HpBar和普通怪HpBar都设为false;
    }
    // Use this for initialization
    void Start () {
        
    }
	
	// Update is called once per frame
	void Update () {
	
	}
    //普通怪物的血条
    public void ShowHpBar(GameObject go)
    {
        hpBarGo.SetActive(true); //显示普通怪血条
        bossHpBarGo.SetActive(false); //隐藏Boss血条
        
        //修改血条属性,
        Enermy enermy = go.GetComponent<Enermy>();
        hpSlider = hpBarGo.transform.GetComponent<UISlider>();
        nameLabel = hpBarGo.transform.GetComponentInChildren<UILabel>();
        nameLabel.text = enermy.monsterName;
        GameObject deadMarkGo = hpBarGo.transform.Find("DeadMark").gameObject;
        int hp = enermy.hp > 0 ? enermy.hp : 0;
        //Debug.Log("ShowHpBar hp=" + hp);
        hpSlider.value = hp / enermy.OriginHp;
        if (!enermy.isDead) //敌人未死亡
        {
            deadMarkGo.SetActive(false);
        }

        else //敌人已死亡
        {
            deadMarkGo.SetActive(true);
            //注意:因为该脚本绑定在多个gameObject上,所以会对多个gameObject判断
            if (this.gameObject.activeInHierarchy)
            {
                StartCoroutine("DestroyHpBar");
            }
        }
    }


    public void ShowBossHpBar(GameObject go)
    {
        bossHpBarGo.SetActive(true);//显示Boss血条
        hpBarGo.SetActive(false); //隐藏普通怪血条

        //修改血条属性,
        Boss boss = go.GetComponent<Boss>();
        hpSlider = bossHpBarGo.transform.GetComponent<UISlider>();
        nameLabel = bossHpBarGo.transform.GetComponentInChildren<UILabel>();
        nameLabel.text = boss.monsterName;
        GameObject deadMarkGo = bossHpBarGo.transform.Find("DeadMark").gameObject;
        if (!boss.isDead) //敌人未死亡
        {
            deadMarkGo.SetActive(false);
            hpSlider.value = boss.hp / boss.OriginHp;
        }
        else //敌人已死亡
        {
            deadMarkGo.SetActive(true);
            hpSlider.value = 0;
            //Destroy(bossHpBarGo, 5.0f);//5秒后销毁血条
            //注意:因为该脚本绑定在多个gameObject上,所以会对多个gameObject判断
            if(this.gameObject.activeInHierarchy)
            {
                StartCoroutine("DestroyBossHpBar");
            }
        }
    }


    IEnumerator DestroyHpBar()
    {
        if(hpBarGo.activeInHierarchy)
        {
            yield return new WaitForSeconds(3);
            hpBarGo.SetActive(false);
        }
    }

    IEnumerator DestroyBossHpBar()
    {
        yield return new WaitForSeconds(5);
        bossHpBarGo.SetActive(false);
    }
}
