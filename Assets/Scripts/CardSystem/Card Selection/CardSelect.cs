using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CardSystem
{
    public class CardSelect : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        private GameObject _cardHighlight;

        [SerializeField] private Sprite _cardSprite;
        [SerializeField] private bool selected = false;//, hovering = false;
        private Card _card;

        //private Vector3 offset;
        //private Vector2 originalLocation;

        private void OnEnable()
        {
            _cardHighlight = transform.Find("CardHighlight").gameObject;
            _cardHighlight.SetActive(false);

            _spriteRenderer = GetComponent<SpriteRenderer>();
            _spriteRenderer.sprite = _cardSprite;

            AbilityEvents.OnAbilityUsed += ClearSelection;
        }
        private void OnDestroy()
        {
            AbilityEvents.OnAbilityUsed -= ClearSelection;
        }
        private void OnMouseEnter()
        {
            _cardHighlight.SetActive(true);
            transform.position += Vector3.up;
        }
        private void OnMouseExit()
        {
            if (!selected)
            {
                _cardHighlight.SetActive(false);
                transform.position -= Vector3.up;
            }
        }
        private void OnMouseDown()
        {
            selected = true;
            //add check for enough ap?

            CardManager.instance.selectedCard = _card; //make me better, this is messy

            AbilityEvents.TargetingStarted();//invoke static event
            _card.GetCardAbility.UseAility(TurnManager.GetCurrentUnit);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ClearSelection();
            }

            /* Drag disabled while setting up ability functionality
             * 
            if (Input.GetMouseButtonDown(0))
            {
                //RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

                if (hovering)
                {
                    selected = true;
                    //card = hit.transform;
                    //originalLocation = hit.transform.position;
                    originalLocation = transform.position;
                    offset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (selected)
                {
                    selected = false;
                    transform.position = originalLocation;
                }
            }

            if (selected)
            {
                transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + offset;
            }
            */
        }
        public void ClearSelection()
        {
            if (selected)
            {
                TurnManager.GetCurrentUnit.StopAllCoroutines();

                selected = false;
                transform.position -= Vector3.up;
                _cardHighlight.SetActive(false);
            }
        }

        public void OnPrefabCreation(Card card)
        {
            _card = card;
            _card.CardTransform = transform;

            transform.name = card.GetCardName;
            TextMeshPro[] cardTextFields = transform.GetComponentsInChildren<TextMeshPro>();
            cardTextFields[0].text = card.GetCardName;
            cardTextFields[1].text = card.GetDescription;
            cardTextFields[2].text = card.GetCardAbility.RootNode.GetApCost.ToString();
        }
    }
}