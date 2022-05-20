using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkateboardController : MonoBehaviour
{
    Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ApplyForces(Vector2 force)
    {
        Vector2 forceToApply = new Vector2(0, force.x);

        Debug.Log("Applying force: " + forceToApply);

        rb.AddTorque(forceToApply);
    }
}
