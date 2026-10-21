using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace BeeHarvest.Editor
{
    public static class LevelOneSmallHiveSetup
    {
        /// <summary>Preserves the existing button bindings while replacing visible blocks with model presentation.</summary>
        public static void Apply()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Stop Play Mode before configuring small hives.");
            var controller = UnityEngine.Object.FindObjectOfType<LevelOneController>();
            if (controller == null || controller.Hud == null) throw new InvalidOperationException("The level controller and HUD are required.");
            if (controller.GetComponent<HiveModelView>() == null) controller.gameObject.AddComponent<HiveModelView>();
            var hud = controller.Hud;
            for (int i = 0; i < 5; i++)
            {
                hud.QueueImages[i].color = Color.clear;
                hud.QueueImages[i].raycastTarget = true;
                hud.QueueButtons[i].transition = Selectable.Transition.None;
                Configure(hud.QueueLabels[i], 32);
                Configure(hud.SlotLabels[i], 24);
                EditorUtility.SetDirty(hud.QueueImages[i]);
                EditorUtility.SetDirty(hud.QueueButtons[i]);
            }
            EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
        }

        static void Configure(Text label, int size)
        {
            label.fontSize = size;
            label.fontStyle = FontStyle.Bold;
            label.color = Color.white;
            label.raycastTarget = false;
            var outline = label.GetComponent<Outline>();
            if (outline == null) outline = label.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(.27f, .15f, .065f);
            outline.effectDistance = new Vector2(2, -2);
            EditorUtility.SetDirty(outline);
            EditorUtility.SetDirty(label);
        }
    }
}
