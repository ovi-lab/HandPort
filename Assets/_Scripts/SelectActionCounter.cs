using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

public class SelectActionCounter : MonoBehaviour
{
    public float MicroAdjustTime => microAdjustTime;
    private CustomActionBasedControllerStable3 rightHandStableController;
    private ActionBasedController rightHandController;
    private InputAction selectAction;
    private int selectActionCount = 0;
    private float lastSelectTime = -Mathf.Infinity;
    private float microAdjustTime = 0f;
    private bool lastFramePressed;
    [SerializeField] private float cooldownTime = 0.2f;


    void Awake()
    {
        if (SceneManager.GetActiveScene().name == "Baseline")
        {
            ActionBasedController [] controllerArray = ActionBasedController.FindObjectsOfType<ActionBasedController>(true);
            foreach (var controller in controllerArray)
            {
                if (controller.name.Equals("Teleport Interactor"))
                {
                    rightHandController = controller;
                }
            }
            selectAction = rightHandController.selectAction.action;
        }
        else
        {
            CustomActionBasedControllerStable3[] controllerArray = FindObjectsOfType<CustomActionBasedControllerStable3>(true);
            foreach (var controller in controllerArray)
            {
                if (controller.name.Equals("Teleport Interactor"))
                {
                    rightHandStableController = controller;
                }
            }
            selectAction = rightHandStableController.selectAction.action;
        }
    }

    void Update()
    {
        if (rightHandController != null || rightHandStableController != null)
        {
            if (selectAction.triggered)
            {

                float currentTime = Time.time;

                // Check if enough time has passed since the last action
                if (currentTime - lastSelectTime >= cooldownTime)
                {
                    // Debug.Log("trigger");
                    selectActionCount++;
                    lastSelectTime = currentTime;
                }
            }

            bool isPinchPressed = selectAction.IsPressed();
            if (isPinchPressed && !lastFramePressed)
            {
                microAdjustTime = 0;
            }
            if (isPinchPressed)
            {
                microAdjustTime += Time.deltaTime;
            }

            lastFramePressed = isPinchPressed;
        }
    }

    public int GetSelectActionCount()
    {
        int count = selectActionCount;
        selectActionCount = 0;
        if (SceneManager.GetActiveScene().name != "Baseline")
        {
            return count-1;
        }
        return count-1;
    }
}
