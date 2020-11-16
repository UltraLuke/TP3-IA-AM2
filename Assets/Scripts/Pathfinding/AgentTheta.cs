using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AgentTheta : MonoBehaviour
{
    public LayerMask mask;
    public float distanceMax;
    public float radius;
    public Vector3 offset;
    public Node init;
    public Node finit;
    public PlayerController pj;
    //public Box box;
    List<Node> _list;
    List<Vector3> _listVector;
    Theta<Node> _theta = new Theta<Node>();
    
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

    float HeuristicVector(Vector3 curr)
    {
        return Vector3.Distance(curr, finit.transform.position);
    }
    Dictionary<Vector3, float> GetNeighboursCostVector(Vector3 curr)
    {
        Dictionary<Vector3, float> dic = new Dictionary<Vector3, float>();
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                if (z == 0 && x == 0) continue;
                Vector3 pos = new Vector3(x + curr.x, curr.y, z + curr.z);

                dic.Add(pos, 1);
            }
        }
        return dic;
    }
    bool SatisfiesVector(Vector3 pos)
    {
        return Vector3.Distance(pos, finit.transform.position) <= distanceMax;
    }
    float Heuristic(Node curr)
    {
        return Vector3.Distance(curr.transform.position, finit.transform.position);
    }
    //float GetCost(Vector3 from, Vector3 to)
    //{
    //    return Vector3.Distance(from, to);
    //}
    float GetCost(Node from, Node to)
    {
        return Vector3.Distance(from.transform.position, to.transform.position);
    }
    //List<Vector3> GetNeighbours(Vector3 curr)
    //{
    //    var list = new List<Vector3>();
    //    for (int x = -1; x <= 1; x++)
    //    {
    //        for (int z = -1; z <= 1; z++)
    //        {
    //            if (x == 0 && z == 0) continue;
    //            Vector3 newPos = new Vector3(curr.x + x, curr.y, curr.z + z);
    //            list.Add(newPos);
    //        }
    //    }
    //    return list;
    //}
    //Dictionary<Node, float> GetNeighboursCost(Node curr)
    //{
    //    Dictionary<Node, float> dic = new Dictionary<Node, float>();
    //    for (int i = 0; i < curr.neightbourds.Count; i++)
    //    {
    //        float cost = 0;
    //        cost += Vector3.Distance(curr.transform.position, curr.neightbourds[i].transform.position);
    //        if (curr.neightbourds[i].hasTrap) cost += 3;
    //        dic[curr.neightbourds[i]] = cost;
    //    }
    //    return dic;
    //}
    List<Node> GetNeighbours(Node curr)
    {
        return curr.neightbourds;
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
