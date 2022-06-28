using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Blockthrow.Environment
{
    public class DoorLoad : MonoBehaviour
    {
        [SerializeField] string sceneName;
        // Start is called before the first frame update
        void Start()
        {
            if (sceneName == "")
                Debug.LogWarning("No scene set for door object " + gameObject.name);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.tag != "Player")
                return;
            SceneManager.LoadScene(sceneName);
        }
    }
}
