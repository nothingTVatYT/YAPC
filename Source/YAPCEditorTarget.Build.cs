using Flax.Build;

public class YAPCEditorTarget : GameProjectEditorTarget
{
    /// <inheritdoc />
    public override void Init()
    {
        base.Init();

        // Reference the modules for editor
        Modules.Add("YAPC");
        Modules.Add("YAPCEditor");
    }
}
