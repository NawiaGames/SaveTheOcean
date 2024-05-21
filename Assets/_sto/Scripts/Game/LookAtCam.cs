using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCam : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
      transform.forward = (Camera.main.transform.position - transform.position).normalized;
    }

    // Update is called once per frame
    void Update()
    {
      transform.forward = -(Camera.main.transform.position - transform.position).normalized;
    }
}
