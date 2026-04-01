using UnityEngine;
using UnityEngine.UI;

public class TutorialToggleHelper : MonoBehaviour
{
    private Toggle _tutorialToggle;

    private void OnEnable()
    {
        if (TryGetComponent(out _tutorialToggle))
        {
            _tutorialToggle.isOn = false;
            _tutorialToggle.onValueChanged.RemoveAllListeners();
            _tutorialToggle.onValueChanged.AddListener(OptionsSettings.UpdateTutorialBool);
        }
    }
}
