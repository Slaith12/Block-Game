using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blockthrow
{
    public class CameraMovement : MonoBehaviour
    {
        [SerializeField] GameObject player;
        [SerializeField] Vector3 offset;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            transform.position = player.transform.position + offset;
        }
    }
}