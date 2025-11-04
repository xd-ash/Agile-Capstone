using UnityEngine;

public class DirectionAnimator : MonoBehaviour
{
    [SerializeField] private Animator _anim;
    
    // Up Left =0, Down Left =1, Up Right =2, Down Right =3
    private int _lastDir = 0;

    private void Start()
    {
        _anim = GetComponent<Animator>();
    }

    public void SetMoving(bool value)
    {
        _anim.SetBool("IsMoving", value);
    }
    
    public void SetDirectionFromDelta(Vector2Int delta)
    {
        if (_anim == null) return;

        int newDir = _lastDir;

        if (delta.x > 0 && delta.y == 0)      _lastDir = 2; // UR
        else if (delta.x < 0 && delta.y == 0) _lastDir = 1; // DL
        else if (delta.y > 0 && delta.x == 0) _lastDir = 0; // UL
        else if (delta.y < 0 && delta.x == 0) _lastDir = 3; // DR

        _anim.SetInteger("Dir", _lastDir);
        _lastDir = newDir;
    }
}
