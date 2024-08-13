using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SelectActionCounter : MonoBehaviour
{
    [SerializeField] private XRController xrController;
    private int selectActionCount = 0;  // Counter for select action

    private void Update()
    {
        // Check if the select action was triggered this frame
        if (xrController.selectInteractionState.activatedThisFrame)
        {
            // Increment the counter
            selectActionCount++;

            // Output the current count to the console (optional)
            Debug.Log("Select action triggered. Count: " + selectActionCount);
        }
    }

    // Optional: Method to reset the counter if needed
    public void ResetCounter()
    {
        selectActionCount = 0;
    }

    // Optional: Method to get the current count
    public int GetSelectActionCount()
    {
        return selectActionCount;
    }
}
