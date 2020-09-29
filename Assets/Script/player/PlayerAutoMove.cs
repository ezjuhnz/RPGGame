using UnityEngine;
using System.Collections;

public class PlayerAutoMove : MonoBehaviour {
    private NavMeshAgent agent;//自动导航
    public float minDistance;
    // Use this for initialization
    void Start () {
      agent = this.GetComponent<NavMeshAgent>();
    }
	
    // Update is called once per frame
    void Update ()
    {
      //监听是否按下鼠标右键,0:左键,1:右键,2:中间键
      /*
      if(Input.GetMouseButtonDown(1))
      {
        Vector3 VecGoalPosition = new Vector3();
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        //如果命中
        if (Physics.Raycast(ray, out hit))
        {
            VecGoalPosition = hit.point;
        }


        StopAuto();
        setDestination(VecGoalPosition);
      }
      */
      if(agent.enabled)
      {
        if(agent.remainingDistance < minDistance && agent.remainingDistance != 0)
        {
          agent.Stop();
          agent.enabled = false;
        }
      }
        float h = Input.GetAxis("Horizontal");//水平
        float v = Input.GetAxis("Vertical");  //垂直
        if (Mathf.Abs(h) > 0.05f || Mathf.Abs(v) > 0.05f)
        {
            StopAuto();//在寻路过程中按下方向键就停止寻路
        }
    }

    void setDestination(Vector3 targetPos)
    {
        agent.enabled = true;
        agent.SetDestination(targetPos);
    }

    public void StopAuto()
    {
        if (agent.enabled)
        {
            agent.Stop();
            agent.enabled = false;
        }
    }
}
