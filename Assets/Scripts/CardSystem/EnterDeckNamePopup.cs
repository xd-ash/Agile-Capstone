using UnityEngine;
using UnityEngine.UI;

public class EnterDeckNamePopup : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_InputField _inputField;

    private void OnEnable()
    {
        _inputField.text = string.Empty;
    }

    public void OnClick()
    {
        DeckBuilderScript.Instance?.CreateNewDeck(_inputField.text);
    }
}
