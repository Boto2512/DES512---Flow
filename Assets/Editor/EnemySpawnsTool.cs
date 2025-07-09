using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using Tools.ToolConstants;
using Unity.Mathematics;


public enum EditType
{
    Normal = 0,
    SpawnEdit = 1
}


[InitializeOnLoad]
public class EnemySpawnsTool : EditorWindow
{
    private SectionDatabase sectionDatabase;
    private SerializedObject serializedDatabase;

    private EditType editType;
    
    int selectedSectionIndex = 0;
    private SectionData selectedSection;
    
    private CharacterType selectedCharType;

    private string newSectionName = "New Section";
    
    
    [MenuItem("Tools/Enemy Spawns Editor")]
    public static void ShowWindow()
    {
        GetWindow<EnemySpawnsTool>("Enemy Spawns Editor");
    }
    
    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGui;
        
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGui;
    }
    
    private void OnGUI()
    {
        EditorGUILayout.LabelField("Enemy Spawns Editor", EditorStyles.largeLabel);
        EditorGUILayout.Space(45f);
        
        #region edit Type UI
        editType = (EditType)EditorGUILayout.EnumPopup("Edit Type", editType);
        #endregion
        
        EditorGUILayout.Space(10f);
        EditorGUILayout.HelpBox("Swap between edit modes, to edit either normally or to click spawn points into the world.", MessageType.Info);
        
        EditorGUILayout.Space(10f);
        
        sectionDatabase = (SectionDatabase)EditorGUILayout.ObjectField("Section Database", sectionDatabase, typeof(SectionDatabase), false);

        if (!sectionDatabase)
        {
            EditorGUILayout.HelpBox("Assign or create a section database to begin.", MessageType.Info);
            return;
        }
        
        EditorGUILayout.Space(30f);
        
        #region Section Selection UI

        if (sectionDatabase.sections == null)
        {
            sectionDatabase.sections = new List<SectionData>();
        }
        
        string[] sectionNames = sectionDatabase.sections.Select(s => s.sectionName).ToArray();
        if (sectionNames.Length > 0)
        {
            selectedSectionIndex = EditorGUILayout.Popup("Section", selectedSectionIndex, sectionNames);
        }
        else
            EditorGUILayout.HelpBox("No sections found. Please add one below!", MessageType.Warning);
        

        #endregion
        
        selectedSection = sectionDatabase.sections[selectedSectionIndex];
        
        EditorGUILayout.Space(10f);

        #region character Type UI
        selectedCharType = (CharacterType)EditorGUILayout.EnumPopup("Character Type", selectedCharType);
        #endregion

        
        
        EditorGUILayout.Space(40f);
        EditorGUILayout.LabelField("Create new Section and new Section Data Buttons:", EditorStyles.boldLabel);
        EditorGUILayout.Space(10f);
        
        EditorGUILayout.HelpBox("Buttons that can be used to create new sections or a new section database.", MessageType.Info);
        EditorGUILayout.Space(10f);
        
        newSectionName = EditorGUILayout.TextField(newSectionName);
        
        if (GUILayout.Button("Create new section"))
        {
            SectionData newSectionData = new SectionData
            {
                sectionName = newSectionName,
                enemySpawnData = new List<SpawnData>()
            };
            
            sectionDatabase.sections.Add(newSectionData);
            
            selectedSectionIndex = sectionDatabase.sections.Count - 1;
            selectedSection = sectionDatabase.sections[selectedSectionIndex];

            newSectionName = "new Section";
            
            EditorUtility.SetDirty(sectionDatabase);
            AssetDatabase.SaveAssets();
        }
        
        EditorGUILayout.Space(30f);

        if (GUILayout.Button("Create new section database"))
        {
            SectionDatabase newDatabase = ScriptableObject.CreateInstance<SectionDatabase>();
            newDatabase.sections = new List<SectionData>();

            string path = EditorUtility.SaveFilePanelInProject("Save Section Database", "NewSectionDatabase", "asset",
                                                                "Save Section Database");
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(newDatabase, path);
                AssetDatabase.SaveAssets();

                sectionDatabase = newDatabase;
                serializedDatabase = new SerializedObject(sectionDatabase);
            }
        }




        //Change Character type, on keyboard press of F1 or F2.
        if (editType == EditType.SpawnEdit)
        {
            Event e = Event.current;
            if (e.type == EventType.KeyDown)
            {
                switch (e.keyCode)
                {
                    case KeyCode.F1:
                        selectedCharType = CharacterType.Default;
                        SceneView.RepaintAll();
                        break;

                    case KeyCode.F2:
                        selectedCharType = CharacterType.Ranged;
                        SceneView.RepaintAll();
                        break;
                    case KeyCode.F3:
                        selectedCharType = CharacterType.Static;
                        SceneView.RepaintAll();
                        break;

                }

                e.Use();
            }
        }
    }

    private void OnSceneGui(SceneView sceneView)
    {
        if (selectedSection == null)
        {
            return;
            
        }
        if (editType == EditType.SpawnEdit)
        {
            Handles.BeginGUI();
            GUI.Label(new Rect(10,10,200,20), $"Current Character Type: {selectedCharType}", EditorStyles.boldLabel);
            Handles.EndGUI();   
            
            
            Event e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    Vector3 spawnPosition = hit.point;

                    if (sectionDatabase != null && sectionDatabase.sections.Count > 0)
                    {
                        if (OnNavMeshOrStaticType(hit))
                        {
                            SectionData section = sectionDatabase.sections[
                                                        selectedSectionIndex]; 

                            SpawnData newSpawnData = new SpawnData
                            {
                                characterType = selectedCharType, //Set to current selected  character type.
                                spawnPoint = spawnPosition,
                                spawnRotation = Quaternion.identity
                            };

                            section.enemySpawnData.Add(newSpawnData);
                            sectionDatabase.sections[selectedSectionIndex] = section; // Struct needs to be re-assigned.

                            EditorUtility.SetDirty(sectionDatabase); // Mark as dirty for saving.

                            SceneView.RepaintAll();
                        }
                        else
                        {
                            Debug.LogWarning("UNABLE TO SPAWN ENEMY: Check you have a NavMesh on the surface, or the character type is 'Static'.");
                        }
                    }

                    e.Use(); // Needs to consume event at end of code.
                }
            }

            int indexToRemove = -1;

            for (int i = 0; i < selectedSection.enemySpawnData.Count; i++)
            {
                Vector3 centerPoint = selectedSection.enemySpawnData[i].spawnPoint;
            
                Vector3[] vertices = GetVertices(centerPoint);
            
                Handles.DrawSolidRectangleWithOutline(vertices, ColourConstants.getCharacterColours(selectedSection.enemySpawnData[i].characterType),
                                                        ToolConstants._outlineColour);
                
                if (e.type == EventType.MouseMove || e.type == EventType.Repaint)
                {
                    if (IsMouseHovering(centerPoint, e.mousePosition))
                    {
                        Color HoverColour = Color.white;
                        SpawnVisualisation(centerPoint, vertices, HoverColour, true);
                    }
                }
                if(e.type == EventType.MouseDown && e.button == 1 && !e.alt)
                {
                    if (IsMouseHovering(centerPoint, e.mousePosition))
                    {
                        indexToRemove = i;
                        e.Use(); // e.Use consumes the event so it must be contained here, to allow the elements to run
                    }
                }
            }

            if (indexToRemove >= 0)
            {
                RemoveIndex(indexToRemove);
            }
        }
    }

    private void RemoveIndex(int index)
    {
        Undo.RecordObject(sectionDatabase, "Remove Spawn Point");

        Debug.Log($"Removing spawn at index {index} for section {selectedSectionIndex}");

        selectedSection.enemySpawnData.RemoveAt(index);
        sectionDatabase.sections[selectedSectionIndex] = selectedSection;

        EditorUtility.SetDirty(sectionDatabase);
        SceneView.RepaintAll();
    }
    

    private bool OnNavMeshOrStaticType(RaycastHit hit)
    {
        NavMeshHit navHit;
        bool onNavMesh = NavMesh.SamplePosition(hit.point, out navHit, 5.0f, NavMesh.AllAreas);

        return onNavMesh || selectedCharType == CharacterType.Static;
    }



    #region Visualisation Methods

    private static Vector3[] GetVertices(Vector3 centerPoint)
    {
        Vector3[] corners = new Vector3[4]
        {
        centerPoint + new Vector3(-ToolConstants._rectWidth/2, 0, -ToolConstants._rectHeight/2),
        centerPoint + new Vector3(-ToolConstants._rectWidth / 2, 0, ToolConstants._rectHeight / 2),
        centerPoint + new Vector3(ToolConstants._rectWidth / 2, 0, ToolConstants._rectHeight / 2),
        centerPoint + new Vector3(ToolConstants._rectWidth / 2, 0, -ToolConstants._rectHeight / 2)
        };
        return corners;
    }
    private static bool IsPointInPolygon(Vector2 mousePos, Vector2[] Shape)
    {
        int j = Shape.Length - 1;
        bool isPointInside = false;

        for(int i = 0; i < Shape.Length; j = i++)
        {
            if (((Shape[i].y > mousePos.y) != (Shape[j].y > mousePos.y)) &&
                (mousePos.x < (Shape[j].x - Shape[i].x) * (mousePos.y - Shape[i].y) /
                (Shape[j].y - Shape[i].y) + Shape[i].x))
            {
                isPointInside = !isPointInside;
            }
        }
        return isPointInside;
    }

    private void SpawnVisualisation(Vector3 centerPoint, Vector3[] vertices, Color colour, bool hovering = false)
    {
        Handles.DrawSolidRectangleWithOutline(vertices, colour, ToolConstants._outlineColour);
        if (hovering)
        {
            Handles.Label(centerPoint + Vector3.up * 0.5f, "Hovered!");
        }
        SceneView.RepaintAll();
    }

    private static bool IsMouseHovering(Vector3 center, Vector2 mousePosition)
    {
        Vector3[] corners = GetVertices(center);
        Vector2[] screenCorners = new Vector2[4];

        for (int i = 0; i < 4; i++)
        {
            screenCorners[i] = HandleUtility.WorldToGUIPoint(corners[i]);
        }

        return IsPointInPolygon(mousePosition, screenCorners);
    }
    #endregion
}
