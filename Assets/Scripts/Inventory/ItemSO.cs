using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Items/Item")]
public class ItemSO : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    public string description;

    public int maxStackSize = 64;

    public GameObject itemPrefab;
    public GameObject handItemPrefab;

    // ✅ ADD THESE
    [Header("Hand Transform")]
    public Vector3 handLocalPosition;
    public Vector3 handLocalRotation;
    public Vector3 handLocalScale = Vector3.one;

    [Header("Combat")]
    public bool doesDamage = false;
    public float damageAmount = 10f;
}