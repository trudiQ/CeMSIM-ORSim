using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public GameObject Tools, Camera;
    public float CameraSpeed = 2;


    private SelectTool selecttool;
    private float hori, vert;
    private float x, y;
    private float moveSpeed = 0.005f;

    private void Start() //references SelectTool component for Enable status
    {
        selecttool = Tools.GetComponent<SelectTool>();

    }
    void Update() //Updates directional input of object in every frame
    {
        hori = Input.GetAxis("Horizontal");
        vert = Input.GetAxis("Vertical");
        x = Input.GetAxis("Mouse X");
        y = Input.GetAxis("Mouse Y");
    }

    void FixedUpdate()
    {
        CheckUse(); //Checks status of enable bool in every frame
    }

    void CheckUse() //Camera moves if no tools are selected
    {
        if (selecttool.Enable == false) 
        {
            MoveCamera();
        }
    }

    void MoveCamera()
    {
        //Uses directional input variables above to translate position of camera
        if (hori != 0 || vert != 0)
        {
            transform.position += moveSpeed * new Vector3(hori, 0, vert);
        }
        //If mouse right-click held, camera will rotate in the desired direction
        if (Input.GetMouseButton(1))
        {
            transform.RotateAround(Camera.transform.position, Vector3.up, x * CameraSpeed);
            transform.RotateAround(Camera.transform.position, Vector3.left, y * CameraSpeed);
        }
        /*
        //If R is pressed, camera resets to original position
        if (Input.GetKey(KeyCode.R))
        {
            Camera.transform.position = new Vector3(-1.6f, .87f, 2.1f);
            Camera.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        */
    }
}
