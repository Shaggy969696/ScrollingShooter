// ============================================================
// ProjectileWeapon.cs
// Capa abstracta intermedia entre Weapon y las armas con proyectiles.
// Agrega lo único que comparten NormalShot y HeavyShot:
//   - Referencia al pool de proyectiles
//   - Configuración de velocidad, daño y cadencia
//   - El método SpawnProjectile() compartido
//
// NO implementa TryFire(): eso lo hace cada arma concreta.
// Esto permite que cada arma concreta tenga su propia lógica futura
// (burst fire, spread, apuntado, etc.) sin tocar la capa de pool/spawn.
//
// Relación: ProjectileWeapon --hereda--> Weapon
//           NormalShot / HeavyShot --heredan--> ProjectileWeapon
//           ProjectileWeapon --usa--> ProjectilePool
// ============================================================

using UnityEngine;

public abstract class ProjectileWeapon : Weapon
{
    [Header("Projectile Weapon Settings")]
    [Tooltip("Pool de proyectiles asignado a esta arma")]
    [SerializeField] protected ProjectilePool pool;

    [Tooltip("Velocidad de los proyectiles al disparar")]
    [SerializeField] public float projectileSpeed = 30f;

    [Tooltip("Daño por proyectil")]
    [SerializeField] public float damage = 10f;

    [Tooltip("Segundos entre disparos (cadencia)")]
    [SerializeField] public float fireRate = 0.15f;

    // Instancia un proyectil desde el pool y lo posiciona en el firePoint dado.
    // La rotación del firePoint determina el ángulo de salida en el plano XZ.
    // Rotar el GameObject firePoint en el editor = cambiar ángulo = sin código extra.
    protected void SpawnProjectile(Transform firePoint)
    {
        Projectile p = pool.GetProjectile();
        p.transform.position = firePoint.position;
        p.transform.rotation = firePoint.rotation;  // el ángulo viene del firePoint
        p.transform.SetParent(null);                // se desvincula del pool para moverse libre
        p.Initialize(pool, projectileSpeed, damage);
    }
}