using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class InvokeManager {

    private static readonly Dictionary<Hash128, Coroutine> invokedTasks = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]                  // because Unity doesn't like to inline functions
    private static Hash128 GenerateHash(MonoBehaviour mb, string key) {
        return Hash128.Compute(mb.GetInstanceID() + "@" + key);         // use of a delimiter is important here
    }

    private static IEnumerator CoroutineToInvoke(Hash128 hash, Action action, float delay) {
        yield return new WaitForSeconds(delay);
        action?.Invoke();

        lock (invokedTasks) {
            invokedTasks.Remove(hash);
        }
    }

    /// <summary>
    /// Invokes the action in delay seconds. Can be dangerous to use.
    /// Calling this using the same MonoBehaviour object and using the same string key before the a previous finishes means the reference to the previous is lost.
    /// </summary>
    /// <param name="mb"></param>
    /// <param name="key"></param>
    /// <param name="action"></param>
    /// <param name="delay"></param>
    public static void Invoke(MonoBehaviour mb, string key, Action action, float delay) {
        Hash128 hash = GenerateHash(mb, key);
        Coroutine invokedCoroutine = mb.StartCoroutine(CoroutineToInvoke(hash, action, delay));

        lock (invokedTasks) {
            invokedTasks[hash] = invokedCoroutine;
        }
    }

    /// <summary>
    /// Invokes the action in delay seconds if a same action isn't currently pending invocation.
    /// </summary>
    /// <param name="mb"></param>
    /// <param name="key"></param>
    /// <param name="action"></param>
    /// <param name="delay"></param>
    public static void InvokeExclusive(MonoBehaviour mb, string key, Action action, float delay) {
        Hash128 hash = GenerateHash(mb, key);

        lock (invokedTasks) {
            if (invokedTasks.ContainsKey(hash))
                return;

            Coroutine invokedCoroutine = mb.StartCoroutine(CoroutineToInvoke(hash, action, delay));
            invokedTasks[hash] = invokedCoroutine;
        }
    }

    /// <summary>
    /// Invokes the action in delay seconds, cancelling any of the same action that's currently pending invocation.
    /// </summary>
    /// <param name="mb"></param>
    /// <param name="key"></param>
    /// <param name="action"></param>
    /// <param name="delay"></param>
    public static void InvokeOverwrite(MonoBehaviour mb, string key, Action action, float delay) {
        Hash128 hash = GenerateHash(mb, key);

        lock (invokedTasks) {
            if (invokedTasks.TryGetValue(hash, out var pendingInvocation)) {
                mb.StopCoroutine(pendingInvocation);
            }

            Coroutine invokedCoroutine = mb.StartCoroutine(CoroutineToInvoke(hash, action, delay));
            invokedTasks[hash] = invokedCoroutine;
        }
    }

    /// <summary>
    /// Cancels any pending invocation attached to the MonoBehaviour object with the same key.
    /// </summary>
    /// <param name="mb"></param>
    /// <param name="key"></param>
    /// <returns>Signifies whether a pending invocation was cancelled</returns>
    public static bool InvokeCancel(MonoBehaviour mb, string key) {
        Hash128 hash = GenerateHash(mb, key);

        bool cancelled = false;
        lock (invokedTasks) {
            if (invokedTasks.TryGetValue(hash, out var pendingInvocation)) {
                mb.StopCoroutine(pendingInvocation);
                invokedTasks.Remove(hash);

                cancelled = true;
            }
        }

        return cancelled;
    }
}
