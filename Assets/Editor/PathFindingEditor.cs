using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

public class PathFindingEditor : EditorWindow
{
    WaypointsContainer _wpContainer;
    GameObject _waypoint;
    List<Node> _nodes;

    //GUI Styles
    private GUIStyle _headerStyle;

    //Settings para creacion de waypoints
    Vector3 _originPoint;
    float _pathfindingAreaWidth = 0;
    float _pathfindingAreaLength = 0;
    int _waypointRows = 1;
    int _waypointColumns = 1;
    bool _enableWPIndicators = true;
    float _gizmoRadius = .75f;
    Color _gizmoColor = new Color(0.34f, 0.84f, 0.86f, 0.6f);
    //List<Vector3> _waypointPositions;
    List<WaypointData> _waypointData;

    //Waypoint obstacles
    bool _detectObstacles;
    float _radiusObstacleDetection;
    LayerMask _obstaclesMask;

    //Waypoints connection
    bool _setConnections = false;
    bool _showOverlaps = true;
    bool _showConnections = true;
    float _radiusDistanceConnection = 0f;

    string _saveFolderPath = "Assets/WaypointsInfo/";
    string _saveFilename = "wpinfo.asset";

    //Indicador area pathfinding
    Vector3 _textAreaPosition;

    Vector2 scrollPos;

    //Ventana Loader
    WPLoad _loadWindow;

    //Flags
    bool _waypointGenerationMode;
    bool _calculatingPositions;
    bool _disableConnectionsButton;

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

        SetStyles();
    }
    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        if (_loadWindow != null) _loadWindow.Close();
    }

    void SetStyles()
    {
        _headerStyle = new GUIStyle();
        _headerStyle.fontStyle = FontStyle.Bold;
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
            EditorGUI.BeginDisabledGroup(false);
            if (GUILayout.Button("Cargar waypoints"))
            {
                OpenLoaderWindow();
                //GenerateWaypoints();
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

            EditorGUILayout.LabelField("Pathfinding Area", _headerStyle);

            EditorGUI.BeginChangeCheck();

            //Si hago un cambio en el area o los waypoints, recalculo las posiciones
            GUILayout.BeginHorizontal();
            _pathfindingAreaWidth = EditorGUILayout.FloatField("Width", _pathfindingAreaWidth);
            _pathfindingAreaLength = EditorGUILayout.FloatField("Length", _pathfindingAreaLength);
            GUILayout.EndHorizontal();
            EditorGUILayout.LabelField("Waypoints amount", _headerStyle);
            GUILayout.BeginHorizontal();
            _waypointRows = EditorGUILayout.IntField("Rows", _waypointRows);
            _waypointColumns = EditorGUILayout.IntField("Columns", _waypointColumns);
            GUILayout.EndHorizontal();
            _originPoint = EditorGUILayout.Vector3Field("Origin Point", _originPoint);

            rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, Color.gray);

            EditorGUILayout.LabelField("Obstacles", _headerStyle);
            _detectObstacles = EditorGUILayout.Toggle("Don't instantiate near obstacles", _detectObstacles);
            _radiusObstacleDetection = EditorGUILayout.FloatField("Radius Detection", _radiusObstacleDetection);

            LayerMask tempMask = EditorGUILayout.MaskField("Obstacles mask", InternalEditorUtility.LayerMaskToConcatenatedLayersMask(_obstaclesMask), InternalEditorUtility.layers);
            _obstaclesMask = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempMask);

            if (EditorGUI.EndChangeCheck())
            {
                _pathfindingAreaWidth = _pathfindingAreaWidth < 0 ? 0 : _pathfindingAreaWidth;
                _pathfindingAreaLength = _pathfindingAreaLength < 0 ? 0 : _pathfindingAreaLength;
                _waypointRows = _waypointRows < 1 ? 1 : _waypointRows;
                _waypointColumns = _waypointColumns < 1 ? 1 : _waypointColumns;
                _radiusObstacleDetection = _radiusObstacleDetection < 0 ? 0 : _radiusObstacleDetection;

                CalculatePositions();
            }

            rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, Color.gray);

            if (GUILayout.Button("Generate Waypoints"))
            {
                GenerateWaypoints(out _nodes);
                _waypointGenerationMode = false;
            }

            _textAreaPosition = _originPoint + new Vector3(-1, 0, -1);

            rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, Color.gray);

            EditorGUILayout.Space();

            //Algunos ajustes de visualizacion de indicadores
            EditorGUILayout.LabelField("Scene Indicators", _headerStyle);
            _enableWPIndicators = EditorGUILayout.ToggleLeft("Show indicators", _enableWPIndicators);
            _gizmoRadius = EditorGUILayout.FloatField("Waypoint indicator radius", _gizmoRadius);
            _gizmoColor = EditorGUILayout.ColorField("Waypoint indicator color", _gizmoColor);
        }
        //Interfaz cuando ya tengo waypoints seteados
        else if (_wpContainer != null && _wpContainer.transform.childCount > 0)
        {
            EditorGUILayout.LabelField("Connections", _headerStyle);

            EditorGUI.BeginChangeCheck();

            _setConnections = EditorGUILayout.Toggle("Enable connections", _setConnections);

            EditorGUI.BeginDisabledGroup(!_setConnections);
            _radiusDistanceConnection = EditorGUILayout.FloatField("Distance Radius", _radiusDistanceConnection);
            EditorGUI.EndDisabledGroup();

            if (EditorGUI.EndChangeCheck()) _disableConnectionsButton = true;

            EditorGUI.BeginDisabledGroup(!_disableConnectionsButton);
            if (GUILayout.Button("Bake connections"))
            {
                GenerateConnections(_nodes);
            }
            EditorGUI.EndDisabledGroup();




            var rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, Color.gray);

            EditorGUILayout.LabelField("Waypoints", _headerStyle);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height - 50)/*GUILayout.Width(withWindow), GUILayout.Height(heightWindow)*/);

            Node[] wps = _wpContainer.transform.GetComponentsInChildren<Node>();
            for (int i = 0; i < wps.Length; i++)
            {
                EditorGUILayout.ObjectField(wps[i].gameObject, typeof(GameObject), true);
            }
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Guardar waypoints"))
            {
                SaveWaypoints(wps);
            }
        }
    }

    private List<GameObject> GenerateWaypoints()
    {
        return GenerateWaypoints(out var nodes);
    }

    private List<GameObject> GenerateWaypoints(out List<Node> nodes)
    {
        var objs = new List<GameObject>();
        nodes = new List<Node>();

        if (_wpContainer == null)
        {
            _wpContainer = new GameObject("waypoint_container").AddComponent<WaypointsContainer>();
            _wpContainer.transform.position = Vector3.zero;
        }

        int setID = 0;
        for (int i = 0; i < _waypointData.Count; i++)
        {
            var obj = (GameObject)PrefabUtility.InstantiatePrefab(_waypoint);
            obj.transform.position = _waypointData[i].position;
            obj.transform.parent = _wpContainer.transform;

            if (i != 0 && _waypointData[i].id == 0)
                setID = i;
            else
                setID = _waypointData[i].id;

            objs.Add(obj);
            nodes.Add(obj.GetComponent<Node>());
            nodes[i].Id = setID;
        }

        return objs;
    }

    private void GenerateConnections(List<Node> nodes)
    {
        WaypointData wpData;

        //ELijo un nodo de la lista
        for (int i = 0; i < nodes.Count; i++)
        {
            wpData = _waypointData[i];
            //Por cada nodo tengo...
            //1. Obtener el ID propio y guardarlo en _wayPointData
            wpData.id = nodes[i].Id;

            //2. Generar las conexiones y obtener el ID de los mismos. Guardarlos en _wayPointData
            nodes[i].RadiusDistance = _radiusDistanceConnection;
            var connectedNodes = nodes[i].GetNeighbours();
            wpData.connectedNodesID = new List<int>();
            //Elijo una de las conexiones de la lista
            for (int j = 0; j < connectedNodes.Count; j++)
            {
                wpData.connectedNodesID.Add(connectedNodes[j].Id);
            }

            _waypointData[i] = wpData;
        }
    }

    private void SaveWaypoints(Node[] wps)
    {
        var scriptable = ScriptableObject.CreateInstance<WaypointsInfo>();
        scriptable.waypointsData = new List<WaypointData>();
        WaypointData wpData = default;

        for (int i = 0; i < wps.Length; i++)
        {
            wpData.id = wps[i].Id;
            wpData.position = wps[i].transform.position;
            scriptable.waypointsData.Add(wpData);
        }

        //Guardo el scriptable
        var path = _saveFolderPath + _saveFilename;
        path = AssetDatabase.GenerateUniqueAssetPath(path);
        AssetDatabase.CreateAsset(scriptable, path);
    }

    private void OpenLoaderWindow()
    {
        _loadWindow = GetWindow<WPLoad>();
        _loadWindow.SaveFolderPath = _saveFolderPath;
        _loadWindow.wpLoader += LoadWaypoints;
        _loadWindow.Show();
    }

    private void LoadWaypoints(string fileName)
    {
        var path = _saveFolderPath + fileName;
        var scriptable = AssetDatabase.LoadAssetAtPath<WaypointsInfo>(path);
        var wpsData = scriptable.waypointsData;

        //_waypointPositions = new List<Vector3>();
        _waypointData = new List<WaypointData>();

        for (int i = 0; i < wpsData.Count; i++)
        {
            //_waypointPositions.Add(wpsData[i].position);
            _waypointData.Add(wpsData[i]);
        }

        GenerateWaypoints();
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


            //Dibujo los indicadores donde se setearían los waypoints.
            if (_enableWPIndicators && !_calculatingPositions && _waypointRows > 0 && _waypointColumns > 0)
            {
                int id = 0;

                Handles.color = _gizmoColor;

                if (_waypointData != null)
                {
                    for (int i = 0; i < _waypointData.Count; i++)
                    {
                        //Handles.SphereHandleCap(id, new Vector3(_waypointPositions[i].x, _originPoint.y, _waypointPositions[i].z),
                        Handles.SphereHandleCap(id, new Vector3(_waypointData[i].position.x, _originPoint.y, _waypointData[i].position.z),
                                                    Quaternion.identity, _gizmoRadius, EventType.Repaint);
                        id++;
                    }
                }
            }

            

            Handles.color = Color.white;
            Handles.DrawDottedLine(_textAreaPosition, _originPoint, 2);
            Handles.BeginGUI();

            var cmraPoint = Camera.current.WorldToScreenPoint(_textAreaPosition);
            var cmraRectHeight = Camera.current.pixelHeight;
            var cmraRectWidth = Camera.current.pixelWidth;
            //var  = Camera.current.pixelHeight;
            var rect = new Rect(cmraPoint.x - 75, cmraRectHeight - cmraPoint.y, 200, 50);
            string text = "Pathfinding Area: " + string.Format("{0}x{1}\n", _pathfindingAreaLength, _pathfindingAreaWidth) +
                          "Total waypoints: " + _waypointRows * _waypointColumns;
            GUI.Box(rect, text);

            Handles.EndGUI();
        }

        if (!_waypointGenerationMode && (_wpContainer != null && _wpContainer.transform.childCount > 0))
        {
            //Indicadores de conexiones
            if (_setConnections || _waypointData != null && !_waypointGenerationMode && (_wpContainer == null || _wpContainer.transform.childCount == 0))
            {
                Handles.color = Color.yellow;
                for (int i = 0; i < _waypointData.Count; i++)
                {
                    var nodesIDs = _waypointData[i].connectedNodesID;
                    if (nodesIDs == null || nodesIDs.Count == 0) break;

                    for (int j = 0; j < nodesIDs.Count; j++)
                    {
                        Handles.DrawLine(_waypointData[i].position, _waypointData[nodesIDs[j]].position);
                    }
                }
            }
        }
    }

    private void CalculatePositions()
    {
        _calculatingPositions = true;

        float xPos, zPos;

        var rowDivision = _pathfindingAreaLength / (_waypointRows + 1);
        var columnDivision = _pathfindingAreaWidth / (_waypointColumns + 1);

        Vector3 positionToAdd;

        //_waypointPositions = new List<Vector3>();
        _waypointData = new List<WaypointData>();

        for (int r = 0; r < _waypointRows; r++)
        {
            zPos = (r + 1) * rowDivision + _originPoint.z;
            for (int c = 0; c < _waypointColumns; c++)
            {
                xPos = (c + 1) * columnDivision + _originPoint.x;
                positionToAdd = new Vector3(xPos, _originPoint.y, zPos);

                if (!_detectObstacles || !CheckIfObstaclesNear(positionToAdd))
                {
                    _waypointData.Add(new WaypointData { position = positionToAdd });
                }
            }
        }

        _calculatingPositions = false;
    }

    private bool CheckIfObstaclesNear(Vector3 positionToAdd)
    {
        Collider[] colliders = Physics.OverlapSphere(positionToAdd, _radiusObstacleDetection, _obstaclesMask);
        return colliders.Length > 0;
    }
}