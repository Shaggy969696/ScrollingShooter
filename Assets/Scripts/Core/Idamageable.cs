// ============================================================
// IDamageable.cs
// Interfaz que implementa CUALQUIER objeto que pueda recibir daño.
// Al usar una interfaz (en vez de herencia) logramos que cosas
// muy distintas (Enemy, Boss, estructura destructible, etc.)
// puedan recibir daño sin compartir clase base.
// ============================================================

public interface IDamageable
{
    // Método principal: recibe una cantidad de daño
    void TakeDamage(float amount);

    // Propiedad de solo lectura: el caller puede consultar si el objeto ya murió
    bool IsDead { get; }
}