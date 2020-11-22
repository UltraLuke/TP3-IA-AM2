using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SetTargetPos : MonoBehaviour
{
    [SerializeField] GameObject waypointObj;
    [SerializeField] Vector3 _clickPlaneReference;
    Plane _plane;
    //Vector3 _clickPoint = Vector3.zero;
    bool _clicked = false;

    Camera _cmra;
    AgentTheta _agentTheta;

    Node initNode;
    Node finitNode;

    private void Awake()
    {
        _agentTheta = GetComponent<AgentTheta>();
    }
    private void Start()
    {
        _plane = new Plane(Vector3.up, _clickPlaneReference);
        _cmra = Camera.main;
    }

    private void Update()
    {
        if (_clicked)
        {
            _agentTheta.PathFindingTheta();
            _clicked = false;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Ray ray = _cmra.ScreenPointToRay(Input.mousePosition);

            if (_plane.Raycast(ray, out float enter))
            {
                var hitPoint = ray.GetPoint(enter);
                Vector3 initPos = new Vector3(transform.position.x, _clickPlaneReference.y, transform.position.z);

                if (initPos == null || finitNode == null)
                {
                    GameObject initObj = Instantiate(waypointObj, initPos, Quaternion.identity);
                    GameObject finitObj = Instantiate(waypointObj, hitPoint, Quaternion.identity);
                    initObj.name = "initNode";
                    finitObj.name = "finitNode";
                    _agentTheta.Init = initNode = initObj.GetComponent<Node>();
                    _agentTheta.Finit = finitNode = finitObj.GetComponent<Node>();
                }
                else
                {
                    initNode.transform.position = initPos;
                    finitNode.transform.position = hitPoint;
                }

                _clicked = true;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawLine(_clickPlaneReference + new Vector3(-1, 0, 1), _clickPlaneReference + new Vector3(1, 0, 1));
        Gizmos.DrawLine(_clickPlaneReference + new Vector3(1, 0, 1), _clickPlaneReference + new Vector3(1, 0, -1));
        Gizmos.DrawLine(_clickPlaneReference + new Vector3(1, 0, -1), _clickPlaneReference + new Vector3(-1, 0, -1));
        Gizmos.DrawLine(_clickPlaneReference + new Vector3(-1, 0, -1), _clickPlaneReference + new Vector3(-1, 0, 1));
    }
}
