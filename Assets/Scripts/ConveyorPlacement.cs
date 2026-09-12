using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ConveyorPlacement : MonoBehaviour
{
    public Camera placementCamera;
    public Collider floor;
    public Conveyor[] conveyorPrefabs;
    public Product[] productPrefabs;
    public float snapDistance = 1f;
    public static bool beltsRunning = true;

    readonly List<Conveyor> placedConveyors = new List<Conveyor>();
    Conveyor preview;
    int selectedConveyor;
    int nextProduct;
    float rotation;
    float nextSpawnTime;

    void Start()
    {
        beltsRunning = true;
        SelectConveyor(0);
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;
        if (keyboard == null || mouse == null) return;

        if (keyboard.digit1Key.wasPressedThisFrame) SelectConveyor(0);
        if (keyboard.digit2Key.wasPressedThisFrame) SelectConveyor(1);
        if (keyboard.digit3Key.wasPressedThisFrame) SelectConveyor(2);
        if (keyboard.pKey.wasPressedThisFrame) beltsRunning = !beltsRunning;
        if (keyboard.rKey.wasPressedThisFrame) rotation += 90f;
        if (keyboard.eKey.wasPressedThisFrame) DeleteConveyor();

        MovePreview(mouse.position.ReadValue());
        if (mouse.leftButton.wasPressedThisFrame && preview.gameObject.activeSelf)
            PlaceConveyor();
        if (keyboard.spaceKey.wasPressedThisFrame)
            SpawnProduct();
    }

    void SelectConveyor(int index)
    {
        selectedConveyor = index;
        if (preview != null) Destroy(preview.gameObject);
        preview = Instantiate(conveyorPrefabs[index]);
        preview.name = "Placement preview";
        preview.enabled = false;
        preview.collectionTray.gameObject.SetActive(false);
        // Ignore the preview when pointing at a placed belt to spawn products.
        foreach (var part in preview.GetComponentsInChildren<Transform>())
            part.gameObject.layer = 2; // Unity's Ignore Raycast layer.

        // Tint only the preview, leaving the prefab's materials unchanged.
        var tint = new MaterialPropertyBlock();
        tint.SetColor("_BaseColor", new Color(0.4f, 1f, 0.4f));
        foreach (var renderer in preview.GetComponentsInChildren<Renderer>())
            renderer.SetPropertyBlock(tint);
    }

    void MovePreview(Vector2 mousePosition)
    {
        Ray ray = placementCamera.ScreenPointToRay(mousePosition);
        bool overFloor = floor.Raycast(ray, out RaycastHit hit, 100f);
        preview.gameObject.SetActive(overFloor);
        if (!overFloor) return;

        preview.transform.SetPositionAndRotation(hit.point, Quaternion.Euler(0, rotation, 0));
        float closestDistance = snapDistance;

        foreach (var conveyor in placedConveyors)
        {
            if (conveyor.nextConveyor == null)
                TrySnap(conveyor.endPoint.position, preview.startPoint.localPosition,
                    conveyor.transform.rotation, hit.point, ref closestDistance);
            if (conveyor.previousConveyor == null)
                TrySnap(conveyor.startPoint.position, preview.endPoint.localPosition,
                    conveyor.transform.rotation, hit.point, ref closestDistance);
        }
    }

    void TrySnap(Vector3 connection, Vector3 previewEnd, Quaternion direction,
        Vector3 mousePosition, ref float closestDistance)
    {
        Vector3 position = connection - direction * previewEnd;
        if (position.y < -0.01f) return; // Do not put an incline beneath the floor.
        float distance = Vector2.Distance(new Vector2(mousePosition.x, mousePosition.z),
            new Vector2(position.x, position.z));
        if (distance >= closestDistance) return;
        closestDistance = distance;
        preview.transform.SetPositionAndRotation(position, direction);
    }

    void PlaceConveyor()
    {
        Conveyor conveyor = Instantiate(conveyorPrefabs[selectedConveyor],
            preview.transform.position, preview.transform.rotation);
        // Connect both touching ends, including when filling a gap in a line.
        foreach (var existing in placedConveyors)
        {
            if (Vector3.Dot(existing.transform.forward, conveyor.transform.forward) < 0.99f)
                continue;
            if (existing.nextConveyor == null && conveyor.previousConveyor == null &&
                Vector3.Distance(existing.endPoint.position, conveyor.startPoint.position) < 0.05f)
            {
                existing.nextConveyor = conveyor;
                conveyor.previousConveyor = existing;
            }
            if (existing.previousConveyor == null && conveyor.nextConveyor == null &&
                Vector3.Distance(conveyor.endPoint.position, existing.startPoint.position) < 0.05f)
            {
                conveyor.nextConveyor = existing;
                existing.previousConveyor = conveyor;
            }
        }
        placedConveyors.Add(conveyor);
    }

    Conveyor ConveyorUnderMouse()
    {
        if (Mouse.current == null) return null;
        Ray ray = placementCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, 100f)) return null;
        return hit.collider.GetComponentInParent<Conveyor>();
    }

    void DeleteConveyor()
    {
        Conveyor conveyor = ConveyorUnderMouse();
        if (conveyor == null) return;
        if (conveyor.previousConveyor != null) conveyor.previousConveyor.nextConveyor = null;
        if (conveyor.nextConveyor != null) conveyor.nextConveyor.previousConveyor = null;
        placedConveyors.Remove(conveyor);
        conveyor.gameObject.SetActive(false);
        Destroy(conveyor.gameObject);
    }

    void SpawnProduct()
    {
        if (Time.time < nextSpawnTime) return;
        Conveyor first = ConveyorUnderMouse();
        if (first == null) return;
        // Find the start of whichever line the mouse is pointing at.
        while (first.previousConveyor != null) first = first.previousConveyor;
        Product product = Instantiate(productPrefabs[nextProduct]);
        product.StartMoving(first);
        nextProduct = (nextProduct + 1) % productPrefabs.Length;
        nextSpawnTime = Time.time + 0.8f;
    }
}
