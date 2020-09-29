using UnityEngine;
using System.Collections;
using System;

public class NPCMoveTest : MonoBehaviour {

    private NavMeshAgent _navMeshAgent;
    private Transform target;
    Vector3 targetPos = new Vector3(-18, 1.5f, 3);

    // Use this for initialization
    void Start () {
        target = GameObject.Find("Target1").gameObject.transform;
        _navMeshAgent = this.GetComponent<NavMeshAgent>();
        if(_navMeshAgent == null)
        {
            Debug.LogError("NavMeshAgent is not attached to " + this.gameObject.name);
        }
        else
        {
            SetDestination(target.position);
        }
        //_navMeshAgent.updatePosition = false;
    }

    private void SetDestination(Vector3 targetPos)
    {
        _navMeshAgent.SetDestination(targetPos);
    }

    void Update()
    {
        Debug.Log("targetPos=" + targetPos);
        if(_navMeshAgent != null)
        {
            SetDestination(targetPos);
        }
    }
}
