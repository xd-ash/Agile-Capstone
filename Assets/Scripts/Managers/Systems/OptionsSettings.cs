using UnityEngine;
using static SettingsData;

public static class OptionsSettings
{
    private static bool _isCardSelectOnClick;
    private static bool _shouldRunTutorial = false;

    public static bool IsCardSelectOnClick => _isCardSelectOnClick;
    public static bool ShouldRunTutorial => _shouldRunTutorial;

    public static void UpdateOptionsData(OptionsSettingsToken optionsData)
    {
        _isCardSelectOnClick = optionsData.IsCardSelectOnClick;
    }

    public static void UpdateCardSelect(bool cardSelectOnClick)
    {
        _isCardSelectOnClick = cardSelectOnClick;
        SaveLoadScript.SaveSettings?.Invoke();
    }

    public static void UpdateTutorialBool(bool shouldRunTutorial)
    {
        _shouldRunTutorial = shouldRunTutorial;
    }

}
