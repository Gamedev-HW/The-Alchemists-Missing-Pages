using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PotionLab.EditorTools
{
    /// <summary>Fills in serialized fields from code, the same way you would drag things into the Inspector.</summary>
    public static class Wiring
    {
        /// <summary>
        /// Fills every empty object field of 'target': by field name when 'byName' has it, otherwise with the first
        /// object in 'byType' of the field's type.
        /// </summary>
        public static void AutoWire(Object target, IDictionary<string, Object> byName, IEnumerable<Object> byType)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty property = serialized.GetIterator();
            bool enterChildren = true;
            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (property.propertyType != SerializedPropertyType.ObjectReference || property.name == "m_Script") continue;
                if (property.objectReferenceValue != null) continue;
                property.objectReferenceValue = byName.TryGetValue(property.name, out Object named) ? named : MatchByType(property.type, byType);
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void Set(Object target, string field, Object value)
        {
            Edit(target, field, property => property.objectReferenceValue = value);
        }

        public static void SetArray<T>(Object target, string field, IList<T> values) where T : Object
        {
            Edit(target, field, property =>
            {
                property.arraySize = values.Count;
                for (int i = 0; i < values.Count; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            });
        }

        public static void SetString(Object target, string field, string value)
        {
            Edit(target, field, property => property.stringValue = value);
        }

        public static void SetBool(Object target, string field, bool value)
        {
            Edit(target, field, property => property.boolValue = value);
        }

        /// <summary>For int and enum fields.</summary>
        public static void SetInt(Object target, string field, int value)
        {
            Edit(target, field, property => property.intValue = value);
        }

        static void Edit(Object target, string field, System.Action<SerializedProperty> change)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(field);
            if (property == null)
            {
                Debug.LogError($"[Potion Lab] {target.GetType().Name} has no field called '{field}'.");
                return;
            }
            change(property);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>'type' looks like "PPtr&lt;$Workbench&gt;" for scripts and "PPtr&lt;Camera&gt;" for Unity's own components.</summary>
        static Object MatchByType(string type, IEnumerable<Object> candidates)
        {
            string typeName = type.Replace("PPtr<", "").Replace(">", "").Replace("$", "");
            foreach (Object candidate in candidates)
            {
                for (System.Type t = candidate.GetType(); t != null; t = t.BaseType)
                {
                    if (t.Name == typeName) return candidate;
                }
            }
            return null;
        }
    }
}
