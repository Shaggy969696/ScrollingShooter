// ============================================================
// LaserWeapon.cs  — Arma 3
// Disparo láser: usa Physics.RaycastAll para impactar TODOS los
// enemigos en la línea de fuego, no solo el primero.
// Modo SinglePulse: dispara UNA VEZ por pulsación de tecla,
// no de forma continua al mantener presionado.
//
// La diferencia con Raycast(): RaycastAll devuelve un array con
// TODOS los hits del rayo, no se detiene en el primero.
//
// Hereda directamente de Weapon (no necesita ProjectileWeapon
// porque no usa proyectiles físicos ni pool).
//
// Inspector setup:
//   firePoints[0]    → firePointC
//   damage           → daño por pulso
//   range            → largo del rayo en unidades de mundo
//   cooldownDuration → tiempo entre pulsos
//   enemyLayer       → seleccionar Layer "Enemy" en el Inspector
// ============================================================

using UnityEngine;

public class LaserWeapon : Weapon
{
    [Header("Laser Settings")]
    [SerializeField] private float damage = 50f;
    [SerializeField] private float range = 100f;
    [SerializeField] private float cooldownDuration = 0.3f;

    [Tooltip("Seleccionar el Layer 'Enemy' en el Inspector")]
    [SerializeField] private LayerMask enemyLayer;

    private void Awake()
    {
        // Sobreescribe el fireMode a SinglePulse:
        // WeaponController solo llamará TryFire() en el momento del press,
        // no de forma continua mientras se mantiene el botón.
        fireMode = FireMode.SinglePulse;
    }

    public override void TryFire()
    {
        if (state != WeaponState.Ready) return;

        Debug.Log("[LaserWeapon] TryFire() ejecutado — disparando láser.");
        FireLaser();
        StartCooldown(cooldownDuration);
    }

    private void FireLaser()
    {
        Transform origin = firePoints[0];  // firePointC

        // RaycastAll: a diferencia de Raycast(), NO se detiene en el primer hit.
        // Devuelve un array con TODOS los colliders que el rayo atraviesa.
        RaycastHit[] hits = Physics.RaycastAll(
            origin.position,
            origin.forward,
            range,
            enemyLayer
        );

        Debug.Log($"[LaserWeapon] Hits detectados: {hits.Length}");

        foreach (var hit in hits)
        {
            // GetComponentInParent: sube por la jerarquía del collider golpeado
            // hasta encontrar IDamageable en el root del prefab enemigo.
            IDamageable target = hit.collider.GetComponentInParent<IDamageable>();
            if (target != null)
            {
                Debug.Log($"[LaserWeapon] Daño aplicado a: {hit.collider.gameObject.name}");
                target.TakeDamage(damage);
            }
        }

        // Visualización en Scene View durante Play Mode
        // Reemplazar con LineRenderer para el efecto visual final
        Debug.DrawRay(origin.position, origin.forward * range, Color.cyan, 0.05f);
    }
}