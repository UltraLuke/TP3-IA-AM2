using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    [SerializeField] int _id;
    [SerializeField] float radiusDistance;
    [SerializeField] LayerMask layerToDetect;
    [SerializeField] LayerMask layerObstacle;
    [SerializeField] List<Node> neightbourds;
    //[SerializeField] bool hasTrap;

    public int Id { get => _id; set => _id = value; }
    public float RadiusDistance { set => radiusDistance = value; }

    public List<Node> GetNeighbours()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radiusDistance, layerToDetect);
        List<Node> newList = new List<Node>();
        Node currentNode;

        for (int i = 0; i < colliders.Length; i++)
        {
            currentNode = colliders[i].gameObject.GetComponent<Node>();

            if(currentNode != this && NodeInSight(currentNode))
            newList.Add(currentNode);
        }
        neightbourds = newList;
        return neightbourds;
    }

    bool NodeInSight(Node node)
    {
        var dir = node.transform.position - transform.position;
        if (Physics.Raycast(transform.position, dir.normalized, dir.magnitude, layerObstacle))
        {
            return false;
        }
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, .63f, .15f);
        Gizmos.DrawWireSphere(transform.position, radiusDistance);

        if (neightbourds != null && neightbourds.Count != 0)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < neightbourds.Count; i++)
            {
                Gizmos.DrawLine(transform.position, neightbourds[i].transform.position);
            }
        }
    }
}
