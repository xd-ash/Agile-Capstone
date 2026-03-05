using TMPro;
using UnityEngine;

public class PackSelector : MonoBehaviour
{
    private CardAndPackLibrary _cardAndPackLibrary;

    [SerializeField] private TMP_Dropdown _packDropdown;

    private void OnEnable()
    {
        _cardAndPackLibrary = Resources.Load<CardAndPackLibrary>("Libraries/CardAndPackLibrary");

        GrabAllPackOptions();
        SwapCurrentPack();
    }

    //Grab all deck options from player data manager
    private void GrabAllPackOptions()
    {
        if (_packDropdown == null) return;
        _packDropdown.options.Clear();

        // put default decks into dropdown options
        foreach (var pack in _cardAndPackLibrary.GetPacksInProject)
            if (pack != null)
                _packDropdown.options.Add(new(pack.GetPackName));

        // put player decks into dropdown options
        foreach (var deck in PlayerDataManager.Instance.GetAllPlayerPacks)
            if (deck != null)
                _packDropdown.options.Add(new(deck.GetPackName));
    }

    //Swap current deck
    public void SwapCurrentPack()
    {
        var temp = GetCurrentPackFromDropdown();
        if (temp == null) return;
        //PlayerDataManager.Instance.SetActiveDeck(temp);
        _packDropdown.captionText.text = temp.GetPackName;
    }

    //Grab correct deck from dropdown value
    private CardPack GetCurrentPackFromDropdown()
    {
        if (_packDropdown == null || _packDropdown.options.Count == 0 || _cardAndPackLibrary == null) return null;

        int entryIndex = _packDropdown.value;

        string selectedPackName = _packDropdown.options[entryIndex].text;
        var pack = _cardAndPackLibrary.GetPackFromName(selectedPackName, false);
        if (pack == null)
            if (PlayerDataManager.Instance != null)
                foreach (var playerPack in PlayerDataManager.Instance.GetAllPlayerPacks)
                    if (playerPack != null && playerPack.GetPackName == selectedPackName)
                        pack = playerPack;

        return pack;
    }
}
