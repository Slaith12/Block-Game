using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blockthrow.Chain
{
    public class ChainManager : MonoBehaviour
    {
        [SerializeField] ChainAttachment charAtt;
        [SerializeField] ChainAttachment blockAtt;

        float maxLength = 5;
        [Range(0, 1)]
        [SerializeField] float pullBalance = 0.5f;

        private void FixedUpdate()
        {
            Vector2 nextCharPos = (Vector2)charAtt.transform.position + (charAtt.rigidbody.velocity * Time.fixedDeltaTime);
            Vector2 nextBlockPos = (Vector2)blockAtt.transform.position + (blockAtt.rigidbody.velocity * Time.fixedDeltaTime);
            float dist = (nextBlockPos - nextCharPos).magnitude;
            if (dist > maxLength)
            {
                UpdateVelocities(dist - maxLength);
            }
        }

        private void UpdateVelocities(float magnitude)
        {
            if (magnitude < 0)
                throw new ArgumentOutOfRangeException();
            Debug.Log("Updating velocities");
            Debug.Log("Magnitude " + magnitude);
            float charPower = magnitude * pullBalance;
            float blockPower = magnitude * (1 - pullBalance);
            if (charPower < charAtt.staticFriction || blockPower < blockAtt.staticFriction) //if the force should be determined by static friction rather than pull balance
            {
                Debug.Log("Using static friction");
                if (magnitude >= charAtt.staticFriction + blockAtt.staticFriction) //happens when static friction distribution doesn't equal pull balance. there is enough force to satisfy both objects' static friction
                {
                    if (pullBalance > charAtt.staticFriction / (charAtt.staticFriction + blockAtt.staticFriction)) //if the pull balance favors the block more than the static friction distribution
                    {
                        //charPower gets its minimum required force while blockPower gets extra force to move closer to the pull balance (this is guaranteed to be greater than or equal to the block's static friction)
                        charPower = charAtt.staticFriction;
                        blockPower = magnitude - charAtt.staticFriction;
                    }
                    else
                    {
                        blockPower = blockAtt.staticFriction;
                        charPower = magnitude - blockAtt.staticFriction;
                    }
                }
                else if (magnitude > Mathf.Min(charAtt.staticFriction, blockAtt.staticFriction))
                {
                    if (charAtt.staticFriction < blockAtt.staticFriction)
                    {
                        charPower = magnitude;
                        blockPower = 0;
                    }
                    else
                    {
                        charPower = 0;
                        blockPower = magnitude;
                    }
                }
                else
                {
                    if (charAtt.staticFriction < blockAtt.staticFriction)
                    {
                        charPower = charAtt.staticFriction;
                        blockPower = 0;
                    }
                    else
                    {
                        charPower = 0;
                        blockPower = blockAtt.staticFriction;
                    }
                }
            }
            charAtt.rigidbody.velocity += (Vector2)(blockAtt.transform.position - charAtt.transform.position).normalized * charPower / Time.fixedDeltaTime;
            blockAtt.rigidbody.velocity += (Vector2)(charAtt.transform.position - blockAtt.transform.position).normalized * blockPower / Time.fixedDeltaTime;
        }
    }
}
