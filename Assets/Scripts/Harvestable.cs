using UnityEngine;
using System.Collections.Generic;
using UnityEngine.VFX;

public class Harvestable : MonoBehaviour
{
    [Header("Required Item")]
    public ItemSO requiredItem;

    [Header("Harvest Range")]
    public float harvestRange = 50f;

    [Header("Spawn Points")]
    public List<GameObject> spawnPoints = new List<GameObject>();

    [Header("Prefab to Spawn")]
    public GameObject prefabToSpawn;

    [Header("VFX")]
    public GameObject destroyVFXPrefab;

    [Header("Audio")]
    public AudioSource audioSource;
    public float audioVolume = 2f;
    private AudioClip cachedClip;

    private Inventory inventory;
    private Transform playerTransform;

    private void Start()
    {
        inventory = FindFirstObjectByType<Inventory>();

        if (inventory == null)
            Debug.LogError("Inventory not found in scene!");

        playerTransform = inventory?.GetComponent<Transform>();

        if (audioSource != null)
            cachedClip = audioSource.clip;
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

        float distanceToHarvest = Vector3.Distance(Camera.main.transform.position, transform.position);
        if (distanceToHarvest > harvestRange)
            return;

        ItemSO heldItem = inventory.GetActiveHotbarItem();

        if (heldItem != requiredItem || heldItem == null)
            return;

        Vector3 playPos = spawnPoints.Count > 0 ? spawnPoints[0].transform.position : transform.position;

        PlayDestroyVFX();
        SpawnItems();

        Destroy(gameObject);

        PlayHarvestSound(playPos);
    }

    private void PlayHarvestSound(Vector3 position)
    {
        if (cachedClip == null)
            return;

        AudioSource.PlayClipAtPoint(cachedClip, position);
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