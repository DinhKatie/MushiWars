using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Menu : MonoBehaviour
{
    public Canvas canvas;
    public GameObject arrowPrefab;

    public static Menu Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void OnArrowClick()
    {

    }

    public void DisplayPushArrow(BaseUnit unit, Campfire campfire)
    {
        if (!campfire.IsUnitAdjacentToCampfire(unit)) return;

        // Get the direction from the unit to the campfire
        Vector3Int directionToUnit = unit.CurrentPosition - campfire.CurrentPosition;

        // Get the opposite direction (push direction)
        Vector3Int pushDirection = -directionToUnit;

        // Check if the push direction is available
        if (campfire.GetValidPushDirections().Contains(pushDirection))
        {
            // Convert the campfire position to screen space
            Vector3 campfireScreenPos = Camera.main.WorldToScreenPoint(campfire.transform.position);

            // Offset the position in the push direction (e.g., 50 pixels in the UI)
            Vector3 arrowScreenPos = campfireScreenPos + new Vector3(pushDirection.x * 50, pushDirection.y * 50);

            // Instantiate the arrow at this position
            GameObject arrow = Instantiate(arrowPrefab, canvas.transform); // canvas should be a reference to your UI Canvas
            RectTransform arrowRect = arrow.GetComponent<RectTransform>();
            arrowRect.anchoredPosition = arrowScreenPos;

            // Optionally: Rotate the arrow based on direction if necessary
            RotateArrow(arrow, pushDirection);
        }
    }

    private void RotateArrow(GameObject arrow, Vector3Int direction)
    {
        // Rotate based on the direction (for example: 0° for right, 90° for up, etc.)
        if (direction == Vector3Int.up)
        {
            arrow.transform.rotation = Quaternion.Euler(0, 0, 90);
        }
        else if (direction == Vector3Int.down)
        {
            arrow.transform.rotation = Quaternion.Euler(0, 0, -90);
        }
        else if (direction == Vector3Int.left)
        {
            arrow.transform.rotation = Quaternion.Euler(0, 0, 180);
        }
        else if (direction == Vector3Int.right)
        {
            arrow.transform.rotation = Quaternion.Euler(0, 0, 0); // Default right
        }
    }
}
