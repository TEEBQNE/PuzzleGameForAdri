using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System;
using System.Text.RegularExpressions;

[CustomPropertyDrawer(typeof(NamedArrayAttribute))]
public class NamedArrayDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Replace label with enum name if possible.
        try
        {
            var config = attribute as NamedArrayAttribute;
            var enum_names = Enum.GetNames(config.TargetEnum);
            var match = Regex.Match(property.propertyPath, "[-0-9]+", RegexOptions.RightToLeft);
            int pos = int.Parse(match.Groups[0].Value);

            // Make names nicer to read (but won't exactly match enum definition).
            var enum_label = ObjectNames.NicifyVariableName(enum_names[pos].ToLower());
            label = new GUIContent(enum_label);
        }
        catch
        {
            // keep default label
            Debug.Log("ERROR");
        }
        EditorGUI.PropertyField(position, property, label, property.isExpanded);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label);
    }
}
# endif

/// <summary>
/// Creates an attribute to convert a list above a list to the actual editor names
/// </summary>
#if UNITY_EDITOR
public class NamedArrayAttribute : PropertyAttribute
{
    public Type TargetEnum;
    public NamedArrayAttribute(Type TargetEnum)
    {
        this.TargetEnum = TargetEnum;
    }
}
#endif