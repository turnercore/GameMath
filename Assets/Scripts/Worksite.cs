using UnityEngine;

public abstract class Worksite : MonoBehaviour
{
    [Tooltip(
        "Optional transform that defines where NPCs should walk to when arriving at this worksite."
    )]
    public Transform arrivalAnchor;

    // Helper so brains can query a valid arrival point
    public virtual Vector3 GetArrivalPoint(Vector3 fromPosition)
    {
        if (arrivalAnchor != null)
            return arrivalAnchor.position;

        // fallback: use the collider surface or transform center
        Collider col = GetComponent<Collider>();
        if (col != null)
            return col.ClosestPoint(fromPosition);

        return transform.position;
    }
}
