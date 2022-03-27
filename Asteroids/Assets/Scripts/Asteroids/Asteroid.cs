using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Asteroid : MonoBehaviour
{
    [SerializeField]
    private float force;

    private Rigidbody2D rb2D;

    private int gridSize;

    private void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        gridSize = AsteroidManager.Instance.GridSize;
    }

    private void OnEnable()
    {
        Vector2 forceVector = Vector2.zero;
        forceVector.x = Random.Range(-1.0f, 1.0f);
        forceVector.y = Random.Range(-1.0f, 1.0f);

        forceVector *= force;

        rb2D.AddForce(forceVector, ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.TryGetComponent(out Asteroid other))
        {
            AsteroidObjectPool.Instance.ReturnToPool(other);

            int x = Random.Range(-gridSize / 2, gridSize / 2);
            int y = Random.Range(-gridSize / 2, gridSize / 2);

            AsteroidManager.Instance.SpawnAsteroid(x, y, 1.0f);
        }
    }
}
