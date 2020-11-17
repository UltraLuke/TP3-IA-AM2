using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AgentTheta : MonoBehaviour
{
    [Header("Gizmos settings")]
    [SerializeField] LayerMask mask;
    [SerializeField] float radius;
    [SerializeField] Vector3 offset;
    //public float distanceMax;

    private Node init;
    private Node finit;
    private PlayerController pj;
    //public Box box;
    List<Node> _list;
    List<Vector3> _listVector;
    Theta<Node> _theta = new Theta<Node>();

    public Node Init { get => init; set => init = value; }
    public Node Finit { get => finit; set => finit = value; }

    private void Awake()
    {
        pj = GetComponent<PlayerController>();
    }

    public void PathFindingTheta()
    {
        _list = _theta.Run(init, Satisfies, GetNeighbours, GetCost, Heuristic, InSight);
        pj.SetWayPoints(_list);
        //box.SetWayPoints(_list);
    }

    bool InSight(Node gP, Node gC)
    {
        Debug.Log(gP + "   " + gC);
        var dir = gC.transform.position - gP.transform.position;
        if (Physics.Raycast(gP.transform.position, dir.normalized, dir.magnitude, mask))
        {
            return false;
        }
        return true;
    }

    float Heuristic(Node curr)
    {
        return Vector3.Distance(curr.transform.position, finit.transform.position);
    }
    float GetCost(Node from, Node to)
    {
        return Vector3.Distance(from.transform.position, to.transform.position);
    }
    
    List<Node> GetNeighbours(Node curr)
    {
        return curr.GetNeighbours();
    }
    bool Satisfies(Node curr)
    {
        return curr == finit;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (init != null)
            Gizmos.DrawSphere(init.transform.position + offset, radius);
        if (finit != null)
            Gizmos.DrawSphere(finit.transform.position + offset, radius);
        if (_list != null)
        {
            Gizmos.color = Color.blue;
            foreach (var item in _list)
            {
                if (item != init && item != finit)
                    Gizmos.DrawSphere(item.transform.position + offset, radius);
            }
        }
        if (_listVector != null)
        {
            Gizmos.color = Color.green;
            foreach (var item in _listVector)
            {
                if (item != init.transform.position && item != finit.transform.position)
                    Gizmos.DrawSphere(item + offset, radius);
            }
        }

    }
}
