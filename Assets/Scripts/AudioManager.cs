using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager _inst;
    public static AudioManager inst
    {
        get
        {
            if (_inst == null)
                _inst = GameObject.FindObjectOfType<AudioManager>();
            return _inst;
        }
    }

    #pragma warning disable 0649
    [SerializeField] private AudioClip[] _audioClips;
    [SerializeField] private AudioClip[] _UIAudioClips;

    public void PlayAudio(int id)
    {
        if(PlayerPrefs.GetInt("sounds", 1) == 0) return;
        // TODO: Create childs to prevent stopping another audio
        this.transform.GetChild(0).GetComponent<AudioSource>().clip = _audioClips[id];
        this.transform.GetChild(0).GetComponent<AudioSource>().Play();
    }

    public void PlayUIAudio(int id)
    {
        if(PlayerPrefs.GetInt("sounds", 1) == 0) return;
        // TODO: Create childs to prevent stopping another audio
        this.transform.GetChild(1).GetComponent<AudioSource>().clip = _UIAudioClips[id];
        this.transform.GetChild(1).GetComponent<AudioSource>().Play();
    }
}
