using System;
using UnityEditor;
using UnityEngine;

[Serializable]
public class SceneNodeData 
{
    public string Guid;
    public string SceneText;
    public Vector2 Position;
    [SerializeField]
    public SceneAsset Scene;
    public string Notes;
    
}
