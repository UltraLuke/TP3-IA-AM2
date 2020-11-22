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
    //Vector3 _initialPos;
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
        //waypoints = FilterOutermostNodes(newPoints, finalPos, false);
        waypoints = newPoints;
        var pos = waypoints[_nextPoint].transform.position;
        pos.y = transform.position.y;
        //transform.position = pos;
        _finalPos = finalPos;
        //_firstRun = true;
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

        //Vector3 dir = posPoint - transform.position;
        //if (dir.magnitude < 0.2f && !_lastConnection)
        //{
        //    if (_nextPoint + 1 < waypoints.Count)
        //        _nextPoint++;
        //    else
        //    {
        //        readyToMove = false;
        //        return;
        //    }
        //}
        _entityModel.Move(dir.normalized);
    }

    //Filtra los nodos que están en las puntas,
    //para una conexión más directa con el punto final o inicio
    //La variable filterTail indica si se filtran los de la cabeza o la cola:
    // -- false: filtra los de la cabeza (los del final)
    // -- true: filtra los de la cola (los del principio)
    List<Node> FilterOutermostNodes(List<Node> nodes, Vector3 refPos, bool filterTail)
    {
        if (nodes.Count <= 1) return nodes;

        int index, initRange, range;

        if (!filterTail)
        {
            index = nodes.Count - 2;
            initRange = 0;
            range = nodes.Count - 1;
        }
        else
        {
            index = 0;
            initRange = 1;
            range = nodes.Count;
        }

        var currNode = nodes[index];
        var dir = currNode.transform.position - refPos;
        if (!Physics.Raycast(currNode.transform.position, dir.normalized, dir.magnitude, _obstacleLayer))
        {
            return FilterOutermostNodes(nodes.GetRange(initRange, range), refPos, filterTail);
        }
        return nodes;
    }
}
