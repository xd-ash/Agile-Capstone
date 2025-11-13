using System;
using UnityEngine;

public static class GameOverEvents
{
    public static event Action<bool> OnGameOver;

    public static void OnGameWinOrLoss(bool didWin) => OnGameOver?.Invoke(didWin);
}
