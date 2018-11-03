using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour {

	public AudioSource audioSource;

	public void PlayAudio(AudioClip value, bool loop = false, bool waitOtherFinish = false)
	{
        //não seguir caso não haja som atribuído
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                return;
            }
        }

		//marcou para tocar somente quando o som anterior acabar e áudio ainda está tocando
		if (waitOtherFinish && audioSource.isPlaying)
			return; //não tocar som

		//toca o som
		audioSource.loop = loop;
		audioSource.clip = value;
		audioSource.Play ();
	}
    public void StopAudio()
    {
        audioSource.Stop();
    }

	public bool IsPlaying()
	{
		return audioSource.isPlaying;
	}
}
