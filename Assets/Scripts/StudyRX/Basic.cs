using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Basic : MonoBehaviour
{
    public void Move(float dx, float dy, float dz)
    {
        transform.position += new Vector3(dx, dy, dz);
    }
}