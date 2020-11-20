using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    [SerializeField] float radiusDistance;
    [SerializeField] int _id;
    //[SerializeField] bool hasTrap;

    [SerializeField] List<Node> neightbourds;

    public int Id { get => _id; set => _id = value; }
    public float RadiusDistance {set => radiusDistance = value; }

    public List<Node> GetNeighbours()
    {
        //GetNeightbourd(Vector3.right);
        //GetNeightbourd(Vector3.left);
        //GetNeightbourd(Vector3.forward);
        //GetNeightbourd(Vector3.back);

        Collider[] colliders = Physics.OverlapSphere(transform.position, radiusDistance, ~gameObject.layer);
        List<Node> newList = new List<Node>();
        Node currentNode;

        for (int i = 0; i < colliders.Length; i++)
        {
            currentNode = colliders[i].gameObject.GetComponent<Node>();
                newList.Add(currentNode);
        }
        neightbourds = newList;
        return neightbourds;
    }

    void GetNeightbourd(Vector3 dir)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, dir, out hit, 2.2f))
        {
            if (neightbourds == null)
                neightbourds = new List<Node>();

            var node = hit.collider.GetComponent<Node>();
            if (node != null)
                neightbourds.Add(node);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, .63f, .15f);
        Gizmos.DrawWireSphere(transform.position, radiusDistance);
    }
}
