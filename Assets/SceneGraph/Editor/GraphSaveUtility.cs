using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class GraphSaveUtility 
{
    private SceneGraphView _targetGraphView;
    private SceneContainer _containerCache;

    //private List<Edge> Edges => _targetGraphView.edges.ToList();
    private List<SceneNode> Nodes => _targetGraphView.nodes.ToList().Cast<SceneNode>().ToList();
    public static GraphSaveUtility GetInstance(SceneGraphView targetGraphView)
    {
        return new GraphSaveUtility
        {
            _targetGraphView = targetGraphView
        };
    }

    public void SaveGraph(string fileName)
    {
        var sceneContainer = ScriptableObject.CreateInstance<SceneContainer>();

        foreach(var sceneNode in Nodes.Where(node => !node.EntryPoint))
        {
            sceneContainer.SceneNodes.Add(new SceneNodeData
            {
                Guid = sceneNode.GUID,
                SceneText = sceneNode.SceneText,
                Position = sceneNode.GetPosition().position,
                Scene = sceneNode.Scene,
                Notes = sceneNode.Notes
                
            });

        }

        if(!AssetDatabase.IsValidFolder("Assets/SceneGraph/Data"))
        {
            AssetDatabase.CreateFolder("Assets/SceneGraph", "Data");
        }

        AssetDatabase.CreateAsset(sceneContainer, $"Assets/SceneGraph/Data/{fileName}.asset");
        AssetDatabase.SaveAssets();
    }

    public void LoadGraph(string fileName)
    {
        _containerCache = AssetDatabase.LoadAssetAtPath<ScriptableObject>($"Assets/SceneGraph/Data/{fileName}.asset") as SceneContainer;

        if(_containerCache == null)
        {
            EditorUtility.DisplayDialog("File Not Found", "Target scene graph file does not exist!", "OK");
            return;
        }
        ClearGraph();
        CreateNodes();
        //ConnectNodes();

    }

    private void ConnectNodes()
    {
        //for (int i = 0; i < Nodes.Count; i++)
        //{
        //    var connections = _containerCache.NodeLinks.Where(x => x.BaseNodeGuid == Nodes[i].GUID).ToList();
        //    for (int j = 0; j < connections.Count; j++)
        //    {
        //        var targetNodeGuid = connections[j].TargetNodeGuid;
        //        var targetNode = Nodes.First(x => x.GUID == targetNodeGuid);
        //        LinkNodes(Nodes[i].outputContainer[j].Q<Port>(), (Port)targetNode.inputContainer[0]);

        //        targetNode.SetPosition(new Rect(
        //            _containerCache.SceneNodes.First(x => x.Guid == targetNodeGuid).Position,
        //            _targetGraphView.defaultNodeSize));
        //    }
        //}
    }

    private void LinkNodes(Port output, Port input)
    {
        var tempEdge = new Edge
        {
            output = output,
            input = input
        };

        tempEdge?.input.Connect(tempEdge);
        tempEdge?.output.Connect(tempEdge);

        _targetGraphView.Add(tempEdge);
    }

    private void CreateNodes()
    {
        foreach (var node in _containerCache.SceneNodes)
        {
            var tempNode = _targetGraphView.CreateSceneNode(node.SceneText, node.Scene, node.Notes);
            tempNode.GUID = node.Guid;
            tempNode.SetPosition(new Rect(node.Position, _targetGraphView.defaultNodeSize));
            _targetGraphView.AddElement(tempNode);


            tempNode.RefreshExpandedState();
            tempNode.RefreshPorts();
            //var nodePorts = _containerCache.NodeLinks.Where(x => x.BaseNodeGuid == node.Guid).ToList();
            //nodePorts.ForEach(x => _targetGraphView.AddChoicePort(tempNode, x.PortName));
        }
    }

    private void ClearGraph()
    {
        //Nodes.Find(x => x.EntryPoint).GUID = _containerCache.NodeLinks[0].BaseNodeGuid;

        foreach(var node in Nodes)
        {
            if (node.EntryPoint) continue;

            //Edges.Where(x => x.input.node == node).ToList().ForEach(edge => _targetGraphView.RemoveElement(edge));

            _targetGraphView.RemoveElement(node);
        }
    }
}
