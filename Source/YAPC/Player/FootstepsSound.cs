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

public struct CategorizedMaterial
{
    public int CategoryId;
    public PhysicalMaterial Material;
}

/// <summary>
/// FootstepsSound Script.
/// </summary>
public class FootstepsSound : Script
{
    public enum MovementType { Idle, Walking, Running }
    public CategorizedSound[] Sounds;
    public CategorizedMaterial[] Materials;
    public AudioSource FootstepsAudioSource;
    //[HideInEditor]
    public MovementType Movement;
    //[HideInEditor]
    public PhysicalMaterial GroundMaterial;

    private int _currentGroundCategory;
    private int _previousGroundCategory;
    private MovementType _previousMovement;
    private PhysicalMaterial _previousGroundMaterial;
    private readonly Random _random = new();
    private AudioClip _previousClip;

    /// <inheritdoc/>
    public override void OnUpdate()
    {
        var groundChanged = _previousGroundMaterial != null && !_previousGroundMaterial.Equals(GroundMaterial);
        var soundsChanges = groundChanged || _previousMovement != Movement;
        if (!soundsChanges)
            return;
        int category;
        // categorize the current movement
        switch (Movement)
        {
            case MovementType.Walking:
                category = 1;
                break;
            case MovementType.Running:
                category = 2;
                break;
            default:
                category = 0;
                break;
        }

        _currentGroundCategory = groundChanged ? GroundCategory(GroundMaterial) : _previousGroundCategory;
        category += _currentGroundCategory;
        AudioClip clip = null;
        foreach (var ac in Sounds)
        {
            if (ac.CategoryId == category)
                clip = ac.Clips[_random.Next(ac.Clips.Length)];
        }

        if (clip != null && clip.Equals(_previousClip))
        {
            _previousGroundCategory = _currentGroundCategory;
            _previousGroundMaterial = GroundMaterial;
            _previousMovement = Movement;
            return;
        }
        if (FootstepsAudioSource.IsActuallyPlayingSth)
            FootstepsAudioSource.Stop();
        FootstepsAudioSource.Clip = clip;
        if (Movement != MovementType.Idle)
            FootstepsAudioSource.Play();
        _previousGroundCategory = _currentGroundCategory;
        _previousGroundMaterial = GroundMaterial;
        _previousMovement = Movement;
        _previousClip = clip;
    }

    private int GroundCategory(PhysicalMaterial material)
    {
        return (from cm in Materials where cm.Material.Equals(material) select cm.CategoryId).FirstOrDefault();
    }
}
