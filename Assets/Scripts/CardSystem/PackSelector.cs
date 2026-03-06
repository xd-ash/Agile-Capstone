using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class PackSelector : MonoBehaviour
{
    private CardAndPackLibrary _cardAndPackLibrary;
    [SerializeField] private int _numberRandomPacks = 2;

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

        var tempPacks = SelectRandomPacksForRun(temp);
        PlayerDataManager.Instance.SetInitialPacks(tempPacks);
        _packDropdown.captionText.text = temp.GetPackName;
    }
    public CardPack[] SelectRandomPacksForRun(CardPack initialPack)
    {
        if (_numberRandomPacks <= 0) return new CardPack[] { initialPack };

        List<CardPack> tempPacks = new();
        int totalPacks = _cardAndPackLibrary.GetPacksInProject.Count;

        tempPacks.Add(initialPack);
        for (int i = 0; i < _numberRandomPacks; i++)
            tempPacks.Add(_cardAndPackLibrary.GetPacksInProject[Random.Range(0, totalPacks)]);
        return tempPacks.ToArray();
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
