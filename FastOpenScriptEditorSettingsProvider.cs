using UnityEditor;
using UnityEngine;

public class FastOpenScriptEditorSettingsProvider : SettingsProvider
{
    const string k_ShowMissingEditorPopup = "ShowFastEditorPopupOption";

    public static bool ShowMissingEditorPopup
    {
        get { return EditorPrefs.GetBool(k_ShowMissingEditorPopup, false); }
        set { EditorPrefs.SetBool(k_ShowMissingEditorPopup, value); }
    }

    public FastOpenScriptEditorSettingsProvider(string path, SettingsScope scopes) : base(path, scopes)
    { }

    public override void OnGUI(string searchContext)
    {
        base.OnGUI(searchContext);

        GUILayout.Space(20f);

        bool enabled = ShowMissingEditorPopup;

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("Show missing editor popup", "Whether to show the missing editor popup when right clicking a script"), GUILayout.Width(200f));
        bool newValue = EditorGUILayout.Toggle(enabled);
        GUILayout.EndHorizontal();

        if (enabled != newValue)
        {
            ShowMissingEditorPopup = newValue;
        }
    }

    [SettingsProvider]
    public static SettingsProvider CreateSettingsProvider()
    {
        return new FastOpenScriptEditorSettingsProvider("Spectr/Tools/Fast Open Script Editor", SettingsScope.User);
    }
}
