using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering; // BlendMode i�in gerekli

public class GridBuildingSystem : MonoBehaviour
{
    [Header("Grid Ayarlar�")]
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private LayerMask groundLayerMask;

    [Header("Prefab Ayarlar�")]
    [SerializeField] private List<GameObject> prefabs;
    [SerializeField] private GameObject deletionMarkerPrefab; // Silme modu i�in k�rm�z� i�aretleyici prefab'� (opsiyonel)

    private Camera mainCamera;
    private int activePrefabIndex = 0;
    private GameObject previewObject; // �n�a �nizlemesi
    private GameObject deletionMarkerInstance; // Silme i�aret�isinin instance'�
    private GameObject currentActiveMarker; // O an aktif olan i�aret�i (ya previewObject ya da deletionMarkerInstance)

    private Dictionary<Vector3, GameObject> placedObjects = new Dictionary<Vector3, GameObject>();
    private bool isDeleting = false;

    void Start()
    {
        mainCamera = Camera.main;
        // Prefab listesi bo� de�ilse ilk �nizlemeyi olu�tur
        if (prefabs != null && prefabs.Count > 0)
        {
            UpdatePreviewObject();
        }
        UpdateActiveMarkerVisuals(); // Mod'a g�re do�ru i�aret�iyi aktif et
    }

    void Update()
    {
        HandleMousePosition();
        HandleInput();
        UpdateActiveMarkerVisuals(); // Her frame mod'a g�re i�aret�ileri g�ncelle
    }

    private void HandleInput()
    {
        for (int i = 0; i < prefabs.Count && i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SetActivePrefab(i);
                break;
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            if (!isDeleting)
            {
                isDeleting = true;
                Debug.Log("Silme Modu Aktif.");
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            // UI �zerinde olup olmad���m�z� kontrol et, e�er UI �zerindeysek i�lem yapma
            // if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            // {
            //     return;
            // }

            if (isDeleting)
            {
                DeleteObjectAtMousePosition();
            }
            else if (previewObject != null && previewObject.activeSelf)
            {
                PlaceObject();
            }
        }
    }

    private void UpdateActiveMarkerVisuals()
    {
        if (isDeleting)
        {
            if (previewObject != null && previewObject.activeSelf)
            {
                previewObject.SetActive(false);
            }
            EnsureDeletionMarkerIsActive();
            currentActiveMarker = deletionMarkerInstance;
        }
        else // �n�a modunda
        {
            if (deletionMarkerInstance != null && deletionMarkerInstance.activeSelf)
            {
                deletionMarkerInstance.SetActive(false);
            }

            if (previewObject != null && !previewObject.activeSelf)
            {
                previewObject.SetActive(true);
            }
            currentActiveMarker = previewObject;
        }
    }

    private void EnsureDeletionMarkerIsActive()
    {
        if (deletionMarkerInstance == null)
        {
            if (deletionMarkerPrefab != null)
            {
                deletionMarkerInstance = Instantiate(deletionMarkerPrefab);
            }
            else
            {
                deletionMarkerInstance = GameObject.CreatePrimitive(PrimitiveType.Cube);
                deletionMarkerInstance.transform.localScale = Vector3.one * gridSize * 0.9f;

                Renderer markerRenderer = deletionMarkerInstance.GetComponent<Renderer>();
                // URP uyumlu k�rm�z� transparan materyal olu�tur
                markerRenderer.material = CreateTransparentURPMaterial(new Color(1.0f, 0.0f, 0.0f, 0.5f));
            }

            Collider markerCollider = deletionMarkerInstance.GetComponent<Collider>();
            if (markerCollider != null)
            {
                markerCollider.enabled = false;
            }
        }

        if (deletionMarkerInstance != null && !deletionMarkerInstance.activeSelf)
        {
            deletionMarkerInstance.SetActive(true);
        }
    }

    private void HandleMousePosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        bool markerShouldBeVisible = false;

        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, groundLayerMask))
        {
            markerShouldBeVisible = true;
            Vector3 gridPosition = SnapToGrid(hitInfo.point);
            if (currentActiveMarker != null)
            {
                currentActiveMarker.transform.position = gridPosition;
            }
        }

        // ��aret�iyi sadece fare zemin �zerindeyken g�ster
        if (currentActiveMarker != null && currentActiveMarker.activeSelf != markerShouldBeVisible)
        {
            currentActiveMarker.SetActive(markerShouldBeVisible);
        }
    }

    private Vector3 SnapToGrid(Vector3 rawPosition)
    {
        float snappedX = Mathf.Round(rawPosition.x / gridSize) * gridSize;
        float snappedY = Mathf.Round(rawPosition.y / gridSize) * gridSize;
        float snappedZ = Mathf.Round(rawPosition.z / gridSize) * gridSize;
        return new Vector3(snappedX, snappedY, snappedZ);
    }

    private void PlaceObject()
    {
        if (previewObject == null || !previewObject.activeSelf) return;

        Vector3 targetPosition = previewObject.transform.position;
        if (!placedObjects.ContainsKey(targetPosition))
        {
            GameObject newObject = Instantiate(prefabs[activePrefabIndex], targetPosition, Quaternion.identity);
            placedObjects.Add(targetPosition, newObject);
            Debug.Log(prefabs[activePrefabIndex].name + " objesi " + targetPosition + " pozisyonuna yerle�tirildi.");
        }
        else
        {
            Debug.LogWarning("Bu pozisyon (" + targetPosition + ") zaten dolu! Yerle�tirme yap�lamad�.");
        }
    }

    private void DeleteObjectAtMousePosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, groundLayerMask))
        {
            Vector3 targetPosition = SnapToGrid(hitInfo.point);
            if (placedObjects.ContainsKey(targetPosition))
            {
                GameObject objectToDestroy = placedObjects[targetPosition];
                placedObjects.Remove(targetPosition);
                Destroy(objectToDestroy);
                Debug.Log(targetPosition + " pozisyonundaki obje silindi.");
            }
            else
            {
                Debug.Log("Bu pozisyonda (" + targetPosition + ") silinecek bir obje yok.");
            }
        }
    }

    public void SetActivePrefab(int newIndex)
    {
        if (newIndex >= 0 && newIndex < prefabs.Count)
        {
            if (isDeleting || activePrefabIndex != newIndex)
            {
                isDeleting = false;
                Debug.Log("�n�a moduna ge�ildi.");
            }
            activePrefabIndex = newIndex;
            UpdatePreviewObject();
            Debug.Log(prefabs[activePrefabIndex].name + " se�ildi.");
        }
    }

    private void UpdatePreviewObject()
    {
        if (previewObject != null)
        {
            Destroy(previewObject);
        }

        if (prefabs != null && prefabs.Count > 0 && activePrefabIndex < prefabs.Count)
        {
            previewObject = Instantiate(prefabs[activePrefabIndex]);
            Collider previewCollider = previewObject.GetComponent<Collider>();
            if (previewCollider != null)
            {
                previewCollider.enabled = false;
            }

            Renderer[] renderers = previewObject.GetComponentsInChildren<Renderer>();
            // URP uyumlu ye�il transparan materyal olu�tur
            Material previewMat = CreateTransparentURPMaterial(new Color(0.0f, 1.0f, 0.0f, 0.5f));

            if (previewMat != null)
            {
                foreach (Renderer renderer in renderers)
                {
                    renderer.material = previewMat;
                }
            }
        }
        else
        {
            previewObject = null;
        }
    }

    /// <summary>
    /// URP i�in belirli bir renkte transparan bir materyal olu�turur.
    /// </summary>
    /// <param name="color">Materyalin rengi (alpha de�eri dahil).</param>
    /// <returns>URP uyumlu transparan materyal.</returns>
    private Material CreateTransparentURPMaterial(Color color)
    {
        // URP'nin varsay�lan Lit shader'�n� bul
        Shader urpLitShader = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLitShader == null)
        {
            Debug.LogError("URP/Lit shader bulunamad�. L�tfen projenizde URP paketinin do�ru �ekilde kurulu oldu�undan emin olun.");
            return null;
        }

        Material material = new Material(urpLitShader);

        // URP Lit Shader'da transparanl�k i�in materyal ayarlar�
        material.SetFloat("_Surface", 1); // Surface Type'� Transparent yap (0=Opaque, 1=Transparent)
        material.SetFloat("_Blend", 0);   // Blend Mode'u Alpha yap (0=Alpha, 1=Premultiply, 2=Additive, 3=Multiply)
        material.SetFloat("_ZWrite", 0);  // Derinlik yazmay� kapat
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON"); // Gerekirse bunu EnableKeyword("_ALPHABLEND_ON") ile de�i�tirin
        material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);

        // Ana rengi ayarla
        material.SetColor("_BaseColor", color);

        return material;
    }
}
