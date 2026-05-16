// ============================================================
// ProjectilePool.cs
// Object Pool para proyectiles: pre-instancia N objetos al inicio
// y los recicla en lugar de hacer Instantiate/Destroy cada disparo.
//
// FIX: CreateAndEnqueue ahora instancia sin padre y luego re-parentea
// con worldPositionStays=true para evitar heredar la escala mundial
// del Player cuando su transform raíz tiene escala != (1,1,1).
// ============================================================

using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    [Tooltip("Prefab del proyectil que administra este pool")]
    [SerializeField] private Projectile prefab;

    [Tooltip("Cantidad de proyectiles pre-instanciados al inicio")]
    [SerializeField] private int initialSize = 20;

    private Queue<Projectile> available = new Queue<Projectile>();

    private void Awake()
    {
        for (int i = 0; i < initialSize; i++)
            CreateAndEnqueue();
    }

    private void CreateAndEnqueue()
    {
        // 1. Instancia sin padre → escala mundial = escala local del prefab (1,1,1)
        Projectile p = Instantiate(prefab);
        // 2. Re-parentea preservando la escala mundial (worldPositionStays = true)
        //    Unity ajusta la escala LOCAL para que la escala MUNDIAL no cambie
        p.transform.SetParent(transform, worldPositionStays: true);
        p.gameObject.SetActive(false);
        available.Enqueue(p);
    }

    public Projectile GetProjectile()
    {
        if (available.Count == 0)
        {
            Debug.LogWarning($"{name}: Pool vacío, expandiendo. Considerá aumentar initialSize.");
            CreateAndEnqueue();
        }

        Projectile p = available.Dequeue();
        p.gameObject.SetActive(true);
        return p;
    }

    public void ReturnProjectile(Projectile p)
    {
        p.gameObject.SetActive(false);
        // worldPositionStays=true: la escala mundial se preserva al volver al pool
        p.transform.SetParent(transform, worldPositionStays: true);
        available.Enqueue(p);
    }
}