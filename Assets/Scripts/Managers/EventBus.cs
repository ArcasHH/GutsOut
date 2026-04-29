using System;
using System.Collections.Generic;
using UnityEngine.InputSystem.LowLevel;

public static class EventBus
{
    //Common
    public static event Action OnGameStart;
    public static event Action OnGameOpen; //open game scene
    public static event Action OnGameEnd;

    public static event Action<bool> OnGamePaused;
    public static event Action<bool> OnMenuOpen;

    public static Action OnInventoryChanged;
    public static Action OnCollectionHumanReady;
    public static Action OnSacrificedButtonPressed;

    //Common
    public static void TriggerGameStart() => OnGameStart?.Invoke();
    public static void TriggerGameOpen() => OnGameOpen?.Invoke();
    public static void TriggerGameEnd() => OnGameEnd?.Invoke();


    public static void TriggerGamePaused(bool isPaused) => OnGamePaused?.Invoke(isPaused);
    public static void TriggerMenuOpen(bool isMenuOpen) => OnMenuOpen?.Invoke(isMenuOpen);

    public static void TriggerInventoryChanged() => OnInventoryChanged?.Invoke();
    public static void TriggerCollectionHumanReady() => OnCollectionHumanReady?.Invoke();
    public static void TriggerSacrificedButtonPressed() => OnSacrificedButtonPressed?.Invoke();


    public static void ClearAllSubscriptions()
    {
        //Common
        OnGameStart = null;
        OnGameOpen = null;
        OnGameEnd = null;

        OnGamePaused = null;
        OnMenuOpen = null;

        OnInventoryChanged = null;
        OnCollectionHumanReady = null;
        OnSacrificedButtonPressed = null;
    }
}