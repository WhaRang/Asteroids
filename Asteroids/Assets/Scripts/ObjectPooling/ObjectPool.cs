using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ObjectPool<T> : MonoBehaviour where T : MonoBehaviour
{
    [SerializeField]
    private T prefab;

    [SerializeField]
    private int poolSize;

    [SerializeField]
    private bool activateObjects;

    public int PoolSize { 
        get => poolSize;
        set => poolSize = value;
    }

    private List<T> pool;

    private int index = 0;

    protected void Awake()
    {
        FillPool();
    }

    public void FillPool()
    {
        if (pool == null)
            pool = new List<T>();

        for (int i = 0; i < poolSize; i++)
        {
            if (pool.Count >= poolSize)
                break;

            T spawnedObj = Instantiate(prefab);
            spawnedObj.gameObject.SetActive(activateObjects);

            pool.Add(spawnedObj);
        }
    }

    public T GetItemFromPool()
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].gameObject.activeInHierarchy)
            {
                return pool[i];
            }
        }

        if (index >= pool.Count)
            index = 0;

        return pool[index++];
    }

    public void ReturnToPool(T item)
    {
        item.gameObject.SetActive(false);
    }

    public void ReturmAllToPool()
    {
        foreach (T item in pool)
        {
            ReturnToPool(item);
        }
    }
}
