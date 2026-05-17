// ============================================================
// SwooperEnemy.cs
// Enemigo que describe una trayectoria en C usando Bézier cúbico.
// Aparece desde Z+, recorre la C y sale por Z-.
// Si impacta con el Player le quita vida y se desactiva.
//
// Estados:
//   Idle     → esperando activación (estado de pool)
//   Swooping → siguiendo la curva Bézier
//
// Prefab setup:
//   [SwooperEnemy]   ← SwooperEnemy.cs + Rigidbody(IsKinematic, NoGravity)
//                    |  Layer: Enemy  |  Tag: Enemy
//     ├─ Model       ← Renderer
//     ├─ HitBox      ← BoxCollider, isTrigger=FALSE, Layer: Enemy
//     └─ ContactBox  ← BoxCollider, isTrigger=TRUE,  Layer: EnemySensor
//                       (ligeramente más grande que HitBox)
//
// El Rigidbody kinematic en el root es OBLIGATORIO para que
// OnTriggerEnter del ContactBox hijo se propague a este script.
// ============================================================

using UnityEngine;

public class SwooperEnemy : Enemy
{
    [Header("Swooper - Movimiento")]
    [Tooltip("Velocidad de recorrido de la trayectoria en C (unidades/seg)")]
    [SerializeField] private float swoopSpeed = 12f;

    [Tooltip("Desplazamiento en X de los puntos de control (anchura de la C)")]
    [SerializeField] private float curveWidth = 22f;

    [Tooltip("Desplazamiento en Z de los puntos de control intermedios (altura de la C)")]
    [SerializeField] private float curveZOffset = 18f;

    [Header("Swooper - Contacto")]
    [Tooltip("Daño que inflige al impactar con el jugador")]
    [SerializeField] private float contactDamage = 20f;

    // ── Bézier ────────────────────────────────────────────────────────────────
    private Vector3 p0, p1, p2, p3; // puntos de control del Bézier cúbico
    private float   t;               // parámetro de recorrido [0, 1]
    private float   approxLength;    // longitud estimada para convertir speed → t/seg
    private bool    isSwooping;

    // ── API pública (llamada por EnemySpawner) ────────────────────────────────

    // Inicia el recorrido en C.
    //   spawnPos : posición de aparición (fuera del área, Z positivo alto)
    //   exitZ    : coordenada Z de salida (fuera del área, Z negativo)
    //   goRight  : true → la C se abre hacia +X | false → se abre hacia -X
    public void BeginSwoop(Vector3 spawnPos, float exitZ, bool goRight)
    {
        transform.position = spawnPos;

        float dir = goRight ? 1f : -1f;

        // Bézier cúbico que dibuja una C:
        //   P0 → spawn (entrada superior)
        //   P1 → control superior lateral  (curva hacia el lado)
        //   P2 → control inferior lateral  (mismo lado, desciende)
        //   P3 → salida (inferior, vuelve al centro X)
        p0 = spawnPos;
        p1 = new Vector3(spawnPos.x + dir * curveWidth,  spawnPos.y,  curveZOffset);
        p2 = new Vector3(spawnPos.x + dir * curveWidth,  spawnPos.y, -curveZOffset);
        p3 = new Vector3(spawnPos.x,                     spawnPos.y,  exitZ);

        t           = 0f;
        isSwooping  = true;
        approxLength = EstimateBezierLength(12);
    }

    // ── Hook de Enemy ─────────────────────────────────────────────────────────

    protected override void OnSpawn()
    {
        isSwooping = false;
        t          = 0f;
    }

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void Update()
    {
        if (!isSwooping) return;

        // Avanzar t proporcional a la velocidad y la longitud de la curva
        t = Mathf.Clamp01(t + swoopSpeed / approxLength * Time.deltaTime);
        transform.position = EvaluateBezier(t);

        if (t >= 1f)
        {
            isSwooping = false;
            ReturnToPool();
        }
    }

    // Detecta impacto con el Player a través del ContactBox (trigger, EnemySensor layer).
    // El Rigidbody kinematic del root garantiza que este evento se reciba aquí.
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (other.TryGetComponent<IDamageable>(out var player))
            player.TakeDamage(contactDamage);

        Debug.Log($"[SwooperEnemy] {gameObject.name} impactó al jugador!");
        isSwooping = false;
        ReturnToPool();
    }

    // ── Bézier cúbico ─────────────────────────────────────────────────────────

    private Vector3 EvaluateBezier(float t)
    {
        float u = 1f - t;
        return  u*u*u      * p0
              + 3f*u*u*t   * p1
              + 3f*u*t*t   * p2
              + t*t*t       * p3;
    }

    // Estima la longitud de la curva por muestreo lineal (usado una sola vez en BeginSwoop)
    private float EstimateBezierLength(int segments)
    {
        float   length = 0f;
        Vector3 prev   = EvaluateBezier(0f);
        for (int i = 1; i <= segments; i++)
        {
            Vector3 curr = EvaluateBezier(i / (float)segments);
            length += Vector3.Distance(prev, curr);
            prev    = curr;
        }
        return Mathf.Max(length, 0.01f); // evitar división por cero
    }
}
