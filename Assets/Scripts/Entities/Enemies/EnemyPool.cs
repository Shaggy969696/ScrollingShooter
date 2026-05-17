// ============================================================
// EnemyPool.cs
// Object Pool para enemigos. Mismo patrón que ProjectilePool.
// Colocar UN componente EnemyPool por cada tipo de enemigo.
//
// Relación: EnemyPool --gestiona--> Enemy (y sus subclases)
//           EnemySpawner --usa--> EnemyPool
// ============================================================

using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    [Tooltip("Prefab del tipo de enemigo que administra este pool")]
    [SerializeField] private Enemy prefab;

    [Tooltip("Cantidad de enemigos pre-instanciados al inicio")]
    [SerializeField] private int initialSize = 5;

    private readonly Queue<Enemy> available = new Queue<Enemy>();
    private int activeCount;

    // Cantidad de enemigos actualmente activos en escena
    public int ActiveCount => activeCount;

    private void Awake()
    {
        for (int i = 0; i < initialSize; i++)
            CreateAndEnqueue();
    }

    private void CreateAndEnqueue()
    {
        Enemy e = Instantiate(prefab);
        e.transform.SetParent(transform, worldPositionStays: true);
        e.gameObject.SetActive(false);
        available.Enqueue(e);
    }

    // Saca un enemigo del pool y lo activa.
    // El caller debe llamar enemy.Initialize(this) y luego configurar posición/ruta.
    public Enemy GetEnemy()
    {
        if (available.Count == 0)
        {
            Debug.LogWarning($"{name}: Pool vacío, expandiendo. Considerá aumentar initialSize.");
            CreateAndEnqueue();
        }

        Enemy e = available.Dequeue();
        e.gameObject.SetActive(true);
        activeCount++;
        return e;
    }

    // Desactiva el enemigo y lo devuelve al pool.
    // Llamado por Enemy.ReturnToPool(), no directamente por el spawner.
    public void ReturnEnemy(Enemy e)
    {
        e.gameObject.SetActive(false);
        e.transform.SetParent(transform, worldPositionStays: true);
        available.Enqueue(e);
        activeCount--;
    }
}