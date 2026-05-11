using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class ItemCarrier : MonoBehaviour
{
    public Transform holdPoint;
    public float placeAnimDistance = 0.3f;
    public float placeAnimDuration = 0.3f;
    public Vector3 inspectionRotation = Vector3.zero;

    private PickupItem heldItem = null;
    private bool mouseFollowEnabled = false;
    private bool isAnimating = false;

    void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame && heldItem == null)
            TryPickup();
        if (Keyboard.current.qKey.wasPressedThisFrame && heldItem != null)
            DropCurrentItem();

        if (mouseFollowEnabled && heldItem != null && !isAnimating)
        {
            Vector3 mousePos = Mouse.current.position.ReadValue();
            mousePos.z = Camera.main.WorldToScreenPoint(holdPoint.position).z;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            heldItem.transform.position = worldPos;
            heldItem.transform.rotation = Quaternion.Euler(inspectionRotation);
        }
    }

    void TryPickup()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 2f);
        float closestDist = 2f;
        ItemDetector best = null;
        foreach (var hit in hits)
        {
            ItemDetector d = hit.GetComponent<ItemDetector>();
            if (d != null && d.IsPlayerInRange() && !d.GetItem().IsHeld)
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < closestDist) { closestDist = dist; best = d; }
            }
        }
        if (best != null)
        {
            PickupItem item = best.GetItem();
            item.PickUp(holdPoint);
            heldItem = item;
        }
    }

    void DropCurrentItem()
    {
        if (heldItem == null) return;
        heldItem.Drop();
        heldItem = null;
        StopAllCoroutines();
        isAnimating = false;
    }

    public void PlaceCurrentItem(Transform slot)
    {
        if (heldItem == null) return;
        heldItem.Place(slot);
        heldItem = null;
        mouseFollowEnabled = false;
        StopAllCoroutines();
        isAnimating = false;
    }

    public void AttemptPlaceAnimation()
    {
        if (heldItem == null || isAnimating) return;
        StartCoroutine(PlaceAnimation());
    }

    IEnumerator PlaceAnimation()
    {
        isAnimating = true;
        Vector3 startPos = heldItem.transform.position;
        Vector3 forwardDir = Camera.main.transform.forward;
        Vector3 endPos = startPos + forwardDir * placeAnimDistance;
        float elapsed = 0f;

        while (elapsed < placeAnimDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (placeAnimDuration / 2f);
            heldItem.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        heldItem.transform.position = endPos;

        elapsed = 0f;
        while (elapsed < placeAnimDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (placeAnimDuration / 2f);
            heldItem.transform.position = Vector3.Lerp(endPos, startPos, t);
            yield return null;
        }
        heldItem.transform.position = startPos;
        isAnimating = false;
    }

    public bool IsHoldingItem() => heldItem != null;
    public void EnableMouseFollow(bool enable) => mouseFollowEnabled = enable;
}