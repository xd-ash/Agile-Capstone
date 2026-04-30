using CardSystem;
using UnityEngine;
using UnityEngine.UI;

public class CardLibraryViewerScript : MonoBehaviour
{
    [Header("Card Spawning")]
    [SerializeField] private GameObject _cardContentPrefab;
    [SerializeField] private ScrollRect _scrollView;

    [SerializeField] private Button[] _tabButtons;
    [SerializeField] private Vector2 _selectedButtonDimensions;
    [SerializeField] private Vector2 _normalButtonDimensions;

    public void FillAllTabCards() => BuildScrollViewContent();
    public void FillRangedTabCards() => BuildScrollViewContent(CardCategory.Ranged);
    public void FillMeleeTabCards() => BuildScrollViewContent(CardCategory.Melee);
    public void FillThrowableTabCards() => BuildScrollViewContent(CardCategory.Throwable);
    public void FillHealTabCards() => BuildScrollViewContent(CardCategory.Heal);
    public void FillShieldTabCards() => BuildScrollViewContent(CardCategory.Shield);
    public void FillTrapTabCards() => BuildScrollViewContent(CardCategory.Trap);
    public void FillGamblingTabCards() => BuildScrollViewContent(CardCategory.Gambling);
    public void FillDrawTabCards() => BuildScrollViewContent(CardCategory.Draw);

    private void OnEnable()
    {
        FillAllTabCards();
    }

    private void BuildScrollViewContent(CardCategory cardsToShow = CardCategory.None)
    {
        ClearScrollviewContent();

        var cardLibrary = Resources.Load<CardAndPackLibrary>("Libraries/CardAndPackLibrary");
        if (cardLibrary == null) return;

        var cards = cardsToShow == CardCategory.None ? cardLibrary.GetCardsInProject.ToArray() : cardLibrary.GetCardsOfCategory(cardsToShow);
        CardScrollviewFiller.BuildScrollViewContent(_scrollView.content, _cardContentPrefab, cards, CardState.DeckViewer);
    }

    public void ClearScrollviewContent()
    {
        CardScrollviewFiller.ClearScrollViewContent(_scrollView.content);
    }
}
