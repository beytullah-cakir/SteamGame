using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI hookRequiredItems;
    public TextMeshProUGUI createHookItems;
    public List<GameObject> panels;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public void TogglePanel(GameObject panel)
    {
        foreach (var p in panels)
        {
            p.SetActive(false);
        }
        panel.SetActive(true);
    }
}
