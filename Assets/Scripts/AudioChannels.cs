using UnityEngine;
using System;

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
