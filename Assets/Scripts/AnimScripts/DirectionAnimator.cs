using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AttackAnimSet
{
    [Tooltip("Matches the key in CardAbilityDefinition (e.g. 'Slash', 'Cast', 'Shoot')")]
    public string key;

    [Tooltip("4 directional attack clips: [0]=NE, [1]=NW, [2]=SE, [3]=SW — matches idle/move dir convention")]
    public AnimationClip[] dirClips = new AnimationClip[4];
}

public class DirectionAnimator : MonoBehaviour
{
    [SerializeField] private AnimationClip[] _idleStateAnims = new AnimationClip[4];
    [SerializeField] private AnimationClip[] _moveStateAnims = new AnimationClip[4];

    [Header("Attack Animations")]
    [SerializeField] private AttackAnimSet[] _attackAnimSets;

    [Tooltip("Small fade to avoid popping when toggling idle/move.")]
    [SerializeField] private float _fade = 0.05f;

    private Animator _anim;

    private int[] _idleHashes;
    private int[] _moveHashes;

    // key -> array of 4 hashes, one per direction
    private Dictionary<string, int[]> _attackHashLookup = new();

    private int _lastDir = 0;
    private bool _isMoving;
    private bool _isPlayingAttack = false;

    private int _currentPlayedHash = 0;

    public int GetLastDir => _lastDir;

    private void Awake()
    {
        _anim = GetComponentInChildren<Animator>();

        _idleHashes = new int[_idleStateAnims.Length];
        _moveHashes = new int[_moveStateAnims.Length];

        for (int i = 0; i < _idleStateAnims.Length; i++)
            _idleHashes[i] = _idleStateAnims[i] ? Animator.StringToHash(_idleStateAnims[i].name) : 0;

        for (int i = 0; i < _moveStateAnims.Length; i++)
            _moveHashes[i] = _moveStateAnims[i] ? Animator.StringToHash(_moveStateAnims[i].name) : 0;

        BuildAttackHashLookup();
        ValidateStatesExist();
        PlayCurrentState(force: true);
    }

    private void BuildAttackHashLookup()
    {
        if (_attackAnimSets == null) return;

        foreach (var set in _attackAnimSets)
        {
            if (string.IsNullOrEmpty(set.key) || set.dirClips == null) continue;

            int[] hashes = new int[4];
            for (int i = 0; i < 4; i++)
                hashes[i] = (i < set.dirClips.Length && set.dirClips[i] != null)
                    ? Animator.StringToHash(set.dirClips[i].name) : 0;

            _attackHashLookup[set.key] = hashes;
        }
    }

    private void ValidateStatesExist()
    {
        if (_anim == null) return;

        for (int i = 0; i < 4; i++)
        {
            if (_idleHashes[i] != 0 && !_anim.HasState(0, _idleHashes[i]))
                Debug.LogWarning($"[DirectionAnimator] Missing IDLE state named '{_idleStateAnims[i].name}' in Animator Controller.");

            if (_moveHashes[i] != 0 && !_anim.HasState(0, _moveHashes[i]))
                Debug.LogWarning($"[DirectionAnimator] Missing MOVE state named '{_moveStateAnims[i].name}' in Animator Controller.");
        }

        if (_attackAnimSets == null) return;
        foreach (var set in _attackAnimSets)
        {
            if (set.dirClips == null) continue;
            for (int i = 0; i < set.dirClips.Length; i++)
            {
                if (set.dirClips[i] == null) continue;
                int h = Animator.StringToHash(set.dirClips[i].name);
                if (!_anim.HasState(0, h))
                    Debug.LogWarning($"[DirectionAnimator] Missing ATTACK state '{set.dirClips[i].name}' for key '{set.key}' dir {i}.");
            }
        }
    }

    public void SetMoving(bool value)
    {
        if (_isMoving == value) return;
        _isMoving = value;
        if (!_isPlayingAttack)
            PlayCurrentState();
    }

    public void SetDirectionFromDelta(Vector2Int delta)
    {
        if (_anim == null) return;

        int newDir = GetDirIndexFromDelta(delta);
        if (newDir == _lastDir) return;
        _lastDir = newDir;
        Debug.Log($"DELTA {delta} -> dirIndex {newDir} (moving={_isMoving})");

        if (!_isPlayingAttack)
            PlayCurrentState();
    }

    /// <summary>
    /// Plays the attack animation for the given key and direction.
    /// onComplete fires immediately if key is null/empty or not found — safe for buff/trap cards.
    /// </summary>
    public void PlayAttack(string key, int dirIndex, Action onComplete)
    {
        if (string.IsNullOrEmpty(key) || !_attackHashLookup.TryGetValue(key, out int[] hashes))
        {
            onComplete?.Invoke();
            return;
        }

        int dir = Mathf.Clamp(dirIndex, 0, 3);
        int hash = hashes[dir];

        if (hash == 0)
        {
            onComplete?.Invoke();
            return;
        }

        _lastDir = dir;
        StartCoroutine(AttackCoro(hash, onComplete));
    }

    private IEnumerator AttackCoro(int hash, Action onComplete)
    {
        _isPlayingAttack = true;
        _anim.CrossFade(hash, _fade, 0);
        _currentPlayedHash = hash;

        // Wait one frame for CrossFade to register
        yield return null;

        // Wait until we're actually IN the attack state
        // (the transition itself takes a few frames to complete)
        float timeout = 2f;
        float elapsed = 0f;
        while (!_anim.GetCurrentAnimatorStateInfo(0).shortNameHash.Equals(hash))
        {
            elapsed += Time.deltaTime;
            if (elapsed >= timeout)
            {
                Debug.LogWarning($"[DirectionAnimator] Timed out waiting to enter attack state {hash}. Aborting.");
                break;
            }
            yield return null;
        }

        // Now wait for the attack state to finish (normalizedTime reaches 1)
        elapsed = 0f;
        while (_anim.GetCurrentAnimatorStateInfo(0).shortNameHash.Equals(hash) &&
               _anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            elapsed += Time.deltaTime;
            if (elapsed >= timeout)
            {
                Debug.LogWarning($"[DirectionAnimator] Timed out waiting for attack state {hash} to finish.");
                break;
            }
            yield return null;
        }

        _isPlayingAttack = false;

        // Clear hash so PlayCurrentState doesn't skip the crossfade thinking it's already correct
        _currentPlayedHash = 0;

        // Return to idle (or move if currently moving) for the current direction
        PlayCurrentState(force: true);

        onComplete?.Invoke();
    }

    /// <summary>
    /// Converts a grid delta to a 0-3 direction index.
    /// 0=NE, 1=NW, 2=SE, 3=SW. Uses dominant-axis snap for off-axis deltas.
    /// </summary>
    public static int GetDirIndexFromDelta(Vector2Int delta)
    {
        if (delta == Vector2Int.zero) return 0;

        if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
            return delta.x > 0 ? 2 : 1; // E -> SE (2), W -> NW (1)
        else
            return delta.y > 0 ? 0 : 3; // N -> NE (0), S -> SW (3)
    }

    private void PlayCurrentState(bool force = false)
    {
        if (_anim == null) return;

        int dir = Mathf.Clamp(_lastDir, 0, 3);
        int targetHash = _isMoving ? _moveHashes[dir] : _idleHashes[dir];

        if (targetHash == 0) return;
        if (!force && targetHash == _currentPlayedHash) return;

        _currentPlayedHash = targetHash;
        _anim.CrossFade(targetHash, _fade, 0);
    }
}