// ============================================================
// NormalShot.cs  — Arma 1
// Disparo normal: spawnea proyectiles desde L1 y R1 simultáneamente.
// Modo Auto: dispara de forma continua mientras se mantiene Spacebar,
// limitado por fireRate (heredado de ProjectileWeapon).
//
// Hereda de ProjectileWeapon → Weapon → MonoBehaviour
//
// Inspector setup en el componente:
//   firePoints[0] → firePointL1
//   firePoints[1] → firePointR1
//   pool          → referencia al ProjectilePool de esta arma
//   projectileSpeed, damage, fireRate → configurables en el Inspector
// ============================================================

using UnityEngine;

public class NormalShot : ProjectileWeapon
{
    public override void TryFire()
    {
        // Chequeo de estado: si está en cooldown, no hace nada
        if (state != WeaponState.Ready) return;

        // Dispara desde cada firePoint asignado (L1 y R1 en simultáneo)
        foreach (var fp in firePoints)
            SpawnProjectile(fp);

        // Inicia el cooldown: no puede volver a disparar hasta que pase fireRate segundos
        StartCooldown(fireRate);
    }
}