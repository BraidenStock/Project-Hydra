using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerCamera : MonoBehaviour
{
    public float sensitivityX;
    public float sensitivityY;

    public Transform playerOrientation;
    private Inventory inventory;

    float rotationX;
    float rotationY;

    private void Start()
    {
        inventory = GetComponent<Inventory>();
        if (inventory == null)
        {
            inventory = Object.FindFirstObjectByType<Inventory>();
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Don't rotate if inventory is open
        if (inventory != null && inventory.container.activeSelf)
        {
            return;
        }

        // Get mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * sensitivityX * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensitivityY * Time.deltaTime;

        rotationY += mouseX;
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);

        // Rotate the camera and orientation
        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0);
        playerOrientation.rotation = Quaternion.Euler(0, rotationY, 0);
    }
}
