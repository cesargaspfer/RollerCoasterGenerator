using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalRailDebugger : MonoBehaviour
{
    
    private Vector3 lastPosition = Vector3.zero;
    private Matrix4x4 lastBasis = Matrix4x4.identity;
    public Constructor constructor;
    
    void Update()
    {   
        if(constructor != null && (lastPosition != transform.position || !lastBasis.Equals(transform.localToWorldMatrix)))
        {
            lastPosition = transform.position;
            lastBasis = transform.localToWorldMatrix;
            Matrix4x4 basis = transform.localToWorldMatrix;
            basis[0, 3] = 0;
            basis[1, 3] = 0;
            basis[2, 3] = 0;
            constructor.TestAddFinalRail(lastPosition, basis);
        }
    }
}
