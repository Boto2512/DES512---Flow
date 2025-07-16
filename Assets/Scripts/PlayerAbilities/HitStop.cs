using Cysharp.Threading.Tasks;
using UnityEngine;

public static class HitStop {
    private static bool slowed = false;

    public static async UniTaskVoid Stop(float duration) {
        _ = Slow(0f, duration, ModifierType.Flat);
    }

    public static async UniTaskVoid Slow(float timeModifier, float duration, ModifierType modType = ModifierType.Flat) {
        if (slowed)
            return;
        slowed = true;

        float prevTimeScale = Time.timeScale;
        float newTimeScale = CalculateNewTimeScale(timeModifier, modType);

        Time.timeScale = newTimeScale;

        await UniTask.Delay((int)(duration * 1000), true);
        Time.timeScale = prevTimeScale;

        slowed = false;
    }

    //public static async UniTask Slow(float newTimeScale, float duration) {
    //    if (slowed)
    //        return;
    //    slowed = true;

    //    float prevTimeScale = Time.timeScale;
    //    Time.timeScale = newTimeScale;

    //    await UniTask.Delay((int)(duration * 1000));
    //    Time.timeScale = prevTimeScale;

    //    slowed = false;
    //}

    private static float CalculateNewTimeScale(float modifier, ModifierType modType) => modType switch {
        ModifierType.Flat => modifier,
        ModifierType.Additive => Time.timeScale + modifier,
        ModifierType.Subtractive => Time.timeScale - modifier,
        ModifierType.Multiplicative => Time.timeScale * modifier,
        ModifierType.PercentageIncrease => Time.timeScale * ModifierConversion.PercentageIncreaseToMultiplicative(modifier),
        ModifierType.PercentageDecrease => Time.timeScale * ModifierConversion.PercentageDecreaseToMultiplicative(modifier),
        _ => Time.timeScale
    };
}
