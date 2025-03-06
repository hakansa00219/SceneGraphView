using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class SceneNode : Node
{
    public string GUID;

    public string SceneText;

    public SceneAsset Scene;

    public string Notes;

    public bool EntryPoint = false;

}
