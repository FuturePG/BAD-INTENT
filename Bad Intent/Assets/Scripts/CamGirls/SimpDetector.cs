using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
    [SerializeField] float TargetVOffset = 1f;
    [SerializeField] float MaxRotationSpeed = 15f;

    [Header("Detection")]
    [SerializeField] float DetectionHalfAngle = 30f;
    [SerializeField] float DetectionRange = 20f;
    [SerializeField] SphereCollider DetectionTrigger;
    [SerializeField] Light DetectionLight;
    [SerializeField] Color Colour_NothingDetected = Color.green;
    [SerializeField] Color Colour_FullyDetected = Color.red;
    [SerializeField] float DetectionBuildRate = 0.5f;
    [SerializeField] float DetectionDecayRate = 0.5f;
    [SerializeField] [Range(0f, 1f)] float SussusMoogas = 0.5f;
    [SerializeField] List<string> DetectableTags;
    [SerializeField] LayerMask DetectionLayerMask = ~0;

    [SerializeField] UnityEvent<GameObject> OnDetected = new UnityEvent<GameObject>();
    [SerializeField] UnityEvent OnAllClear = new UnityEvent();

    public RenderTexture OutputTexture { get; private set; }
    public string DisplayName => _DisplayName;

    public GameObject CurrentlyDetectedTarget { get; private set; }

    public bool HasDetectedSimp { get; private set; } = false;

    float CurrentAngle = 0f;
    float CosDetectionHalfAngle;
    bool SweepClockwise = true;
    List<DiscordMOD> CurrentlyWatchingConsoles = new List<DiscordMOD>();

    class PotentialTarget
    {
        public GameObject LinkedGO;
        public bool InFOV;
        public float DetectionLevel;
        public bool OnDetectedEventSent;
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

        Quaternion desiredRotation = PivotPoint.transform.rotation;

        // if sus simp enters then stop rotating automatically
        if (CurrentlyDetectedTarget != null && AllTargets[CurrentlyDetectedTarget].DetectionLevel >= SussusMoogas)
        {
            if (AllTargets[CurrentlyDetectedTarget].InFOV)
            {
                var vecToTarget = (CurrentlyDetectedTarget.transform.position + TargetVOffset * Vector3.up - PivotPoint.transform.position).normalized;

                desiredRotation = Quaternion.LookRotation(vecToTarget, Vector3.up) * Quaternion.Euler(0f, 90f, 0f);
            }
        }
        else
        {
            // Update the angle
            CurrentAngle += SweepSpeed * Time.deltaTime * (SweepClockwise ? 1f : -1f);
            if (Mathf.Abs(CurrentAngle) >= (AngleSwept * 0.5f))
                SweepClockwise = !SweepClockwise;

            // Calculate Rotation
            desiredRotation = PivotPoint.transform.parent.rotation * Quaternion.Euler(0f, CurrentAngle, DefaultPitch);
        }

        PivotPoint.transform.rotation = Quaternion.RotateTowards(PivotPoint.transform.rotation, desiredRotation, MaxRotationSpeed * Time.deltaTime);
    }

    void RefreshTargetInfo()
    {
        float highestDetectionLevel = 0f;
        CurrentlyDetectedTarget = null;

        // refresh each target
        foreach (var target in AllTargets)
        {
            var targetInfo = target.Value;

            bool isVisible = false;

            // is the SIMP in the field of view
            Vector3 vecToTarget = (targetInfo.LinkedGO.transform.position + TargetVOffset * Vector3.up - LinkedCamera.transform.position).normalized;
            if (Vector3.Dot(LinkedCamera.transform.forward, vecToTarget) >= CosDetectionHalfAngle)
            {
                // check if we can see the target
                RaycastHit hitInfo;
                if (Physics.Raycast(LinkedCamera.transform.position, vecToTarget, 
                                    out hitInfo, DetectionRange, DetectionLayerMask, QueryTriggerInteraction.Ignore))
                {
                    if (hitInfo.collider.gameObject == targetInfo.LinkedGO)
                        isVisible = true;
                }
            }

            // update detection level
            targetInfo.InFOV = isVisible;
            if (isVisible)
            {
                float oldDetectionLevel = targetInfo.DetectionLevel;

                targetInfo.DetectionLevel = Mathf.Clamp01(targetInfo.DetectionLevel + DetectionBuildRate * Time.deltaTime);

                // notify all government operatives that the simp is simping
                if (targetInfo.DetectionLevel < 1f && !targetInfo.OnDetectedEventSent)
                {
                    HasDetectedSimp = true;
                    targetInfo.OnDetectedEventSent = true;
                    OnDetected.Invoke(targetInfo.LinkedGO);
                }
            }
            else
                targetInfo.DetectionLevel = Mathf.Clamp01(targetInfo.DetectionLevel + DetectionDecayRate * Time.deltaTime);

            // found new more detected target?
            if (targetInfo.DetectionLevel > highestDetectionLevel)
            {
                highestDetectionLevel = targetInfo.DetectionLevel;
                CurrentlyDetectedTarget = targetInfo.LinkedGO;
            }
        }

        // update the light colour
        if (CurrentlyDetectedTarget != null)
            DetectionLight.color = Color.Lerp(Colour_NothingDetected, Colour_FullyDetected, highestDetectionLevel);
        else
        {
            DetectionLight.color = Colour_NothingDetected;
            if (HasDetectedSimp)
            {
                HasDetectedSimp = false;
                OnAllClear.Invoke();
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

    public void StartWatching(DiscordMOD linkedConsole)
    {
        if (!CurrentlyWatchingConsoles.Contains(linkedConsole))
            CurrentlyWatchingConsoles.Add(linkedConsole);

        OnWatchersChanged();
    }

    public void StopWatching(DiscordMOD linkedConsole)
    {
        CurrentlyWatchingConsoles.Remove(linkedConsole);

        OnWatchersChanged();
    }

    void OnWatchersChanged()
    {
        LinkedCamera.enabled = CurrentlyWatchingConsoles.Count > 0;
    }
}
