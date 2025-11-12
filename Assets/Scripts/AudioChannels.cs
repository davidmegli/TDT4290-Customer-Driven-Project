using UnityEngine;
using System;

/// <summary>
/// Defines distinct audio output channels that can be used within the Unity project.
/// The [Flags] attribute allows combining multiple channel values using bitwise operations.
/// </summary>
[Flags]
public enum AudioChannel
{
    None = 0,
    A = 1 << 0,
    B = 1 << 1, 
    C = 1 << 2,
    D = 1 << 3,
    All = A | B | C | D
}
