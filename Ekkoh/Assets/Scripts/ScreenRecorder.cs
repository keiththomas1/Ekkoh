using System.Collections;
using System.Collections.Generic;
using VoxelBusters.ReplayKit;
using UnityEngine;
using UnityEngine.Events;

public class ScreenRecorder : MonoBehaviour
{
    public class VideoGeneratedEvent : UnityEvent<string> { }

    public VideoGeneratedEvent OnVideoGenerated = new VideoGeneratedEvent();

    private bool hasRecordedAtLeastOneVideo = false;

    void Start()
    {
        if (ReplayKitManager.IsRecordingAPIAvailable())
        {
            ReplayKitManager.DidInitialise += this.DidInitialise;
            ReplayKitManager.DidRecordingStateChange += this.RecordingStateChanged;
            ReplayKitManager.Initialise();
        }
        else
        {
            Debug.Log("Recording API is unavailable..");
        }
    }

    void Update()
    {
    }

    // If we are currently "processing" recorded video but don't have a preview yet.
    public bool IsProcessing()
    {
        return this.hasRecordedAtLeastOneVideo && !ReplayKitManager.IsPreviewAvailable();
    }

    public void StartRecording()
    {
        if (ReplayKitManager.IsRecording())
        {
            ReplayKitManager.StopRecording();
        }
        ReplayKitManager.StartRecording(true);
    }

    public void StopRecording()
    {
        if (ReplayKitManager.IsRecording())
        {
            ReplayKitManager.StopRecording();
            this.hasRecordedAtLeastOneVideo = true;
        }
    }

    private void RecordingStateChanged(ReplayKitRecordingState state, string message)
    {
        switch (state)
        {
            case ReplayKitRecordingState.Started:
                Debug.Log("Recording started.");
                break;
            case ReplayKitRecordingState.Stopped:
                Debug.Log("Recording stopped.");
                break;
            case ReplayKitRecordingState.Failed:
                Debug.Log($"There was a failure with the recording API: {message}");
                break;
            case ReplayKitRecordingState.Available:
                var previewPath = ReplayKitManager.GetPreviewFilePath();
                Debug.Log($"Preview is abavilable at: {previewPath}");
                ReplayKitManager.SavePreview((error) =>
                {
                    Debug.Log("Saved preview to gallery with error : " + ((error == null) ? "null" : error));
                });
                this.OnVideoGenerated.Invoke(previewPath);
                break;
            default:
                Debug.Log($"Unknown state reached in RecordingStateChanged");
                break;
        }
    }

    private void DidInitialise(ReplayKitInitialisationState state, string message)
    {
        switch (state)
        {
            case ReplayKitInitialisationState.Success:
                Debug.Log("ReplayKitManager.DidInitialise : Initialisation Success");
                break;
            case ReplayKitInitialisationState.Failed:
                Debug.Log("ReplayKitManager.DidInitialise : Initialisation Failed with message[" + message + "]");
                break;
            default:
                Debug.Log("Unknown State");
                break;
        }
    }
}
