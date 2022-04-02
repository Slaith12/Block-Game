using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blockthrow
{
    public class Chain : MonoBehaviour 
    {
        [SerializeField] GameObject singleChain;
        [SerializeField] BlockManager block;
        [SerializeField] PlayerController character;

        HingeJoint2D[] chains;
        HingeJoint2D characterJoint;
        DistanceJoint2D tether;

        bool thrown;

        Vector2 blockPos { get { return (Vector2)block.transform.position + block.chainOffset; } }
        Vector2 charPos { get { return (Vector2)character.transform.position + character.chainOffset; } }
        List<Vector2> corners; //chain will travel in the order: charPos -> corners[0] ...> corners[length-1] -> blockPos
        public float slack
        {   get
            {
                if(corners.Count == 0)
                {
                    return chainLength - (charPos - blockPos).magnitude;
                }
                float distance = (charPos - corners[0]).magnitude;
                for(int i = 0; i < corners.Count - 1; i++)
                {
                    distance += (corners[i] - corners[i + 1]).magnitude;
                }
                distance += (corners[corners.Count - 1] - blockPos).magnitude;
                return chainLength - distance;
            }
        }

        public float reducedLength //used in HangControl to allow the player to climb up or down the chain to get on the ledge.
        {
            get
            {
                return redLength;
            }
            set
            {
                redLength = value;
                if (redLength < 0)
                {
                    redLength = 0;
                }
                if(redLength > pieceLength*chains.Length)
                {
                    redLength = pieceLength * chains.Length; //done to make sure final chain never despawns
                }
                for(int i = 0; i < chains.Length; i++)
                {
                    if((chains.Length-i)*pieceLength < redLength)
                    {
                        chains[i].gameObject.SetActive(false);
                        chains[i].transform.position = charPos;
                    }
                    else
                    {
                        chains[i].gameObject.SetActive(true);
                    }
                }
                characterJoint.connectedBody = chains[(int)(CHAINQUANTITY - Mathf.Max(1, redLength / pieceLength))].attachedRigidbody; // x >  chains.Length - redLength/pieceLength
                //change chain length
                characterJoint.connectedAnchor = new Vector2(pieceLength / 2 - (redLength % pieceLength), 0); //pieceLength / 2 gives the right edge of the chain piece (since the center of the chain is at 0), 
                                                                                                              //and redLength % pieceLength gives the position in the current chain piece where the reduced length ends up
            }
        }

        private float redLength;

        const int CHAINQUANTITY = 11;
        static float pieceLength;

        public static float chainLength { get { return CHAINQUANTITY * pieceLength; } }
        
        public void Init()
        {
            characterJoint = character.GetComponent<HingeJoint2D>();
            chains = new HingeJoint2D[CHAINQUANTITY];
            chains[0] = Instantiate(singleChain, transform).GetComponent<HingeJoint2D>();
            chains[0].connectedBody = block.rigidbody;
            Debug.Log(chains[0].connectedBody);
            chains[0].gameObject.layer = 9; //chain layer
            for(int i = 1; i < CHAINQUANTITY; i++)
            {
                chains[i] = Instantiate(singleChain, transform).GetComponent<HingeJoint2D>();
                chains[i].connectedBody = chains[i - 1].attachedRigidbody;
                chains[i].gameObject.layer = 9;
            }
            characterJoint.connectedBody = chains[CHAINQUANTITY - 1].attachedRigidbody;
            DistributeChains();
            pieceLength = (chains[1].anchor - chains[1].connectedAnchor).magnitude;

            tether = character.GetComponent<DistanceJoint2D>();
            tether.distance = chainLength;
            tether.anchor = character.chainOffset;
            tether.connectedAnchor = block.chainOffset;
            tether.enabled = false;

            corners = new List<Vector2>();
            thrown = false;
        }

        private void OnEnable()
        {
            if (chains != null)
            {
                characterJoint.connectedBody = chains[CHAINQUANTITY - 1].attachedRigidbody;
                DistributeChains();
            }
        }

        public void DistributeChains()
        {
            for(int i = 0; i < CHAINQUANTITY; i++)
            {
                chains[i].transform.position = blockPos + ((charPos - blockPos) / CHAINQUANTITY) * i;
            }
        }
        
        private void FixedUpdate()
        {
            if(thrown)
            {
                chains[0].transform.position = blockPos;
                chains[chains.Length - 1].transform.position = charPos;
            }
        }

        public void Throw()
        {
            chains[0].enabled = false;
            characterJoint.enabled = false;
            tether.enabled = true;
            thrown = true;
        }

        public void Land()
        {
            chains[0].enabled = true;
            characterJoint.enabled = true;
            tether.enabled = false;
            thrown = false;
        }
    }
}
