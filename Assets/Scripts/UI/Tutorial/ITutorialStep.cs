using TMPro;

/// <summary>
/// Interface to block out logic for tutorial step criteria, display, execution, completed state logic
/// </summary>
public interface ITutorialStep 
{
    /// <summary>
    /// needs to retrieve some sort of trigger for tutorial step to be shown (state, if something is active, etc)
    /// </summary>
    public void GetCriteria() { }

    /// <summary>
    /// show the UI element for this step and display unique text
    /// </summary>
    public void DisplayStepUI(TextMeshProUGUI txt, string desc) { }

    /// <summary>
    /// checks for what will complete the step (state, if something is active, etc)
    /// </summary>
    public void CheckCompleteCriteria() { }

    /// <summary>
    /// completes and removes from major list of tutorial
    /// </summary>
    public void CompleteStep() { }
}