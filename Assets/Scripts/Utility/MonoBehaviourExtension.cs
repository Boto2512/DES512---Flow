using System;
using System.Collections;
using UnityEngine;

public static class MonoBehaviourExtension
{
    [Obsolete("Doesn't subscribe to the new key system. Please use InvokeExclusive(string, Action, float) or InvokeOverwrite(string, Action, float) instead.")]
    /// <summary>
    /// Invokes the action in delay seconds
    /// </summary>
    /// <param name="self">Necessary to make it a MonoBehaviour extension method</param>
    /// <param name="action">Method to invoke</param>
    /// <param name="delay">Delay in seconds before method invocation</param>
    public static void Invoke(this MonoBehaviour self, Action action, float delay) {
        self.StartCoroutine(CoroutineToInvoke(action, delay));
    }

    private static IEnumerator CoroutineToInvoke(Action method, float time) {
        yield return new WaitForSeconds(time);
        method();
    }

    /// <summary>
    /// Invokes the action in delay seconds. Can be dangerous to use.
    /// Calling this using the same MonoBehaviour object and using the same string key before the a previous finishes means the reference to the previous is lost.
    /// </summary>
    /// <param name="key">Identifier string for the inner coroutine (key not shared amonst different MonoBehaviour instances)</param>
    /// <param name="action">Action to invoke</param>
    /// <param name="delay">Time till invokation</param>
    public static void Invoke(this MonoBehaviour self, string key, Action action, float delay) {
        InvokeManager.Invoke(self, key, action, delay);
    }

    /// <summary>
    /// Invokes the action in delay seconds if a same action isn't currently pending invokation.
    /// </summary>
    /// <param name="key">Identifier string for the inner coroutine (key not shared amonst different MonoBehaviour instances)</param>
    /// <param name="action">Action to invoke</param>
    /// <param name="delay">Time till invokation</param>
    public static void InvokeExclusive(this MonoBehaviour self, string key, Action action, float delay) {
        InvokeManager.InvokeExclusive(self, key, action, delay);
    }


    /// <summary>
    /// Invokes the action in delay seconds, cancelling any of the same action that's currently pending invokation.
    /// </summary>
    /// <param name="key">Identifier string for the inner coroutine (key not shared amonst different MonoBehaviour instances)</param>
    /// <param name="action">Action to invoke</param>
    /// <param name="delay">Time till invokation</param>
    public static void InvokeOverwrite(this MonoBehaviour self, string key, Action action, float delay) {
        InvokeManager.InvokeOverwrite(self, key, action, delay);
    }
}
