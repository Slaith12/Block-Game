using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blockthrow.Enironment
{
    public class Floor : MonoBehaviour
    {
        public Vector2 topLeftCorner { get { return new Vector2(transform.position.x - (transform.lossyScale.x / 2), transform.position.y + (transform.lossyScale.y / 2)); } }
        public Vector2 topRightCorner { get { return new Vector2(transform.position.x + (transform.lossyScale.x / 2), transform.position.y + (transform.lossyScale.y / 2)); } }
        public Vector2 bottomLeftCorner { get { return new Vector2(transform.position.x - (transform.lossyScale.x / 2), transform.position.y - (transform.lossyScale.y / 2)); } }
        public Vector2 bottomRightCorner { get { return new Vector2(transform.position.x + (transform.lossyScale.x / 2), transform.position.y - (transform.lossyScale.y / 2)); } }

        Collider2D leftCorner;
        Collider2D rightCorner;
        /// <summary>
        /// Should only be checked when player is hanging off THIS floor.
        /// </summary>
        public bool touchingLeftCorner;
        [SerializeField] LayerMask chainLayer;
        // Start is called before the first frame update
        void Start()
        {
            Collider2D[] colliders = GetComponents<Collider2D>();
            leftCorner = colliders[1];
            rightCorner = colliders[2];
            leftCorner.offset = new Vector2(-0.5f, 0.5f);
            rightCorner.offset = new Vector2(0.5f, 0.5f);
        }

        // Update is called once per frame
        void Update()
        {

        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (((1 << collision.gameObject.layer) & chainLayer.value) == 0) //checking if the object has chain layer
                return;
            touchingLeftCorner = collision.IsTouching(leftCorner);
            Debug.Log("Touching corner " + touchingLeftCorner);
        }
    }
}
