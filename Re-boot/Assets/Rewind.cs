using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.Networking;

public class Rewind : NetworkBehaviour
{
    private static readonly List<IRewindable> Rewindables = new List<IRewindable>();

    [Server]
    public static void AddRewindable(IRewindable obj)
    {
        Rewindables.Add(obj);
    }

    [Server]
    public static void RewindEveryone()
    {
        Rewindables.ForEach(obj => obj.Rewind());
    }
}