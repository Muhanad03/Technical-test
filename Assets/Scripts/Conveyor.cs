using UnityEngine;

public class Conveyor : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    public Conveyor previousConveyor;
    public Conveyor nextConveyor;
    public Transform collectionTray;
    public Renderer statusLight;
    public Material runningMaterial;
    public Material stoppedMaterial;
    int collectedCount;

    public float Length => Vector3.Distance(startPoint.position, endPoint.position);

    void Update()
    {
        statusLight.sharedMaterial = ConveyorPlacement.beltsRunning ? runningMaterial : stoppedMaterial;

        // When the line is extended, move collected products to its new end.
        if (nextConveyor != null && collectedCount > 0)
        {
            foreach (var product in collectionTray.GetComponentsInChildren<Product>(true))
                nextConveyor.Collect(product);
            collectedCount = 0;
        }
        collectionTray.gameObject.SetActive(nextConveyor == null);
    }

    public void Collect(Product product)
    {
        if (nextConveyor != null)
        {
            nextConveyor.Collect(product);
            return;
        }

        // Arrange products in layers of nine: three rows of three.
        int column = collectedCount % 3;
        int row = (collectedCount / 3) % 3;
        int layer = collectedCount / 9;
        product.transform.SetParent(collectionTray);
        product.transform.localPosition = new Vector3((column - 1) * 0.42f,
            0.05f + layer * 0.3f, (row - 1) * 0.42f);
        product.transform.localRotation = Quaternion.identity;
        product.enabled = false;
        collectedCount++;
    }

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
