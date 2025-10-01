using UnityEngine;

namespace CardSystem
{
    public class CardManager : MonoBehaviour
    {
        public int _topCardOfDeck = 0;
        public int _nextCardInHandIndex = 0;
        public int _maxCards = 5;
        public DeckSO _testDeck;
        public CardBase[] _cardsInHand = new CardBase[100];//make visible in inspector in some way

        public CardCreator DetermineCardCreator(CardSO cardSO)
        {
            switch (cardSO.GetCardTypeID()[0])
            {
                case '1':
                    return new MiscCardCreator(cardSO);
                case '2':
                    return new RangeCardCreator(cardSO);
                case '3':
                    return new MeleeCardCreator(cardSO);
                default:
                    throw new System.NotImplementedException();
            }
        }
        public void DrawCard()
        {
            _cardsInHand[_nextCardInHandIndex] = DetermineCardCreator(_testDeck._deck[_topCardOfDeck]).CreateCard(transform);
            _topCardOfDeck++;
            _nextCardInHandIndex++;
            ArangeCardGOs();
        }
        public void UseCard()
        {
            _cardsInHand[0].Use();
        }
        public void ArangeCardGOs()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform card = transform.GetChild(i);
                switch (i)
                {
                    case 0:
                        card.localPosition = new Vector3(-6, 0, 0);
                        break;
                    case 1:
                        card.localPosition = new Vector3(-3, 0, 0);
                        break;
                    case 2:
                        card.localPosition = new Vector3(0, 0, 0);
                        break;
                    case 3:
                        card.localPosition = new Vector3(3, 0, 0);
                        break;
                    case 4:
                        card.localPosition = new Vector3(6, 0, 0);
                        break;
                }
            }
        }
    }
}
