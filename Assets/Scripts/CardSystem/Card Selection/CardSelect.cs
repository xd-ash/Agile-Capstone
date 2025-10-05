using OldCardSystem;
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
        [SerializeField] private bool selected = false, hovering = false;
        private Card _card;
        //private Transform cardTransform = null;

        private Vector3 offset;
        private Vector2 originalLocation;

        private void OnEnable()
        {
            _cardHighlight = transform.Find("CardHighlight").gameObject;
            _cardHighlight.SetActive(false);

            _spriteRenderer = GetComponent<SpriteRenderer>();
            _spriteRenderer.sprite = _cardSprite;
        }
        private void OnMouseEnter()
        {
            hovering = true;
            CardManager.instance.selectedCard = _card;
            CardManager.instance.haveSelected = true;
            _cardHighlight.SetActive(true);
        }
        private void OnMouseExit()
        {
            if (!selected)
            {
                _cardHighlight.SetActive(false);
                CardManager.instance.haveSelected = false;
                CardManager.instance.selectedCard = null;
            }
            hovering = false;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (hovering)
                {
                    selected = true;
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                selected = false;
                _cardHighlight.SetActive(false);
                CardManager.instance.haveSelected = false;
                CardManager.instance.selectedCard = null;
                CardManager.instance.StopAllCoroutines();
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

        public void OnPrefabCreation(Card card)
        {
            _card = card;

            transform.name = card.GetCardName;
            TextMeshPro[] cardTextFields = transform.GetComponentsInChildren<TextMeshPro>();
            cardTextFields[0].text = card.GetCardName;
            cardTextFields[1].text = card.GetDescription;
            cardTextFields[2].text = card.APCost.ToString();
        }
    }
}