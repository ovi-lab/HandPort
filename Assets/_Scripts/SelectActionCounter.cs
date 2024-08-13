using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class SelectActionCounter : MonoBehaviour
{
    private CustomActionBasedControllerStable3 rightHandController;
    private InputAction selectAction; 
    private int selectActionCount = 0;

    void Awake()
    {
        CustomActionBasedControllerStable3[] controllerArray = CustomActionBasedControllerStable3.FindObjectsOfType<CustomActionBasedControllerStable3>(true);
        foreach (var controller in controllerArray)
        {
            if (controller.name.Equals("Teleport Interactor"))
            {
                rightHandController = controller;
            }
        }
        
    }

    void Update()
    {
        if (rightHandController != null)
        {
            selectAction = rightHandController.selectAction.action;
            if (selectAction.triggered)
            {
                selectActionCount++;
                Debug.Log("Select action triggered on the right controller.");
            }
        }
    }
    
    public int GetSelectActionCount()
    {
        int count = selectActionCount;
        selectActionCount = 0; 
        return count;
    }
}
