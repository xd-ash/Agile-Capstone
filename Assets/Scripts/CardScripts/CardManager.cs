using UnityEngine;

namespace CardSystem
{
    public class CardManager : MonoBehaviour
    {
        public CardBase[] _cardsInHand = new CardBase[5];//make visible in inspector in some way
        public int _numCards = 0, _maxCards = 5;
        public CardSO[] _testCardDeck;
        //public CardSO _testCard;

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
            if (_cardsInHand.Length <= _maxCards)
            {
                int newCardIndex = Random.Range(0, _testCardDeck.Length);
                _cardsInHand[_numCards] = DetermineCardCreator(_testCardDeck[newCardIndex]).CreateCard(transform);
                _numCards++;
            }
        }
        public void UseCard()
        {
            _cardsInHand[0].Use();
        }
        public void ArangeCardGOs()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform card = transform.Find(_cardsInHand[i].CardName);
                switch (i)
                {
                    case 0:
                        card.position = new Vector3(-6, 0, 0);
                        break;
                    case 1:
                        card.position = new Vector3(-3, 0, 0);
                        break;
                    case 2:
                        card.position = new Vector3(0, 0, 0);
                        break;
                    case 3:
                        card.position = new Vector3(3, 0, 0);
                        break;
                    case 4:
                        card.position = new Vector3(6, 0, 0);
                        break;
                }
            }
        }
    }
}
