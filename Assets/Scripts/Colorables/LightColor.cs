using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightColor : MonoBehaviour, IColorable
{
    [SerializeField] Light _light;

    private void Awake()
    {
        _light = GetComponent<Light>();
    }

    public void SetColor(Color color)
    {
        if (_light == null) return;
        _light.color = color;
    }
}
