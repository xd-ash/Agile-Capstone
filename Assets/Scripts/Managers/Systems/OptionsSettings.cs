using UnityEngine;
using static SettingsData;

public static class OptionsSettings
{
    private static bool _isCardSelectOnClick;
    private static bool _shouldRunTutorial = false;
    private static int _targetFrameRate = 60;
    private static bool _autoEndTurn = false;
    private static int _resolutionWidth;
    private static int _resolutionHeight;
    private static int _fullscreenMode = 1; // stored as int for serialization; maps to FullScreenMode enum

    public static bool IsCardSelectOnClick => _isCardSelectOnClick;
    public static bool ShouldRunTutorial => _shouldRunTutorial;
    public static int TargetFrameRate => _targetFrameRate;
    public static bool AutoEndTurn => _autoEndTurn;
    public static int ResolutionWidth => _resolutionWidth;
    public static int ResolutionHeight => _resolutionHeight;
    public static int FullscreenModeIndex => _fullscreenMode;

    public static void UpdateOptionsData(OptionsSettingsToken optionsData)
    {
        _isCardSelectOnClick = optionsData.IsCardSelectOnClick;
        _targetFrameRate = optionsData.TargetFrameRate;
        _autoEndTurn = optionsData.AutoEndTurn;
        _resolutionWidth = optionsData.ResolutionWidth;
        _resolutionHeight = optionsData.ResolutionHeight;
        _fullscreenMode = optionsData.FullscreenModeIndex;

        ApplyFrameRate();
        ApplyResolutionAndFullscreen();
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

    public static void UpdateTargetFrameRate(int frameRate)
    {
        _targetFrameRate = frameRate;
        ApplyFrameRate();
        SaveLoadScript.SaveSettings?.Invoke();
    }

    public static void UpdateAutoEndTurn(bool autoEndTurn)
    {
        _autoEndTurn = autoEndTurn;
        SaveLoadScript.SaveSettings?.Invoke();
    }

    public static void UpdateResolution(int width, int height)
    {
        _resolutionWidth = width;
        _resolutionHeight = height;
        ApplyResolutionAndFullscreen();
        SaveLoadScript.SaveSettings?.Invoke();
    }

    public static void UpdateFullscreenMode(int modeIndex)
    {
        _fullscreenMode = modeIndex;
        ApplyResolutionAndFullscreen();
        SaveLoadScript.SaveSettings?.Invoke();
    }

    private static void ApplyFrameRate()
    {
        Application.targetFrameRate = _targetFrameRate;
    }

    private static void ApplyResolutionAndFullscreen()
    {
        if (_resolutionWidth <= 0 || _resolutionHeight <= 0) return;
        Screen.SetResolution(_resolutionWidth, _resolutionHeight, (FullScreenMode)_fullscreenMode);
    }
}