using UnityEngine;

namespace AStarPathfinding
{
    public class TileHoverScript : MonoBehaviour
    {
        [SerializeField] private GameObject tileHighlight;
        private MapCreator _mapCreator;

        private void Start()
        {
            _mapCreator = GetComponentInParent<MapCreator>();

            tileHighlight.SetActive(false);
        }
        private void OnMouseEnter()
        {
            _mapCreator.tileMousePos = new Vector2Int((int)transform.localPosition.x, (int)transform.localPosition.y);
            tileHighlight.SetActive(true);
        }
        private void OnMouseExit()
        {
            tileHighlight.SetActive(false);
            _mapCreator.tileMousePos = new Vector2Int(-1,-1);
        }
    }
}
