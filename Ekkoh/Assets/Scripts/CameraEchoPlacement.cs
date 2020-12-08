using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraEchoPlacement : MonoBehaviour
{
	[SerializeField]
	private Camera ARCamera;
	[SerializeField]
	private Button RecordButton;

    private bool isRecording = false;
    private const float RECORDING_POINT_INTERVAL = 0.2f;
    private float recordingPointTimer = 0f;
    private List<Matrix4x4> currentRecordingPoints = new List<Matrix4x4>();

    // Start is called before the first frame update
    void Start()
    {
        this.RecordButton.onClick.AddListener(this.StartRecording);
    }

    // Update is called once per frame
    void Update()
    {
        if (this.recordingPointTimer > 0f && this.isRecording)
        {
            this.recordingPointTimer -= Time.deltaTime;

            if (this.recordingPointTimer <= 0f)
            {
                this.recordingPointTimer = RECORDING_POINT_INTERVAL;

                this.currentRecordingPoints.Add(this.ARCamera.transform.worldToLocalMatrix);
            }
        }
    }

    private void StartRecording()
    {
        if (!this.isRecording)
        {
            this.isRecording = true;

            this.currentRecordingPoints.Clear();
            this.currentRecordingPoints.Add(this.ARCamera.transform.worldToLocalMatrix);

            this.recordingPointTimer = RECORDING_POINT_INTERVAL;
        }
    }
}
