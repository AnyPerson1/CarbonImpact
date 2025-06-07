using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic; // List kullanmak i�in

public class BuildingButtonView : MonoBehaviour
{
    public Button buttonComponent;

    // �n�a edilecek as�l prefab (�rn: 3D duvar modeli)
    private GameObject _associatedBuildingPrefab;
    private ProductCreator _creator;

    // Child s�ras�na g�re text component'lerini tutacak liste
    private List<TextMeshProUGUI> textFields;

    void Awake()
    {
        // Bu objenin alt�ndaki t�m TextMeshPro component'lerini s�ras�yla bul ve listeye ekle.
        // Bu sayede Inspector'dan tek tek atama yapmana gerek kalmaz.
        textFields = new List<TextMeshProUGUI>(GetComponentsInChildren<TextMeshProUGUI>());

        if (buttonComponent == null)
        {
            buttonComponent = GetComponent<Button>();
        }
    }

    /// <summary>
    /// Butonu, in�a edilecek prefab'�n �zellikleriyle ayarlar.
    /// ProductCreator taraf�ndan �a�r�l�r.
    /// </summary>
    public void Setup(GameObject buildingPrefab, ProductCreator creator)
    {
        _associatedBuildingPrefab = buildingPrefab;
        _creator = creator;

        if (_associatedBuildingPrefab == null)
        {
            Debug.LogError("Setup fonksiyonuna null bir prefab g�nderildi!", this.gameObject);
            return;
        }

        // Prefab'dan Building component'ini al
        Building buildingData = _associatedBuildingPrefab.GetComponent<Building>();

        if (buildingData == null)
        {
            Debug.LogError("Prefab '" + _associatedBuildingPrefab.name + "' �zerinde 'Building' script'i bulunamad�!", this.gameObject);
            this.gameObject.SetActive(false); // Bu butonu devre d��� b�rak
            return;
        }

        // Text alanlar�n� child s�ras�na g�re doldur
        // Child 0: Productname
        if (textFields.Count > 0 && textFields[0] != null)
        {
            // �sim olarak prefab'�n kendi ad�n� kullan�yoruz
            textFields[0].text = _associatedBuildingPrefab.name.Replace("Prefab", "").Trim();
        }

        // Child 1: Carbon impact
        if (textFields.Count > 1 && textFields[1] != null)
        {
            textFields[1].text = "Carbon: " + buildingData.carbonImpact.ToString("F1");
        }

        // Child 2: Value
        if (textFields.Count > 2 && textFields[2] != null)
        {
            textFields[2].text = "Value: " + buildingData.value.ToString("N0");
        }

        // Child 3: Income
        if (textFields.Count > 3 && textFields[3] != null)
        {
            textFields[3].text = "Income: " + buildingData.income.ToString();
        }

        // Button click event'ini ayarla
        if (buttonComponent != null)
        {
            buttonComponent.onClick.RemoveAllListeners();
            buttonComponent.onClick.AddListener(OnButtonClick);
        }
    }

    private void OnButtonClick()
    {
        if (_associatedBuildingPrefab == null || _creator == null)
        {
            Debug.LogError("T�klama olay� i�in gerekli referanslar eksik!");
            return;
        }

        // GridBuildingSystem'e hangi prefab'�n in�a edilece�ini s�yle
        GridBuildingSystem.Instance.PrefabToBuild = _associatedBuildingPrefab;

        // Paneli kapat
        _creator.CloseSelectionPanel();
    }
}