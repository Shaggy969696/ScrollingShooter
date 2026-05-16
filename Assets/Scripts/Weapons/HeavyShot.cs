// ============================================================
// HeavyShot.cs  — Arma 2
// Disparo intenso: spawnea desde L2, L1, R1, R2 simultáneamente.
// Modo Auto: igual que NormalShot, pero con más puntos de fuego.
// La diferencia real está en la configuración del Inspector:
//   más firePoints asignados, mayor daño, menor cadencia (fireRate más alto).
//
// Esta clase existe separada de NormalShot para poder evolucionar
// independientemente: burst fire, proyectiles con spread, etc.
//
// Hereda de ProjectileWeapon → Weapon → MonoBehaviour
//
// Inspector setup:
//   firePoints[0] → firePointL2
//   firePoints[1] → firePointL1
//   firePoints[2] → firePointR1
//   firePoints[3] → firePointR2
//   pool          → referencia al ProjectilePool de esta arma
//   projectileSpeed, damage, fireRate → configurables en el Inspector
// ============================================================

using UnityEngine;

public class HeavyShot : ProjectileWeapon
{
    public override void TryFire()
    {
        if (state != WeaponState.Ready) return;

        // Dispara desde los 4 puntos laterales simultáneamente
        foreach (var fp in firePoints)
            SpawnProjectile(fp);

        StartCooldown(fireRate);
    }
}