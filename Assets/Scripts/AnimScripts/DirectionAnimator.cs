using UnityEngine;

public class DirectionAnimator : MonoBehaviour
{
    [SerializeField] private Animator _anim;

    // 0 = Up Left, 1 = Down Left, 2 = Up Right, 3 = Down Right
    [Header("Idle states (UL, DL, UR, DR)")]
    [SerializeField] private string[] _idleStateNames = new string[4] { "Idle_UL", "Idle_DL", "Idle_UR", "Idle_DR" };

    [Header("Move states (UL, DL, UR, DR)")]
    [SerializeField] private string[] _moveStateNames = new string[4] { "Walk_UL", "Walk_DL", "Walk_UR", "Walk_DR" };

    private int[] _idleHashes;
    private int[] _moveHashes;

    private int _lastDir = 0;
    private bool _isMoving;

    private void Awake()
    {
        _anim = GetComponentInChildren<Animator>();

        _idleHashes = new int[_idleStateNames.Length];
        _moveHashes = new int[_moveStateNames.Length];

        for (int i = 0; i < _idleStateNames.Length; i++)
        {
            _idleHashes[i] = Animator.StringToHash(_idleStateNames[i]);
        }

        for (int i = 0; i < _moveStateNames.Length; i++)
        {
            _moveHashes[i] = Animator.StringToHash(_moveStateNames[i]);
        }
    }

    private void Start()
    {
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
        if (delta.x > 0 && delta.y == 0) _lastDir = 2; // UR
        else if (delta.x < 0 && delta.y == 0) _lastDir = 1; // DL
        else if (delta.y > 0 && delta.x == 0) _lastDir = 0; // UL
        else if (delta.y < 0 && delta.x == 0) _lastDir = 3; // DR

        PlayCurrentState();
    }

    private void PlayCurrentState()
    {
        int dir = Mathf.Clamp(_lastDir, 0, 3);

        if (_isMoving)
        {
            if (dir < _moveHashes.Length && _moveHashes[dir] != 0)
            {
                _anim.Play(_moveHashes[dir]);
            }
        }
        else
        {
            if (dir < _idleHashes.Length && _idleHashes[dir] != 0)
            {
                _anim.Play(_idleHashes[dir]);
            }
        }
    }
}