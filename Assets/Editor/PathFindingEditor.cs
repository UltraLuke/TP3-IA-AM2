using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PathFindingEditor : EditorWindow
{
    WaypointsContainer _wpContainer;
    GameObject _waypoint;

    //Settings para creacion de waypoints
    Vector3 _originPoint;
    float _pathfindingAreaWidth = 0;
    float _pathfindingAreaLength = 0;
    int _waypointRows = 1;
    int _waypointColumns = 1;
    bool _enableWPIndicators = false;
    float _gizmoRadius = .75f;
    Color _gizmoColor;

    //Flags
    bool _waypointsInScene;
    bool _waypointGenerationMode;

    [MenuItem("Tools/Pathfinding Editor")]
    public static void OpenWindow()
    {
        var window = GetWindow<PathFindingEditor>();
        window.wantsMouseMove = true;

        window.Show();
    }

    private void OnEnable()
    {
        _waypoint = Resources.Load("waypoint/waypoint", typeof(GameObject)) as GameObject;
        _wpContainer = FindObjectOfType<WaypointsContainer>();
        SceneView.duringSceneGui += OnSceneGUI;
    }
    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        //Interfaz que se muestra si no hay ningún waypoint o contenedor de waypoints cargado
        if (!_waypointGenerationMode && (_wpContainer == null || _wpContainer.transform.childCount == 0))
        {
            EditorGUILayout.HelpBox("No se han encontrado waypoints o contenedores de waypoints", MessageType.Warning);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Crear waypoints"))
            {
                _waypointGenerationMode = true;
            }
            EditorGUI.BeginDisabledGroup(true);
            if (GUILayout.Button("Cargar waypoints"))
            {

            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }
        //Interfaz de creacion de waypoints
        else if (_waypointGenerationMode)
        {
            EditorGUILayout.LabelField("Waypoints generation");

            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, Color.gray);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Pathfinding Area");
            GUILayout.BeginHorizontal();
            _pathfindingAreaWidth = EditorGUILayout.FloatField("Width", _pathfindingAreaWidth);
            _pathfindingAreaLength = EditorGUILayout.FloatField("Length", _pathfindingAreaLength);
            _pathfindingAreaWidth = _pathfindingAreaWidth < 0 ? 0 : _pathfindingAreaWidth;
            _pathfindingAreaLength = _pathfindingAreaLength < 0 ? 0 : _pathfindingAreaLength;
            GUILayout.EndHorizontal();
            EditorGUILayout.LabelField("Waypoints amount");
            GUILayout.BeginHorizontal();
            _waypointRows = EditorGUILayout.IntField("Rows", _waypointRows);
            _waypointColumns = EditorGUILayout.IntField("Columns", _waypointColumns);
            _waypointRows = _waypointRows < 1 ? 1 : _waypointRows;
            _waypointColumns = _waypointColumns < 1 ? 1 : _waypointColumns;
            GUILayout.EndHorizontal();
            _originPoint = EditorGUILayout.Vector3Field("Origin Point", _originPoint);
            EditorGUI.DrawRect(rect, Color.gray);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Scene Indicators");
            _enableWPIndicators = EditorGUILayout.ToggleLeft("Show indicators", _enableWPIndicators);
            _gizmoRadius = EditorGUILayout.FloatField("Waypoint indicator radius", _gizmoRadius);
            _gizmoColor = EditorGUILayout.ColorField("Waypoint indicator color", _gizmoColor);
        }
    }

    private void GenerateWaypoints()
    {
        if (_wpContainer == null)
        {
            _wpContainer = new GameObject("waypoint_container").AddComponent<WaypointsContainer>();
            _wpContainer.transform.position = Vector3.zero;
        }

        var obj = (GameObject)PrefabUtility.InstantiatePrefab(_waypoint);
        obj.transform.position = Vector3.zero;
        obj.transform.parent = _wpContainer.transform;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        //Dibujo el area donde se instanciaran los waypoints
        if (_waypointGenerationMode)
        {
            var MyPosForward = _originPoint + Vector3.forward * _pathfindingAreaLength;
            var MyPosRight = _originPoint + Vector3.right * _pathfindingAreaWidth;

            Handles.DrawDottedLine(_originPoint, MyPosForward, 2);
            Handles.DrawDottedLine(_originPoint, MyPosRight, 2);
            Handles.DrawDottedLine(MyPosForward, MyPosForward + Vector3.right * _pathfindingAreaWidth, 2);
            Handles.DrawDottedLine(MyPosRight, MyPosRight + Vector3.forward * _pathfindingAreaLength, 2);
        }

        if (_enableWPIndicators && _waypointRows > 0 && _waypointColumns > 0)
        {
            int id = 0;
            float xPos = _originPoint.x;
            float zPos = _originPoint.z;

            var rowDivision = _pathfindingAreaLength / (_waypointRows + 1);
            var columnDivision = _pathfindingAreaWidth / (_waypointColumns + 1);

            Handles.color = _gizmoColor;

            for (int r = 0; r < _waypointRows; r++)
            {
                zPos = (r + 1) * rowDivision + _originPoint.z;
                for (int c = 0; c < _waypointColumns; c++)
                {
                    xPos = (c + 1) * columnDivision + _originPoint.x;
                    //if (_wiredIndicator)
                    Handles.SphereHandleCap(id, new Vector3(xPos, _originPoint.y, zPos), Quaternion.identity, _gizmoRadius, EventType.Repaint);
                    //Handles.DrawSphere(id, new Vector3(xPos, _originPoint.y, zPos), Quaternion.identity, _gizmoRadius);
                    id++;
                }
            }

        }
    }
}