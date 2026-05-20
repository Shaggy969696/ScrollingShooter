// ============================================================
// ShooterEnemy.cs
// MODIFICADO: el enemigo se retira automáticamente tras stayDuration
// segundos en estado Shooting si no es destruido antes por el player.
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

    [Tooltip("Mínimo delay antes del primer disparo al llegar a posición (0 = dispara enseguida)")]
    [SerializeField] private float firstShotMinDelay = 0.1f;

    [Tooltip("Máximo delay antes del primer disparo al llegar a posición (≤ fireInterval)")]
    [SerializeField] private float firstShotMaxDelay = 1.4f;

    [Tooltip("Daño que inflige cada proyectil al player")]
    [SerializeField] private float projectileDamage = 10f;

    [Tooltip("Velocidad de los proyectiles disparados")]
    [SerializeField] private float projectileSpeed = 20f;

    [Tooltip("Pool de proyectiles enemigos (prefab con layer EnemyProjectile)")]
    [SerializeField] private ProjectilePool projectilePool;

    [Tooltip("Punto de origen del disparo — debe apuntar hacia -Z en world space")]
    [SerializeField] private Transform firePoint;

    [Header("Shooter - Tiempo en escena")]
    [Tooltip("Segundos que permanece disparando antes de retirarse solo (0 = no se retira)")]
    [SerializeField] private float stayDuration = 8f;

    // ── Estado interno ────────────────────────────────────────────────────────

    private enum State { Idle, Entering, Shooting, Retreating }
    private State state = State.Idle;

    private float stopAtZ;
    private float retreatToZ;
    private float fireTimer;
    private float stayTimer;    // cuenta regresiva desde stayDuration

    // ── API pública ───────────────────────────────────────────────────────────

    public void BeginEntry(Vector3 spawnPos, float topBorderZ)
    {
        transform.position = spawnPos;
        stopAtZ    = topBorderZ - distanceFromTopBorder;
        retreatToZ = spawnPos.z;
        state      = State.Entering;
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
        stayTimer = 0f;
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
            fireTimer = fireInterval;
            stayTimer = stayDuration;   // arranca la cuenta regresiva al detenerse
            state     = State.Shooting;
        }
    }

    private void UpdateShooting()
    {
        // ── Disparo ───────────────────────────────────────────────────────────
        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            Shoot();
            fireTimer = fireInterval;
        }

        // ── Retirada automática ───────────────────────────────────────────────
        if (stayDuration > 0f)
        {
            stayTimer -= Time.deltaTime;
            if (stayTimer <= 0f)
                Retreat();
        }
    }

    private void UpdateRetreating()
    {
        Vector3 target = new Vector3(transform.position.x, transform.position.y, retreatToZ);
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

        if (transform.position.z >= retreatToZ - 0.05f)
            ReturnToPool();
    }

    // ── Disparo ───────────────────────────────────────────────────────────────

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