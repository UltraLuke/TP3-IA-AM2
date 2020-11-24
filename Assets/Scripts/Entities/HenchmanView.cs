using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class HenchmanView : MonoBehaviour
{
    [SerializeField] Color color;
    IColorable[] _colorables;
    Renderer _renderer;

    private void OnValidate()
    {
        SetColor(color);
    }

    private void Awake()
    {
        _colorables = GetComponentsInChildren<IColorable>();
        _renderer = GetComponent<Renderer>();
    }

    public void SetColor(Color color)
    {
        if(_renderer != null)
            _renderer.material.color = color;

        if(_colorables == null)
            _colorables = GetComponentsInChildren<IColorable>();

        if (_colorables != null && _colorables.Length > 0)
        {
            for (int i = 0; i < _colorables.Length; i++)
                _colorables[i].SetColor(color);
        }
    }
}
