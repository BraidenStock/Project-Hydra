using UnityEngine;
using System.Collections.Generic;
using UnityEngine.VFX;

public class Harvestable : MonoBehaviour
{
    [Header("Required Item")]
    public ItemSO requiredItem;

    [Header("Spawn Points")]
    public List<GameObject> spawnPoints = new List<GameObject>();

    [Header("Prefab to Spawn")]
    public GameObject prefabToSpawn;

    [Header("VFX")]
    public GameObject destroyVFXPrefab;

    private Inventory inventory;

    private void Start()
    {
        inventory = FindFirstObjectByType<Inventory>();

        if (inventory == null)
            Debug.LogError("Inventory not found in scene!");
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            HandleClick();
    }

    private void HandleClick()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        if (!Physics.Raycast(ray, out RaycastHit hit))
            return;

        if (!IsPartOfThisObject(hit.collider.gameObject))
            return;

        ItemSO heldItem = inventory.GetActiveHotbarItem();

        if (heldItem != requiredItem || heldItem == null)
            return;

        PlayDestroyVFX();
        SpawnItems();

        Destroy(gameObject, 0.05f);
    }

    private void PlayDestroyVFX()
    {
        if (destroyVFXPrefab == null || spawnPoints.Count == 0)
            return;

        foreach (GameObject spawnPoint in spawnPoints)
        {
            GameObject vfxObj = Instantiate(destroyVFXPrefab, spawnPoint.transform.position, Quaternion.identity);
            VisualEffect vfx = vfxObj.GetComponent<VisualEffect>();

            if (vfx == null)
                continue;

            vfx.Play();
        }
    }

    private Mesh BuildCombinedMesh()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();

        if (meshFilters.Length == 0)
            return null;

        List<CombineInstance> combine = new List<CombineInstance>();

        foreach (MeshFilter mf in meshFilters)
        {
            if (mf.sharedMesh == null)
                continue;

            CombineInstance ci = new CombineInstance();
            ci.mesh = mf.sharedMesh;
            ci.transform = mf.transform.localToWorldMatrix;

            combine.Add(ci);
        }

        if (combine.Count == 0)
            return null;

        Mesh combinedMesh = new Mesh();
        combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        combinedMesh.CombineMeshes(combine.ToArray(), true, true);

        return combinedMesh;
    }

    private void SpawnItems()
    {
        if (spawnPoints.Count == 0 || prefabToSpawn == null)
            return;

        foreach (GameObject spawnPoint in spawnPoints)
        {
            Instantiate(
                prefabToSpawn,
                spawnPoint.transform.position,
                spawnPoint.transform.rotation * Quaternion.Euler(90, 0, 0)
            );
        }
    }

    private bool IsPartOfThisObject(GameObject hitObject)
    {
        return hitObject == gameObject || hitObject.transform.IsChildOf(transform);
    }
}