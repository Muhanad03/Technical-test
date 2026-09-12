using UnityEngine;

public class Product : MonoBehaviour
{
    public float speed = 1f;
    Conveyor conveyor;
    float distance;

    public void StartMoving(Conveyor firstConveyor)
    {
        conveyor = firstConveyor;
        transform.position = conveyor.startPoint.position;
    }

    void Update()
    {
        if (conveyor == null || !conveyor.gameObject.activeSelf)
        {
            Destroy(gameObject); // The belt underneath this product was deleted.
            return;
        }
        if (!ConveyorPlacement.beltsRunning) return;
        distance += speed * Time.deltaTime;

        // Keep any extra distance when crossing onto the next conveyor.
        while (distance >= conveyor.Length)
        {
            distance -= conveyor.Length;
            if (conveyor.nextConveyor == null)
            {
                conveyor.Collect(this);
                return;
            }
            conveyor = conveyor.nextConveyor;
        }

        transform.position = Vector3.Lerp(conveyor.startPoint.position,
            conveyor.endPoint.position, distance / conveyor.Length) + Vector3.up * 0.01f;
        transform.rotation = Quaternion.LookRotation(
            conveyor.endPoint.position - conveyor.startPoint.position);
    }
}
