using UnityEngine;
using System.Collections;

public class PlayerEffect : MonoBehaviour {

    private Renderer[] rendererArray;
    private NcCurveAnimation[] curveAnimArray;
    private GameObject effectOffset;
    
	void Start () {
        //获取特效的renderer,将它设置为enable就能显示特效
        rendererArray = this.GetComponentsInChildren<Renderer>();
        //获取特效中的NcCurveAnimation脚本,它有销毁特效的功能.避免特效一直存在
        curveAnimArray = this.GetComponentsInChildren<NcCurveAnimation>();
        //寒冰特效组件
        if(transform.Find("EffectOffset"))
        {
            effectOffset = transform.Find("EffectOffset").gameObject;
        }
       
    }
	
	public void Show()
    {
        if(effectOffset != null)
        {
            effectOffset.gameObject.SetActive(false);
            effectOffset.gameObject.SetActive(true);
        }
        foreach(Renderer renderer in rendererArray)
        {
            renderer.enabled = true;
        }
        foreach(NcCurveAnimation anim in curveAnimArray)
        {
            anim.ResetAnimation();
        }
    }
}
