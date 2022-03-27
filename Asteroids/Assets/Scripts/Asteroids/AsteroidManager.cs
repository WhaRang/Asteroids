using System.Collections;
using UnityEngine;

public class AsteroidManager : MonoBehaviour
{
    public static AsteroidManager Instance { get; private set; }

    //[SerializeField] 
    //private AsteroidObjectPool asteroidPool;

    [SerializeField]
    private ProjectileRenderer projectileRenderer;

    [SerializeField] 
    private int gridSize;

    public int GridSize => gridSize;

    private const float DEFAULT_CELL_SIZE = 3.0f;

    private Vector3 startPos;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // asteroidPool.PoolSize = gridSize * gridSize;
        // asteroidPool.FillPool();

        startPos = Vector3.zero;
        startPos.y = gridSize / 2 * DEFAULT_CELL_SIZE;
        startPos.x = -startPos.y;

        SetupGrid();
    }

    private void SetupGrid()
    {
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                // Test
                // SpawnAsteroid(i, j, 0.0f);

                Vector3 newPos = startPos;
                newPos.x += i * DEFAULT_CELL_SIZE;
                newPos.y -= j * DEFAULT_CELL_SIZE;

                projectileRenderer.SpawnProjectile(newPos, Quaternion.identity);
            }
        }
    }

    public void SpawnAsteroid(int gridX, int gridY, float delay)
    {
        //StartCoroutine(SpawnAsteroidCoroutine(gridX, gridY, delay));
    }

    /*private IEnumerator SpawnAsteroidCoroutine(int gridX, int gridY, float delay)
    {
        yield return new WaitForSeconds(delay);

        Vector3 newPos = startPos;
        newPos.x += gridX * DEFAULT_CELL_SIZE;
        newPos.y -= gridY * DEFAULT_CELL_SIZE;

        Asteroid asteroid = asteroidPool.GetItemFromPool();
        asteroid.gameObject.SetActive(true);
        asteroid.transform.position = newPos;
    }*/
}
