using UnityEngine;

public class EnterDeckNamePopup : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_InputField _inputField;

    private void OnEnable()
    {
        _inputField.text = string.Empty;
    }

    public void OnClick()
    {
        if (_inputField.text == string.Empty)
        {
            Debug.Log("Empty pack name attempt");
            return; //add other name string checks
        }

        PackBuilderScript.Instance?.CreateNewPack(_inputField.text);
        gameObject.SetActive(false);
    }
}
