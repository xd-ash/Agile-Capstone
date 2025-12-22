using UnityEngine;

public class DirectionAnimator : MonoBehaviour
{
    private Animator _anim;

    // 0 = Up Left, 1 = Down Left, 2 = Up Right, 3 = Down Right
    [Header("Idle states (UL, DL, UR, DR)")]
    [SerializeField] private AnimationClip[] _idleStateAnims = new AnimationClip[4];

    [Header("Move states (UL, DL, UR, DR)")]
    [SerializeField] private AnimationClip[] _moveStateAnims = new AnimationClip[4];

    private int[] _idleHashes;
    private int[] _moveHashes;

    private int _lastDir = 0;
    private bool _isMoving;

    private void Awake()
    {
        _anim = GetComponentInChildren<Animator>();

        _idleHashes = new int[_idleStateAnims.Length];
        _moveHashes = new int[_moveStateAnims.Length];

        for (int i = 0; i < _idleStateAnims.Length; i++)
            _idleHashes[i] = Animator.StringToHash(_idleStateAnims[i].name);

        for (int i = 0; i < _moveStateAnims.Length; i++)
            _moveHashes[i] = Animator.StringToHash(_moveStateAnims[i].name);

        // start facing whatever dir we have with idle
        PlayCurrentState();
    }

    public void SetMoving(bool value)
    {
        _isMoving = value;
        PlayCurrentState();
    }

    public void SetDirectionFromDelta(Vector2Int delta)
    {
        if (_anim == null) return;

        // Up Left =0, Down Left =1, Up Right =2, Down Right =3
        if (delta.y > 0 && delta.x == 0) _lastDir = 0; // UL
        else if (delta.x < 0 && delta.y == 0) _lastDir = 1; // DL
        else if (delta.x > 0 && delta.y == 0) _lastDir = 2; // UR
        else if (delta.y < 0 && delta.x == 0) _lastDir = 3; // DR

        PlayCurrentState();
    }

    private void PlayCurrentState()
    {
        int dir = Mathf.Clamp(_lastDir, 0, 3);

        if (_isMoving && dir < _moveHashes.Length && _moveHashes[dir] != 0)
            _anim.Play(_moveHashes[dir]);
        else if (dir < _idleHashes.Length && _idleHashes[dir] != 0)
            _anim.Play(_idleHashes[dir]);
    }
}