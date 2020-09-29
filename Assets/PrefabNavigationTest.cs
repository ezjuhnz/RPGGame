using UnityEngine;
using System.Collections;

public class PrefabNavigationTest : MonoBehaviour {
    private GameObject playerGo;
    public Transform _playerTransform;
    public Transform _targetTransform;
    private NavMeshAgent _navMeshAgent;
    // Use this for initialization
    void Start () {
        playerGo = GameObject.Instantiate(Resources.Load("Player/Player-Test"), _playerTransform.position, Quaternion.identity) as GameObject;
        _navMeshAgent = playerGo.GetComponent<NavMeshAgent>();
        if (_navMeshAgent == null)
        {
            Debug.LogError("NavMeshAgent is not attached to " + this.gameObject.name);
        }
        else
        {
            Vector3 targetPos = _targetTransform.position;
            SetDestination(targetPos);
        }
    }

    private void SetDestination(Vector3 targetPos)
    {
        _navMeshAgent.SetDestination(targetPos);
    }
}
