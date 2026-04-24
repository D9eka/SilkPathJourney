using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Internal.Scripts.Attributes.Editor
{
    [CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
    public sealed class SubclassSelectorDrawer : PropertyDrawer
    {
        private static readonly Dictionary<string, (Type[] types, string[] names)> Cache = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            var (types, names) = GetTypes(property);
            DrawTypeDropdown(position, property, label, types, names);

            if (property.managedReferenceValue != null)
                DrawChildProperties(position, property);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;

            if (property.propertyType != SerializedPropertyType.ManagedReference || property.managedReferenceValue == null)
                return height;

            foreach (var child in EnumerateVisibleChildren(property))
                height += EditorGUI.GetPropertyHeight(child, true) + EditorGUIUtility.standardVerticalSpacing;

            return height;
        }

        private static void DrawTypeDropdown(Rect position, SerializedProperty property, GUIContent label, Type[] types, string[] names)
        {
            int currentIndex = GetCurrentTypeIndex(property, types);

            var dropdownRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            int selected = EditorGUI.Popup(dropdownRect, label.text, currentIndex, names);

            if (selected == currentIndex)
                return;

            property.managedReferenceValue = selected == 0
                ? null
                : Activator.CreateInstance(types[selected - 1]);
        }

        private static int GetCurrentTypeIndex(SerializedProperty property, Type[] types)
        {
            if (property.managedReferenceValue == null)
                return 0;

            return Array.IndexOf(types, property.managedReferenceValue.GetType()) + 1;
        }

        private static void DrawChildProperties(Rect position, SerializedProperty property)
        {
            EditorGUI.indentLevel++;

            foreach (var child in EnumerateVisibleChildren(property))
            {
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                position.height = EditorGUI.GetPropertyHeight(child, true);
                EditorGUI.PropertyField(position, child, true);
            }

            EditorGUI.indentLevel--;
        }

        private static IEnumerable<SerializedProperty> EnumerateVisibleChildren(SerializedProperty property)
        {
            var iterator = property.Copy();
            var end = property.GetEndProperty();

            if (!iterator.NextVisible(true))
                yield break;

            while (!SerializedProperty.EqualContents(iterator, end))
            {
                yield return iterator;
                if (!iterator.NextVisible(false))
                    yield break;
            }
        }

        private static (Type[] types, string[] names) GetTypes(SerializedProperty property)
        {
            string typeName = property.managedReferenceFieldTypename;
            if (Cache.TryGetValue(typeName, out var cached))
                return cached;

            var baseType = GetManagedReferenceFieldType(property);
            var types = baseType == null ? Array.Empty<Type>() : FindDerivedTypes(baseType);
            var names = BuildTypeNames(types);

            var result = (types, names);
            Cache[typeName] = result;
            return result;
        }

        private static Type[] FindDerivedTypes(Type baseType)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(SafeGetTypes)
                .Where(t => !t.IsAbstract && !t.IsInterface && !t.IsGenericTypeDefinition && baseType.IsAssignableFrom(t))
                .OrderBy(t => t.Name)
                .ToArray();
        }

        private static Type[] SafeGetTypes(Assembly assembly)
        {
            try { return assembly.GetTypes(); }
            catch { return Array.Empty<Type>(); }
        }

        private static string[] BuildTypeNames(Type[] types)
        {
            return types
                .Select(t => ObjectNames.NicifyVariableName(t.Name.Replace("InputConfig", "")))
                .Prepend("(None)")
                .ToArray();
        }

        private static Type GetManagedReferenceFieldType(SerializedProperty property)
        {
            var parts = property.managedReferenceFieldTypename.Split(' ');
            if (parts.Length != 2)
                return null;

            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == parts[0]);

            return assembly?.GetType(parts[1]);
        }
    }
}
