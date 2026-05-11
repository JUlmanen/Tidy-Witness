using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPickup : MonoBehaviour
{
    public Transform holdPoint;
    private IPickupable heldItem = null;

    void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (heldItem == null) TryPickup();
            else DropCurrent();
        }
        if (Keyboard.current.qKey.wasPressedThisFrame && heldItem != null)
            DropCurrent();
    }

    void TryPickup()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 2f);
        foreach (var hit in hits)
        {
            IPickupable item = hit.GetComponent<IPickupable>();
            if (item != null && !item.IsHeld)
            {
                item.PickUp(holdPoint);
                heldItem = item;
                return;
            }
        }
    }

    void DropCurrent()
    {
        if (heldItem == null) return;
        heldItem.Drop();
        heldItem = null;
    }

    public void PlaceCurrent(Transform slot)
    {
        if (heldItem == null) return;
        heldItem.Place(slot);
        heldItem = null;
    }

    public bool HasItem => heldItem != null;
}