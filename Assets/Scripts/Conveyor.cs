using UnityEngine;

public class Conveyor : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    public Conveyor previousConveyor;
    public Conveyor nextConveyor;

    public float Length => Vector3.Distance(startPoint.position, endPoint.position);

    // Show the product's path when looking at the prefab in the Scene view.
    void OnDrawGizmos()
    {
        if (startPoint == null || endPoint == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(startPoint.position, 0.05f);
        Gizmos.DrawLine(startPoint.position, endPoint.position);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(endPoint.position, 0.05f);
    }
}
