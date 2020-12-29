using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.AR;

public class EkkohVideo : MonoBehaviour
{
    [SerializeField]
    private VideoPlayer VideoPlayer;
    [SerializeField]
    private Renderer VideoRenderer;
    [SerializeField]
    private GameObject GhostVideoPlayer;
    [SerializeField]
    private ARSelectionInteractable ARSelectionInteractable;
    [SerializeField]
    private ARScaleInteractable ARScaleInteractable;

    private LineRendererController lineRendererController;
    private List<EkkohWaypoint> waypoints;
    
    private bool is3DPlaying = false;
    private float currentTimeInMilliseconds = 0f;
    private int nextWaypointIndex = 0;

    public class VideoSelectedEvent : UnityEvent<EkkohVideo>
    {
    }
    public VideoSelectedEvent OnVideoSelected = new VideoSelectedEvent();
    private bool selected = false;

    private void Update()
    {
        if (this.is3DPlaying)
        {
            if (this.waypoints.Count >= 2)
            {
                this.currentTimeInMilliseconds += Time.deltaTime;

                while ((this.nextWaypointIndex < this.waypoints.Count - 1)
                    && this.waypoints[nextWaypointIndex].TimeInMilliseconds < this.currentTimeInMilliseconds)
                {
                    this.nextWaypointIndex++;
                }

                var previousWaypointTime = this.waypoints[this.nextWaypointIndex - 1].TimeInMilliseconds;
                var nextWaypointTime = this.waypoints[this.nextWaypointIndex].TimeInMilliseconds;
                var percentTowardsNextWaypoint = (this.currentTimeInMilliseconds - previousWaypointTime) / (nextWaypointTime - previousWaypointTime);
                this.transform.position = Vector3.Lerp(
                    this.waypoints[this.nextWaypointIndex - 1].Position,
                    this.waypoints[this.nextWaypointIndex].Position,
                    percentTowardsNextWaypoint);
                this.transform.rotation = Quaternion.Slerp(
                    this.waypoints[this.nextWaypointIndex - 1].Rotation,
                    this.waypoints[this.nextWaypointIndex].Rotation,
                    percentTowardsNextWaypoint);
            }
        }
    }

    public void Initialize(List<EkkohWaypoint> waypoints, LineRendererController lineRendererController, string url = "")
    {
        var interactionManager = FindObjectOfType<XRInteractionManager>();
        this.ARScaleInteractable.interactionManager = interactionManager;
        this.ARSelectionInteractable.interactionManager = interactionManager;

        this.ARSelectionInteractable.onSelectEntered.AddListener(this.HandleSelected);

        this.VideoRenderer.material = new Material(this.VideoRenderer.material);
        this.VideoPlayer.started += (VideoPlayer _) =>
        { 
            this.Start3DMovement();
        };
        this.VideoPlayer.loopPointReached += (VideoPlayer _) =>
        {
            if (this.VideoPlayer.isLooping)
            {
                this.Start3DMovement();
            }
        };

        if (url != "")
        {
            this.VideoPlayer.url = url;
        }

        this.waypoints = new List<EkkohWaypoint>(waypoints);
        this.lineRendererController = lineRendererController;

        this.GhostVideoPlayer.transform.localPosition = this.waypoints[this.waypoints.Count - 1].Position;
    }

    public void Play()
    {
        this.VideoPlayer.Play();
        this.GhostVideoPlayer.SetActive(false);
    }

    public void Stop()
    {
        this.is3DPlaying = false;
        this.VideoPlayer.Stop();
        this.GhostVideoPlayer.SetActive(true);
    }

    private void HandleSelected(XRBaseInteractor xRBaseInteractor)
    {
        this.OnVideoSelected.Invoke(this);

        this.selected = !this.selected;

        this.lineRendererController.SetSelected(this.selected);

        if (this.selected)
        {
            this.Play();
        } else
        {
            this.Stop();
        }
    }

    private void Start3DMovement()
    {
        this.transform.position = this.waypoints[0].Position;
        this.transform.rotation = this.waypoints[0].Rotation;
        this.currentTimeInMilliseconds = 0f;
        this.nextWaypointIndex = 1;
        this.is3DPlaying = true;
    }
}
