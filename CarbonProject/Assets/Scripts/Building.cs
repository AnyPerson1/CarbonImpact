using System.Resources;
using UnityEngine;

public class Building : MonoBehaviour
{
    [Header("Bina Ýstatistikleri")]
    public float carbonImpact;
    public float value;
    public int income;       
    public float interval;    
    private float timer;
    private GridBuildingSystem controller;
    public bool isBuilded = false;

    void Start()
    {
        controller = GridBuildingSystem.Instance;
        if (controller == null)
        {
            Debug.LogError("Sahneye bir 'ResourceManager' objesi eklemelisiniz!", this);
            this.enabled = false; 
            return;
        }

        timer = Random.Range(0, interval);
    }

    void Update()
    {
        if (!isBuilded)
        {
            return;
        }
        if (interval <= 0)
        {
            return;
        }

        timer += Time.deltaTime;

        if (timer >= interval)
        {
            // Kaynaklarý üret
            ProduceResources();
            timer -= interval;
        }
    }

    private void ProduceResources()
    {
        controller.money += income;
        controller.totalEmission += carbonImpact;
        FloatingTextManager.Instance.ShowFloatingText("+"+income+"$",Color.green, transform, new Vector3(0,1.5f,0));
        FloatingTextManager.Instance.ShowFloatingText("+"+carbonImpact,Color.black, transform, new Vector3(0, 2.5f, 0));
    }
}