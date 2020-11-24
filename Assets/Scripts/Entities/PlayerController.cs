using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public List<Node> waypoints;
    public bool readyToMove;
    [SerializeField] LayerMask _obstacleLayer;

    EntityModel _entityModel;
    Vector3 _finalPos;
    int _nextPoint = 0;
    bool _lastConnection;

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
    public void SetWayPoints(List<Node> newPoints, Vector3 finalPos)
    {
        _nextPoint = 0;
        if (newPoints.Count == 0) return;
        waypoints = newPoints;
        var pos = waypoints[_nextPoint].transform.position;
        pos.y = transform.position.y;
        _finalPos = finalPos;
        _lastConnection = false;
        readyToMove = true;
    }
    public void Run()
    {
        var point = waypoints[_nextPoint];
        var posPoint = point.transform.position;
        posPoint.y = transform.position.y;

        Vector3 dir;
        if (!_lastConnection)
            dir = posPoint - transform.position;
        else
            dir = _finalPos - transform.position;

        if (dir.magnitude < 0.2f)
        {
            if (!_lastConnection)
            {
                if (_nextPoint + 1 < waypoints.Count)
                    _nextPoint++;

                if (_nextPoint + 1 >= waypoints.Count)
                    _lastConnection = true;
            }
        }

        
        _entityModel.Move(dir.normalized);
    }
}
