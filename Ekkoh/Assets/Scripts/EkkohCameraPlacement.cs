using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct EkkohWaypoint
{
    public EkkohWaypoint(Vector3 position, Quaternion rotation, float timeInMilliseconds)
    {
        this.Position = position;
        this.Rotation = rotation;
        this.TimeInMilliseconds = timeInMilliseconds;
    }
    public Vector3 Position { get; }
    public Quaternion Rotation { get; }
    public float TimeInMilliseconds { get; }
}

public class EkkohCameraPlacement : MonoBehaviour
{
	[SerializeField]
	private Camera ARCamera;
	[SerializeField]
	private Button RecordButton;
    [SerializeField]
    private ScreenRecorder ScreenRecorder;
    [SerializeField]
    private RandomCameraMovement RandomCameraMovement;
    [SerializeField]
    private EkkohVideoManager EkkohVideoManager;
    [SerializeField]
    private GameObject RecordingIndicator;

    private bool isRecording = false;
    private const float RECORDING_POINT_INTERVAL = 0.2f;
    private float recordingPointTimer = 0f;
    private float totalRecordingTime = 0f;
    private DateTime startingRecordTime;
    private List<EkkohWaypoint> currentWaypoints = new List<EkkohWaypoint>();
    private int currentFingerID = -1;
    private LineRendererController currentLineRendererController = null;

    private void Start()
    {
        this.ScreenRecorder.OnVideoGenerated.AddListener((string url) =>
        {
#if UNITY_EDITOR
            url = ""; // We will use this as a "don't set URL for video and just use default"
#endif
            // TODO: May have to translate this position from the AR camera's local space into world space.
            this.EkkohVideoManager.CreateNewVideoPlacement(
                url, this.currentWaypoints[0].Position, this.currentWaypoints, this.currentLineRendererController);
        });

        this.RecordingIndicator.SetActive(false);
    }

    void Update()
    {
        // Mobile
        foreach (Touch touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began)
            {
                if (!this.isRecording)
                {
                    this.CheckTouch(touch.position);
                    this.currentFingerID = touch.fingerId;
                    this.startingRecordTime = DateTime.Now;
                }
            } 
            else if (touch.phase == TouchPhase.Ended)
            {
                if (this.currentFingerID == touch.fingerId)
                {
                    this.StopRecording();
                }
            }
        }

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            this.CheckTouch(Input.mousePosition);
        }
        if (Input.GetMouseButtonUp(0))
        {
            this.StopRecording();
        }
#endif

        if (this.recordingPointTimer > 0f && this.isRecording)
        {
            this.recordingPointTimer -= Time.deltaTime;
            this.totalRecordingTime += Time.deltaTime;

            if (this.recordingPointTimer <= 0f)
            {
                this.recordingPointTimer = RECORDING_POINT_INTERVAL;

                this.currentWaypoints.Add(new EkkohWaypoint(
                    this.ARCamera.transform.localPosition,
                    this.ARCamera.transform.localRotation,
                    this.totalRecordingTime
                ));
            }
        }
    }

    public void StartRecording()
    {
        // TODO: If we ever allow recording while processing, we have to cache multiple levels of the line renderer and whatever
        //        else is associated with the video object because we create that object in a callback.
        var canRecordNewVideo = (!this.isRecording && !this.ScreenRecorder.IsProcessing());

#if UNITY_EDITOR
        this.RandomCameraMovement.StartMoving();
        canRecordNewVideo = true; // We don't care about this in-editor
#endif

        if (canRecordNewVideo)
        {
            this.ScreenRecorder.StartRecording();
            this.RecordingIndicator.SetActive(true);

            this.isRecording = true;

            this.currentWaypoints.Clear();
            this.currentWaypoints.Add(new EkkohWaypoint(
                    this.ARCamera.transform.localPosition,
                    this.ARCamera.transform.localRotation,
                    0f
            ));

            this.recordingPointTimer = RECORDING_POINT_INTERVAL;
            this.totalRecordingTime = 0f;
        } else
        {
            var cantRecordReason = this.isRecording ?
                "Recording flag still set to true." :
                "ScreenRecorder is still processing.";
            Debug.LogWarning($"Can't record yet. {cantRecordReason}");
        }
    }

    public void StopRecording()
    {
#if UNITY_EDITOR
        this.RandomCameraMovement.StopMoving();
#endif

        if (this.isRecording)
        {
            Debug.Log("Total record time: " + (DateTime.Now - this.startingRecordTime).ToString());

            this.ScreenRecorder.StopRecording();
            this.RecordingIndicator.SetActive(false);

            this.currentWaypoints.Add(new EkkohWaypoint(
                    this.ARCamera.transform.localPosition,
                    this.ARCamera.transform.localRotation,
                    this.totalRecordingTime
            ));

            var renderedPoints = new List<EkkohWaypoint>();
            EkkohWaypoint? lastWaypoint = null;
            foreach (var waypoint in this.currentWaypoints)
            {
                if (lastWaypoint.HasValue)
                {
                    if (Vector3.Distance(waypoint.Position, lastWaypoint.Value.Position) > 0.02f)
                    {
                        renderedPoints.Add(waypoint);
                    }
                }
                lastWaypoint = waypoint;
            }

            if (renderedPoints.Count > 0)
            {
                var positions = new Vector3[renderedPoints.Count];
                for (int i = 0; i < renderedPoints.Count; i++)
                {
                    positions[i] = renderedPoints[i].Position;
                }

                var lineRenderer = GameObject.Instantiate(Resources.Load("LineRenderer") as GameObject).GetComponent<LineRendererController>();
                lineRenderer.Setup(positions);
                this.currentLineRendererController = lineRenderer;
            }

            this.isRecording = false;
        }
    }

    private void CheckTouch(Vector3 screenPosition)
    {
        var recordButtonTransform = this.RecordButton.GetComponent<RectTransform>();

        Vector3[] corners = new Vector3[4];
        recordButtonTransform.GetWorldCorners(corners);
        Rect newRect = new Rect(corners[0], corners[2] - corners[0]);

        if (newRect.Contains(screenPosition))
        {
            this.StartRecording();
        }
    }
}
