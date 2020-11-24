using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

public class FlockingCreator : EditorWindow
{
    //Settings Basicos
    float speed = 0f;
    float speedRot = 0f;
    Transform target;
    GameObject entityPrefab;
    Vector3 offset;

    //Settings flocking
    float spawningAreaWidth = 0;
    float spawningAreaLength = 0;
    int entitiesRows = 1;
    int entitiesColumns = 1;
    LayerMask entityMask;
    float entityRadius;
    float cohesionWeight;
    float alineationWeight;
    float leaderWeight;
    float separationWeight;
    float separationRange;
    float avoidanceWeight;
    LayerMask avoidanceMask;
    float avoidanceRange;

    //Flags
    bool _creationLocked = false;
    bool _referenceLocked = false;

    GUIStyle _headerStyle;
    List<Vector3> _positions;


    [MenuItem("Tools/Flocking Creator")]
    public static void OpenWindow()
    {
        var window = GetWindow<FlockingCreator>();
        window.wantsMouseMove = true;

        window.Show();
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        _positions = new List<Vector3>();
        Styles();
    }
    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        _referenceLocked = false;
        _creationLocked = false;

        RequiredReferences();
        if (_referenceLocked) return;
        Line();
        BasicSettings();
        Line();
        SpawnSettings();
        if (_creationLocked) return;
        Line();
        FlockEntitySettings();
        Line();
        CohesionSettings();
        Line();
        AlineationSettings();
        Line();
        LeaderSettings();
        Line();
        SeparationSettings();
        Line();
        AvoidanceSettings();
        Line();

        if(GUILayout.Button("Create Flock"))
        {
            CreateFlock();
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (_referenceLocked) return;
        if (!_creationLocked)
        {
            var MyPosForward = (target.position + offset) + Vector3.forward * spawningAreaLength;
            var MyPosRight = (target.position + offset) + Vector3.right * spawningAreaWidth;
            Handles.color = Color.cyan;
            Handles.DrawDottedLine((target.position + offset), MyPosForward, 2);
            Handles.DrawDottedLine((target.position + offset), MyPosRight, 2);
            Handles.DrawDottedLine(MyPosForward, MyPosForward + Vector3.right * spawningAreaWidth, 2);
            Handles.DrawDottedLine(MyPosRight, MyPosRight + Vector3.forward * spawningAreaLength, 2);

            int id = 0;

            Handles.color = Color.yellow;
            for (int i = 0; i < _positions.Count; i++)
            {
                Handles.SphereHandleCap(id, _positions[i], Quaternion.identity, 0.5f, EventType.Repaint);
                id++;
            }
        }

    }

    private void RequiredReferences()
    {
        entityPrefab = EditorGUILayout.ObjectField("Entity prefab", entityPrefab, typeof(GameObject), false) as GameObject;
        target = EditorGUILayout.ObjectField("Target", target, typeof(Transform), true) as Transform;
        if (target == null || entityPrefab == null) _referenceLocked = true;
    }
    private void BasicSettings()
    {
        EditorGUILayout.LabelField("Basic Settings", _headerStyle);
        speed = EditorGUILayout.FloatField("Entities Speed", speed);
        speedRot = EditorGUILayout.FloatField("Entities Rotation Speed", speedRot);
    }
    private void SpawnSettings()
    {
        EditorGUILayout.LabelField("Area Settings", _headerStyle);

        EditorGUI.BeginChangeCheck();
        spawningAreaWidth = EditorGUILayout.FloatField("Spawning Area Width", spawningAreaWidth);
        spawningAreaLength = EditorGUILayout.FloatField("Spawning Area Length", spawningAreaLength);
        entitiesRows = EditorGUILayout.IntField("Entities Rows", entitiesRows);
        entitiesColumns = EditorGUILayout.IntField("Entities Columns", entitiesColumns);
        offset = EditorGUILayout.Vector3Field("Offset", offset);

        if (EditorGUI.EndChangeCheck())
        {
            spawningAreaWidth = spawningAreaWidth > 0 ? spawningAreaWidth : 0;
            spawningAreaLength = spawningAreaLength > 0 ? spawningAreaLength : 0;
            entitiesRows = entitiesRows >= 1 ? entitiesRows : 1;
            entitiesColumns = entitiesColumns >= 1 ? entitiesColumns : 1;

            if (spawningAreaWidth <= 0 || spawningAreaLength <= 0 || entitiesRows < 1 || entitiesColumns < 1)
                _creationLocked = true;

            _positions = CalculatePositions(entitiesRows, entitiesColumns, spawningAreaWidth, spawningAreaLength);
        }
    }
    private void FlockEntitySettings()
    {
        EditorGUILayout.LabelField("Flock Entity Settings", _headerStyle);
        LayerMask tempMask = EditorGUILayout.MaskField("Entity Mask", InternalEditorUtility.LayerMaskToConcatenatedLayersMask(entityMask), InternalEditorUtility.layers);
        entityMask = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempMask);
        entityRadius = EditorGUILayout.FloatField("Entity Radius", entityRadius);
    }
    private void CohesionSettings()
    {
        EditorGUILayout.LabelField("Cohesion Settings", _headerStyle);
        cohesionWeight = EditorGUILayout.FloatField("Cohesion Weight", cohesionWeight);
    }
    private void AlineationSettings()
    {
        EditorGUILayout.LabelField("Alineation Settings", _headerStyle);
        alineationWeight = EditorGUILayout.FloatField("Alineation Weight", alineationWeight);
    }
    private void LeaderSettings()
    {
        EditorGUILayout.LabelField("Leader Settings", _headerStyle);
        leaderWeight = EditorGUILayout.FloatField("Leader Weight", leaderWeight);
    }
    private void SeparationSettings()
    {
        EditorGUILayout.LabelField("Separation Settings", _headerStyle);
        separationWeight = EditorGUILayout.FloatField("Separation Weight", separationWeight);
        separationRange = EditorGUILayout.FloatField("Separation Range", separationRange);
    }
    private void AvoidanceSettings()
    {
        EditorGUILayout.LabelField("Avoidance Settings", _headerStyle);
        avoidanceWeight = EditorGUILayout.FloatField("Avoidance Weight", avoidanceWeight);
        LayerMask tempMask = EditorGUILayout.MaskField("Entity Mask", InternalEditorUtility.LayerMaskToConcatenatedLayersMask(avoidanceMask), InternalEditorUtility.layers);
        avoidanceMask = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempMask);
        avoidanceRange = EditorGUILayout.FloatField("Avoidance Range", avoidanceRange);
    }
    private List<Vector3> CalculatePositions(int rows, int columns, float width, float length)
    {
        List<Vector3> positions = new List<Vector3>();

        var rowDivision = length / (rows + 1);
        var columnDivision = width / (columns + 1);

        Vector3 positionToAdd;
        Vector3 originPoint = target.position + offset;

        for (int r = 0; r < rows; r++)
        {
            var zPos = (r + 1) * rowDivision + originPoint.z;
            for (int c = 0; c < columns; c++)
            {
                var xPos = (c + 1) * columnDivision + originPoint.x;
                positionToAdd = new Vector3(xPos, originPoint.y, zPos);

                positions.Add(positionToAdd);
            }
        }
        return positions;
    }
    private void CreateFlock()
    {
        for (int i = 0; i < _positions.Count; i++)
        {
            var obj = (GameObject)PrefabUtility.InstantiatePrefab(entityPrefab);
            obj.transform.position = _positions[i];

            var fModel = obj.GetComponent<EntityModel>();
            fModel.speed = speed;
            fModel.speedRot = speedRot;
            var fEntity = obj.GetComponent<FlockEntity>();
            fEntity.maskEntity = entityMask;
            fEntity.radius = entityRadius;
            obj.GetComponent<CohesionBehavior>().cohesionWeight = cohesionWeight;
            obj.GetComponent<AlineationBehavior>().alineationWeight = alineationWeight;
            var fLeader = obj.GetComponent<LeaderBehavior>();
            fLeader.leaderWeight = leaderWeight;
            fLeader.target = target;
            var fSeparation = obj.GetComponent<SeparationBehavior>();
            fSeparation.separationWeight = separationWeight;
            fSeparation.range = separationRange;
            var fAvoidance = obj.GetComponent<AvoidanceBehavior>();
            fAvoidance.avoidanceWeight = avoidanceWeight;
            fAvoidance.mask = avoidanceMask;
            fAvoidance.range = avoidanceRange;

            PrefabUtility.RecordPrefabInstancePropertyModifications(obj);
        }
    }

    void Styles()
    {
        _headerStyle = new GUIStyle();
        _headerStyle.fontStyle = FontStyle.Bold;
    }
    void Line()
    {
        Rect rect = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(rect, Color.gray);
    }
}
