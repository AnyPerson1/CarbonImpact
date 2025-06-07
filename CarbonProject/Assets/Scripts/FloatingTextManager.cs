using UnityEngine;

public class FloatingTextManager : MonoBehaviour
{
    public static FloatingTextManager Instance { get; private set; }

    [Header("Kurulum")]
    [Tooltip("Hareketli yazý için kullanýlacak prefab.")]
    public GameObject floatingTextPrefab;

    [Tooltip("Yazýlarýn oluþturulacaðý ana UI Canvas.")]
    public Canvas parentCanvas;

    [Tooltip("Yazýnýn, binanýn merkezinden ne kadar yukarýda baþlayacaðýný belirler.")]
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
        // Eðer Canvas inspector'dan atanmadýysa, sahnede bir tane bulmayý dene.
        if (parentCanvas == null)
        {
            //parentCanvas = FindObjectOfType<Canvas>();
            if (parentCanvas == null)
            {
                Debug.LogError("FloatingTextManager: Sahnede bir Canvas bulunamadý!");
            }
        }
    }

    /// <summary>
    /// Belirtilen hedef üzerinde hareketli bir yazý oluþturur.
    /// </summary>
    /// <param name="text">Görüntülenecek metin</param>
    /// <param name="color">Metin rengi</param>
    /// <param name="target">Yazýnýn üzerinde belireceði Transform</param>
    public void ShowFloatingText(string text, Color color, Transform target)
    {
        if (floatingTextPrefab == null || parentCanvas == null)
        {
            Debug.LogError("FloatingTextManager düzgün ayarlanmamýþ! (Prefab veya Canvas eksik)");
            return;
        }

        // Prefab'ý oluþtur
        GameObject textGO = Instantiate(floatingTextPrefab, parentCanvas.transform);

        // FloatingText script'ini al
        FloatingText floatingText = textGO.GetComponent<FloatingText>();
        if (floatingText == null)
        {
            Debug.LogError("Prefab üzerinde 'FloatingText' scripti bulunamadý!");
            Destroy(textGO);
            return;
        }

        // Yazýnýn özelliklerini ayarla
        floatingText.SetText(text);
        floatingText.SetColor(color);

        // Yazýnýn pozisyonunu ayarla
        // Hedef objenin dünya pozisyonunu ekran pozisyonuna çeviriyoruz.
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(target.position + textOffset);
        textGO.transform.position = screenPosition;
    }

    public void ShowFloatingText(string text, Color color, Transform target, Vector3 offset)
    {
        if (floatingTextPrefab == null || parentCanvas == null)
        {
            Debug.LogError("FloatingTextManager düzgün ayarlanmamýþ! (Prefab veya Canvas eksik)");
            return;
        }

        // Prefab'ý oluþtur
        GameObject textGO = Instantiate(floatingTextPrefab, parentCanvas.transform);

        // FloatingText script'ini al
        FloatingText floatingText = textGO.GetComponent<FloatingText>();
        if (floatingText == null)
        {
            Debug.LogError("Prefab üzerinde 'FloatingText' scripti bulunamadý!");
            Destroy(textGO);
            return;
        }

        // Yazýnýn özelliklerini ayarla
        floatingText.SetText(text);
        floatingText.SetColor(color);

        // Yazýnýn pozisyonunu ayarla
        // Hedef objenin dünya pozisyonunu ekran pozisyonuna çeviriyoruz.
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(target.position + offset);
        textGO.transform.position = screenPosition;
    }
}
