using UnityEngine;
using TMPro;

public class APHoverIndicator : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private TextMeshPro _apText;
    [SerializeField] private GameObject _xIconRoot;

    public static APHoverIndicator instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        Hide();
    }
    
    //Show an AP cost on the hovered tile within movement range.
    public void ShowCost(Vector3 worldPos, int apCost)
    {
        transform.position = worldPos;
        _apText.gameObject.SetActive(true);
        _apText.text = apCost.ToString();
        _xIconRoot.SetActive(false);
    }

    //Show a red X for out of range tiles.
    public void ShowOutOfRange(Vector3 worldPos, int apCost)
    {
        transform.position = worldPos; 
        _apText.gameObject.SetActive(false);
        _xIconRoot.SetActive(true);
    }
    
    public void Hide()
    {
        if (_apText != null)
        {
            _apText.gameObject.SetActive(false);
            _apText.text = string.Empty;
        }

        if (_xIconRoot != null)
            _xIconRoot.SetActive(false);
    }
}