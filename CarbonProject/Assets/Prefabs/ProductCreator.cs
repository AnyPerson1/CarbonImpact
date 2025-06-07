using UnityEngine;
using System.Collections.Generic;

public class ProductCreator : MonoBehaviour
{
    [Header("UI Kurulumu")]
    [Tooltip("UI buton prefab�. Bu prefab �zerinde BuildingButtonView script'i olmal�.")]
    public GameObject uiButtonPrefab;
    [Tooltip("Olu�turulan UI butonlar�n�n eklenece�i parent Transform.")]
    public Transform buttonParent;
    [Tooltip("Se�im yap�ld�ktan sonra kapat�lacak olan ana panel.")]
    public GameObject selectionPanel;

    [Header("�n�a Edilecek Binalar")]
    [Tooltip("Butonlar� olu�turulacak bina prefablar�n�n (Building.cs i�eren) listesi.")]
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
            Debug.LogError("ProductCreator'da Button Parent veya UI Button Prefab atanmam��!", this);
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
                Debug.LogError("UI Button Prefab �zerinde 'BuildingButtonView' script'i bulunamad�!", buttonGO);
                Destroy(buttonGO);
            }
        }
        CloseSelectionPanel();
    }

    /// <summary>
    /// Bina se�imi yap�ld���nda paneli kapat�r. BuildingButtonView taraf�ndan �a�r�l�r.
    /// </summary>
    public void CloseSelectionPanel()
    {
        if (selectionPanel != null)
        {
            selectionPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Se�im panelini a�mak i�in kullan�labilir (�rne�in ba�ka bir butonla).
    /// </summary>
    public void OpenSelectionPanel()
    {
        selectionPanel.SetActive(!isOpen);
        isOpen = !isOpen;
    }
}