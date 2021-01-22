using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace cakeslice {
    public class InteractableObject : MonoBehaviour {


        #region Private Fields

        Outline outline;

        #endregion



        void Start()
        {
            outline = GetComponent<Outline>();
            outline.enabled = false;
        }


        void Update()
        {
            if (outline.enabled)
            {
                if (Input.GetKeyDown(KeyCode.F))
                {

                    //TODO Inventory management
                    Destroy( gameObject );
                }
            }
        }


        private void OnTriggerEnter() {
            outline.enabled = true;
            Debug.Log("Press F");
        }

        private void OnTriggerExit() {
            outline.enabled = false;
        }
    }
}


