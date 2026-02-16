using UnityEngine;

public class TextBehaviour : MonoBehaviour
{
    [Tooltip("The Transform this UI should follow.")]
    public Transform target;

    [SerializeField] private Vector3 worldOffset = new Vector3(0, 1.5f, 0);
    private RectTransform rectTransform;
    private Canvas canvas;
    private Camera cam;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        cam = Camera.main;
    }

    public void SetTarget(Transform t)
    {
        target = t;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 worldPos = target.position + worldOffset;
        Vector3 viewportPos = cam.WorldToViewportPoint(worldPos);

        if (viewportPos.z < 0) return;

        Vector2 canvasSize = canvas.GetComponent<RectTransform>().sizeDelta;
        Vector2 anchoredPos = new Vector2(
            (viewportPos.x * canvasSize.x) - (canvasSize.x * 0.5f),
            (viewportPos.y * canvasSize.y) - (canvasSize.y * 0.5f)
        );

        rectTransform.anchoredPosition = anchoredPos;
    }
}
