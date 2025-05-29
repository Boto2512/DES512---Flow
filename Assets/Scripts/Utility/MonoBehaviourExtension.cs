using System;
using System.Collections;
using UnityEngine;

public static class MonoBehaviourExtension
{
    /// <summary>
    /// Invokes the method in time seconds
    /// </summary>
    /// <param name="self">Necessary to make it a MonoBehaviour extension method</param>
    /// <param name="method">Method to invoke</param>
    /// <param name="time">Delay in seconds before method invocation</param>
    public static void Invoke(this MonoBehaviour self, Action method, float time) {
        self.StartCoroutine(CoroutineToInvoke(method, time));
    }

    private static IEnumerator CoroutineToInvoke(Action method, float time) {
        yield return new WaitForSeconds(time);
        method();
    }
}
