using System;
using System.Collections.Generic;
using UnityEngine.InputSystem.LowLevel;

public static class EventBus
{
    //Common
    public static event Action OnGameStart; //open game scene
    public static event Action OnGameEnd;

    public static event Action<bool> OnGamePaused;
    public static event Action<bool> OnMenuOpen;

    public static Action OnInventoryChanged;
    public static Action OnCollectionHumanReady;
    public static Action OnSacrificedButtonPressed;

    public static Action OnDayEnd;


    //Common

    public static void TriggerGameOpen() => OnGameStart?.Invoke();
    public static void TriggerGameEnd() => OnGameEnd?.Invoke();


    public static void TriggerGamePaused(bool isPaused) => OnGamePaused?.Invoke(isPaused);
    public static void TriggerMenuOpen(bool isMenuOpen) => OnMenuOpen?.Invoke(isMenuOpen);

    public static void TriggerInventoryChanged() => OnInventoryChanged?.Invoke();
    public static void TriggerCollectionHumanReady() => OnCollectionHumanReady?.Invoke();
    public static void TriggerSacrificedButtonPressed() => OnSacrificedButtonPressed?.Invoke();
    public static void TriggerDayEnd() => OnDayEnd?.Invoke();


    public static void ClearAllSubscriptions()
    {
        //Common
        OnGameStart = null;
        OnGameEnd = null;

        OnGamePaused = null;
        OnMenuOpen = null;

        OnInventoryChanged = null;
        OnCollectionHumanReady = null;
        OnSacrificedButtonPressed = null;
        OnDayEnd = null;
    }
}