using UnityEngine;

public class CardSelect : MonoBehaviour
{
    public bool selected = false;
    private Transform card = null;
    private Vector3 offset;

    private Vector2 originalLocation;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit)
            {
                selected = true;
                card = hit.transform;
                originalLocation = hit.transform.position;
                offset = card.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (card != null)
            {
                selected = false;
                card.transform.position = originalLocation;
                card = null;
            }
        }

        if(card != null)
        {
            card.position = Camera.main.ScreenToWorldPoint(Input.mousePosition) + offset;
        }
    }
}
