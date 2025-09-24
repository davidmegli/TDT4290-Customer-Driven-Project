using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Manual Movement (fallback)")]
    public float speed = 5f;
    public float gravity = -9.81f;

    [Header("Follow Settings")]
    [SerializeField] private Transform followTarget;
    [SerializeField] private Vector3 followOffset = new Vector3(0f, -1.6f, 0.15f);
    [SerializeField] private bool autoCaptureInitialOffset = false;
    [SerializeField] private float followPositionLerpSpeed = 12f;
    [SerializeField] private float followRotationLerpSpeed = 12f;
    [SerializeField] private bool matchTargetYawOnly = true;
    [SerializeField] private bool disableControllerWhileFollowing = true;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isFollowing;
    private bool loggedMissingTarget;
    private bool hasCapturedInitialOffset;
    private bool hasSnappedToFollowTarget;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        TryResolveFollowTarget();
    }

    private void Start()
    {
        if (HasFollowTarget())
            CaptureInitialOffset();
        else
            transform.position = new Vector3(0f, 1f, 0f);
    }

    private void Update()
    {
        if (!HasFollowTarget()) TryResolveFollowTarget();

        if (HasFollowTarget())
        {
            if (!isFollowing)
            {
                velocity = Vector3.zero;
                isFollowing = true;
                hasSnappedToFollowTarget = false;
                CaptureInitialOffset();
            }
            FollowTarget();
        }
        else
        {
            if (!loggedMissingTarget)
            {
                Debug.LogWarning("PlayerMovement: No follow target assigned. Falling back to manual input.", this);
                loggedMissingTarget = true;
            }

            if (isFollowing)
            {
                isFollowing = false;
                hasSnappedToFollowTarget = false;
                if (controller != null && disableControllerWhileFollowing)
                    controller.enabled = true;
            }

            HandleManualMovement();
        }
    }

    private bool HasFollowTarget() => followTarget != null;

    private void TryResolveFollowTarget()
    {
        if (followTarget != null && followTarget != transform) return;

        var mainCamera = Camera.main;
        if (mainCamera && mainCamera.transform != transform) { followTarget = mainCamera.transform; return; }

        Camera stereo = null;
        foreach (var cam in Camera.allCameras)
        {
            if (!cam || cam.transform == transform) continue;
            if (cam.stereoTargetEye != StereoTargetEyeMask.None) { stereo = cam; break; }
        }
        if (stereo) { followTarget = stereo.transform; return; }

        foreach (var cam in Camera.allCameras)
        {
            if (cam && cam.transform != transform) { followTarget = cam.transform; return; }
        }

        string[] names = { "CenterEyeAnchor","Main Camera","MainCamera","Camera Offset","CameraOffset","XR Origin","XRRig","OVRCameraRig" };
        foreach (var name in names)
        {
            var go = GameObject.Find(name);
            if (go && go.transform != transform) { followTarget = go.transform; return; }
        }
    }

    private void CaptureInitialOffset()
    {
        if (!autoCaptureInitialOffset || hasCapturedInitialOffset || !followTarget) return;
        followOffset = followTarget.InverseTransformPoint(transform.position);
        hasCapturedInitialOffset = true;
    }

    private void FollowTarget()
    {
        if (!followTarget) return;

        if (controller && disableControllerWhileFollowing && controller.enabled)
            controller.enabled = false;

        Vector3 desiredPos = followTarget.TransformPoint(followOffset);
        bool snapPos = !hasSnappedToFollowTarget || followPositionLerpSpeed <= 0f;

        Vector3 newPos = snapPos
            ? desiredPos
            : Vector3.Lerp(transform.position, desiredPos, 1f - Mathf.Exp(-followPositionLerpSpeed * Time.deltaTime));

        if (controller && controller.enabled)
            controller.Move(newPos - transform.position);
        else
            transform.position = newPos;

        Vector3 fwd = followTarget.forward;
        if (matchTargetYawOnly) fwd.y = 0f;
        if (fwd.sqrMagnitude < 1e-4f) fwd = transform.forward;

        Quaternion desiredRot = matchTargetYawOnly
            ? Quaternion.LookRotation(fwd.normalized, Vector3.up)
            : followTarget.rotation;

        bool snapRot = !hasSnappedToFollowTarget || followRotationLerpSpeed <= 0f;
        transform.rotation = snapRot
            ? desiredRot
            : Quaternion.Slerp(transform.rotation, desiredRot, 1f - Mathf.Exp(-followRotationLerpSpeed * Time.deltaTime));

        if (snapPos) hasSnappedToFollowTarget = true;
    }

    private void HandleManualMovement()
    {
        if (controller && !controller.enabled) controller.enabled = true;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0f) velocity.y = -2f;
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (followPositionLerpSpeed < 0f) followPositionLerpSpeed = 0f;
        if (followRotationLerpSpeed < 0f) followRotationLerpSpeed = 0f;
    }
#endif
}
