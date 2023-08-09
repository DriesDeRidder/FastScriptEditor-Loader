using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

public class QuickFixerEditor : EditorWindow
{
    #region regions you need to be for adding your own editor

    #region Editors

    public struct EditorInfo
    {
        public string name;
        public string path;
        public string downloadLink;
    }

    public enum EditorType
    {
        VisualStudioCode,
        NotepadPlusPlus
    }

    public static EditorInfo[] editors = new EditorInfo[]
    {
        new EditorInfo { name = "VS Code" , path = @"C:\Users\" + Environment.UserName + @"\AppData\Local\Programs\Microsoft VS Code\Code.exe", downloadLink = "https://code.visualstudio.com/Download"},

        new EditorInfo { name = "NotePad++" , path = @"C:\Program Files\Notepad++\notepad++.exe", downloadLink = "https://notepad-plus-plus.org/downloads/"}
    };

    //These menu strings provide support for when you right click on a component in the Project window
    //The context commands provide support when right clicking a monobehaviour component in the inspector window
    const string OpenVSCodeMenu = "Assets/Open With VSCode";
    const string openVSCodeContext = "CONTEXT/MonoBehaviour/Edit Script With VSCode";

    const string OpenNotepadMenu = "Assets/Open With Notepad++";
    const string openNotepadContext = "CONTEXT/MonoBehaviour/Edit Script With Notepad++";

    private MonoScript scriptToEdit;

    #endregion


    #region Menu Commands

    //You can delete the editors you don't want to use here

    #region VSCode
    [MenuItem(OpenVSCodeMenu, false, 50)]
    private static void OpenWithVSCode()
    {
        OpenEditor(EditorType.VisualStudioCode);
    }

    [MenuItem(OpenVSCodeMenu, true)]
    private static bool ValidateOpenWithVSCode()
    {
        return ValidateOpenWithPath(EditorType.VisualStudioCode);
    }

    [MenuItem(openVSCodeContext, false, priority = -100)]
    private static void OpenWithVSCodeContext(MenuCommand command)
    {
        OpenEditor(EditorType.VisualStudioCode, command);
    }

    [MenuItem(openVSCodeContext, true, priority = -100)]
    private static bool ValidateOpenWithVSCodeContext()
    {
        return ValidateOpenWithContext(EditorType.VisualStudioCode);
    }
    #endregion

    #region Notepad++

    [MenuItem(OpenNotepadMenu, false, 50)]
    private static void OpenWithNotepad()
    {
        OpenEditor(EditorType.NotepadPlusPlus);
    }

    [MenuItem(OpenNotepadMenu, true)]
    private static bool ValidateOpenWithNotepadPlusPlus()
    {
        return ValidateOpenWithPath(EditorType.NotepadPlusPlus);
    }


    [MenuItem(openNotepadContext, false, priority = -100)]
    private static void OpenWithNotepadContext(MenuCommand command)
    {
        OpenEditor(EditorType.NotepadPlusPlus, command);
    }

    [MenuItem(openNotepadContext, true, priority = -100)]
    private static bool ValidateOpenWithNotepadContext()
    {
        return ValidateOpenWithContext(EditorType.NotepadPlusPlus);

    }
    #endregion

    #endregion

    #endregion



    #region regions you shouldn't be for adding your own editor
    #region validate functions


    // Validates if the selected asset in the project window is a script
    // and checks if the editor path exists.
    // If one of those returns false, the button will be greyed out
    private static bool ValidateOpenWithPath(EditorType editorType)
    {
        if (IsSelectedScript() == false) return false;

        EditorInfo editor = editors[(int)editorType];

        bool editorExists = File.Exists(editor.path.Replace("\"", ""));

        if (editorExists)
        {
            return true;
        }
        else
        {
            SetMissingEditorPopup(editorType);
            return false;
        }
    }

    private static bool ValidateOpenWithContext(EditorType editorType)
    {
        EditorInfo editor = editors[(int)editorType];

        bool editorExists = File.Exists(editor.path.Replace("\"", ""));

        if (editorExists)
        {
            return true;
        }
        else
        {
            SetMissingEditorPopup(editorType);
            return false;
        }
    }


    private static bool IsSelectedScript()
    {
        return Selection.activeObject is MonoScript;
    }
    #endregion


    #region Open Editor functions and arguments
    public static void OpenEditor(EditorType editorType)
    {
        OpenActualEditor(editorType, Selection.activeObject as MonoScript);
    }

    public static void OpenEditor(EditorType editorType, MonoScript scriptPath)
    {
        OpenActualEditor(editorType, scriptPath);
    }

    private static void OpenEditor(EditorType editorType, MenuCommand command)
    {
        OpenActualEditor(editorType, MonoScript.FromMonoBehaviour(command.context as MonoBehaviour));
    }

    private static void OpenActualEditor(EditorType editorType, MonoScript scriptPath)
    {
        QuickFixerEditor window = EditorWindow.GetWindow<QuickFixerEditor>();
        window.scriptToEdit = scriptPath;
        window.OpenEditorType(editorType);
        window.Close();
    }

    public void OpenEditorType(EditorType editorType)
    {
        string scriptPath = AssetDatabase.GetAssetPath(scriptToEdit);
        EditorInfo editor = editors[(int)editorType];

        // Added quotes around the script path so The process sees it as a single argument in case there are spaces in the path
        Process.Start(editor.path, $"\"{scriptPath}\"");
    }

    #endregion


    #region GUI

    public static void ShowWindow()
    {
        GetWindow(typeof(QuickFixerEditor));
    }
    private void OnGUI()
    {
        Rect popupRect = new(position.x + 20f, position.y + 60f, 260f, 100f);

        EditorWindow popup = GetWindowWithRect<EditorWindow>(popupRect, true, "Popup");
        popup.titleContent = new GUIContent("Quick Fixer");
        popup.ShowPopup();

        GUIContent missingEditorContent = new(missingEditorInfo);

        GUILayout.Label(missingEditorContent, EditorStyles.boldLabel);

        GUILayout.Space(5);

        GUIContent linkContent = new(infoURL);

        if (GUILayout.Button(linkContent, EditorStyles.linkLabel))
        {
            Application.OpenURL(linkURL);
            popup.Close();
        }

        if (GUILayout.Button("Don't show these popups again"))
        {
            EditorPrefs.SetBool("ShowQuickFixerPopupOption", false);
            UnityEngine.Debug.Log("<b>QuickFixer</b> Popups won't show up anymore. In case you'd like to see them again, you can go to preferences -> QuickFixer.");
            popup.Close();
        }
    }
    #endregion


    #region Missing editor popups
    private static string missingEditorInfo;
    private static string linkURL;
    private static string infoURL;

    private static void SetMissingEditorPopup(EditorType editorType)
    {
        //In Preferences you can set this to true in case you'd like to get a popup with the missing script editor

        if (QuickFixerEditorSettingsProvider.ShowMissingEditorPopup == false) return;

        EditorInfo editor = editors[(int)editorType];

        missingEditorInfo = "It seems you don't have " + editor.name + " installed";
        infoURL = "Click me to go to the " + editor.name + " download page";
        linkURL = editor.downloadLink;

        QuickFixerEditor window = EditorWindow.GetWindow<QuickFixerEditor>();
        window.Show();
    }
    #endregion
    #endregion
}