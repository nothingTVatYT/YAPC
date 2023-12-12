using System;
using FlaxEngine;

namespace YAPC;

public class YAPC : GamePlugin
{
    public YAPC()
    {
        _description = new PluginDescription
        {
            Name = "YAPC",
            Category = "Other",
            Author = "nothingTVatYT",
            AuthorUrl = null,
            HomepageUrl = null,
            RepositoryUrl = "https://github.com/nothingTVatYT/YAPC",
            Description = "This is yet another player controller.",
            Version = new Version(1, 0),
            IsAlpha = false,
            IsBeta = false,
        };
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Deinitialize()
    {
        base.Deinitialize();
    }
}