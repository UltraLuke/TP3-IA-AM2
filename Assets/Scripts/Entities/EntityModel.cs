using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityModel : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] float speedRot;

    public void Move(Vector3 dir)
    {
        dir.y = 0;
        transform.position += Time.deltaTime * dir * speed; ;
        transform.forward = Vector3.Lerp(transform.forward, dir, speedRot * Time.deltaTime);
        //_anim.SetFloat("Vel", 1);
    }
}
