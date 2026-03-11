using System;
using System.Collections.Generic;
using UnityEngine;

public class ByteMapController : MonoBehaviour
{
    private MapCreator _mapCreator;

    private byte[,] _map;
    private Dictionary<Unit, Vector2Int> _unitPositions = new();

    public byte[,] GetByteMap => _map;

    public static Action<Vector2Int, Unit> TileEntered;

    public static ByteMapController Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);

        _mapCreator = GetComponent<MapCreator>();
    }

    private void Start()
    {
        _map = _mapCreator?.CreateMap();
    }

    public byte GetByteAtPosition(Vector2Int pos) => _map[pos.x, pos.y];
    public Vector2Int GetPositionOfUnit(Unit unit) => _unitPositions.ContainsKey(unit) ? _unitPositions[unit] : new Vector2Int(-1, -1);
    public Unit GetUnitAtPosition(Vector2Int pos)
    {
        if (!_unitPositions.ContainsValue(pos))
            return null;
        foreach (var kvp in _unitPositions)
            if (kvp.Value == pos)
                return kvp.Key;
        return null;
    }

    public void InitUnitPosition(Unit unit, Vector2Int startPos)
    {
        if (!_unitPositions.ContainsKey(unit))
            _unitPositions.Add(unit, startPos);
    }

    public void UpdateUnitPositionByteMap(Unit unit, Vector2Int startPos, Vector2Int endPos)
    {
        _map[startPos.x, startPos.y] = 0;
        _map[endPos.x, endPos.y] = unit.GetTeam == Team.Friendly ? (byte)1 : (byte)3;

        if (!_unitPositions.ContainsKey(unit))
            _unitPositions.Add(unit, endPos);
        else
            _unitPositions[unit] = endPos;
    }
    public void UpdateUnitPositionByteMap(Unit unit, Vector2Int deathPos)
    {
        _map[deathPos.x, deathPos.y] = 0;

        if (!_unitPositions.ContainsKey(unit)) return;
        _unitPositions.Remove(unit);
    }
}
