using UnityEngine;
using UnityEngine.UI;

//small script for non main menu buttons to be able to play audio sfx easily
public class NonMainMenuButtonScript : MonoBehaviour
{
    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        var am = AudioManager.Instance;
        if (am == null) return;
        _button?.onClick.AddListener(am.PlayButtonSFX);
    }
    private void OnDestroy()
    {
        _button?.onClick.RemoveAllListeners();
    }
}
