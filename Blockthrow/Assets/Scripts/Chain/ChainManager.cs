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
                float magnitude = (dist - maxLength) / Time.fixedDeltaTime;
                charAtt.rigidbody.velocity += (Vector2)(blockAtt.transform.position - charAtt.transform.position).normalized * magnitude * pullBalance;
                blockAtt.rigidbody.velocity += (Vector2)(charAtt.transform.position - blockAtt.transform.position).normalized * magnitude * (1 - pullBalance);
            }
        }
    }
}
