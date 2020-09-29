using UnityEngine;
using System.Collections;
using ExitGames.Client.Photon;
using XueCommon;
using System;

public abstract class ControllerBase : MonoBehaviour {

    public abstract OperationCode OpCode { get; }

	public virtual void Start () {
        PhotonEngine.Instance.RegisterController(OpCode, this);
	}
	
	public virtual void OnDestroy () {
        PhotonEngine.Instance.UnRegisterController(OpCode);
    }

    public abstract void OnOperationResponse(OperationResponse response);

    public virtual void OnEvent(EventData eventData)
    {
        
    }
}
