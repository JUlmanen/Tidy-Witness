using UnityEngine;

public interface IPickupable
{
    void PickUp(Transform holdPoint);
    void Drop();
    void Place(Transform slot);
    bool IsHeld { get; }
}