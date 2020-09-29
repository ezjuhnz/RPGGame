using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour {

    public static SoundManager _instance;
    public AudioClip[] audioClipArray;
    public AudioSource audioSource;
    public bool isQuiet;
    private Dictionary<string, AudioClip> audioDict = new Dictionary<string, AudioClip>();
	void Awake () {
        _instance = this;
	}
	
	void Start () {
	  foreach(AudioClip ac in audioClipArray)
      {
            audioDict.Add(ac.name, ac);
      }
	}

    public void Play(string audioName)
    {
        if (isQuiet) return;
        AudioClip ac;
        if(audioDict.TryGetValue(audioName,out ac))
        {
            AudioSource.PlayClipAtPoint(ac, Vector3.zero);
            //audioSource.PlayOneShot(ac);
        }
    }

    public void Play(string audioName,AudioSource audioSource)
    {
        if (isQuiet) return;
        AudioClip ac;
        if (audioDict.TryGetValue(audioName, out ac))
        {
            AudioSource.PlayClipAtPoint(ac, Vector3.zero);
            audioSource.PlayOneShot(ac);
        }
    }
}
