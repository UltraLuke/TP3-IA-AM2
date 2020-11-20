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
    List<Vector3> _waypointPositions;

    string _saveFolderPath = "Assets/WaypointsInfo/";
    string _saveFilename = "wpinfo.asset";

    //Indicador area pathfinding
    Vector3 _textAreaPosition;

    Vector2 scrollPos;

    //Ventana Loader
    WPLoad _loadWindow;

    //Flags
    bool _waypointsInScene;
    bool _waypointGenerationMode;
    bool _calculatingPositions;

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

        //_titleArea = "Pathfinding Area: ";
    }
    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        _loadWindow.Close();
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

            EditorGUILayout.LabelField("Pathfinding Area");

            EditorGUI.BeginChangeCheck();

            //Si hago un cambio en el area o los waypoints, recalculo las posiciones
            GUILayout.BeginHorizontal();

            _pathfindingAreaWidth = EditorGUILayout.FloatField("Width", _pathfindingAreaWidth);
            _pathfindingAreaLength = EditorGUILayout.FloatField("Length", _pathfindingAreaLength);
            GUILayout.EndHorizontal();
            EditorGUILayout.LabelField("Waypoints amount");
            GUILayout.BeginHorizontal();
            _waypointRows = EditorGUILayout.IntField("Rows", _waypointRows);
            _waypointColumns = EditorGUILayout.IntField("Columns", _waypointColumns);
            GUILayout.EndHorizontal();
            _originPoint = EditorGUILayout.Vector3Field("Origin Point", _originPoint);

            if(GUILayout.Button("Generate Waypoints"))
            {
                GenerateWaypoints();
                _waypointGenerationMode = false;
            }

            if (EditorGUI.EndChangeCheck())
            {
                _pathfindingAreaWidth = _pathfindingAreaWidth < 0 ? 0 : _pathfindingAreaWidth;
                _pathfindingAreaLength = _pathfindingAreaLength < 0 ? 0 : _pathfindingAreaLength;
                _waypointRows = _waypointRows < 1 ? 1 : _waypointRows;
                _waypointColumns = _waypointColumns < 1 ? 1 : _waypointColumns;

                CalculatePositions();
            }

            _textAreaPosition = _originPoint + new Vector3(-1, 0, -1);

            rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, Color.gray);

            EditorGUILayout.Space();

            //Algunos ajustes de visualizacion de indicadores
            EditorGUILayout.LabelField("Scene Indicators");
            _enableWPIndicators = EditorGUILayout.ToggleLeft("Show indicators", _enableWPIndicators);
            _gizmoRadius = EditorGUILayout.FloatField("Waypoint indicator radius", _gizmoRadius);
            _gizmoColor = EditorGUILayout.ColorField("Waypoint indicator color", _gizmoColor);
        }
        //Interfaz cuando ya tengo waypoints seteados
        else if(_wpContainer != null && _wpContainer.transform.childCount > 0)
        {
            EditorGUILayout.LabelField("Waypoints");
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height - 50)/*GUILayout.Width(withWindow), GUILayout.Height(heightWindow)*/);

            Node[] wps = _wpContainer.transform.GetComponentsInChildren<Node>();
            for (int i = 0; i < wps.Length; i++)
            {
                EditorGUILayout.ObjectField(wps[i].gameObject, typeof(GameObject), true);
            }
            EditorGUILayout.EndScrollView();

            if(GUILayout.Button("Guardar waypoints"))
            {
                SaveWaypoints(wps);
            }
        }
    }

    private void GenerateWaypoints()
    {
        if (_wpContainer == null)
        {
            _wpContainer = new GameObject("waypoint_container").AddComponent<WaypointsContainer>();
            _wpContainer.transform.position = Vector3.zero;
        }

        for (int i = 0; i < _waypointPositions.Count; i++)
        {
            var obj = (GameObject)PrefabUtility.InstantiatePrefab(_waypoint);
            obj.transform.position = _waypointPositions[i];
            obj.transform.parent = _wpContainer.transform;
        }
    }

    private void SaveWaypoints(Node[] wps)
    {
        var scriptable = ScriptableObject.CreateInstance<WaypointsInfo>();
        scriptable.waypointsData = new List<WaypointData>();
        WaypointData wpData = default;

        for (int i = 0; i < wps.Length; i++)
        {
            wpData.id = wps[i].gameObject.GetInstanceID();
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

        _waypointPositions = new List<Vector3>();

        for (int i = 0; i < wpsData.Count; i++)
        {
            _waypointPositions.Add(wpsData[i].position);
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

                for (int i = 0; i < _waypointPositions.Count; i++)
                {
                    Handles.SphereHandleCap(id, new Vector3(_waypointPositions[i].x, _originPoint.y, _waypointPositions[i].z),
                                                Quaternion.identity, _gizmoRadius, EventType.Repaint);
                    id++;
                }
            }

            Handles.color = Color.white;
            Handles.DrawDottedLine(_textAreaPosition, _originPoint, 2);
            Handles.BeginGUI();

            var cmraPoint = Camera.current.WorldToScreenPoint(_textAreaPosition);
            var cmraRect = Camera.current.pixelHeight;
            //var  = Camera.current.pixelHeight;
            var rect = new Rect(cmraPoint.x - 75, cmraRect - cmraPoint.y, 200, 50);
            string text = "Pathfinding Area: " + string.Format("{0}x{1}\n", _pathfindingAreaLength, _pathfindingAreaWidth) +
                          "Total waypoints: " + _waypointRows * _waypointColumns;
            GUI.Box(rect, text);

            Handles.EndGUI();
        }
    }

    private void CalculatePositions()
    {
        _calculatingPositions = true;

        float xPos = _originPoint.x;
        float zPos = _originPoint.z;

        var rowDivision = _pathfindingAreaLength / (_waypointRows + 1);
        var columnDivision = _pathfindingAreaWidth / (_waypointColumns + 1);

        Vector3 positionToAdd = Vector3.zero;

        _waypointPositions = new List<Vector3>();

        for (int r = 0; r < _waypointRows; r++)
        {
            zPos = (r + 1) * rowDivision + _originPoint.z;
            for (int c = 0; c < _waypointColumns; c++)
            {
                xPos = (c + 1) * columnDivision + _originPoint.x;
                positionToAdd = new Vector3(xPos, _originPoint.y, zPos);
                _waypointPositions.Add(positionToAdd);
            }
        }

        _calculatingPositions = false;
    }
}