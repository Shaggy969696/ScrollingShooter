// ============================================================
// WeaponController.cs
// Vive en el Player GameObject junto con PlayerController.
// Administra el arma activa y enruta los comandos de disparo.
//
// FLUJO COMPLETO DE COMUNICACIÓN:
//
//   Input System
//     └─► PlayerController.OnAttack()
//           └─► WeaponController.OnFirePressed() / OnFireReleased()
//                 └─► Weapon.TryFire()                   [arma activa]
//                       ├─► ProjectilePool.GetProjectile()  [si es ProjectileWeapon]
//                       │     └─► Projectile.Initialize()
//                       │           └─► IDamageable.TakeDamage()  [en OnTriggerEnter]
//                       │                 └─► ProjectilePool.ReturnProjectile()
//                       └─► IDamageable.TakeDamage() directo  [si es LaserWeapon]
//
//   Input System
//     └─► PlayerController.OnNext() / OnPrevious()
//           └─► WeaponController.SwitchNext() / SwitchPrevious()
//
// Inspector setup:
//   weapons[0] → NormalShot component del Player
//   weapons[1] → HeavyShot component del Player
//   weapons[2] → LaserWeapon component del Player
// ============================================================

using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Tooltip("Todos los componentes Weapon del jugador, en orden de selección")]
    [SerializeField] private Weapon[] weapons;

    private int currentIndex = 0;
    private Weapon activeWeapon;
    private bool isFiring;

    private void Start()
    {
        if (weapons == null || weapons.Length == 0)
        {
            Debug.LogError("WeaponController: no hay armas asignadas en el Inspector.");
            return;
        }
        EquipWeapon(0);
    }

    private void Update()
    {
        // Solo las armas Auto se disparan cada frame (la cadencia la controla el cooldown del arma)
        // SinglePulse (Laser) se dispara directamente en OnFirePressed(), no aquí
        if (isFiring && activeWeapon != null && activeWeapon.fireMode == Weapon.FireMode.Auto)
            activeWeapon.TryFire();
    }

    // Llamado por PlayerController cuando se PRESIONA el botón de disparo
    public void OnFirePressed()
    {
        isFiring = true;

        // Las armas SinglePulse disparan al instante del press, sin pasar por Update()
        if (activeWeapon != null && activeWeapon.fireMode == Weapon.FireMode.SinglePulse)
            activeWeapon.TryFire();
    }

    // Llamado por PlayerController cuando se SUELTA el botón de disparo
    public void OnFireReleased()
    {
        isFiring = false;
    }

    // Avanza al siguiente arma en el array (cíclico)
    public void SwitchNext()
    {
        EquipWeapon((currentIndex + 1) % weapons.Length);
    }

    // Retrocede al arma anterior (cíclico)
    public void SwitchPrevious()
    {
        EquipWeapon((currentIndex - 1 + weapons.Length) % weapons.Length);
    }

    private void EquipWeapon(int index)
    {
        currentIndex = index;
        activeWeapon = weapons[currentIndex];
        isFiring = false;  // resetea el estado de disparo al cambiar arma
        Debug.Log($"Arma equipada: {activeWeapon.GetType().Name}");
    }
}