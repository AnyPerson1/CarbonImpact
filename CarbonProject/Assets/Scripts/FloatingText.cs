using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    [Header("Animasyon Ayarlar�")]
    public float moveSpeed = 50f;     // Yaz�n�n yukar� hareket h�z� (pixel/saniye)
    public float fadeOutDuration = 1f; // Yaz�n�n kaybolma s�resi

    private TextMeshProUGUI textMesh;
    private float fadeTimer;
    private Color startColor;

    void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        if (textMesh == null)
        {
            Debug.LogError("Bu objede TextMeshProUGUI component'i bulunamad�!");
            this.enabled = false;
        }
    }

    void Start()
    {
        // Ba�lang�� ayarlar�
        startColor = textMesh.color;
        fadeTimer = fadeOutDuration;
        // Yaz�y� �mr� dolunca yok et
        Destroy(gameObject, fadeOutDuration);
    }

    void Update()
    {
        // Yukar� do�ru hareket ettir
        transform.position += new Vector3(0, moveSpeed * Time.deltaTime, 0);

        // Zamanla rengini soldur (alpha de�erini d���r)
        fadeTimer -= Time.deltaTime;
        if (fadeTimer <= 0)
        {
            // Tamamen �effaf yap
            textMesh.alpha = 0;
        }
        else
        {
            // Kalan s�reye g�re alpha'y� ayarla
            float alpha = fadeTimer / fadeOutDuration;
            textMesh.alpha = alpha;
        }
    }

    // Bu metodlar Manager taraf�ndan �a�r�l�r.
    public void SetText(string text)
    {
        if (textMesh != null) textMesh.text = text;
    }

    public void SetColor(Color color)
    {
        if (textMesh != null) textMesh.color = color;
    }
}
