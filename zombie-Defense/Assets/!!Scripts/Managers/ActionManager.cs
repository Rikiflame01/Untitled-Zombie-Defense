using UnityEngine;
using System;
using System.Diagnostics;
using Unity.VisualScripting;

public static class ActionManager
{
    public static event Action OnDefenseStart;
    public static event Action OnDefenseStop;
    public static event Action OnBuildStart;
    public static event Action OnBuildStop;
    public static event Action OnBuildSkip;
    public static event Action OnPaused;
    public static event Action OnUnpaused;

    public static event Action OnChooseCard;
    public static event Action OnChooseCardEnd;

    public static event Action<string> OnCardChosen;

    public static event Action OnWallDestroyed;

    public static void InvokeChooseCard()
    {
        OnChooseCard?.Invoke();
    }

    public static void InvokeChooseCardEnd()
    {
        OnChooseCardEnd?.Invoke();
    }

    public static void InvokeCardChosen(string cardChosen){
        OnCardChosen?.Invoke(cardChosen);
    }

    public static void InvokeWallDestroyed()
    {
        OnWallDestroyed?.Invoke();
    }
    
    public static void InvokeDefenseStart()
    {
        SoundManager.Instance.PlaySFX("waveStart",1f);
        SoundManager.Instance.SwitchToBattleMode();
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
