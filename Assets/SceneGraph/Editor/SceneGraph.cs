using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class SceneGraph : EditorWindow
{
    private SceneGraphView _graphView;
    private string _fileName = "SceneGraph";

    [MenuItem("Graph/Scene Graph")]
    public static void OpenSceneGraphWindow()
    {
        var window = GetWindow<SceneGraph>();
        window.titleContent = new GUIContent("Scene Graph");
    }

    private void ConstructGraph()
    {
        _graphView = new SceneGraphView
        {
            name = "Scene Graph"
        };
        _graphView.StretchToParentSize();
        rootVisualElement.Add(_graphView);
    }

    private void GenerateToolbar()
    {
        var toolbar = new Toolbar();

        var fileNameTextField = new TextField("File Name");
        fileNameTextField.SetValueWithoutNotify(_fileName);
        fileNameTextField.MarkDirtyRepaint();
        fileNameTextField.RegisterValueChangedCallback(e => _fileName = e.newValue);
        toolbar.Add(fileNameTextField);

        toolbar.Add(new Button(() => RequestDataOperation(true))
        {
            text = "Save Data"
        });
        toolbar.Add(new Button(() => RequestDataOperation(false))
        {
            text = "Load Data"
        });

        var nodeCreateButton = new Button(() =>
        {
            _graphView.CreateNode("SceneGraph");
        });
        nodeCreateButton.text = "Create Node";
        toolbar.Add(nodeCreateButton);

        rootVisualElement.Add(toolbar);
    }

    private void RequestDataOperation(bool save)
    {
        if(string.IsNullOrEmpty(_fileName))
        {
            EditorUtility.DisplayDialog("Invalid file name!", "Please enter a valid file name.", "OK");
            return;
        }

        var saveUtility = GraphSaveUtility.GetInstance(_graphView);

        if (save) saveUtility.SaveGraph(_fileName);
        else saveUtility.LoadGraph(_fileName);
    }

    private void OnEnable()
    {
        ConstructGraph();
        GenerateToolbar();
        //GenerateMinimap();
        LoadLastSavedGraph();
    }

    private void LoadLastSavedGraph()
    {
        GraphSaveUtility.GetInstance(_graphView).LoadGraph("SceneGraph");
        //a
    }

    private void GenerateMinimap()
    {
        var miniMap = new MiniMap()
        {
            anchored = false
        };
        miniMap.SetPosition(new Rect(10, 30, 200, 140));
        _graphView.Add(miniMap);
    }

    private void OnDisable()
    {
        rootVisualElement.Remove(_graphView);
    }
}
