using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SceneGraphView : GraphView
{
    public readonly Vector2 defaultNodeSize = new Vector2(150, 200);
    public readonly Vector2 defaultNodePosition = new Vector2(400, 400);
    public SceneGraphView()
    {
        styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/SceneGraph/Resources/SceneGraph.uss"));
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var grid = new GridBackground();
        Insert(0,grid);
        grid.StretchToParentSize();

        //AddElement(GenerateEntryPointNode());
    }

    private Port GeneratePort(SceneNode node, Direction portDirection, Port.Capacity capacity = Port.Capacity.Single)
    {
        return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float));
    }


    private SceneNode GenerateEntryPointNode()
    {
        var node = new SceneNode
        {
            title = "START",
            GUID = Guid.NewGuid().ToString(),
            SceneText = "Default",
            EntryPoint = true
        };

        var generatedPort = GeneratePort(node, Direction.Output);
        generatedPort.portName = "Next";
        node.outputContainer.Add(generatedPort);

        node.capabilities &= Capabilities.Movable;
        node.capabilities &= Capabilities.Deletable;

        node.RefreshExpandedState();
        node.RefreshPorts();

        node.SetPosition(new Rect(100, 200, 100, 150));

        return node;
    }

    public void CreateNode(string nodeName)
    {
        AddElement(CreateSceneNode(nodeName));
    }

    public SceneNode CreateSceneNode(string nodeName, SceneAsset sceneAsset = null, string notes = null)
    {
        var sceneNode = new SceneNode
        {
            title = nodeName,
            SceneText = nodeName,
            GUID = Guid.NewGuid().ToString()
        };

        //var inputPort = GeneratePort(sceneNode, Direction.Input, Port.Capacity.Multi);
        //inputPort.portName = "Input";
        //sceneNode.inputContainer.Add(inputPort);

        var editNameButton = new Button(() =>
        {
            var textField = new TextField(string.Empty);
            sceneNode.titleContainer.Add(textField);
            textField.Focus();
            textField.RegisterValueChangedCallback((e) => { sceneNode.title = e.newValue; sceneNode.SceneText = e.newValue; });
            textField.RegisterCallback<FocusOutEvent>(e => sceneNode.titleContainer.Remove(textField));
            
        });
        editNameButton.text = "*";
        editNameButton.style.height = 17;
        editNameButton.style.width = 17;
        
        sceneNode.titleContainer.Add(editNameButton);

        sceneNode.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/SceneGraph/Resources/Node.uss"));

        //var oldTitle = sceneNode.titleContainer.Q<Label>();
        //sceneNode.titleContainer.Remove(oldTitle);

        //var textField = new TextField(string.Empty);
        //textField.RegisterValueChangedCallback(e =>
        //{
        //    sceneNode.SceneText = e.newValue;
        //    sceneNode.title = e.newValue;
        //});
        //textField.SetValueWithoutNotify(sceneNode.title);
        //sceneNode.titleContainer.Add(textField);
        var button = new Button(() =>
        {
            if (sceneNode.Scene == null) return;
            EditorSceneManager.SaveOpenScenes();
            EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(sceneNode.Scene));
            //AddChoicePort(sceneNode);
        });
        button.text = "Open";
        sceneNode.extensionContainer.Add(new Label(""));
        button.style.paddingTop = 10;
        button.style.paddingBottom = 10;
        button.style.marginTop = 20;
        button.style.marginBottom = 20;        //button.style.marginLeft = 20;
        //button.style.marginRight = 20;


        ObjectField sceneField = new ObjectField();
        sceneField.objectType = typeof(SceneAsset);

        sceneField.value = sceneAsset != null ? sceneAsset : null;
        sceneNode.Scene = sceneAsset != null ? sceneAsset : null;

        sceneField.RegisterValueChangedCallback(e =>
            {
                if(e.newValue == null)
                {
                    sceneNode.title = "None";
                    sceneNode.SceneText = "None";
                    sceneNode.Scene = null;
                }
                else
                {
                    sceneNode.title = e.newValue.name;
                    sceneNode.SceneText = e.newValue.name;
                    sceneNode.Scene = (SceneAsset)e.newValue;
                }


            });
        sceneNode.extensionContainer.Add(sceneField);


        
        sceneNode.extensionContainer.Add(button);
        sceneNode.extensionContainer.Add(new Label(""));

        TextField description = new TextField();
        description.multiline = true;
        description.style.whiteSpace = WhiteSpace.Normal;
        description.style.maxWidth = 200;
        description.SetValueWithoutNotify(notes != null ? notes : "Notes");
        sceneNode.Notes = notes != null ? notes : "Notes";

        description.RegisterValueChangedCallback(e => sceneNode.Notes = e.newValue);
        sceneNode.extensionContainer.Add(description);

        sceneNode.extensionContainer.style.backgroundColor = Color.grey;

        sceneNode.RefreshExpandedState();
        sceneNode.RefreshPorts();

        sceneNode.SetPosition(new Rect(defaultNodePosition, defaultNodeSize));

        return sceneNode;
    }

    public void AddChoicePort(SceneNode sceneNode, string overriddenPortName = "")
    {
        var generatedPort = GeneratePort(sceneNode, Direction.Output);

        var oldLabel = generatedPort.contentContainer.Q<Label>("type");
        generatedPort.contentContainer.Remove(oldLabel);

        var outputPortCount = sceneNode.outputContainer.Query("connector").ToList().Count;
       

        var choicePortName = string.IsNullOrEmpty(overriddenPortName)
            ? $"Choice {outputPortCount + 1}"
            : overriddenPortName;

        generatedPort.portName = choicePortName;

        var textField = new TextField
        {
            name = string.Empty,
            value = choicePortName
        };
        textField.RegisterValueChangedCallback(e => generatedPort.portName = e.newValue);
        generatedPort.contentContainer.Add(new Label(""));
        generatedPort.contentContainer.Add(textField);

        var deleteButton = new Button(() => RemovePort(sceneNode, generatedPort))
        {
            text = "X"
        };
        generatedPort.contentContainer.Add(deleteButton);


        sceneNode.outputContainer.Add(generatedPort);

        sceneNode.RefreshExpandedState();
        sceneNode.RefreshPorts();
    }

    private void RemovePort(SceneNode sceneNode, Port generatedPort)
    {
        var targetEdge = edges.ToList().Where(x => x.output.portName == generatedPort.portName && x.output.node == generatedPort.node);

        if (!targetEdge.Any())
        {
            var edge = targetEdge.First();
            edge.input.Disconnect(edge);
            RemoveElement(targetEdge.First());
        }

   

        sceneNode.outputContainer.Remove(generatedPort);
        sceneNode.RefreshExpandedState();
        sceneNode.RefreshPorts();
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();
        ports.ForEach((port) =>
        {
            if (startPort != port && startPort.node != port.node)
            {
                compatiblePorts.Add(port);
            }
        });
        return compatiblePorts;
    }
}
