using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class RunnerCameraFit : MonoBehaviour
{
    [Header("Referência do personagem")]
    [SerializeField] private Transform player;

    [Header("Largura do mundo que aparece na tela")]
    [SerializeField, Min(0.1f)]
    private float visibleWorldWidth = 4.5f;

    [Header("Enquadramento original")]
    [SerializeField, Min(0.1f)]
    private float referenceOrthographicSize = 5f;

    [SerializeField]
    private float referenceCameraY = 0f;

    private Camera targetCamera;

    private void OnEnable()
    {
        ApplyFit();
    }

    private void LateUpdate()
    {
        ApplyFit();
    }

    private void OnValidate()
    {
        visibleWorldWidth = Mathf.Max(0.1f, visibleWorldWidth);
        referenceOrthographicSize =
            Mathf.Max(0.1f, referenceOrthographicSize);

        ApplyFit();
    }

    private void ApplyFit()
    {
        if (targetCamera == null)
            targetCamera = GetComponent<Camera>();

        if (targetCamera == null || !targetCamera.orthographic)
            return;

        float aspect = targetCamera.aspect;

        if (aspect <= 0f)
            return;

        float newSize = visibleWorldWidth / (2f * aspect);

        targetCamera.orthographicSize = newSize;

        Vector3 position = transform.position;

        if (player != null)
        {
            // Mantém o personagem na mesma altura proporcional
            // que ele ocupava no enquadramento original.
            float originalOffset =
                referenceCameraY - player.position.y;

            position.y = player.position.y
                + originalOffset
                * (newSize / referenceOrthographicSize);
        }
        else
        {
            position.y = referenceCameraY;
        }

        transform.position = position;
    }
}

