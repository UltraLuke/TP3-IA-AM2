using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityModel : MonoBehaviour
{
    public float speed;
    public float speedRot;

    public virtual void Move(Vector3 dir)
    {
        dir.y = 0;
        transform.position += Time.deltaTime * dir * speed;
        transform.forward = Vector3.Lerp(transform.forward, dir, speedRot * Time.deltaTime);
    }
}
