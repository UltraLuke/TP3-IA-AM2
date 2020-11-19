using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WPLoad : EditorWindow
{
    string _saveFolderPath;
    List<WaypointsInfo> _waypointsInfos;
    public string SaveFolderPath { set => _saveFolderPath = value; }

    private void OnEnable()
    {
        _waypointsInfos = new List<WaypointsInfo>();
    }

    private void OnGUI()
    {
        if(_saveFolderPath != null && (_waypointsInfos == null || _waypointsInfos.Count <= 0))
        {
            var wpInfosGUID = AssetDatabase.FindAssets("t:WaypointsInfo");

            for (int i = 0; i < wpInfosGUID.Length; i++)
            {
                var wpPath = AssetDatabase.GUIDToAssetPath(wpInfosGUID[0]);
                var wp = AssetDatabase.LoadAssetAtPath<WaypointsInfo>(wpPath);
                _waypointsInfos.Add(wp);
            }
        }
        else if(_waypointsInfos != null && _waypointsInfos.Count > 0)
        {
            for (int i = 0; i < _waypointsInfos.Count; i++)
            {
                EditorGUILayout.ObjectField(_waypointsInfos[i], typeof(WaypointsInfo), false);
            }
        }
    }
}
