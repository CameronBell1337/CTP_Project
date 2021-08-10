using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    public CharacterController col;
    public Material mat;
    void Start()
    {

        
        col = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (col.isGrounded)
        {

            mat.color = Color.red;
        }
        else
        {
            mat.color = Color.green;
        }
    }
}

    
