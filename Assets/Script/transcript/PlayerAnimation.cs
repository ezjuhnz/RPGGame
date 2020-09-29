using UnityEngine;
using System.Collections;

//这个脚本用来处理副本人物攻击的动画播放
public class PlayerAnimation : MonoBehaviour {
    private static PlayerAnimation _instance;
    private Animator anim;
    public static PlayerAnimation Instance
    {
        get { return _instance; }
    }
    

    void Start()
    {
        _instance = this;
        anim = this.GetComponent<Animator>();
    }

    void Update()
    {
       
    }

    public void OnAttackButtonClick(bool isPress, PosType posType)
    {
        if(this.GetComponent<PlayerAttack>().isDead)
        {
            Debug.Log("Player has die");
            return;
        }
        if (posType == PosType.Basic)
        {
            if(isPress)
            {
                anim.SetTrigger("Attack");
            }
        }
        else
        {
            anim.SetBool("Skill" + (int)posType, isPress);
        }
        //判断是否组队,如果是就发送请求到服务器
       
        if (TeamInviteController.Instance.isTeam)
        {
            PlayerAnimationModel model = new PlayerAnimationModel()
            {
                attack = (posType == PosType.Basic && isPress) ? true : false,
                skill1 = (posType == PosType.One && isPress) ? true : false,
                skill2 = (posType == PosType.Two && isPress) ? true : false,
                skill3 = (posType == PosType.Three && isPress) ? true : false,
            };
            BattleController.Instance.SyncPlayerAttackAnimationRequest(model,PhotonEngine.Instance.role.ID);
        }
    }
    

    //暂时派不上用场
    public bool IsEmptyState()
    {
        if(anim.GetCurrentAnimatorStateInfo(1).IsName("Empty State"))
        {
            return true;
        }
        return false;
    }
}
