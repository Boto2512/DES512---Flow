public enum ModifierType {
    Flat,
    Additive,
    Subtractive,
    Multiplicative,
    PercentageIncrease,
    PercentageDecrease
}

public static class ModifierConversion {
    public static float PercentageIncreaseToMultiplicative(float percentageIncrease) {
        return 1 + percentageIncrease;
    }

    public static float PercentageDecreaseToMultiplicative(float percentageDecrease) {
        return 1 - percentageDecrease;
    }

    public static float MultiplicativeToPercentageIncrease(float multiplicative) {
        return 1 - multiplicative;
    }

    public static float MultiplicativeToPercentageDecrease(float multiplicative) {
        return 1 - multiplicative;
    }
}
