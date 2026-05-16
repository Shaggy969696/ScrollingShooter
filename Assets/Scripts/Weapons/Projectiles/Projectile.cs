// ============================================================
// Projectile.cs
// Clase base abstracta para TODOS los proyectiles del juego.
// Define el contrato: lifetime, damage, pool de origen, colisión.
// Cada tipo concreto solo define cómo se mueve (Move()).
//
// Relación: Projectile <-- BasicProjectile (y futuros tipos)
//           Projectile --referencia--> ProjectilePool (para devolverse)
//           Projectile --llama--> IDamageable.TakeDamage() (en colisión)
//
// IMPORTANTE: Los proyectiles usan Object Pool.
// NO se destruyen con Destroy(), se devuelven al pool con ReturnToPool().
// Initialize() reemplaza a Start()/Awake() porque el objeto se reutiliza.
// ============================================================

using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    [Header("Stats")]
    public float damage = 10f;
    public float speed = 30f;

    [Header("Lifetime")]
    [SerializeField] private float lifetime = 3f;

    // Referencia al pool que creó este proyectil (para poder devolverse)
    private ProjectilePool originPool;
    private float lifeTimer;

    // Llamado por ProjectilePool al sacar el proyectil del pool
    // Configura los valores del arma que lo disparó (velocidad, daño)
    public void Initialize(ProjectilePool pool, float speed, float damage)
    {
        this.originPool = pool;
        this.speed = speed;
        this.damage = damage;
        lifeTimer = lifetime;
    }

    protected virtual void Update()
    {
        // Temporizador de vida: si se va de pantalla sin colisionar, vuelve al pool
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
        {
            ReturnToPool();
            return;
        }

        Move();
    }

    // Cada subclase define su propio movimiento (recto, zigzag, curvo, etc.)
    protected abstract void Move();

    // Devuelve el proyectil al pool en lugar de destruirlo
    // Llámalo siempre que el proyectil "muera" (colisión o timeout)
    protected void ReturnToPool()
    {
        if (originPool != null)
            originPool.ReturnProjectile(this);
        else
            Destroy(gameObject);  // fallback si se usa sin pool (no recomendado)
    }

    // Al entrar en un trigger: aplica daño si el target implementa IDamageable
    // Proyectiles físicos usan Trigger (no Physics collision)
    // Setup requerido: Collider del proyectil en IsTrigger = true
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IDamageable>(out var target))
        {
            target.TakeDamage(damage);
            ReturnToPool();
        }
    }
}