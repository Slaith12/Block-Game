using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blockthrow
{
    public class PlayerInitializer : MonoBehaviour
    {
        [SerializeField] PlayerController player;
        [SerializeField] Chain chain;
        [SerializeField] BlockManager block;
        // Start is called before the first frame update
        void Start()
        {
            player.Init();
            block.Init();
            chain.Init(); //this is done last since it requires block to be initialized first.
        }
    }
}
