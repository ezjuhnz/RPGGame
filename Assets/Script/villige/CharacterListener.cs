using UnityEngine;
using System.Collections;

public class CharacterListener : MonoBehaviour {
    //private bool clicked = false;//是否点击过
    private UISprite sprite;
    private RoleProperty rp;
    private CharacterGrid characterGrid;
    void Awake()
    {
        sprite = this.GetComponent<UISprite>();
    }

    void Start()
    {
        characterGrid = this.gameObject.GetComponentInParent<CharacterGrid>();
    }

    void Update()
    {
        
    }

    public void OnPress(bool isPress)
    {
        if (isPress == false)
        {
            //获取角色组件中的RoleProperty
            rp = this.GetComponent<RoleProperty>();
            StartMenuController.selectedRoleName = transform.Find("name-label").GetComponent<UILabel>().text;
            if(characterGrid.currentSprite == null)
            {
                characterGrid.currentSprite = sprite;
            }
            else
            {
                characterGrid.formerSprite = characterGrid.currentSprite;
                characterGrid.currentSprite = sprite;
            }
            //在PhotonEngine中保存当前选择角色的信息
            //PhotonEngine.Instance.role.Name = StartMenuController.selectedRoleName;
            PhotonEngine.Instance.role.ID = rp.Id;
            PhotonEngine.Instance.role.Name = rp.Name;
            PhotonEngine.Instance.role.IsMan = rp.IsMan;
            PhotonEngine.Instance.role.Exp = rp.Exp;
            PhotonEngine.Instance.role.Coin = rp.Coin;
            PhotonEngine.Instance.role.Diamond = rp.Diamond;
            PhotonEngine.Instance.role.Energy = rp.Energy;
            PhotonEngine.Instance.role.Toughen = rp.Toughen;
            PhotonEngine.Instance.role.CharacterId = rp.CharacterId;
            PhotonEngine.Instance.role.Level = rp.Level;
            Debug.Log("characterid=" + rp.CharacterId + ",name=" + rp.Name);

        }
        if(characterGrid.formerSprite != null)
        {
            characterGrid.formerSprite.spriteName = StartMenuController.originSprite;
        }
        if(characterGrid.currentSprite != null)
        {
            characterGrid.currentSprite.spriteName = StartMenuController.characterSelectedSprite;
        } 
    }

    //只适用于3D Object,此处并不会执行
    public void OnMouseExit()
    {
        Debug.Log("OnMouseExit");
    }
    //只适用于3D Object,此处并不会执行
    public void OnMouseEnter()
    {
        Debug.Log("OnMouseEnter");
    }
}
