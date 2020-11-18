using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tool : MonoBehaviour //inherits SelectTool class
{
    public float speed;
    public float AngularSpeed;
    public bool Enable;

    private Vector3 movement;
    private float x, y, z;
    private float angle = 0;
    private Rigidbody m_tool;

    void Start()
    {
        m_tool = this.GetComponent<Rigidbody>(); //references rigidbody component of selected tool
    }

    void Update()
    {
        Enabler(); //checks for selected tools in each frame
    }

    void FixedUpdate()
    {
        movePlayer(movement); //calls velocity function
    }

    void movePlayer(Vector3 direction)
    {
        m_tool.velocity = direction * speed; //defines velocity function
    }

    public void TranslationalInput()
    {
        //Sets WASD as the directional keys for moving an object forward, back, left, and right
        x = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        z = Input.GetAxis("Vertical") * speed * Time.deltaTime;
        angle = AngularSpeed * Time.deltaTime;

        //Sets Q & E as the directional keys for moving an object up and down
        if (Input.GetKey(KeyCode.Q))
        {
            y = speed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.E))
        {
            y = -speed * Time.deltaTime;
        }
        else
        {
            y = 0;
        }

        //Directional key variable updates the parameters of an object in 3-dimensions
        movement = new Vector3(x, y, z);
    }

    public void RotationalInput()
    {
        //y-axis rotation
        if (Input.GetKey(KeyCode.L))
        {
            transform.Rotate(Vector3.down * angle, Space.Self);
        }
        else if (Input.GetKey(KeyCode.J))
        {
            transform.Rotate(Vector3.up * angle, Space.Self);
        }

        //z-axis rotation
        if (Input.GetKey(KeyCode.U))
        {
            transform.Rotate(Vector3.back * angle, Space.Self);
        }
        else if (Input.GetKey(KeyCode.O))
        {
            transform.Rotate(Vector3.forward * angle, Space.Self);
        }

        //x-axis rotation
        if (Input.GetKey(KeyCode.K))
        {
            transform.Rotate(Vector3.right * angle, Space.Self);
        }
        else if (Input.GetKey(KeyCode.I))
        {
            transform.Rotate(Vector3.left * angle, Space.Self);
        }
    }

    public void Enabler() //Allows translation and rotation of selected tool
    {
        if (Enable == true)
        {
            TranslationalInput();
            RotationalInput();
        }
        else
        {
            
        }
    }
}
