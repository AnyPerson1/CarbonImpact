using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class GridBuildingSystem : MonoBehaviour
{
    public static GridBuildingSystem Instance { get; private set; }

    [Header("Grid Ayarlarý")]
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private LayerMask groundLayerMask;

    [Header("Prefab Ayarlarý")]
    [SerializeField] private GameObject deletionMarkerPrefab;

    [Header("Global Kaynaklar ve UI")]
    [SerializeField] private TextMeshProUGUI emissionText;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] public float totalEmission = 5f;
    [SerializeField] public float money = 1000;

    [Header("Oyun Durumu Ayarlarý")]
    [Tooltip("Bu emisyon deðerine ulaþýldýðýnda oyun kaybedilir.")]
    [SerializeField] private float emissionLoseThreshold = 500f;
    [Tooltip("Bu emisyon deðerine ulaþýldýðýnda oyun kazanýlýr.")]
    [SerializeField] private float emissionWinThreshold = 0f;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    public GameObject PrefabToBuild
    {
        get => _prefabToBuild;
        set
        {
            if (isGameOver) return; // Oyun bittiyse prefab seçimi yapma

            GameObject previousPrefab = _prefabToBuild;
            _prefabToBuild = value;

            if (_prefabToBuild != null)
            {
                bool wasDeleting = isDeleting;
                isDeleting = false;

                if (wasDeleting)
                {
                    Debug.Log("Ýnþa moduna geçildi (Silme modu deaktif edildi).");
                }
                else if (previousPrefab == null)
                {
                    Debug.Log("Ýnþa moduna geçildi.");
                }
                UpdatePreviewObject();
                Debug.Log(_prefabToBuild.name + " seçildi.");
            }
            else
            {
                if (previewObject != null)
                {
                    Destroy(previewObject);
                    previewObject = null;
                }
                if (previousPrefab != null)
                {
                    Debug.Log(previousPrefab.name + " seçimi kaldýrýldý. Önizleme temizlendi.");
                }
            }
        }
    }
    private GameObject _prefabToBuild;

    private Camera mainCamera;
    private GameObject previewObject;
    private GameObject deletionMarkerInstance;
    private GameObject currentActiveMarker;

    private Dictionary<Vector3, GameObject> placedObjects = new Dictionary<Vector3, GameObject>();
    private bool isDeleting = false;
    private bool isGameOver = false; // Oyunun bitip bitmediðini kontrol eder

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        mainCamera = Camera.main;
        // Baþlangýçta panelleri gizle
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        UpdateActiveMarkerVisuals();
    }

    void Update()
    {
        // UI güncellemeleri oyun bitse bile devam edebilir.
        emissionText.text = totalEmission.ToString("F0") + " / " + emissionLoseThreshold;
        moneyText.text = money.ToString("N0") + "$";

        // Oyun bittiyse, kontrolleri ve diðer iþlemleri atla.
        if (isGameOver)
        {
            return;
        }

        HandleMousePosition();
        HandleInput();
        UpdateActiveMarkerVisuals();
        CheckForEndGameConditions();
    }

    private void HandleInput()
    {
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
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (isDeleting)
            {
                DeleteObjectAtMousePosition();
            }
            else if (!isDeleting && _prefabToBuild != null && previewObject != null && previewObject.activeSelf)
            {
                PlaceObject();
            }
        }
    }

    private void PlaceObject()
    {
        if (_prefabToBuild == null || previewObject == null || !previewObject.activeSelf) return;

        Building buildingComponent = _prefabToBuild.GetComponent<Building>();
        if (buildingComponent.value > money)
        {
            Debug.LogWarning("Yeterli paran yok!");
            // Opsiyonel: Yetersiz bakiye için bir uyarý yazýsý gösterilebilir.
            // FloatingTextManager.Instance.ShowFloatingText("Yetersiz Bakiye!", Color.yellow, previewObject.transform);
            return;
        }

        money -= buildingComponent.value;

        Vector3 targetPosition = previewObject.transform.position;
        if (!placedObjects.ContainsKey(targetPosition))
        {
            GameObject newObject = Instantiate(_prefabToBuild, targetPosition, Quaternion.identity);
            placedObjects.Add(targetPosition, newObject);
            Debug.Log(_prefabToBuild.name + " objesi " + targetPosition + " pozisyonuna yerleþtirildi.");
            newObject.GetComponent<Building>().isBuilded = true;
            FloatingTextManager.Instance.ShowFloatingText("-" + buildingComponent.value + "$", Color.red, newObject.transform);
        }
        else
        {
            Debug.LogWarning("Bu pozisyon (" + targetPosition + ") zaten dolu! Yerleþtirme yapýlamadý.");
        }
    }

    // --- OYUN DURUMU KONTROL METODLARI ---

    private void CheckForEndGameConditions()
    {
        // Önce kaybetme durumunu kontrol et
        if (totalEmission >= emissionLoseThreshold)
        {
            TriggerGameOver(false); // Kaybettin
        }
        // Sonra kazanma durumunu kontrol et
        else if (totalEmission <= emissionWinThreshold)
        {
            TriggerGameOver(true); // Kazandýn
        }
    }

    private void TriggerGameOver(bool didWin)
    {
        isGameOver = true;

        // Aktif olan önizleme veya silme iþaretçisini gizle
        if (currentActiveMarker != null)
        {
            currentActiveMarker.SetActive(false);
        }

        if (didWin)
        {
            Debug.Log("OYUN KAZANILDI! Emisyon hedefine ulaþýldý.");
            if (winPanel != null) winPanel.SetActive(true);
        }
        else
        {
            Debug.Log("OYUN KAYBEDÝLDÝ! Emisyon limiti aþýldý.");
            if (losePanel != null) losePanel.SetActive(true);
        }
    }

    // --- MEVCUT DÝÐER METODLAR ---

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
        else
        {
            if (deletionMarkerInstance != null && deletionMarkerInstance.activeSelf)
            {
                deletionMarkerInstance.SetActive(false);
            }

            if (_prefabToBuild != null)
            {
                if (previewObject == null)
                {
                    UpdatePreviewObject();
                }

                if (previewObject != null && !previewObject.activeSelf)
                {
                    previewObject.SetActive(true);
                }
                currentActiveMarker = previewObject;
            }
            else
            {
                if (previewObject != null && previewObject.activeSelf)
                {
                    previewObject.SetActive(false);
                }
                currentActiveMarker = null;
            }
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
                Debug.Log("Bu pozisyonda (" + targetPosition + ") silinecek bir obje yok (Grid tabanlý kontrol).");
            }
        }
    }

    private void UpdatePreviewObject()
    {
        if (previewObject != null)
        {
            Destroy(previewObject);
            previewObject = null;
        }

        if (_prefabToBuild != null)
        {
            previewObject = Instantiate(_prefabToBuild);
            Collider previewCollider = previewObject.GetComponent<Collider>();
            if (previewCollider != null)
            {
                previewCollider.enabled = false;
            }

            Renderer[] renderers = previewObject.GetComponentsInChildren<Renderer>();
            Material previewMat = CreateTransparentURPMaterial(new Color(0.0f, 1.0f, 0.0f, 0.5f));

            if (previewMat != null)
            {
                foreach (Renderer renderer in renderers)
                {
                    renderer.material = previewMat;
                }
            }
        }
    }

    private Material CreateTransparentURPMaterial(Color color)
    {
        Shader urpLitShader = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLitShader == null)
        {
            Debug.LogError("URP/Lit shader bulunamadý. Lütfen projenizde URP paketinin doðru þekilde kurulu olduðundan emin olun.");
            return null;
        }

        Material material = new Material(urpLitShader);
        material.SetFloat("_Surface", 1); // Transparent
        material.SetFloat("_Blend", 0);   // Alpha
        material.SetFloat("_ZWrite", 0); // Disable ZWrite for proper transparency
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        material.SetColor("_BaseColor", color);
        return material;
    }
}
