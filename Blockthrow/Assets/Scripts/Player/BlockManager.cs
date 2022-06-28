using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Blockthrow.Environment;

namespace Blockthrow
{
    public class BlockManager : MonoBehaviour
    {
        [SerializeField] PlayerController player;
        public Vector2 chainOffset;
        [SerializeField] LayerMask enviroLayer;

        [HideInInspector] public new Rigidbody2D rigidbody;
        /// <summary>
        /// Shows what floor(s) the block is sitting on, in order of when the block first landed on them.
        /// </summary>
        [HideInInspector] public List<Floor> ground;
        public bool grounded { get { return contacts != 0; } }
        int contacts;
        [HideInInspector] public bool flying;

        public void Awake()
        {
            rigidbody = GetComponent<Rigidbody2D>();
            contacts = 0;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if(flying && grounded)
            {
                Debug.Log("Ending Flight");
                EndFly();
            }
            else if(!flying && !grounded)
            {
                Debug.Log("Block Flying");
                Fly();
            }
        }

        public void Fly()
        {
            flying = true;
        }

        public void EndFly()
        {
            flying = false;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (((1 << collision.gameObject.layer) & enviroLayer.value) == 0) //this checks if the collided object is in a layer set by enviroLayer
            {
                return;
            }
            if(collision.isTrigger) //makes sure the collision isn't from a floor's corner check collider
            {
                return;
            }
            if (contacts == 0)
            {
                Debug.Log("Block Landing");
            }
            Debug.Log("Block entered " + collision.gameObject.name);
            contacts++;
            Floor newFloor = collision.GetComponent<Floor>();
            if (newFloor == null)
            {
                Debug.LogWarning("Block landed on non-floor. Name: " + collision.gameObject.name);
            }
            else
            {
                ground.Add(newFloor);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (((1 << collision.gameObject.layer) & enviroLayer.value) == 0) //this checks if the collided object is in a layer set by enviroLayer
            {
                return;
            }
            if (collision.isTrigger) //makes sure the collision isn't from a floor's corner check collider
            {
                return;
            }
            Debug.Log("Block left " + collision.gameObject.name);
            contacts--;
            if (contacts == 0)
            {
                Debug.Log("Block Airborne");
            }
            if (contacts < 0)
            {
                Debug.LogError("Number of contacts on block is negative");
            }
            Floor oldFloor = collision.GetComponent<Floor>();
            if (oldFloor != null)
            {
                bool correct = ground.Remove(oldFloor);
                if(!correct)
                {
                    Debug.LogWarning("Block left unregistered ground. Name: " + collision.gameObject.name);
                }
            }
        }
    }
}
