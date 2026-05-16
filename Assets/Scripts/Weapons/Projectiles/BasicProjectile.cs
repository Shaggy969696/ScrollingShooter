// ============================================================
// BasicProjectile.cs
// Proyectil concreto de prueba: se mueve en línea recta.
// La dirección de viaje la define la rotación del firePoint
// en el momento del spawn (firePoint.rotation → p.transform.rotation).
// Rotar el firePoint en el editor = cambiar el ángulo del disparo.
//
// Relación: BasicProjectile --hereda--> Projectile
//
// Prefab setup:
//   - MeshRenderer / SpriteRenderer (visuals)
//   - Collider con IsTrigger = true
//   - Componente BasicProjectile
//   - Layer: "PlayerProjectile" (para ignorar colisión con Player)
// ============================================================

using UnityEngine;

public class BasicProjectile : Projectile
{
    // Solo implementa el movimiento: recto en la dirección local Z
    // (que es el forward del proyectil, heredado del firePoint al spawnear)
    protected override void Move()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
    }
}