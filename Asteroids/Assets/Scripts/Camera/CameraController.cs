using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [SerializeField] private ShipController shipController;

    private Camera cameraComponent;
    private Vector3 followOffset;

    public float Height { get; private set; }
    public float Width { get; private set; }

    private void Awake()
    {
        cameraComponent = GetComponent<Camera>();

        Height = cameraComponent.orthographicSize * 2;
        Width = (float)Screen.width / (float)Screen.height * Height;
    }

    private void Start()
    {
        followOffset = transform.position;
    }

    private void Update()
    {
        if (shipController == null)
            return;

        transform.position = followOffset + shipController.transform.position;
    }
}
