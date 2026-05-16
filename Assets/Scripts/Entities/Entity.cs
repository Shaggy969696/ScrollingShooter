// ============================================================
// Entity.cs
// Clase base abstracta para CUALQUIER entidad con vida del juego
// (Player, Enemy, Boss, NPC, etc.)
//
// Implementa IDamageable para que cualquier Entity sea un
// target válido de armas, láser, explosiones, etc.
//
// Relación: Entity --implementa--> IDamageable
//           Enemy  --hereda-->     Entity
// ============================================================

using UnityEngine;

public abstract class Entity : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] protected float maxHealth = 100f;

    // Estado interno de la salud - solo las subclases pueden leer/modificar
    protected float currentHealth;

    // Implementación de IDamageable.IsDead
    public bool IsDead => currentHealth <= 0f;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    // Implementación de IDamageable.TakeDamage
    // virtual: las subclases pueden sobreescribirlo si necesitan lógica pre/post daño
    public virtual void TakeDamage(float amount)
    {
        if (IsDead) return;

        currentHealth -= amount;

        // Hook para subclases: efectos, animaciones, sonidos al recibir daño
        OnDamageTaken(amount);

        if (IsDead) Die();
    }

    // Hook sobreescribible: lógica al recibir daño (sin ser obligatorio implementarlo)
    protected virtual void OnDamageTaken(float amount) { }

    // Obligatorio implementar en cada subclase: ¿qué pasa cuando muere?
    // Un Enemy → se destruye. Un Boss → activa fase 2. Un Player → game over.
    protected abstract void Die();
}