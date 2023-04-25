using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DiscordMOD : MonoBehaviour
{
    [SerializeField] List<SimpDetector> LinkedCameras;
    [SerializeField] RawImage CameraImage;
    [SerializeField] TextMeshProUGUI ActiveCameraLabel;

    [SerializeField] bool AutoswitchEnable = false;
    [SerializeField] float AutoswitchInterval = 15f;
    [SerializeField] float AutoswitchStartTime = 10f;

    float TimeUntilNextAutoswitch = -1f;

    public int ActiveCameraIndex { get; private set; } = -1;
    public SimpDetector ActiveCamera => ActiveCameraIndex < 0 ? null : LinkedCameras[ActiveCameraIndex];

    // Start is called before the first frame update
    void Start()
    {
        ActiveCameraLabel.text = "Camera: None";
    }

    // Update is called once per frame
    void Update()
    {
        // is autoswitch active
        if (AutoswitchEnable)
        {
            TimeUntilNextAutoswitch -= Time.deltaTime;

            // time to switch?
            if (TimeUntilNextAutoswitch < 0)
            {
                TimeUntilNextAutoswitch = AutoswitchInterval;
                SelectNextCamera();
            }
        }
    }

    public void OnClicked()
    {
        SelectNextCamera();

        if (AutoswitchEnable)
        {
            TimeUntilNextAutoswitch = AutoswitchStartTime;
        }
    }

    void SelectNextCamera()
    {
        var previousCamera = ActiveCamera;

        // switch to the next camera
        ActiveCameraIndex = (ActiveCameraIndex + 1) % LinkedCameras.Count;

        // Tell the previous camera to stop it
        if (previousCamera != null)
            previousCamera.StopWatching(this);

        // Tell the new camera to get on with it
        ActiveCamera.StartWatching(this);
        ActiveCameraLabel.text = $"Camera: {ActiveCamera.DisplayName}";
        CameraImage.texture = ActiveCamera.OutputTexture;
    }

    public void OnDetected(GameObject target)
    {
        Debug.Log($"Detected {target.name}");
    }

    public void OnAllClear()
    {
        Debug.Log("All clear");
    }
}
