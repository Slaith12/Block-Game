using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blockthrow.Enironment
{
    public class GateButton : MonoBehaviour
    {
        [SerializeField] LayerMask canTriggerButton;
        [SerializeField] Gate[] linkedGates;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (((1 << collision.gameObject.layer) & canTriggerButton) == 0 || collision.isTrigger)
                return;
            foreach (Gate gate in linkedGates)
            {
                gate.Open();
            }
        }
    }
}