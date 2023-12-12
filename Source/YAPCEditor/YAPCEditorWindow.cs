using System.Collections.Generic;
using FlaxEditor;
using FlaxEditor.Content.Settings;
using FlaxEditor.CustomEditors;
using FlaxEngine;

namespace YAPCEditor;

public class YAPCEditorWindow : CustomEditorWindow
{
    private List<ActionConfig> _actionsMissing;
    private List<AxisConfig> _axisMappingsMissing;
    
    private ActionConfig[] _neededActions =
    {
        new() { Name = "Jump", Mode = InputActionMode.Press, Key = KeyboardKeys.Spacebar, MouseButton = MouseButton.None, GamepadButton = GamepadButton.RightTrigger, Gamepad = InputGamepadIndex.All },
        new() { Name = "Crouch", Mode = InputActionMode.Pressing, Key = KeyboardKeys.X, MouseButton = MouseButton.None, GamepadButton = GamepadButton.LeftShoulder, Gamepad = InputGamepadIndex.All },
        new() { Name = "Sprint", Mode = InputActionMode.Pressing, Key = KeyboardKeys.Shift, MouseButton = MouseButton.None, GamepadButton = GamepadButton.RightShoulder, Gamepad = InputGamepadIndex.All },
        new() { Name = "Fire", Mode = InputActionMode.Press, Key = KeyboardKeys.F, MouseButton = MouseButton.Left, GamepadButton = GamepadButton.A, Gamepad = InputGamepadIndex.All }
    };
    private AxisConfig[] _neededAxisMappings =
    {
        new() { Name = "Horizontal", Axis = InputAxisType.KeyboardOnly, Gamepad = InputGamepadIndex.Gamepad0, PositiveButton = KeyboardKeys.D, NegativeButton = KeyboardKeys.A, DeadZone = 0.01f, Sensitivity = 5f, Gravity = 5f, Scale = 1f, Snap = true },
        new() { Name = "Horizontal", Axis = InputAxisType.KeyboardOnly, Gamepad = InputGamepadIndex.Gamepad0, PositiveButton = KeyboardKeys.ArrowRight, NegativeButton = KeyboardKeys.ArrowLeft, DeadZone = 0.01f, Sensitivity = 5f, Gravity = 5f, Scale = 1f, Snap = true },
        new() { Name = "Horizontal", Axis = InputAxisType.GamepadLeftStickX, Gamepad = InputGamepadIndex.Gamepad0, PositiveButton = KeyboardKeys.None, NegativeButton = KeyboardKeys.None, DeadZone = 0.19f, Sensitivity = 1f, Gravity = 1f, Scale = 1f, Snap = false },
        new() { Name = "Vertical", Axis = InputAxisType.KeyboardOnly, Gamepad = InputGamepadIndex.Gamepad0, PositiveButton = KeyboardKeys.W, NegativeButton = KeyboardKeys.S, DeadZone = 0.01f, Sensitivity = 5f, Gravity = 5f, Scale = 1f, Snap = true },
        new() { Name = "Vertical", Axis = InputAxisType.KeyboardOnly, Gamepad = InputGamepadIndex.Gamepad0, PositiveButton = KeyboardKeys.ArrowUp, NegativeButton = KeyboardKeys.ArrowDown, DeadZone = 0.01f, Sensitivity = 5f, Gravity = 5f, Scale = 1f, Snap = true },
        new() { Name = "Vertical", Axis = InputAxisType.GamepadLeftStickY, Gamepad = InputGamepadIndex.Gamepad0, PositiveButton = KeyboardKeys.None, NegativeButton = KeyboardKeys.None, DeadZone = 0.19f, Sensitivity = 1f, Gravity = 1f, Scale = 1f, Snap = false },
        new() { Name = "Mouse X", Axis = InputAxisType.MouseX, Gamepad = InputGamepadIndex.Gamepad0, PositiveButton = KeyboardKeys.None, NegativeButton = KeyboardKeys.None, DeadZone = 1f, Sensitivity = 0.4f, Gravity = 1f, Scale = 1f, Snap = false },
        new() { Name = "Mouse X", Axis = InputAxisType.GamepadRightStickX, Gamepad = InputGamepadIndex.Gamepad0, PositiveButton = KeyboardKeys.None, NegativeButton = KeyboardKeys.None, DeadZone = 0.19f, Sensitivity = 1f, Gravity = 1f, Scale = 4f, Snap = false },
        new() { Name = "Mouse Y", Axis = InputAxisType.MouseY, Gamepad = InputGamepadIndex.Gamepad0, PositiveButton = KeyboardKeys.None, NegativeButton = KeyboardKeys.None, DeadZone = 1f, Sensitivity = 0.4f, Gravity = 1f, Scale = 1f, Snap = false },
        new() { Name = "Mouse X", Axis = InputAxisType.GamepadRightStickY, Gamepad = InputGamepadIndex.Gamepad0, PositiveButton = KeyboardKeys.None, NegativeButton = KeyboardKeys.None, DeadZone = 0.19f, Sensitivity = 1f, Gravity = 1f, Scale = 4f, Snap = false },
    };

    /// <inheritdoc />
    public override void Initialize(LayoutElementsContainer layout)
    {
        if (_actionsMissing != null)
        {
            var inputActionsGroup = layout.Group("Input Actions");
            if (_actionsMissing.Count == 0)
                inputActionsGroup.Label("All necessary input actions found.");
            else
            {
                foreach (var action in _actionsMissing)
                {
                    inputActionsGroup.Label($"{action.Name} ({action.Key})");
                }

                var addAll = inputActionsGroup.Button("Add all");
                addAll.Button.Clicked += OnAddAllActions;
            }
        }

        if (_axisMappingsMissing != null)
        {
            var axisMappingsGroup = layout.Group("Input Axis Mappings");
            if (_axisMappingsMissing.Count == 0)
                axisMappingsGroup.Label("All necessary input axis mappings found.");
            else
            {
                foreach (var axisMapping in _axisMappingsMissing)
                {
                    var description = axisMapping.Name + " (" + (axisMapping.Axis == InputAxisType.KeyboardOnly
                        ? (axisMapping.PositiveButton + ", " + axisMapping.NegativeButton)
                        : axisMapping.Axis) + ")";
                    axisMappingsGroup.Label(description);
                }

                var addAll = axisMappingsGroup.Button("Add all");
                addAll.Button.Clicked += OnAddAllAxisMappings;
            }
        }
    }

    private void OnAddAllActions()
    {
        var inputSettings = GameSettings.Load<InputSettings>() ?? new InputSettings();
        var oldCount = inputSettings.ActionMappings?.Length ?? 0;
        var newActionConfigs = new ActionConfig[oldCount + _actionsMissing.Count];
        inputSettings.ActionMappings?.CopyTo(newActionConfigs, 0);
        for (var i = 0; i < _actionsMissing.Count; i++)
        {
            newActionConfigs[oldCount + i] = _actionsMissing[i];
        }

        inputSettings.ActionMappings = newActionConfigs;
        GameSettings.Save(inputSettings);
        GameSettings.Apply();
        CheckInputSettings();
        RebuildLayout();
    }

    private void OnAddAllAxisMappings()
    {
        var inputSettings = GameSettings.Load<InputSettings>() ?? new InputSettings();
        var oldCount = inputSettings.AxisMappings?.Length ?? 0;
        var newAxisMappings = new AxisConfig[oldCount + _axisMappingsMissing.Count];
        inputSettings.AxisMappings?.CopyTo(newAxisMappings, 0);
        for (var i = 0; i < _axisMappingsMissing.Count; i++)
        {
            newAxisMappings[oldCount + i] = _axisMappingsMissing[i];
        }

        inputSettings.AxisMappings = newAxisMappings;
        GameSettings.Save(inputSettings);
        GameSettings.Apply();
        CheckInputSettings();
        RebuildLayout();
    }

    /// <summary>
    /// check the configured input settings for missing pieces needed for the player controller
    /// </summary>
    public void CheckInputSettings()
    {
        var actionsToAdd = new List<ActionConfig>(_neededActions);
        var axisMappingsToAdd = new List<AxisConfig>(_neededAxisMappings);

        foreach (var actionConfig in Input.ActionMappings)
        {
            actionsToAdd.RemoveAll(a => a.Name == actionConfig.Name);
        }

        _actionsMissing = actionsToAdd;

        foreach (var axisMapping in Input.AxisMappings)
        {
            axisMappingsToAdd.RemoveAll(a => a.Name == axisMapping.Name);
        }

        _axisMappingsMissing = axisMappingsToAdd;
    }

}