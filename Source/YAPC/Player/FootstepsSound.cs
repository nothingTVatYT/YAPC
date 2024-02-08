using System;
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
    public float MinTimeOfWalkingClip = 0.4f;
    public float MinTimeOfRunningClip = 0.3f;
    public Tag[] RelevantGroundTags = { Tag.Default };

    public bool CheckIsPlaying = false;
    //[HideInEditor]
    public MovementType Movement;

    private float _lastClipStarted;

    /// <summary>
    /// set the ground tags to define which footstep sound should be used
    /// </summary>
    public Tag[] GroundTags
    {
        set
        {
            _currentGroundTag = Tag.Default;
            if (value != null)
                foreach (var tag in value)
                    if (RelevantGroundTags.Contains(tag))
                    {
                        _currentGroundTag = tag;
                        break;
                    }
        }
    }

    private Tag _currentGroundTag;
    private readonly Random _random = new();

    /// <inheritdoc/>
    public override void OnUpdate()
    {
        // categorize the current movement
        var category = Movement switch
        {
            MovementType.Walking => 1,
            MovementType.Running => 2,
            _ => 0
        };

        AudioClip clip = null;

        if (category > 0)
        {
            var tagIndex = RelevantGroundTags.Length == 0
                ? 0
                : Array.FindIndex(RelevantGroundTags, tag => tag == _currentGroundTag);
            if (tagIndex < 0)
                tagIndex = 0;
            category += tagIndex;
            foreach (var ac in Sounds)
            {
                if (ac.CategoryId == category)
                    clip = ac.Clips[_random.Next(ac.Clips.Length)];
            }
        }

        if (clip != null)
        {
            if (FootstepsAudioSource.IsActuallyPlayingSth && CheckIsPlaying)
                return;
            var interval = Movement switch
            {
                MovementType.Walking => MinTimeOfWalkingClip,
                MovementType.Running => MinTimeOfRunningClip,
                _ => 1
            };
            if (Time.GameTime >= _lastClipStarted + interval)
            {
                FootstepsAudioSource.Stop();
                FootstepsAudioSource.Clip = clip;
                FootstepsAudioSource.Play();
                _lastClipStarted = Time.GameTime;
            }
        }
    }
}
