using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HenchmanController : MonoBehaviour
{
    Vector3 _dir = Vector3.zero;
    EntityModel _entityModel;
    private FlockEntity flock;

    private void Awake()
    {
        flock = GetComponent<FlockEntity>();
    }
    void Start()
    {
        _entityModel = GetComponent<EntityModel>();
    }
    void Update()
    {
        _dir = flock.GetDir();
        _entityModel.Move(_dir);
    }
}
