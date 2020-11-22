using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    EntityModel _entityModel;
    public List<Node> waypoints;
    public bool readyToMove;
    int _nextPoint = 0;

    private void Awake()
    {
        _entityModel = GetComponent<EntityModel>();
    }
    private void Update()
    {
        if (readyToMove)
        {
            Run();
        }
    }
    public void SetWayPoints(List<Node> newPoints)
    {
        _nextPoint = 0;
        if (newPoints.Count == 0) return;
        waypoints = newPoints;
        var pos = waypoints[_nextPoint].transform.position;
        pos.y = transform.position.y;
        transform.position = pos;
        readyToMove = true;
    }
    public void Run()
    {
        var point = waypoints[_nextPoint];
        var posPoint = point.transform.position;
        posPoint.y = transform.position.y;
        Vector3 dir = posPoint - transform.position;
        if (dir.magnitude < 0.2f)
        {
            if (_nextPoint + 1 < waypoints.Count)
                _nextPoint++;
            else
            {
                readyToMove = false;
                return;
            }
        }
        _entityModel.Move(dir.normalized);
    }
}
