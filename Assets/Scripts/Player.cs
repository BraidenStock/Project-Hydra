using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player instance;

    public ScriptableObject equippedItem;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}