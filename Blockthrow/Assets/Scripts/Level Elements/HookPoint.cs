using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blockthrow.Environment
{
    public class HookPoint : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            BlockManager block = collision.GetComponent<BlockManager>();
            if (block == null || block.hookedPoint != null)
            {
                return;
            }
            block.hookedPoint = transform;
        }
    }
}
