// ============================================================
// Weapon.cs  (reemplaza el vacío existente)
// Clase base abstracta para TODAS las armas del juego.
//
// MÁQUINA DE ESTADOS (WeaponState):
//   Ready ──TryFire()──► Firing logic ──StartCooldown()──► Cooldown
//   Cooldown ──timer──► Ready
//
// Esta máquina de estados vive aquí porque es común a TODAS las armas:
// ninguna puede disparar mientras está en cooldown.
// La transición Ready→Cooldown la activa cada subclase al final de TryFire().
//
// FIRE MODE:
//   Auto        → WeaponController llama TryFire() cada frame mientras se mantiene el botón.
//                 La cadencia real la regula el cooldown.
//   SinglePulse → WeaponController llama TryFire() SOLO al presionar (no al mantener).
//                 Ideal para el laser.
//
// Relación: Weapon <-- ProjectileWeapon <-- NormalShot / HeavyShot
//           Weapon <-- LaserWeapon
//           WeaponController --llama--> Weapon.TryFire()
// ============================================================

using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    // --- Máquina de estados del arma ---
    public enum WeaponState { Ready, Cooldown }
    protected WeaponState state = WeaponState.Ready;
    private float cooldownTimer;

    // --- Modo de disparo ---
    public enum FireMode { Auto, SinglePulse }
    public FireMode fireMode = FireMode.Auto;  // sobreescribible en subclases (Awake)

    [Header("Fire Points")]
    [Tooltip("Transforms de los puntos de spawn. Orden según el arma.")]
    [SerializeField] protected Transform[] firePoints;

    protected virtual void Update()
    {
        // Tick de cooldown: hace la transición Cooldown → Ready cuando termina el timer
        if (state == WeaponState.Cooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
                state = WeaponState.Ready;
        }
    }

    // Cada arma concreta implementa su propia lógica de disparo
    // Siempre debe chequear "if (state != WeaponState.Ready) return;" al inicio
    public abstract void TryFire();

    // Inicia el período de cooldown → transición Ready → Cooldown
    protected void StartCooldown(float duration)
    {
        state = WeaponState.Cooldown;
        cooldownTimer = duration;
    }
}