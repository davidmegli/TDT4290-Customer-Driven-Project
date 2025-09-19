using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Manual Movement (fallback)")]
    public float speed = 5f;
    public float gravity = -9.81f;

    [Header("Follow Settings")]
    [Tooltip("Target the avatar should follow. If left empty the script falls back to manual movement.")]
    [SerializeField] private Transform followTarget;
    [Tooltip("Desired offset from the follow target, evaluated in the target's local space.")]
    [SerializeField] private Vector3 followOffset = new Vector3(0f, -1.6f, 0.15f);
    [Tooltip("Capture the initial offset from the target when play starts.")]
    [SerializeField] private bool autoCaptureInitialOffset = false;
    [Tooltip("How quickly the avatar interpolates towards the target position.")]
    [SerializeField] private float followPositionLerpSpeed = 12f;
    [Tooltip("How quickly the avatar interpolates towards the target rotation.")]
    [SerializeField] private float followRotationLerpSpeed = 12f;
    [Tooltip("If enabled, the avatar only copies the target's yaw (Y axis rotation).")]
    [SerializeField] private bool matchTargetYawOnly = true;
    [Tooltip("Disable the CharacterController while following so collisions do not push the avatar away from the rig.")]
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
        {
            CaptureInitialOffset();
        }
        else
        {
            transform.position = new Vector3(0f, 1f, 0f);
        }
    }

    private void Update()
    {
        if (!HasFollowTarget())
        {
            TryResolveFollowTarget();
        }

        if (HasFollowTarget())
        {
            if (!isFollowing)
            {
                velocity = Vector3.zero; // clear any residual gravity from manual mode
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
                {
                    controller.enabled = true;
                }
            }

            HandleManualMovement();
        }
    }

    private bool HasFollowTarget() => followTarget != null;

    private void TryResolveFollowTarget()
    {
        if (followTarget != null && followTarget != transform)
        {
            return;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera != null && mainCamera.transform != transform)
        {
            followTarget = mainCamera.transform;
            return;
        }

        Camera stereoCamera = null;
        foreach (Camera cam in Camera.allCameras)
        {
            if (cam == null || cam.transform == transform)
            {
                continue;
            }

            if (cam.stereoTargetEye != StereoTargetEyeMask.None)
            {
                stereoCamera = cam;
                break;
            }
        }

        if (stereoCamera != null)
        {
            followTarget = stereoCamera.transform;
            return;
        }

        foreach (Camera cam in Camera.allCameras)
        {
            if (cam != null && cam.transform != transform)
            {
                followTarget = cam.transform;
                return;
            }
        }

        string[] candidateNames = { "CenterEyeAnchor", "Main Camera", "MainCamera", "Camera Offset", "CameraOffset", "XR Origin", "XRRig", "OVRCameraRig" };
        foreach (string name in candidateNames)
        {
            GameObject found = GameObject.Find(name);
            if (found != null && found.transform != transform)
            {
                followTarget = found.transform;
                return;
            }
        }
    }

    private void CaptureInitialOffset()
    {
        if (!autoCaptureInitialOffset || hasCapturedInitialOffset || followTarget == null)
        {
            return;
        }

        followOffset = followTarget.InverseTransformPoint(transform.position);
        hasCapturedInitialOffset = true;
    }

    private void FollowTarget()
    {
        if (followTarget == null)
        {
            return;
        }

        if (controller != null && disableControllerWhileFollowing && controller.enabled)
        {
            controller.enabled = false;
        }

        Vector3 desiredPosition = followTarget.TransformPoint(followOffset);
        bool snapToTarget = !hasSnappedToFollowTarget || followPositionLerpSpeed <= 0f;

        Vector3 newPosition;
        if (snapToTarget)
        {
            newPosition = desiredPosition;
        }
        else
        {
            float positionLerpT = 1f - Mathf.Exp(-followPositionLerpSpeed * Time.deltaTime);
            newPosition = Vector3.Lerp(transform.position, desiredPosition, positionLerpT);
        }

        if (controller != null && controller.enabled)
        {
            Vector3 delta = newPosition - transform.position;
            controller.Move(delta);
        }
        else
        {
            transform.position = newPosition;
        }

        Vector3 forward = followTarget.forward;
        if (matchTargetYawOnly)
        {
            forward.y = 0f;
        }

        if (forward.sqrMagnitude < 0.0001f)
        {
            forward = transform.forward;
        }

        Quaternion desiredRotation = matchTargetYawOnly
            ? Quaternion.LookRotation(forward.normalized, Vector3.up)
            : followTarget.rotation;

        bool snapRotation = !hasSnappedToFollowTarget || followRotationLerpSpeed <= 0f;
        if (snapRotation)
        {
            transform.rotation = desiredRotation;
        }
        else
        {
            float rotationLerpT = 1f - Mathf.Exp(-followRotationLerpSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationLerpT);
        }

        if (snapToTarget)
        {
            hasSnappedToFollowTarget = true;
        }
    }

    private void HandleManualMovement()
    {
        if (controller != null && !controller.enabled)
        {
            controller.enabled = true;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

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