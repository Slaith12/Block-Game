using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit2D a = Physics2D.Raycast(transform.position, Vector2.down, 1f, LayerMask.GetMask("Enviroment"));
        Debug.Log(LayerMask.GetMask("Enviroment"));
        Debug.Log(a.transform.gameObject.name);
    }
}
