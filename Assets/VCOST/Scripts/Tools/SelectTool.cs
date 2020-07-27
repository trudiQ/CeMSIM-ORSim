using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices;
using System.Security.Policy;
using UnityEditor;
using UnityEngine;

public class SelectTool : MonoBehaviour
{
    public bool Enable;

    void Start()
    {
        print("Select a tool; Main Camera enabled.");
    }
    void FixedUpdate()
    {
        RunChildren(); //checks if linear stapler component is selected in each frame
    }

    public void ClickAndCount()
    {
        //Sets enable to true and prints name of selected tool when mouse left-clicked
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 1000.0f))
            {
                if (hit.transform && hit.transform.gameObject.GetComponent<Tool>())
                {
                    hit.transform.gameObject.GetComponent<Tool>().Enable = true;
                    print(hit.transform.gameObject.name + " selected; Main Camera disabled.");
                    Enable = true;
                }
            }
        }
        //Sets enable to false, prints name of deselected tool when mouse right-clicked, gives camera option
        else if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 1000.0f))
            {
                if (hit.transform && hit.transform.gameObject.GetComponent<Tool>())
                {
                    hit.transform.gameObject.GetComponent<Tool>().Enable = false;
                    print(hit.transform.gameObject.name + " deselected; Press 'Esc' to enable Main Camera only.");
                    Enable = false;
                }
            }
        }
        //If escape button is pressed, all tool movement is disabled and camera is enabled
        else if (Input.GetKey(KeyCode.Escape))
        {
            Enable = false;
            print("No selections; Main Camera enabled.");
            foreach (Transform child in this.transform)
            {
                child.GetComponent<Tool>().Enable = false;
            }
        }
    }

    public void RunChildren() //Enables consecutive selection of linear stapler components
    { 
        Tools[] children = new Tools[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            ClickAndCount();
        }
    }
}
