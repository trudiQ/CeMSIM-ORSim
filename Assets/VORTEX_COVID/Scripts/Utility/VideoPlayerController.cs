using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class VideoPlayerController : MonoBehaviour
{
    public VideoClip[] videoClips;

    private VideoPlayer videoPlayer;

    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
    }

    public void PlayClip(int index)
    {
        if (index >= 0 && index < videoClips.Length)
        {
            if (videoPlayer.isPlaying)
                videoPlayer.Stop();

            videoPlayer.clip = videoClips[index];
            videoPlayer.Play();
        }
    }
}
