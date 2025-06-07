using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic; // List kullanmak için

public class BuildingButtonView : MonoBehaviour
{
    public Button buttonComponent;

    // Ýnþa edilecek asýl prefab (örn: 3D duvar modeli)
    private GameObject _associatedBuildingPrefab;
    private ProductCreator _creator;

    // Child sýrasýna göre text component'lerini tutacak liste
    private List<TextMeshProUGUI> textFields;

    void Awake()
    {
        // Bu objenin altýndaki tüm TextMeshPro component'lerini sýrasýyla bul ve listeye ekle.
        // Bu sayede Inspector'dan tek tek atama yapmana gerek kalmaz.
        textFields = new List<TextMeshProUGUI>(GetComponentsInChildren<TextMeshProUGUI>());

        if (buttonComponent == null)
        {
            buttonComponent = GetComponent<Button>();
        }
    }

    /// <summary>
    /// Butonu, inþa edilecek prefab'ýn özellikleriyle ayarlar.
    /// ProductCreator tarafýndan çaðrýlýr.
    /// </summary>
    public void Setup(GameObject buildingPrefab, ProductCreator creator)
    {
        _associatedBuildingPrefab = buildingPrefab;
        _creator = creator;

        if (_associatedBuildingPrefab == null)
        {
            Debug.LogError("Setup fonksiyonuna null bir prefab gönderildi!", this.gameObject);
            return;
        }

        // Prefab'dan Building component'ini al
        Building buildingData = _associatedBuildingPrefab.GetComponent<Building>();

        if (buildingData == null)
        {
            Debug.LogError("Prefab '" + _associatedBuildingPrefab.name + "' üzerinde 'Building' script'i bulunamadý!", this.gameObject);
            this.gameObject.SetActive(false); // Bu butonu devre dýþý býrak
            return;
        }

        // Text alanlarýný child sýrasýna göre doldur
        // Child 0: Productname
        if (textFields.Count > 0 && textFields[0] != null)
        {
            // Ýsim olarak prefab'ýn kendi adýný kullanýyoruz
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
            Debug.LogError("Týklama olayý için gerekli referanslar eksik!");
            return;
        }

        // GridBuildingSystem'e hangi prefab'ýn inþa edileceðini söyle
        GridBuildingSystem.Instance.PrefabToBuild = _associatedBuildingPrefab;

        // Paneli kapat
        _creator.CloseSelectionPanel();
    }
}