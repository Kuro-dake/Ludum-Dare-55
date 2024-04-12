using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using IEnumRunner;
using IEnumRunner.Transitions;

public class Blobber : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("step1");
            Make.The(gameObject).In(1f).ScaleTo(new Vector3(2f, 2f, 1f))
                .then.ScaleTo(new Vector3(1f, 1f, 1f)).Happen();
            
        }
    }
}
