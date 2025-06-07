using UnityEngine;
using System.Collections.Generic;

public class ProductCreator : MonoBehaviour
{
    [Header("UI Kurulumu")]
    [Tooltip("UI buton prefabý. Bu prefab üzerinde BuildingButtonView script'i olmalý.")]
    public GameObject uiButtonPrefab;
    [Tooltip("Oluþturulan UI butonlarýnýn ekleneceði parent Transform.")]
    public Transform buttonParent;
    [Tooltip("Seçim yapýldýktan sonra kapatýlacak olan ana panel.")]
    public GameObject selectionPanel;

    [Header("Ýnþa Edilecek Binalar")]
    [Tooltip("Butonlarý oluþturulacak bina prefablarýnýn (Building.cs içeren) listesi.")]
    public List<GameObject> buildingPrefabs;

    bool isOpen = false;

    void Start()
    {
        CreateBuildingButtons();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            OpenSelectionPanel();
        }
    }

    public void CreateBuildingButtons()
    {
        if (buttonParent == null || uiButtonPrefab == null)
        {
            Debug.LogError("ProductCreator'da Button Parent veya UI Button Prefab atanmamýþ!", this);
            return;
        }

        foreach (Transform child in buttonParent)
        {
            Destroy(child.gameObject);
        }


        foreach (GameObject prefab in buildingPrefabs)
        {
            if (prefab == null) continue;

            GameObject buttonGO = Instantiate(uiButtonPrefab, buttonParent);

            BuildingButtonView buttonView = buttonGO.GetComponent<BuildingButtonView>();
            if (buttonView != null)
            {
                buttonView.Setup(prefab, this);
            }
            else
            {
                Debug.LogError("UI Button Prefab üzerinde 'BuildingButtonView' script'i bulunamadý!", buttonGO);
                Destroy(buttonGO);
            }
        }
        CloseSelectionPanel();
    }

    /// <summary>
    /// Bina seçimi yapýldýðýnda paneli kapatýr. BuildingButtonView tarafýndan çaðrýlýr.
    /// </summary>
    public void CloseSelectionPanel()
    {
        if (selectionPanel != null)
        {
            selectionPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Seçim panelini açmak için kullanýlabilir (örneðin baþka bir butonla).
    /// </summary>
    public void OpenSelectionPanel()
    {
        selectionPanel.SetActive(!isOpen);
        isOpen = !isOpen;
    }
}