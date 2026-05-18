using UnityEngine;
using System.Collections;

public class BookSlotDetector : MonoBehaviour
{
    private PickupItem pickupItem;
    private bool hasPlaced = false;
    private bool doorMoved = false;

    public GameObject secretDoorShelf;
    public float doorMoveDuration = 1f;
    public Vector3 moveOffset = new Vector3(0f, 0f, 2f); // XYZ movement in world space

    void Start()
    {
        pickupItem = GetComponentInParent<PickupItem>();
        if (secretDoorShelf == null)
        {
            secretDoorShelf = GameObject.Find("SecretDoorShelf");
            if (secretDoorShelf == null)
                Debug.LogWarning("SecretDoorShelf not found.");
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (hasPlaced) return;
        if (pickupItem == null || !pickupItem.IsHeld) return;

        PlacementSlot slot = other.GetComponent<PlacementSlot>();
        if (slot != null && !slot.IsOccupied)
        {
            ItemCarrier carrier = FindFirstObjectByType<ItemCarrier>();
            if (carrier != null && carrier.IsHoldingItem())
            {
                carrier.PlaceCurrentItem(slot.transform);
                slot.IsOccupied = true;

                Collider slotCollider = other.GetComponent<Collider>();
                if (slotCollider != null)
                    slotCollider.enabled = false;

                if (!doorMoved && secretDoorShelf != null)
                {
                    StartCoroutine(MoveDoorSmooth());
                    doorMoved = true;
                }

                hasPlaced = true;
            }
        }
    }

    private IEnumerator MoveDoorSmooth()
    {
        Vector3 startPos = secretDoorShelf.transform.position;
        Vector3 endPos = startPos + moveOffset;
        float elapsed = 0f;

        while (elapsed < doorMoveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / doorMoveDuration;
            secretDoorShelf.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        secretDoorShelf.transform.position = endPos;
        Debug.Log($"SecretDoorShelf moved by {moveOffset} over {doorMoveDuration} seconds.");
    }

    private void OnDisable()
    {
        hasPlaced = false;
    }
}