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
            Category = "Input",
            Author = "nothingTVatYT",
            AuthorUrl = null,
            HomepageUrl = null,
            RepositoryUrl = "https://github.com/nothingTVatYT/YAPC",
            Description = "This is yet another player controller but based on a RigidBody.",
            Version = new Version(1, 0),
            IsAlpha = false,
            IsBeta = false,
        };
    }
}