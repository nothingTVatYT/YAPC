﻿using Flax.Build;

/// <inheritdoc />
public class YAPCTarget : GameProjectTarget
{
    /// <inheritdoc />
    public override void Init()
    {
        base.Init();

        // Reference the modules for game
        Modules.Add("YAPC");
    }
}
