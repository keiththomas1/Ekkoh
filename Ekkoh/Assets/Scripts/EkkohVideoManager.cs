using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EkkohVideoManager : MonoBehaviour
{
    private List<EkkohVideo> activeVideos = new List<EkkohVideo>();

    void Start()
    {
    }

    void Update()
    {
    }

    public void CreateNewVideoPlacement(string url, Vector3 position, List<EkkohWaypoint> waypoints, LineRendererController lineRenderer)
    {
        var newVideo = GameObject.Instantiate(Resources.Load("VideoObject") as GameObject).GetComponent<EkkohVideo>();
        newVideo.Initialize(waypoints, lineRenderer, url);
        newVideo.transform.localScale = Vector3.one;
        newVideo.transform.position = position;
        newVideo.OnVideoSelected.AddListener(this.VideoSelected);

        this.activeVideos.Add(newVideo);
    }

    private void VideoSelected(EkkohVideo ekkohVideo)
    {
        foreach (var video in this.activeVideos)
        {
            if (video != ekkohVideo)
            {
                video.Stop();
            }
        }
    }
}
