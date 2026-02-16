using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class CurvedTextFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform target;                // Player or any object
    public Vector3 worldOffset = new Vector3(0, 2f, 0); // Offset above target
    public Camera cam;

    [Header("Curve Settings")]
    [Range(0, 180)] public float arcAngle = 90f;   // Total angle of the arc
    public float radius = 50f;                      // Radius of the curve
    public bool curveClockwise = true;              // Flip arc direction

    private TextMeshProUGUI tmp;
    private RectTransform rectTransform;
    private Canvas canvas;

    void Awake()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        rectTransform = tmp.GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        if (cam == null) cam = Camera.main;
    }

    void LateUpdate()
    {
        if (target == null || string.IsNullOrEmpty(tmp.text)) return;

        // 1️⃣ Follow target
        Vector3 worldPos = target.position + worldOffset;
        Vector3 viewportPos = cam.WorldToViewportPoint(worldPos);
        if (viewportPos.z < 0) return;

        Vector2 canvasSize = canvas.GetComponent<RectTransform>().sizeDelta;
        Vector2 anchoredPos = new Vector2(
            (viewportPos.x * canvasSize.x) - (canvasSize.x * 0.5f),
            (viewportPos.y * canvasSize.y) - (canvasSize.y * 0.5f)
        );
        rectTransform.anchoredPosition = anchoredPos;

        // 2️⃣ Curve text
        tmp.ForceMeshUpdate();
        TMP_TextInfo textInfo = tmp.textInfo;
        int charCount = textInfo.characterCount;

        if (charCount == 0) return;

        float startAngle = -arcAngle / 2f * Mathf.Deg2Rad;
        float endAngle = arcAngle / 2f * Mathf.Deg2Rad;

        for (int i = 0; i < charCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int vertexIndex = charInfo.vertexIndex;
            int meshIndex = charInfo.materialReferenceIndex;
            Vector3[] vertices = textInfo.meshInfo[meshIndex].vertices;

            // Interpolate angle along the arc
            float t = (float)i / (charCount - 1);
            float angle = Mathf.Lerp(startAngle, endAngle, t);
            if (curveClockwise) angle = -angle;

            // Compute circular offset
            Vector3 offset = new Vector3(Mathf.Sin(angle), Mathf.Cos(angle) - 1f, 0) * radius;

            // Center the character and apply offset
            Vector3 charMid = (vertices[vertexIndex + 0] + vertices[vertexIndex + 2]) / 2;
            for (int j = 0; j < 4; j++)
                vertices[vertexIndex + j] += offset - charMid;
        }

        // Update mesh
        for (int i = 0; i < tmp.textInfo.meshInfo.Length; i++)
        {
            TMP_MeshInfo meshInfo = tmp.textInfo.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            tmp.UpdateGeometry(meshInfo.mesh, i);
        }
    }

    // Optional runtime setter
    public void SetTarget(Transform t)
    {
        target = t;
    }
}
