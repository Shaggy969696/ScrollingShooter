// ============================================================
// SwooperEnemy.cs
// MODIFICADO: trayectoria en S usando dos Béziers cúbicos encadenados.
// El enemigo pasa 3 veces por el X de entrada (inicio, mitad, salida).
        // La unión entre segmentos es C1-continua (tangente suave sin quiebres).
//
// Prefab setup: sin cambios respecto a la versión anterior.
// ============================================================

using UnityEngine;

public class SwooperEnemy : Enemy
{
    [Header("Swooper - Movimiento")]
    [Tooltip("Velocidad de recorrido de la trayectoria en S (unidades/seg)")]
    [SerializeField] private float swoopSpeed = 12f;

    [Tooltip("Apertura lateral de cada curva de la S en X")]
    [SerializeField] private float curveWidth = 15f;

    [Tooltip("Extensión vertical (Z) de cada mitad de la S")]
    [SerializeField] private float curveZSpan = 15f;

    [Header("Swooper - Contacto")]
    [Tooltip("Daño que inflige al impactar con el jugador")]
    [SerializeField] private float contactDamage = 20f;

    // ── Bézier doble (S = seg1 + seg2) ───────────────────────────────────────
    // seg1: p0 → p1 → p2 → p3   (primera mitad de la S)
    // seg2: p3 → p4 → p5 → p6   (segunda mitad, p3 compartido)
    // Condición de suavidad C1: p4 = p3 + (p3 - p2)  ← aplicada en BeginSwoop
    private Vector3 p0, p1, p2, p3, p4, p5, p6;

    private float t;            // parámetro global [0, 1]
    private float approxLength; // longitud total estimada
    private bool  isSwooping;

    // ── API pública ───────────────────────────────────────────────────────────

    // spawnPos.x  : X del player en el momento del spawn
    // spawnPos.z  : Z de entrada (fuera del área, Z alto)
    // exitZ       : Z de salida (fuera del área, Z bajo)
    // goRight     : true → primera curva abre a +X, segunda a -X (S normal)
    //               false → primera curva abre a -X, segunda a +X (S espejo)
    public void BeginSwoop(Vector3 spawnPos, float exitZ, bool goRight)
    {
        transform.position = spawnPos;

        float x   = spawnPos.x;
        float y   = spawnPos.y;
        float dir = goRight ? 1f : -1f;

        // ── Punto medio (cruce de playerX por 2da vez) ────────────────────────
        float midZ = (spawnPos.z + exitZ) * 0.5f;

        // ── Segmento 1: entrada → punto medio ────────────────────────────────
        p0 = spawnPos;                                                          // playerX, entrada
        p1 = new Vector3(x + dir * curveWidth, y, spawnPos.z - curveZSpan);    // jala hacia dir
        p2 = new Vector3(x + dir * curveWidth, y, midZ       + curveZSpan);    // jala hacia dir antes del cruce
        p3 = new Vector3(x,                    y, midZ);                        // cruce central (2da vez en playerX)

        // ── Segmento 2: punto medio → salida ─────────────────────────────────
        // C1: p4 = p3 + (p3 - p2) → tangente continua sin quiebre
        p4 = p3 + (p3 - p2);                                                    // automático, no tocar
        p5 = new Vector3(x - dir * curveWidth, y, exitZ      + curveZSpan);    // jala hacia -dir antes de salida
        p6 = new Vector3(x,                    y, exitZ);                       // salida (3ra vez en playerX)

        t            = 0f;
        isSwooping   = true;
        approxLength = EstimateTotalLength(20);
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

        t = Mathf.Clamp01(t + swoopSpeed / approxLength * Time.deltaTime);
        transform.position = EvaluateS(t);

        if (t >= 1f)
        {
            isSwooping = false;
            ReturnToPool();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (other.TryGetComponent<IDamageable>(out var player))
            player.TakeDamage(contactDamage);

        Debug.Log($"[SwooperEnemy] {gameObject.name} impactó al jugador!");
        isSwooping = false;
        ReturnToPool();
    }

    // ── Evaluación de la S ────────────────────────────────────────────────────

    // t en [0, 0.5] → segmento 1 con t local en [0, 1]
    // t en [0.5, 1] → segmento 2 con t local en [0, 1]
    private Vector3 EvaluateS(float t)
    {
        if (t <= 0.5f)
        {
            float lt = t * 2f;
            return EvaluateCubic(p0, p1, p2, p3, lt);
        }
        else
        {
            float lt = (t - 0.5f) * 2f;
            return EvaluateCubic(p3, p4, p5, p6, lt);
        }
    }

    private static Vector3 EvaluateCubic(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
    {
        float u = 1f - t;
        return u*u*u * a + 3f*u*u*t * b + 3f*u*t*t * c + t*t*t * d;
    }

    private float EstimateTotalLength(int segments)
    {
        float   length = 0f;
        Vector3 prev   = EvaluateS(0f);
        for (int i = 1; i <= segments; i++)
        {
            Vector3 curr = EvaluateS(i / (float)segments);
            length += Vector3.Distance(prev, curr);
            prev    = curr;
        }
        return Mathf.Max(length, 0.01f);
    }
}
