using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class cancelbutton : MonoBehaviour
{
    public Button backgroundButton;
    public GameObject backgroundCanvas;
    public Button cancelButton;
    // Start is called before the first frame update

    private void Start()
    {
        cancelButton.onClick.AddListener(OnCancelButtonClicked);
    }
    public void OnCancelButtonClicked()
    {
        Debug.Log("cancel clicked.");
       
        backgroundCanvas.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
