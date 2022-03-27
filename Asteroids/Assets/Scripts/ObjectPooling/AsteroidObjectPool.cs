public class AsteroidObjectPool : ObjectPool<Asteroid>
{
    public static AsteroidObjectPool Instance { get; private set; }

    protected new void Awake()
    {
        if (Instance == null)
            Instance = GetComponent<AsteroidObjectPool>();
    }
}
