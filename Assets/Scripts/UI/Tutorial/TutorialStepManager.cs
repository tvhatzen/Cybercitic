using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialStepManager : MonoBehaviour
{
    // sets up all tutorial step interfaces, manages step list, adds/removes steps
    #region Variables

    // UI component that displays currently focused item
    // UI text compoennt to show current step text
    public TextMeshProUGUI stepText;

    // list of ITutorialStep interfaces

    #endregion

    private void Start()
    {
        
    }

    void AdvanceStep()
    {

    }

    void CheckStepState()
    {

    }

    void FinishTutorialLevels()
    {

    }


}

#region STEP 1
public class TutorialStep1 : ITutorialStep
{
    public void GetCriteria()
    {

    }

    public void DisplayStepUI()
    {
        
    }

    public void CheckCompleteCriteria()
    {

    }

    public void CompleteStep()
    {

    }
}

#endregion
