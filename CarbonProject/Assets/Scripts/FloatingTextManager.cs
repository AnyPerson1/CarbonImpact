using UnityEngine;

public class FloatingTextManager : MonoBehaviour
{
    public static FloatingTextManager Instance { get; private set; }

    [Header("Kurulum")]
    [Tooltip("Hareketli yaz� i�in kullan�lacak prefab.")]
    public GameObject floatingTextPrefab;

    [Tooltip("Yaz�lar�n olu�turulaca�� ana UI Canvas.")]
    public Canvas parentCanvas;

    [Tooltip("Yaz�n�n, binan�n merkezinden ne kadar yukar�da ba�layaca��n� belirler.")]
    public Vector3 textOffset = new Vector3(0, 1.5f, 0);

    private Camera mainCamera;

    void Awake()
    {
        // Singleton kurulumu
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
        // E�er Canvas inspector'dan atanmad�ysa, sahnede bir tane bulmay� dene.
        if (parentCanvas == null)
        {
            //parentCanvas = FindObjectOfType<Canvas>();
            if (parentCanvas == null)
            {
                Debug.LogError("FloatingTextManager: Sahnede bir Canvas bulunamad�!");
            }
        }
    }

    /// <summary>
    /// Belirtilen hedef �zerinde hareketli bir yaz� olu�turur.
    /// </summary>
    /// <param name="text">G�r�nt�lenecek metin</param>
    /// <param name="color">Metin rengi</param>
    /// <param name="target">Yaz�n�n �zerinde belirece�i Transform</param>
    public void ShowFloatingText(string text, Color color, Transform target)
    {
        if (floatingTextPrefab == null || parentCanvas == null)
        {
            Debug.LogError("FloatingTextManager d�zg�n ayarlanmam��! (Prefab veya Canvas eksik)");
            return;
        }

        // Prefab'� olu�tur
        GameObject textGO = Instantiate(floatingTextPrefab, parentCanvas.transform);

        // FloatingText script'ini al
        FloatingText floatingText = textGO.GetComponent<FloatingText>();
        if (floatingText == null)
        {
            Debug.LogError("Prefab �zerinde 'FloatingText' scripti bulunamad�!");
            Destroy(textGO);
            return;
        }

        // Yaz�n�n �zelliklerini ayarla
        floatingText.SetText(text);
        floatingText.SetColor(color);

        // Yaz�n�n pozisyonunu ayarla
        // Hedef objenin d�nya pozisyonunu ekran pozisyonuna �eviriyoruz.
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(target.position + textOffset);
        textGO.transform.position = screenPosition;
    }

    public void ShowFloatingText(string text, Color color, Transform target, Vector3 offset)
    {
        if (floatingTextPrefab == null || parentCanvas == null)
        {
            Debug.LogError("FloatingTextManager d�zg�n ayarlanmam��! (Prefab veya Canvas eksik)");
            return;
        }

        // Prefab'� olu�tur
        GameObject textGO = Instantiate(floatingTextPrefab, parentCanvas.transform);

        // FloatingText script'ini al
        FloatingText floatingText = textGO.GetComponent<FloatingText>();
        if (floatingText == null)
        {
            Debug.LogError("Prefab �zerinde 'FloatingText' scripti bulunamad�!");
            Destroy(textGO);
            return;
        }

        // Yaz�n�n �zelliklerini ayarla
        floatingText.SetText(text);
        floatingText.SetColor(color);

        // Yaz�n�n pozisyonunu ayarla
        // Hedef objenin d�nya pozisyonunu ekran pozisyonuna �eviriyoruz.
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(target.position + offset);
        textGO.transform.position = screenPosition;
    }
}
