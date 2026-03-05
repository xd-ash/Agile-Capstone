using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using CardSystem;
using static GameObjectPool;

public class PackBuilderScript : MonoBehaviour
{
    private CardAndPackLibrary _cardAndDeckLibrary;

    [SerializeField] private TMP_Dropdown _packDropdown;
    [SerializeField] private ScrollRect _cardScrollView;
    [SerializeField] private ScrollRect _packScrollView;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _deletePackButton;

    [SerializeField] private GameObject _cardContentPrefab;
    [SerializeField] private GameObject _packContentPrefab;

    [SerializeField] private CardPack _currPack;
    [SerializeField] private List<CardAbilityDefinition> _tempPack = new();
    private bool _isCurrentPackEditable => !_cardAndDeckLibrary.GetPacksInProject.Contains(_currPack);

    public static PackBuilderScript Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            this.gameObject.SetActive(false);
    }
    private void OnEnable()
    {
        _cardAndDeckLibrary = Resources.Load<CardAndPackLibrary>("Libraries/CardAndPackLibrary");

        Invoke(nameof(LateStartInits), 0.1f);
    }
    private void LateStartInits()
    {
        GrabAllPackOptions();
        SwapCurrentPack(); //initial pack grab, temp pack setup, & pack/card scrollview builds
    }

    //Create all card content in the card library scrollview
    private void BuildCardLibraryScrollViewContent()
    {
        if (_currPack == null || _cardContentPrefab == null || _cardScrollView == null) return;

        ClearScrollviewContent(_cardScrollView.content);

        foreach (var card in _cardAndDeckLibrary.GetCardsInProject)
        {
            if (card == null) continue;

            GameObject content = Spawn(_cardContentPrefab, Vector3.zero, Quaternion.identity, _cardContentPrefab.transform.localScale, _cardScrollView.content);

            TextMeshProUGUI[] cardTextFieldsUI = content.GetComponentsInChildren<TextMeshProUGUI>();
            // Update text content
            cardTextFieldsUI[0].text = card.GetCardName;
            cardTextFieldsUI[1].text = card.GetDescription;
            cardTextFieldsUI[2].text = card.GetApCost.ToString();

            var addCardButton = content.GetComponentInChildren<Button>();
            if (addCardButton == null) continue;
            addCardButton?.onClick.AddListener(() => AddCardToTempPack(card));
            addCardButton.interactable = _isCurrentPackEditable;
        }
    }

    //Create all pack content in the pack scrollview
    private void BuildPackScrollViewContent()
    {
        if (_currPack == null || _packContentPrefab == null || _packScrollView == null) return;

        ClearScrollviewContent(_packScrollView.content);

        foreach (var card in _tempPack)
        {
            if (card == null) continue;
            
            GameObject content = GameObject.Instantiate(_packContentPrefab, Vector3.zero, Quaternion.identity, _packScrollView.content);
            content.name = card.name;

            var texts = content.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts == null || texts.Length < 2) continue;
            texts[0].text = card.name;
            //texts[1].text = GetCardAmountInCurrentDeck(card).ToString();

            var removeCardButton = content.GetComponentInChildren<Button>();
            if (removeCardButton == null) continue;

            removeCardButton?.onClick.AddListener(() => RemoveCardFromTempPack(card));
            removeCardButton.gameObject.SetActive(_isCurrentPackEditable);
        }
        ToggleConfirmbutton();
    }
    //toggle confirm button interable if temp pack differes from the actual pack
    private void ToggleConfirmbutton()
    {
        bool areListsEqual = _currPack.GetCardsInPack.Count == _tempPack.Count;

        //sloppy and quick way to compare the pack lists. Does not account for order of cards
        if (areListsEqual)
            for (int i = 0; i < _currPack.GetCardsInPack.Count; i++)
                if (_currPack.GetCardsInPack[i] != _tempPack[i])
                {
                    areListsEqual = false;
                    break;
                }

        _confirmButton.interactable = !areListsEqual && (_tempPack.Count != 0 || _currPack.GetCardsInPack.Count != 0) && _isCurrentPackEditable;
    }

    //Add card to pending/temp pack list
    private void AddCardToTempPack(CardAbilityDefinition card)
    {
        if (card == null || _currPack == null ||!_isCurrentPackEditable) return;
        if (_tempPack == null) _tempPack = new();
        _tempPack.Add(card);

        Rebuild();
    }

    //remove card from pending/temp pack list
    private void RemoveCardFromTempPack(CardAbilityDefinition card)
    {
        if (card == null) return;
        if (_tempPack == null) _tempPack = new();
        if (_tempPack.Contains(card))
            _tempPack.Remove(card);

        Rebuild();
    }

    //add pending/temp cards to current pack & update player data packs
    public void ConfirmPackAdditions()
    {
        if (!_isCurrentPackEditable) return;

        _currPack.ClearPack(_tempPack); 

        PlayerDataManager.Instance?.CreateOrAdjustPack(_currPack);
        SaveLoadScript.SaveGame?.Invoke();

        ToggleConfirmbutton();
    }

    //create new pack to edit
    public void CreateNewPack(string packName)
    {
        _currPack = new CardPack(packName);
        AddPackOption(_currPack);
        SetDropdownValueFromPackName(packName);

        //PlayerDataManager.Instance.SetActiveDeck(_currDeck);
        _tempPack = new(_currPack.GetCardsInPack);
        _packDropdown.captionText.text = _currPack.GetPackName;

        Rebuild();
        PlayerDataManager.Instance?.CreateOrAdjustPack(_currPack);
    }
    //delete pack from options & player data
    public void DeletePack()
    {
        if (_currPack == null || !_isCurrentPackEditable)
        {
            Debug.Log("Cannot delete starter packs. Replace this message with a popup or other system at some point");
            return;
        }

        PlayerDataManager.Instance?.DeletePack(_currPack);
        RemovePackOption(_currPack);
        _packDropdown.value = 0; //reset to first value
        SwapCurrentPack();
        SaveLoadScript.SaveGame?.Invoke();
    }

    //Return the amount of the given card in the current pack & in the pending/temp pack additions
    private int GetCardAmountInCurrentPack(CardAbilityDefinition cardToCount)
    {
        if (_currPack == null || cardToCount == null) return -1;

        int cardAmount = 0;

        foreach (var card in _currPack.GetCardsInPack)
            if (card == cardToCount)
                cardAmount++;

        foreach (var card in _tempPack)
            if (card == cardToCount)
                cardAmount++;

        return cardAmount;
    }

    //Swap current pack
    public void SwapCurrentPack()
    {
        ClearScrollviewContent(_packScrollView.content);

        var temp = GetCurrentPackFromDropdown();
        if (temp == null) return;
        _currPack = temp;
        //PlayerDataManager.Instance.SetActiveDeck(_currDeck);
        _tempPack = new(_currPack.GetCardsInPack);
        _packDropdown.captionText.text = _currPack.GetPackName;

        Rebuild();
    }
    private void Rebuild()
    {
        BuildCardLibraryScrollViewContent();
        BuildPackScrollViewContent();

        if (_deletePackButton == null) return;
        _deletePackButton.interactable = _isCurrentPackEditable;
    }

    //Grab correct pack from dropdown value
    private CardPack GetCurrentPackFromDropdown()
    {
        if (_packDropdown == null || _packDropdown.options.Count == 0 || _cardAndDeckLibrary == null) return null;

        int entryIndex = _packDropdown.value;

        string selectedPackName = _packDropdown.options[entryIndex].text;
        var pack = _cardAndDeckLibrary.GetPackFromName(selectedPackName, false);
        if (pack == null)
            if (PlayerDataManager.Instance != null)
                foreach (var playerPack in PlayerDataManager.Instance.GetAllPlayerPacks)
                    if (playerPack != null && playerPack.GetPackName == selectedPackName)
                        pack = playerPack;
            
        return pack;
    }

    //Grab all pack options from player data manager
    private void GrabAllPackOptions()
    {
        if (_packDropdown == null) return;
        _packDropdown.options.Clear();

        // put default packs into dropdown options
        foreach (var pack in _cardAndDeckLibrary.GetPacksInProject)
            if (pack != null)
                _packDropdown.options.Add(new(pack.GetPackName));

        // put player packs into dropdown options
        foreach (var pack in PlayerDataManager.Instance.GetAllPlayerPacks)
            if (pack != null)
                _packDropdown.options.Add(new(pack.GetPackName));
    }
    //Add pack dropdown option
    private void AddPackOption(CardPack pack)
    {
        if (_packDropdown == null) return;
        foreach (var option in _packDropdown.options)
            if (option != null && option.text == pack.GetPackName)
                return; // if packname exists in options, just return

        _packDropdown.options.Add(new(pack.GetPackName));
    }
    //Remove pack dropdown option
    private void RemovePackOption(CardPack pack)
    {
        if (_packDropdown == null) return;

        for (int i = _packDropdown.options.Count - 1; i >= 0; i--)
            if (_packDropdown.options[i].text == pack.GetPackName)
            {
                _packDropdown.options.RemoveAt(i);
                break;
            }
    }
    //Set dropdown value/pack from given pack name
    private bool SetDropdownValueFromPackName(string packName)
    {
        if (_packDropdown == null) return false;

        foreach (var option in _packDropdown.options)
            if (option != null && option.text == packName)
            {
                _packDropdown.value = _packDropdown.options.IndexOf(option);
                break;
            }
        return true;
    }
    private void ClearScrollviewContent(RectTransform contentTransform)
    {
        for (int i = contentTransform.childCount - 1; i >= 0; i--)
            Destroy(contentTransform.GetChild(i).gameObject);
    }
}
