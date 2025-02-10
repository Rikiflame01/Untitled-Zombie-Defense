using UnityEngine;
using System;
using System.Diagnostics;

public static class ActionManager
{
    public static event Action OnDefenseStart;
    public static event Action OnDefenseStop;
    public static event Action OnBuildStart;
    public static event Action OnBuildStop;
    public static event Action OnBuildSkip;
    public static event Action OnPaused;
    public static event Action OnUnpaused;

    public static event Action OnWallDestroyed;

    public static void InvokeWallDestroyed()
    {
        OnWallDestroyed?.Invoke();
    }
    
    public static void InvokeDefenseStart()
    {
        OnDefenseStart?.Invoke();
    }

    public static void InvokeDefenseStop()
    {
        OnDefenseStop?.Invoke();
    }

    public static void InvokeBuildStart()
    {
        OnBuildStart?.Invoke();
    }

    public static void InvokeBuildStop()
    {
        OnBuildStop?.Invoke();
    }

    public static void InvokeBuildSkip()
    {
        OnBuildSkip?.Invoke();
    }

    public static void InvokePaused()
    {
        OnPaused?.Invoke();
    }

    public static void InvokeUnpaused()
    {
        OnUnpaused?.Invoke();
    }
}
