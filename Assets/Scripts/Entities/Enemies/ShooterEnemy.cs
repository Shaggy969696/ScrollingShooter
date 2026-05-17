// ============================================================
// ShooterEnemy.cs
// Enemigo estático: entra desde Z+, se detiene cerca del borde
// superior y dispara proyectiles hasta ser destruido o retirado.
//
// Estados:
//   Idle       → esperando activación (estado de pool)
//   Entering   → avanzando desde spawn hasta stopAtZ
//   Shooting   → estático, dispara a intervalos
//   Retreating → retrocede hacia Z+ y vuelve al pool
//
// Prefab setup:
//   [ShooterEnemy]  ← ShooterEnemy.cs  |  Layer: Enemy  |  Tag: Enemy
//     ├─ Model      ← Renderer
//     ├─ HitBox     ← BoxCollider, isTrigger=FALSE, Layer: Enemy
//     └─ FirePoint  ← Transform vacío, apunta hacia -Z (hacia el player)
// ============================================================

using UnityEngine;

public class ShooterEnemy : Enemy
{
    [Header("Shooter - Movimiento")]
    [Tooltip("Velocidad de entrada y de retirada (unidades/seg)")]
    [SerializeField] private float moveSpeed = 10f;

    [Tooltip("Distancia al borde superior del área de juego en la que se detiene")]
    [SerializeField] private float distanceFromTopBorder = 5f;

    [Header("Shooter - Disparo")]
    [Tooltip("Segundos entre disparos")]
    [SerializeField] private float fireInterval = 1.5f;

    [Tooltip("Punto de origen del disparo (asignado desde el prefab)")]
    [SerializeField] private Transform firePoint;

    // ── Estado interno ────────────────────────────────────────────────────────

    private enum State { Idle, Entering, Shooting, Retreating }
    private State state = State.Idle;

    private float stopAtZ;    // Z donde se detiene (topBorderZ - distanceFromTopBorder)
    private float retreatToZ; // Z de salida (igual que el Z de spawn)
    private float fireTimer;

    // ── API pública (llamada por EnemySpawner) ────────────────────────────────

    // Inicia la entrada del enemigo.
    //   spawnPos   : posición de aparición fuera del área (Z > topBorderZ)
    //   topBorderZ : coordenada Z del borde superior del espacio de juego
    public void BeginEntry(Vector3 spawnPos, float topBorderZ)
    {
        transform.position = spawnPos;
        stopAtZ = topBorderZ - distanceFromTopBorder;
        retreatToZ = spawnPos.z; // se retira al mismo Z desde donde entró
        state = State.Entering;
        fireTimer = fireInterval;
    }

    // Ordena la retirada. Llamado por EnemySpawner para limpiar la oleada.
    public void Retreat()
    {
        if (state == State.Entering || state == State.Shooting)
            state = State.Retreating;
    }

    // ── Hook de Enemy ─────────────────────────────────────────────────────────

    protected override void OnSpawn()
    {
        state = State.Idle;
        fireTimer = 0f;
    }

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void Update()
    {
        switch (state)
        {
            case State.Entering: UpdateEntering(); break;
            case State.Shooting: UpdateShooting(); break;
            case State.Retreating: UpdateRetreating(); break;
        }
    }

    // ── Máquina de estados ────────────────────────────────────────────────────

    private void UpdateEntering()
    {
        Vector3 target = new Vector3(transform.position.x, transform.position.y, stopAtZ);
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

        if (Mathf.Abs(transform.position.z - stopAtZ) < 0.05f)
        {
            transform.position = target; // snap exacto
            state = State.Shooting;
        }
    }

    private void UpdateShooting()
    {
        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            Shoot();
            fireTimer = fireInterval;
        }
    }

    private void UpdateRetreating()
    {
        Vector3 target = new Vector3(transform.position.x, transform.position.y, retreatToZ);
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

        if (transform.position.z >= retreatToZ - 0.05f)
            ReturnToPool();
    }

    private void Shoot()
    {
        // TODO: obtener proyectil del pool de proyectiles enemigos y lanzarlo
        string origin = firePoint != null ? firePoint.position.ToString("F1") : transform.position.ToString("F1");
        Debug.Log($"[ShooterEnemy] {gameObject.name} disparó desde {origin}");
    }
}