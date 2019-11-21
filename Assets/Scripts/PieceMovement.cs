using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PieceMovement : MonoBehaviour
{
    public RectTransform panel;
    
    
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(Screen.currentResolution.height);

    }

    // Update is called once per frame
    void Update()
    {
        int count = transform.childCount;
        for (int i = 0; i < count; i++)
        {
            Transform child = transform.GetChild(i);
            Debug.Log(child.name);
            RectTransform rectT = child.GetComponent<RectTransform>();

            
            // ...
        }
    }
}
