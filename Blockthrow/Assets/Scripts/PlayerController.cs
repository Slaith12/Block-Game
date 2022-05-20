using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Blockthrow
{
    public enum State { Walking, Flying, Hanging }
    //State Descriptions (some parts of each description may not be implemented yet. currently describes what's planned)
    //Walking: Standard ground movement, only impeded by chain and block. Occurs when player is grounded, block can be grounded or airborne
    //Flying: Standard air movement, . Occurs when both player and block are airborne
    //Hanging: Hanging off an edge while block is on land, only moves up and down (can drag block towards edge). Can transition to walking (moving up to land), swinging (player moves sideways), or flying (player drags block off ledge). Can be transitioned from swinging (player comes to a standstill), but NOT walking or flying (transitions to swinging first)
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] BlockManager block;
        [SerializeField] Chain chain;
        public LayerMask enviroLayer;
        public LayerMask blockLayer;
        public State state;

        [SerializeField] float killDepth = -6;

        [Header("Walking Properties")]
        [SerializeField] float walkSpeed;
        float currentSpeed;
        [SerializeField] float accelTime;
        [SerializeField] float deccelTime;
        [SerializeField] float pullStrength;
        [SerializeField] float throwStrength;
        bool touchingBlock
        {
            get
            {
                return Mathf.Abs(transform.position.x - block.transform.position.x) <= 1.25f && Mathf.Abs(transform.position.y - block.transform.position.y) <= 1.25f;
            }
        }
        bool holdingBlock;

        [Header("Hanging properties")]
        [SerializeField] float tugStrength;
        [SerializeField] float swingStrength;
        [SerializeField] float hangMoveSpeed;
        [SerializeField] float hangHeightOffset;

        [Header("Flying properties")]
        [SerializeField] float chainPullForce = 5000;
        [SerializeField] float followStrength = 1;
        [Range(0,1)]
        [SerializeField] float forceMix = 0.5f;
        [Range(0,1)]
        [SerializeField] float distanceCutoff = 0.5f;
        float pullCooldown;

        //for general use
        bool facingLeft; //used mainly for animations
        int contacts; //used to determine if player is grounded
        public bool grounded { get { return contacts != 0; } }
        public Vector2 chainOffset;

        [HideInInspector] public new Rigidbody2D rigidbody;
        new SpriteRenderer renderer;

        public void Init()
        {
            rigidbody = GetComponent<Rigidbody2D>();
            renderer = GetComponent<SpriteRenderer>();
            currentSpeed = 0f;
            state = State.Walking;
        }

        // Update is called once per frame
        void Update()
        {
            if(transform.position.y < killDepth)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            switch(state)
            {
                case State.Walking:
                    GroundControl();
                    break;
                case State.Flying:
                    FlyControl();
                    break;
                case State.Hanging:
                    HangControl();
                    break;
            }
        }

        private void FixedUpdate()
        {
            switch(state)
            {
                case State.Walking:
                    GroundPhysics();
                    break;
                case State.Flying:
                    FlyPhysics();
                    break;
                case State.Hanging:
                    HangPhysics();
                    break;
            }
        }

        void GroundControl()
        {
            renderer.color = holdingBlock ? Color.green : Color.white;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                transform.position = new Vector3(block.transform.position.x + (transform.position.x > block.transform.position.x ? -1.1f : 1.1f), transform.position.y);
            }
            if (Input.GetMouseButton(0) && holdingBlock)
            {
                Aim();
            }
            else if(Input.GetMouseButtonUp(0) && holdingBlock)
            {
                Throw();
            }
            if(Input.GetMouseButtonDown(1))
            {
                if (holdingBlock)
                {
                    holdingBlock = false;
                    block.transform.position = new Vector3(transform.position.x + (facingLeft ? -1.1f : 1.1f), transform.position.y - 0.5f);
                    block.gameObject.SetActive(true);
                    chain.gameObject.SetActive(true);
                }
                else if(touchingBlock)
                {
                    PickUpBlock();
                }
                else
                {
                    DragBlock();
                }
            }
        }

        void GroundPhysics()
        {
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            {
                facingLeft = true;
                currentSpeed -= walkSpeed * Time.fixedDeltaTime / accelTime;
                currentSpeed = Mathf.Max(-walkSpeed, currentSpeed);
            }
            else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                facingLeft = false;
                {
                    currentSpeed += walkSpeed * Time.fixedDeltaTime / accelTime;
                    currentSpeed = Mathf.Min(walkSpeed, currentSpeed);
                }
            }
            else
            {
                if (currentSpeed > 0)
                {
                    currentSpeed -= walkSpeed * Time.fixedDeltaTime / deccelTime;
                    currentSpeed = Mathf.Max(0, currentSpeed);
                }
                else if (currentSpeed < 0)
                {
                    currentSpeed += walkSpeed * Time.fixedDeltaTime / deccelTime;
                    currentSpeed = Mathf.Min(0, currentSpeed);
                }
            }
            rigidbody.velocity = new Vector2(currentSpeed, rigidbody.velocity.y);
            if(!grounded) //if the player is airborne
            {
                if(block.grounded)
                {
                    Debug.Log("Player hanging, ending walk");
                    state = State.Hanging;
                }
                else
                {
                    Debug.Log("Player flying, ending walk");
                    StartFly();
                    state = State.Flying;
                }
            }
        }

        void FlyControl()
        {
            renderer.color = Color.red;

            pullCooldown -= Time.deltaTime;
            if(Input.GetMouseButtonDown(0) && pullCooldown <= 0f)
            {
                Vector2 mouseAngle = ((Vector2)(Camera.main.ScreenToWorldPoint(Input.mousePosition) - block.transform.position)).normalized;
                block.rigidbody.AddForce(mouseAngle * chainPullForce, ForceMode2D.Impulse);
                pullCooldown = 1f;
            }
        }

        void FlyPhysics()
        {
            //Vector2 distance = ((Vector2)transform.position + chainOffset) - ((Vector2)block.transform.position + block.chainOffset);
            //Vector2 force = -distance.normalized * (Mathf.Max(0, distance.magnitude - Chain.chainLength)) * chainPullForce;
            //Debug.Log(force);
            //rigidbody.AddForce(force, ForceMode2D.Force);

            //if (distance.magnitude > Chain.chainLength)
            //{
            //    transform.position = block.transform.position - (Vector3)(1.01f * Chain.chainLength * block.rigidbody.velocity.normalized);
            //    rigidbody.velocity = block.rigidbody.velocity;
            //    transform.position = block.transform.position - (Vector3)(block.rigidbody.velocity.normalized * Chain.chainLength);
            //    rigidbody.velocity = 0.9f * block.rigidbody.velocity;
            //    Debug.Log("Block pos: " + block.transform.position + "Velocity offset: " + (block.rigidbody.velocity.normalized * Chain.chainLength));
            //}

            if (grounded)
            {
                Debug.Log("Player grounded, ending flight");
                EndFly();
                state = State.Walking;
            }
            else if (block.grounded)
            {
                Debug.Log("Block grounded, ending flight");
                EndFly();
                state = State.Hanging;
            }
            else
            {
                Vector2 playerPos = (Vector2)transform.position + chainOffset;
                Vector2 blockPos = (Vector2)block.transform.position + block.chainOffset;
                float dist = (playerPos - blockPos).magnitude;
                if(dist/Chain.chainLength > 0.5f)
                {
                    Vector2 sharedVel = block.rigidbody.velocity * dist / Chain.chainLength * forceMix;
                    Vector2 trailVel = (blockPos - playerPos) * followStrength * (1 - forceMix);
                    rigidbody.velocity = sharedVel + trailVel;
                }
                RaycastHit2D hit = Physics2D.Raycast(playerPos, blockPos - playerPos, (blockPos - playerPos).magnitude, enviroLayer);
                if(hit)
                {
                    EndFly();
                    state = State.Hanging;
                    Debug.Log("Obstacle found, ending flight " + hit.transform.name);
                }
            }
        }
        
        void HangControl()
        {
            renderer.color = holdingBlock ? Color.green : Color.blue;
            facingLeft = block.transform.position.x < transform.position.x;
            if(Input.GetMouseButtonDown(1))
            {
                DragBlock();
            }
            if (transform.position.y + hangHeightOffset >= block.transform.position.y)
            {
                if(Input.GetKeyDown(KeyCode.Space))
                {
                    Debug.Log("Get on ledge");
                    transform.position = GetCorner();
                    chain.reducedLength = 0;
                    state = State.Walking;
                }
            }
            Vector2 GetCorner()
            {
                if(block.ground.Count == 0)
                {
                    return transform.position;
                }
                Floor floor = block.ground[0];
                if (!floor.touchingLeftCorner)
                {
                    return floor.topRightCorner + new Vector2(-1f, 1f);
                }
                else
                {
                    return floor.topLeftCorner + new Vector2(1f, 1f);
                }
            }
        }

        void HangPhysics()
        {
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            {
                chain.reducedLength += Time.fixedDeltaTime * hangMoveSpeed;
            }
            else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            {
                chain.reducedLength -= Time.fixedDeltaTime * hangMoveSpeed;
                if (chain.reducedLength == 0)
                {
                    block.rigidbody.AddForce(GetSpeed(), ForceMode2D.Impulse);
                }
            }
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            {
                rigidbody.AddForce(Vector2.left * swingStrength, ForceMode2D.Force);
            }
            else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                rigidbody.AddForce(Vector2.right * swingStrength, ForceMode2D.Force);
            }
            if (grounded)
            {
                Debug.Log("Player grounded, ending hang");
                state = State.Walking;
            }
            if (!block.grounded)
            {
                Vector2 playerPos = (Vector2)transform.position + chainOffset;
                Vector2 blockPos = (Vector2)block.transform.position + block.chainOffset;
                RaycastHit2D hit = Physics2D.Raycast(playerPos, blockPos - playerPos, (blockPos - playerPos).magnitude, enviroLayer);
                if (!hit)
                {
                    StartFly();
                    state = State.Flying;
                    Debug.Log("Block falling, switching to flying");
                }
            }
            
            Vector2 GetSpeed()
            {
                Vector2 ret = transform.position.x > block.transform.position.x ? Vector2.right : Vector2.left;
                if (Mathf.Abs(block.transform.position.x - transform.position.x) < 0.5f)
                    ret *= 1.5f;
                ret *= tugStrength;
                return ret;
            }
        }

        public void DragBlock()
        {
            block.rigidbody.drag = 1;
            facingLeft = block.transform.position.x < transform.position.x;
            if(facingLeft)
            {
                block.rigidbody.velocity = new Vector2(pullStrength, block.rigidbody.velocity.y);
            }
            else
            {
                block.rigidbody.velocity = new Vector2(-pullStrength, block.rigidbody.velocity.y);
            }
        }

        void Aim()
        {

        }

        void Throw()
        {
            StartFly();
            Vector2 mouseAngle = ((Vector2)(Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position)).normalized;
            //if(mouseAngle.y >= 0.866f) //angle can't be greater than about 60 degrees
            //{
            //    mouseAngle = new Vector2((mouseAngle.x < 0 ? -0.5f : 0.5f), 0.866f).normalized;
            //}
            block.transform.position = transform.position + (Vector3)(mouseAngle * 1.5f);
            Debug.Log("Angle: " + mouseAngle + " Force: " + (mouseAngle * throwStrength));
            block.rigidbody.velocity = mouseAngle * throwStrength;
            chain.DistributeChains();
        }

        void StartFly()
        {
            holdingBlock = false;
            block.gameObject.SetActive(true);
            chain.gameObject.SetActive(true);
            chain.Throw();
            block.Fly();
        }

        public void EndFly()
        {
            chain.Land();
        }

        void PickUpBlock()
        {
            holdingBlock = true;
            block.gameObject.SetActive(false);
            chain.gameObject.SetActive(false);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (((1 << collision.gameObject.layer) & (enviroLayer | blockLayer)) == 0 || collision.isTrigger) //first part makes sure the collision is a floor (it looks like that because of how layer masks work), the second part makes sure it's not caused by entering the floor's corner triggers
            {
                return;
            }
            //Debug.Log("Enter " + collision.gameObject.name + " " + collision.isTrigger);
            if (contacts == 0)
            {
                rigidbody.drag = 8;
                Debug.Log("Player Landing");
            }
            contacts++;
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (((1 << collision.gameObject.layer) & (enviroLayer | blockLayer)) == 0 || collision.isTrigger) //first part makes sure the collision is a floor (it looks like that because of how layer masks work), the second part makes sure it's not caused by entering the floor's corner triggers
            {
                return;
            }
            //Debug.Log("Exit " + collision.gameObject.name + " " + collision.isTrigger);
            contacts--;
            if (contacts == 0)
            {
                rigidbody.drag = 0;
                Debug.Log("Player Airborne");
            }
            if(contacts < 0)
            {
                Debug.LogError("Number of contacts on player is negative");
            }
        }
    }
}