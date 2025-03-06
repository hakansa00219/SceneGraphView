using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SceneContainer : ScriptableObject
{
    //public List<NodeLinkingData> NodeLinks = new List<NodeLinkingData>();
    public List<SceneNodeData> SceneNodes = new List<SceneNodeData>();
}
