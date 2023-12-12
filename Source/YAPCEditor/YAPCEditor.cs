﻿using System;
using FlaxEditor;
using FlaxEditor.GUI.ContextMenu;

namespace YAPCEditor;

/// <summary>
/// YAPCEditor Plugin.
/// </summary>
public class YAPCEditor : EditorPlugin
{
    public override Type GamePluginType => typeof(YAPC.YAPC);

    private ContextMenuChildMenu _menu;

    /// <inheritdoc />
    public override void InitializeEditor()
    {
        base.InitializeEditor();
        _menu = Editor.UI.MenuTools.ContextMenu.AddChildMenu("YAPC");
        var button = _menu.ContextMenu.AddButton("Check Input Settings");
        button.Clicked += CheckInputSettings;
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

    private void CheckInputSettings()
    {
        var window = new YAPCEditorWindow();
        window.CheckInputSettings();
        window.Show();
    }
}
