// ============================================================
// ShooterEnemy.cs
// MODIFICADO: Shoot() ahora obtiene un proyectil del pool enemigo
// y lo lanza desde el firePoint hacia el player (-Z).
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

    [Tooltip("Daño que inflige cada proyectil al player")]
    [SerializeField] private float projectileDamage = 10f;

    [Tooltip("Velocidad de los proyectiles disparados")]
    [SerializeField] private float projectileSpeed = 20f;

    [Tooltip("Pool de proyectiles enemigos (prefab con layer EnemyProjectile)")]
    [SerializeField] private ProjectilePool projectilePool;

    [Tooltip("Punto de origen del disparo — debe apuntar hacia -Z en world space")]
    [SerializeField] private Transform firePoint;

    // ── Estado interno ────────────────────────────────────────────────────────

    private enum State { Idle, Entering, Shooting, Retreating }
    private State state = State.Idle;

    private float stopAtZ;
    private float retreatToZ;
    private float fireTimer;

    // ── API pública ───────────────────────────────────────────────────────────

    public void BeginEntry(Vector3 spawnPos, float topBorderZ)
    {
        transform.position = spawnPos;
        stopAtZ    = topBorderZ - distanceFromTopBorder;
        retreatToZ = spawnPos.z;
        state      = State.Entering;
        // fireTimer se resetea al llegar a stopAtZ, en UpdateEntering()
    }

    public void Retreat()
    {
        if (state == State.Entering || state == State.Shooting)
            state = State.Retreating;
    }

    // ── Hooks ─────────────────────────────────────────────────────────────────

    protected override void OnSpawn()
    {
        state     = State.Idle;
        fireTimer = 0f;
    }

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void Update()
    {
        switch (state)
        {
            case State.Entering:   UpdateEntering();   break;
            case State.Shooting:   UpdateShooting();   break;
            case State.Retreating: UpdateRetreating(); break;
        }
    }

    // ── Estados ───────────────────────────────────────────────────────────────

    private void UpdateEntering()
    {
        Vector3 target = new Vector3(transform.position.x, transform.position.y, stopAtZ);
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

        if (Mathf.Abs(transform.position.z - stopAtZ) < 0.05f)
        {
            transform.position = target;
            fireTimer = fireInterval; // ← resetea aquí, no en BeginEntry
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
        if (projectilePool == null)
        {
            Debug.LogWarning($"[ShooterEnemy] {gameObject.name}: projectilePool no asignado.");
            return;
        }

        Transform origin = firePoint != null ? firePoint : transform;

        Projectile p = projectilePool.GetProjectile();
        p.transform.position = origin.position;
        p.transform.rotation = origin.rotation;
        p.transform.SetParent(null);
        p.Initialize(projectilePool, projectileSpeed, projectileDamage);
    }
}