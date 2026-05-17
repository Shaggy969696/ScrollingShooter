// ============================================================
// EnemySpawnerTester.cs  — Script de prueba temporal
// Spawnea enemigos automáticamente a intervalos configurables.
// NO depende del Input System.
// Eliminar o desactivar este componente cuando ya no se necesite.
// ============================================================

using UnityEngine;

public class EnemySpawnerTester : MonoBehaviour
{
    [Header("Referencia")]
    [Tooltip("Se busca automáticamente en este mismo GameObject si no se asigna")]
    [SerializeField] private EnemySpawner spawner;

    [Header("Shooter - Spawneo automático")]
    [Tooltip("Segundos entre cada ShooterEnemy")]
    [SerializeField] private float shooterInterval = 5f;

    [Tooltip("Posiciones X en las que pueden aparecer los Shooters (se elige cíclicamente)")]
    [SerializeField] private float[] shooterLanes = { -10f, 0f, 10f };

    [Header("Swooper - Spawneo automático")]
    [Tooltip("Segundos entre cada SwooperEnemy")]
    [SerializeField] private float swooperInterval = 4f;

    private float shooterTimer;
    private float swooperTimer;
    private int laneIndex;

    private void Awake()
    {
        if (spawner == null)
            spawner = GetComponent<EnemySpawner>();

        if (spawner == null)
            Debug.LogError("[EnemySpawnerTester] No se encontró EnemySpawner.");

        // Arranca los timers desfasados para que no spawneen al mismo tiempo
        shooterTimer = shooterInterval;
        swooperTimer = swooperInterval * 0.5f;
    }

    private void Update()
    {
        if (spawner == null) return;

        // ── ShooterEnemy ──────────────────────────────────────────────────────
        shooterTimer -= Time.deltaTime;
        if (shooterTimer <= 0f)
        {
            float lane = shooterLanes.Length > 0
                ? shooterLanes[laneIndex % shooterLanes.Length]
                : 0f;

            spawner.SpawnShooter(lane);
            laneIndex++;
            shooterTimer = shooterInterval;
        }

        // ── SwooperEnemy ──────────────────────────────────────────────────────
        swooperTimer -= Time.deltaTime;
        if (swooperTimer <= 0f)
        {
            spawner.SpawnSwooper();
            swooperTimer = swooperInterval;
        }
    }
}