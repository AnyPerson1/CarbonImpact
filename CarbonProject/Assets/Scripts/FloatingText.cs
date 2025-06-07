using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    [Header("Animasyon Ayarlarý")]
    public float moveSpeed = 50f;     // Yazýnýn yukarý hareket hýzý (pixel/saniye)
    public float fadeOutDuration = 1f; // Yazýnýn kaybolma süresi

    private TextMeshProUGUI textMesh;
    private float fadeTimer;
    private Color startColor;

    void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        if (textMesh == null)
        {
            Debug.LogError("Bu objede TextMeshProUGUI component'i bulunamadý!");
            this.enabled = false;
        }
    }

    void Start()
    {
        // Baþlangýç ayarlarý
        startColor = textMesh.color;
        fadeTimer = fadeOutDuration;
        // Yazýyý ömrü dolunca yok et
        Destroy(gameObject, fadeOutDuration);
    }

    void Update()
    {
        // Yukarý doðru hareket ettir
        transform.position += new Vector3(0, moveSpeed * Time.deltaTime, 0);

        // Zamanla rengini soldur (alpha deðerini düþür)
        fadeTimer -= Time.deltaTime;
        if (fadeTimer <= 0)
        {
            // Tamamen þeffaf yap
            textMesh.alpha = 0;
        }
        else
        {
            // Kalan süreye göre alpha'yý ayarla
            float alpha = fadeTimer / fadeOutDuration;
            textMesh.alpha = alpha;
        }
    }

    // Bu metodlar Manager tarafýndan çaðrýlýr.
    public void SetText(string text)
    {
        if (textMesh != null) textMesh.text = text;
    }

    public void SetColor(Color color)
    {
        if (textMesh != null) textMesh.color = color;
    }
}
