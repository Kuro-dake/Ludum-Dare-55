using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

using IEnumRunner;
using IEnumRunner.Transitions;


public class GM : MonoBehaviour
{
    public static CultureInfo ci = CultureInfo.CreateSpecificCulture("en-US"); 
    // Start is called before the first frame update
    void Start()
    {
        Sequence.EnableRunner();
        

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
