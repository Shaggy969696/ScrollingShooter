// ============================================================
// ScoreManager.cs
// Singleton que lleva el puntaje global de la partida.
// Los enemigos llaman Add() al morir. La UI escucha OnScoreChanged.
//
// Uso:
//   ScoreManager.Instance.Add(100);
// ============================================================

using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────────────────
    public static ScoreManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // ── Estado ────────────────────────────────────────────────────────────────
    public int CurrentScore { get; private set; }

    // ── Evento para la UI ─────────────────────────────────────────────────────
    public event Action<int> OnScoreChanged;

    // Suma puntos y notifica a la UI
    public void Add(int points)
    {
        CurrentScore += points;
        OnScoreChanged?.Invoke(CurrentScore);
    }

    public void Reset()
    {
        CurrentScore = 0;
        OnScoreChanged?.Invoke(CurrentScore);
    }
}
