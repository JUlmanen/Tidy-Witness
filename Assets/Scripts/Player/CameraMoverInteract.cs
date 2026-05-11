using UnityEngine;
using StarterAssets;
using UnityEngine.InputSystem;
using System.Collections;

public class CameraMoverInteract : MonoBehaviour
{
    public Transform playerCamera;
    public Transform inspectionPoint;
    public GameObject player;
    public bool exitOnMoveAway = true;
    public float transitionDuration = 0.5f;

    private ThirdPersonController playerController;
    private Renderer[] playerRenderers;
    private ItemCarrier itemCarrier;
    private bool isPlayerInRange = false;
    private bool isViewing = false;
    private bool isTransitioning = false;
    private bool wasHoldingItem = false;

    private Transform originalCameraParent;
    private Vector3 originalCameraLocalPos;
    private Quaternion originalCameraLocalRot;
    private Collider bookshelfCollider;

    private InputAction interactAction;

    void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main.transform;
        if (player == null) player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<ThirdPersonController>();
            playerRenderers = player.GetComponentsInChildren<Renderer>();
            itemCarrier = player.GetComponent<ItemCarrier>();
        }
        bookshelfCollider = GetComponent<Collider>();
        interactAction = new InputAction("Interact", binding: "<Keyboard>/e");
        interactAction.performed += _ => { if (isPlayerInRange && !isTransitioning) ToggleView(); };
        interactAction.Enable();
    }

    void OnDestroy() => interactAction?.Disable();

    void ToggleView()
    {
        if (isViewing) StartCoroutine(TransitionToPlayer());
        else StartCoroutine(TransitionToInspection());
    }

    IEnumerator TransitionToInspection()
    {
        if (inspectionPoint == null || playerController == null) yield break;
        isTransitioning = true;
        isViewing = true;
        playerController.enabled = false;
        if (bookshelfCollider) bookshelfCollider.enabled = false;
        HidePlayer();

        // Store original camera state
        originalCameraParent = playerCamera.parent;
        originalCameraLocalPos = playerCamera.localPosition;
        originalCameraLocalRot = playerCamera.localRotation;

        playerCamera.SetParent(null, true);
        Vector3 startPos = playerCamera.position;
        Quaternion startRot = playerCamera.rotation;
        Vector3 endPos = inspectionPoint.position;
        Quaternion endRot = inspectionPoint.rotation;
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;
            playerCamera.position = Vector3.Lerp(startPos, endPos, t);
            playerCamera.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }
        playerCamera.position = endPos;
        playerCamera.rotation = endRot;

        playerCamera.SetParent(inspectionPoint);
        playerCamera.localPosition = Vector3.zero;
        playerCamera.localRotation = Quaternion.identity;

        if (itemCarrier != null && itemCarrier.IsHoldingItem())
        {
            wasHoldingItem = true;
            itemCarrier.EnableMouseFollow(true);
        }
        else wasHoldingItem = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isTransitioning = false;
    }

    IEnumerator TransitionToPlayer()
    {
        if (playerController == null) yield break;
        isTransitioning = true;
        isViewing = false;

        playerCamera.SetParent(null, true);
        Vector3 startPos = playerCamera.position;
        Quaternion startRot = playerCamera.rotation;

        // Compute target world position/rotation from original parent and local transforms
        Vector3 targetWorldPos = originalCameraParent != null
            ? originalCameraParent.TransformPoint(originalCameraLocalPos)
            : originalCameraLocalPos;
        Quaternion targetWorldRot = originalCameraParent != null
            ? originalCameraParent.rotation * originalCameraLocalRot
            : originalCameraLocalRot;

        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;
            playerCamera.position = Vector3.Lerp(startPos, targetWorldPos, t);
            playerCamera.rotation = Quaternion.Slerp(startRot, targetWorldRot, t);
            yield return null;
        }
        playerCamera.position = targetWorldPos;
        playerCamera.rotation = targetWorldRot;

        // Restore original parent and local transforms
        playerCamera.SetParent(originalCameraParent);
        playerCamera.localPosition = originalCameraLocalPos;
        playerCamera.localRotation = originalCameraLocalRot;

        ShowPlayer();
        playerController.enabled = true;
        if (bookshelfCollider) bookshelfCollider.enabled = true;
        if (wasHoldingItem && itemCarrier != null) itemCarrier.EnableMouseFollow(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isTransitioning = false;
    }

    void HidePlayer()
    {
        if (playerRenderers == null) return;
        foreach (var r in playerRenderers) if (r) r.enabled = false;
    }

    void ShowPlayer()
    {
        if (playerRenderers == null) return;
        foreach (var r in playerRenderers) if (r) r.enabled = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerInRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (isViewing && exitOnMoveAway && !isTransitioning)
                StartCoroutine(TransitionToPlayer());
        }
    }

    void Update()
    {
        if (isViewing && !isTransitioning && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = playerCamera.GetComponent<Camera>().ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, 10f))
            {
                PlacementSlot slot = hit.collider.GetComponent<PlacementSlot>();
                if (slot != null && !slot.IsOccupied && itemCarrier != null && itemCarrier.IsHoldingItem())
                {
                    itemCarrier.PlaceCurrentItem(slot.transform);
                    slot.IsOccupied = true;
                    return;
                }
            }
            if (itemCarrier != null && itemCarrier.IsHoldingItem())
                itemCarrier.AttemptPlaceAnimation();
        }
    }
}