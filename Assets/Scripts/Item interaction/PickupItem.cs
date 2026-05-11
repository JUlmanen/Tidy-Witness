using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public Vector3 holdOffset = new Vector3(0.6f, -0.3f, 0.8f);
    public Vector3 holdRotation = Vector3.zero;

    private bool isHeld = false;
    private Collider physCollider;
    private Rigidbody rb;
    private Vector3 originalScale;

    void Start()
    {
        physCollider = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        originalScale = transform.localScale;
    }

    public void PickUp(Transform holdPoint)
    {
        if (isHeld) return;
        isHeld = true;
        transform.SetParent(holdPoint);
        transform.localPosition = holdOffset;
        transform.localRotation = Quaternion.Euler(holdRotation);
        if (rb) rb.isKinematic = true;
        if (physCollider) physCollider.enabled = false;
    }

    public void Drop()
    {
        if (!isHeld) return;
        isHeld = false;
        transform.SetParent(null);
        transform.localScale = originalScale;
        if (rb) rb.isKinematic = false;
        if (physCollider) physCollider.enabled = true;
    }

    public void Place(Transform slot)
    {
        if (!isHeld) return;
        isHeld = false;
        // Detach from player
        transform.SetParent(null);
        // Snap to slot position and rotation
        transform.position = slot.position;
        transform.rotation = slot.rotation;
        transform.localScale = originalScale;
        // Re‑enable collider and physics
        if (physCollider) physCollider.enabled = true;
        if (rb)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public bool IsHeld => isHeld;
}