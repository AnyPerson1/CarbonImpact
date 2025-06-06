using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering; // BlendMode için gerekli

public class GridBuildingSystem : MonoBehaviour
{
    [Header("Grid Ayarlarý")]
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private LayerMask groundLayerMask;

    [Header("Prefab Ayarlarý")]
    [SerializeField] private List<GameObject> prefabs;
    [SerializeField] private GameObject deletionMarkerPrefab; // Silme modu için kýrmýzý iþaretleyici prefab'ý (opsiyonel)

    private Camera mainCamera;
    private int activePrefabIndex = 0;
    private GameObject previewObject; // Ýnþa önizlemesi
    private GameObject deletionMarkerInstance; // Silme iþaretçisinin instance'ý
    private GameObject currentActiveMarker; // O an aktif olan iþaretçi (ya previewObject ya da deletionMarkerInstance)

    private Dictionary<Vector3, GameObject> placedObjects = new Dictionary<Vector3, GameObject>();
    private bool isDeleting = false;

    void Start()
    {
        mainCamera = Camera.main;
        // Prefab listesi boþ deðilse ilk önizlemeyi oluþtur
        if (prefabs != null && prefabs.Count > 0)
        {
            UpdatePreviewObject();
        }
        UpdateActiveMarkerVisuals(); // Mod'a göre doðru iþaretçiyi aktif et
    }

    void Update()
    {
        HandleMousePosition();
        HandleInput();
        UpdateActiveMarkerVisuals(); // Her frame mod'a göre iþaretçileri güncelle
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
            // UI üzerinde olup olmadýðýmýzý kontrol et, eðer UI üzerindeysek iþlem yapma
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
        else // Ýnþa modunda
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
                // URP uyumlu kýrmýzý transparan materyal oluþtur
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

        // Ýþaretçiyi sadece fare zemin üzerindeyken göster
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
            Debug.Log(prefabs[activePrefabIndex].name + " objesi " + targetPosition + " pozisyonuna yerleþtirildi.");
        }
        else
        {
            Debug.LogWarning("Bu pozisyon (" + targetPosition + ") zaten dolu! Yerleþtirme yapýlamadý.");
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
                Debug.Log("Ýnþa moduna geçildi.");
            }
            activePrefabIndex = newIndex;
            UpdatePreviewObject();
            Debug.Log(prefabs[activePrefabIndex].name + " seçildi.");
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
            // URP uyumlu yeþil transparan materyal oluþtur
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
    /// URP için belirli bir renkte transparan bir materyal oluþturur.
    /// </summary>
    /// <param name="color">Materyalin rengi (alpha deðeri dahil).</param>
    /// <returns>URP uyumlu transparan materyal.</returns>
    private Material CreateTransparentURPMaterial(Color color)
    {
        // URP'nin varsayýlan Lit shader'ýný bul
        Shader urpLitShader = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLitShader == null)
        {
            Debug.LogError("URP/Lit shader bulunamadý. Lütfen projenizde URP paketinin doðru þekilde kurulu olduðundan emin olun.");
            return null;
        }

        Material material = new Material(urpLitShader);

        // URP Lit Shader'da transparanlýk için materyal ayarlarý
        material.SetFloat("_Surface", 1); // Surface Type'ý Transparent yap (0=Opaque, 1=Transparent)
        material.SetFloat("_Blend", 0);   // Blend Mode'u Alpha yap (0=Alpha, 1=Premultiply, 2=Additive, 3=Multiply)
        material.SetFloat("_ZWrite", 0);  // Derinlik yazmayý kapat
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON"); // Gerekirse bunu EnableKeyword("_ALPHABLEND_ON") ile deðiþtirin
        material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);

        // Ana rengi ayarla
        material.SetColor("_BaseColor", color);

        return material;
    }
}
