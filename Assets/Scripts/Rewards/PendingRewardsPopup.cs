using System;
using UnityEngine;
using UnityEngine.UI;

public class PendingRewardsPopup : MonoBehaviour
{
    [SerializeField] private Button _continueButton, _backButton;

    private void OnEnable()
    {
        _backButton.onClick.AddListener(() => gameObject.SetActive(false));
    }
    public void SetContinueButtonOnClick(Action onClick)
    {
        _continueButton.onClick.AddListener(() => 
        { 
            onClick?.Invoke();
            gameObject.SetActive(false);
        });
    }
}
