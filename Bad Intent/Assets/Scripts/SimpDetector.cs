using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpDetector : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField] string _DisplayName;
    [SerializeField] Camera LinkedCamera;

    [SerializeField] bool SyncToMainCameraConfig = true;

    [SerializeField] AudioListener CameraAudio;
    [SerializeField] Transform PivotPoint;
    [SerializeField] float DefaultPitch = 20f;
    [SerializeField] float AngleSwept = 60f;
    [SerializeField] float SweepSpeed = 6f;
    [SerializeField] int OutputTextureSize = 256;

    [Header("Detection")]
    [SerializeField] float DetectionHalfAngle = 30f;
    [SerializeField] float DetectionRange = 20f;
    [SerializeField] SphereCollider DetectionTrigger;
    [SerializeField] Light DetectionLight;
    [SerializeField] Color Colour_NothingDetected = Color.green;
    [SerializeField] Color Colour_FullyDetected = Color.red;
    [SerializeField] float DetectionBuildRate = 0.5f;
    [SerializeField] float DetectionDecayRate = 0.5f;
    [SerializeField] List<string> DetectableTags;
    [SerializeField] LayerMask DetectionLayerMask = ~0;
    float CosDetectionHalfAngle;

    public RenderTexture OutputTexture { get; private set; }
    public string DisplayName => _DisplayName;

    float CurrentAngle = 0f;
    bool SweepClockwise = true;
    List<SecurityConsole> CurrentlyWatchingConsoles = new List<SecurityConsole>();

    class PotentialTarget
    {
        public GameObject LinkedGO;
        public bool InFOV;
        public float DetectionLevel;
    }

    Dictionary<GameObject, PotentialTarget> AllTargets = new Dictionary<GameObject, PotentialTarget>();

    // Start is called before the first frame update
    void Start()
    {
        //Camera off by default
        LinkedCamera.enabled = false;
        CameraAudio.enabled = false;

        // setup the collider and Light
        DetectionLight.color = Colour_NothingDetected;
        DetectionLight.range = DetectionRange;
        DetectionLight.spotAngle = DetectionHalfAngle * 2f;
        DetectionTrigger.radius = DetectionRange;

        // cache the detection data
        CosDetectionHalfAngle = Mathf.Cos(Mathf.Deg2Rad * DetectionHalfAngle);

        if (SyncToMainCameraConfig)
        {
            LinkedCamera.clearFlags = Camera.main.clearFlags;
            LinkedCamera.backgroundColor = Camera.main.backgroundColor;
        }

        // Setup the render texture
        OutputTexture = new RenderTexture(OutputTextureSize, OutputTextureSize, 32);
        LinkedCamera.targetTexture = OutputTexture;
    }

    // Update is called once per frame
    void Update()
    {
        RefreshTargetInfo();

        // Update the angle
        CurrentAngle += SweepSpeed * Time.deltaTime * (SweepClockwise ? 1f : -1f);
        if (Mathf.Abs(CurrentAngle) >= (AngleSwept * 0.5f))
            SweepClockwise = !SweepClockwise;

        // Rotate Camera
        PivotPoint.transform.localEulerAngles = new Vector3(0f, CurrentAngle, DefaultPitch);
    }

    GameObject CurrentlyDetectedTarget;

    void RefreshTargetInfo()
    {
        float highestDetectionLevel = 0f;
        CurrentlyDetectedTarget = null;

        // refresh each target
        foreach (var target in AllTargets)
        {
            var targetInfo = target.Value;bool isVisible = false;

            // is the SIMP in the field of view
            Vector3 vecToTarget = targetInfo.LinkedGO.transform.position - LinkedCamera.transform.position;
            if (Vector3.Dot(LinkedCamera.transform.forward, vecToTarget.normalized) >= CosDetectionHalfAngle)
            {
                // check if we can see the target
                RaycastHit hitInfo;
                if (Physics.Raycast(LinkedCamera.transform.position, LinkedCamera.transform.forward, 
                                    out hitInfo, DetectionRange, DetectionLayerMask, QueryTriggerInteraction.Ignore))
                {
                    if (hitInfo.collider.gameObject == targetInfo.LinkedGO)
                        isVisible = true;
                }
            }

            // update detection level
            targetInfo.InFOV = isVisible;
            if (isVisible)
                targetInfo.DetectionLevel = Mathf.Clamp01(targetInfo.DetectionLevel + DetectionBuildRate * Time.deltaTime);
            else
                targetInfo.DetectionLevel = Mathf.Clamp01(targetInfo.DetectionLevel + DetectionDecayRate * Time.deltaTime);

            // found new more detected target?
            if (targetInfo.DetectionLevel > highestDetectionLevel)
            {
                highestDetectionLevel = targetInfo.DetectionLevel;
                CurrentlyDetectedTarget = targetInfo.LinkedGO;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // skip if the tag isn't supported
        if (!DetectableTags.Contains(other.tag))
            return;

        // add to our target list
        AllTargets[other.gameObject] = new PotentialTarget() { LinkedGO = other.gameObject };
    }

    private void OnTriggerExit(Collider other)
    {
        // skip if the tag isn't supported
        if (!DetectableTags.Contains(other.tag))
            return;

        // remove to our target list
        AllTargets.Remove(other.gameObject);
    }
}
