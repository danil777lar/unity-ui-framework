using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Larje.Core.Ids;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Larje.UI.Editor
{
    public static class UIViewSync
    {
        private static readonly (string Folder, string Field, Type IdType)[] Kinds =
        {
            ("Screens", "screens", typeof(UIScreenId)),
            ("Popups", "popups", typeof(UIPopupId)),
            ("Toasts", "toasts", typeof(UIToastId))
        };

        [MenuItem("Tools/Larje/UI/Sync Views")]
        public static void SyncFromMenu()
        {
            Debug.Log(Sync());
        }

        public static void RunFromCommandLine()
        {
            int exitCode = 0;
            try
            {
                Debug.Log(Sync());
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                exitCode = 1;
            }

            EditorApplication.Exit(exitCode);
        }

        public static string Sync()
        {
            AssetDatabase.Refresh();
            UIConfig config = FindConfig();
            StringBuilder report = new StringBuilder("UI view sync:\n");
            SerializedObject serialized = new SerializedObject(config);
            foreach ((string folder, string field, Type idType) in Kinds)
            {
                string directory = $"{config.ViewsFolder}/{folder}";
                if (!AssetDatabase.IsValidFolder(directory))
                {
                    report.AppendLine($"  {folder}: no folder");
                }
                else
                {
                    SyncKind(serialized.FindProperty(field), directory, config.IdsFolder, idType, report);
                }
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            return report.ToString();
        }

        public static string ToKey(string fileName)
        {
            return string.Concat(fileName.Split(new[] { '_', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(part => char.ToUpperInvariant(part[0]) + part.Substring(1)));
        }

        private static UIConfig FindConfig()
        {
            string guid = AssetDatabase.FindAssets("t:UIConfig").FirstOrDefault();
            if (guid == null)
            {
                throw new InvalidOperationException("No UIConfig asset in the project");
            }

            return AssetDatabase.LoadAssetAtPath<UIConfig>(AssetDatabase.GUIDToAssetPath(guid));
        }

        private static void SyncKind(SerializedProperty entries, string directory, string idsFolder, Type idType, StringBuilder report)
        {
            foreach (string guid in AssetDatabase.FindAssets("t:VisualTreeAsset", new[] { directory }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                VisualTreeAsset layout = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
                string key = ToKey(Path.GetFileNameWithoutExtension(path));
                LarjeId id = LoadOrCreateId(idType, $"{idsFolder}/{idType.Name}/{key}.asset");
                SerializedProperty entry = FindEntry(entries, id);
                if (entry == null)
                {
                    entries.arraySize++;
                    entry = entries.GetArrayElementAtIndex(entries.arraySize - 1);
                    entry.FindPropertyRelative("id").objectReferenceValue = id;
                    report.AppendLine($"  added {key} -> {path}");
                }

                entry.FindPropertyRelative("layout").objectReferenceValue = layout;
            }
        }

        private static SerializedProperty FindEntry(SerializedProperty entries, LarjeId id)
        {
            for (int i = 0; i < entries.arraySize; i++)
            {
                SerializedProperty entry = entries.GetArrayElementAtIndex(i);
                if (entry.FindPropertyRelative("id").objectReferenceValue == id)
                {
                    return entry;
                }
            }

            return null;
        }

        private static LarjeId LoadOrCreateId(Type type, string path)
        {
            LarjeId id = (LarjeId)AssetDatabase.LoadAssetAtPath(path, type);
            if (id != null)
            {
                return id;
            }

            EnsureFolder(Path.GetDirectoryName(path).Replace('\\', '/'));
            id = (LarjeId)ScriptableObject.CreateInstance(type);
            AssetDatabase.CreateAsset(id, path);
            return id;
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }
    }
}
