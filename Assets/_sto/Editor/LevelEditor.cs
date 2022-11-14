using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Level))]
[CanEditMultipleObjects]
public class LevelEditor : Editor {
    float timePerMove = .5f;

    Level level = null;
    private void OnEnable() {
        level = (Level)target;
    }

    // public override void OnInspectorGUI() {
    //     base.OnInspectorGUI();
    //     GUILayout.Label("Dev Info");
    //     GUILayout.Label("Solvable: " + level.IsSolvable() + " / max level items do not apply"); 
    //     int moves = level.GetNumberOfMovesToSolve();
    //     GUILayout.Label("Moves: " + moves); 
    //     GUILayout.Label("Rewards: " + (level._resItemPerItems == 0 ? "-" : Mathf.RoundToInt(moves/level._resItemPerItems))); 
    //     GUILayout.Label("Anticipated time: " + System.TimeSpan.FromSeconds(moves * timePerMove).ToString(@"mm\:ss") + " | @" + timePerMove + " sec/move");
    // }   
}
