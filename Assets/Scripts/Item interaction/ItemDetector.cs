using UnityEngine;

public class ItemDetector : MonoBehaviour
{
    private PickupItem rootItem;
    private bool playerInRange = false;

    void Start()
    {
        rootItem = GetComponentInParent<PickupItem>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) playerInRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) playerInRange = false;
    }

    public bool IsPlayerInRange() => playerInRange;
    public PickupItem GetItem() => rootItem;
}