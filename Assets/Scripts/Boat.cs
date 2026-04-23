using UnityEngine;
using TMPro; // remove this if you're using legacy UI Text

public class Boat : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject winTextObject;

    private void Start()
    {
        if (winTextObject != null)
            winTextObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            WinGame();
        }
    }

    private void WinGame()
    {
        Debug.Log("YOU WIN!");

        if (winTextObject != null)
            winTextObject.SetActive(true);

        Time.timeScale = 0f;
    }
}