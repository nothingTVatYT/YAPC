using System;
using System.Collections.Generic;
using System.Linq;
using FlaxEngine;

namespace YAPC;

public struct CategorizedSound
{
    public int CategoryId;
    public AudioClip[] Clips;
}

/// <summary>
/// FootstepsSound Script.
/// </summary>
public class FootstepsSound : Script
{
    public enum MovementType { Idle, Walking, Running }
    public CategorizedSound[] Sounds;
    public AudioSource FootstepsAudioSource;

    public Tag[] RelevantGroundTags = { Tag.Default };
    //[HideInEditor]
    public MovementType Movement;
    //[HideInEditor]
    /// <summary>
    /// set the ground tags to define which footstep sound should be used
    /// </summary>
    public Tag[] GroundTags
    {
        set
        {
            _currentGroundTag = Tag.Default;
            foreach (var tag in value)
                if (RelevantGroundTags.Contains(tag))
                {
                    _currentGroundTag = tag;
                    break;
                }
        }
    }

    private Tag _currentGroundTag;
    private MovementType _previousMovement;
    private Tag _previousGroundTag = Tag.Default;
    private readonly Random _random = new();
    private AudioClip _previousClip;

    /// <inheritdoc/>
    public override void OnUpdate()
    {
        var groundChanged = !_previousGroundTag.Equals(_currentGroundTag);
        var soundsChanges = groundChanged || _previousMovement != Movement;
        if (!soundsChanges)
            return;
        // categorize the current movement
        var category = Movement switch
        {
            MovementType.Walking => 1,
            MovementType.Running => 2,
            _ => 0
        };

        var tagIndex = RelevantGroundTags.Length == 0
            ? 0
            : Array.FindIndex(RelevantGroundTags, tag => tag == _currentGroundTag);
        if (tagIndex < 0)
            tagIndex = 0;
        category += tagIndex;
        AudioClip clip = null;
        foreach (var ac in Sounds)
        {
            if (ac.CategoryId == category)
                clip = ac.Clips[_random.Next(ac.Clips.Length)];
        }

        if (clip != null && clip.Equals(_previousClip))
        {
            _previousGroundTag = _currentGroundTag;
            _previousMovement = Movement;
            return;
        }
        if (FootstepsAudioSource.IsActuallyPlayingSth)
            FootstepsAudioSource.Stop();
        FootstepsAudioSource.Clip = clip;
        if (Movement != MovementType.Idle)
            FootstepsAudioSource.Play();
        _previousGroundTag = _currentGroundTag;
        _previousMovement = Movement;
        _previousClip = clip;
    }
}
