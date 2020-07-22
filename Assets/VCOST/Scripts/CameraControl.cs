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
    private float moveSpeed = 0.5f;

    private void Start()
    {
        selecttool = Tools.GetComponent<SelectTool>();

    }
    void Update()
    {
        hori = Input.GetAxis("Horizontal");
        vert = Input.GetAxis("Vertical");
        x = Input.GetAxis("Mouse X");
        y = Input.GetAxis("Mouse Y");
    }

    void FixedUpdate()
    {
        CheckUse();
    }

    void CheckUse()
    {
        if (selecttool.Enable == false) 
        {
            MoveCamera();
        }
    }

    void MoveCamera()
    {
        if (hori != 0 || vert != 0)
        {
            transform.position += moveSpeed * new Vector3(-hori, 0, -vert);
        }
        if (Input.GetMouseButton(1))
        {
            transform.RotateAround(Camera.transform.position, Vector3.up, x * CameraSpeed);
            transform.RotateAround(Camera.transform.position, Vector3.left, y * -CameraSpeed);

        }
        if (Input.GetKey(KeyCode.R))
        {
            Camera.transform.position = new Vector3(-65, 115, 115);
            Camera.transform.rotation = Quaternion.Euler(30, -180, 0);
        }
    }
}
