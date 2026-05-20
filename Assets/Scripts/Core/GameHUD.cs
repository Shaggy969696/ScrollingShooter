using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ============================================================
// GameHUD.cs
// Actualiza la UI en pantalla: barra de vida, arma equipada y puntaje.
// Se suscribe a eventos de PlayerEntity, WeaponController y ScoreManager.
// No hace polling (no lee valores en Update), solo reacciona a eventos.
//
// Jerarquía Canvas sugerida:
//   Canvas
//     ├─ HealthPanel
//     │    ├─ HealthBar   ← Slider  (asignar a healthBar)
//     │    └─ HealthText  ← TMP_Text (asignar a healthText,  opcional)
//     ├─ WeaponPanel
//     │    └─ WeaponText  ← TMP_Text (asignar a weaponText)
//     └─ ScorePanel
//          └─ ScoreText   ← TMP_Text (asignar a scoreText)
// ============================================================

public class GameHUD : MonoBehaviour
{
    [Header("Referencias de juego")]
    [SerializeField] private PlayerEntity    playerEntity;
    [SerializeField] private WeaponController weaponController;

    [Header("Vida")]
    [SerializeField] private Slider  healthBar;
    [Tooltip("Opcional: muestra HP como '75 / 100'")]
    [SerializeField] private TMP_Text healthText;

    [Header("Arma equipada")]
    [SerializeField] private TMP_Text weaponText;

    [Header("Puntaje")]
    [SerializeField] private TMP_Text scoreText;
    [Tooltip("Prefijo mostrado antes del número (ej. 'SCORE: ')")]
    [SerializeField] private string scorePrefix = "SCORE: ";

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void OnEnable()
    {
        if (playerEntity != null)
            playerEntity.OnHealthChanged += UpdateHealth;

        if (weaponController != null)
            weaponController.OnWeaponChanged += UpdateWeapon;

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnScoreChanged += UpdateScore;
    }

    private void OnDisable()
    {
        if (playerEntity != null)
            playerEntity.OnHealthChanged -= UpdateHealth;

        if (weaponController != null)
            weaponController.OnWeaponChanged -= UpdateWeapon;

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnScoreChanged -= UpdateScore;
    }

    private void Start()
    {
        // Fuerza actualización inicial con los valores actuales
        if (playerEntity != null)
            UpdateHealth(playerEntity.CurrentHealth, playerEntity.MaxHealth);

        UpdateScore(ScoreManager.Instance != null ? ScoreManager.Instance.CurrentScore : 0);
    }

    // ── Callbacks de eventos ──────────────────────────────────────────────────

    private void UpdateHealth(float current, float max)
    {
        if (healthBar != null)
        {
            healthBar.minValue = 0f;
            healthBar.maxValue = max;
            healthBar.value    = current;
        }

        if (healthText != null)
            healthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
    }

    private void UpdateWeapon(string weaponName)
    {
        if (weaponText != null)
            weaponText.text = weaponName;
    }

    private void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = $"{scorePrefix}{score}";
    }
}
