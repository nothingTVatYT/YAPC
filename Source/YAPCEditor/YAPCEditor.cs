using System;
using FlaxEditor;
using FlaxEditor.GUI.ContextMenu;

namespace YAPCEditor;

/// <summary>
/// YAPCEditor Plugin.
/// </summary>
public class YAPCEditor : EditorPlugin
{
    /// <inheritdoc />
    public override Type GamePluginType => typeof(YAPC.YAPC);

    private ContextMenuChildMenu _menu;

    /// <inheritdoc />
    public override void InitializeEditor()
    {
        base.InitializeEditor();
        _menu = Editor.UI.MenuTools.ContextMenu.AddChildMenu("YAPC");
        var button = _menu.ContextMenu.AddButton("Check Input Settings and Tags");
        button.Clicked += CheckSettings;
    }

    /// <inheritdoc />
    public override void DeinitializeEditor()
    {
        base.DeinitializeEditor();
        if (_menu != null)
        {
            _menu.Dispose();
            _menu = null;
        }
    }

    private void CheckSettings()
    {
        var window = new YAPCEditorWindow();
        window.CheckInputSettings();
        window.CheckTags();
        window.Show();
    }
}
