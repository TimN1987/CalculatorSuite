namespace CalculatorSuite.Struct;

public struct Fraction
{
    private int _integer;
    private int _numerator;
    private readonly int _denominator;

    /// <summary>
    /// Creates a unit fraction with the given denominator.
    /// </summary>
    public Fraction(int denominator) : this(0, 1, denominator) { }

    /// <summary>
    /// Creates a proper or improper fraction with the given numerator and denominator.
    /// </summary>
    public Fraction(int numerator, int denominator) : this(0, numerator, denominator) { }

    /// <summary>
    /// Creates a mixed number.
    /// </summary>
    public Fraction(int integer, int numerator, int denominator)
    {
        ValidateInputs(integer, numerator, denominator);
        NormalizeInputs(ref integer, ref numerator, ref denominator);

        _integer = integer;
        _numerator = numerator;
        _denominator = denominator;
    }

    /// <summary>
    /// Check that inputs are valid. For use in constructors.
    /// </summary>
    private void ValidateInputs(int integer, int numerator, int denominator)
    {
        if (numerator < 0 || integer < 0)
            throw new ArgumentOutOfRangeException(nameof(Fraction), "Numerator and integer cannot be negative.");
        if (denominator <= 0)
            throw new ArgumentOutOfRangeException(nameof(Fraction), "Denomator must be > 0.");
        if ((int.MaxValue - numerator) / denominator <= integer)
            throw new ArgumentOutOfRangeException(nameof(Fraction), "Improper fractions too large to handle.");
    }

    /// <summary>
    /// Normalizes the fraction values to ensure calculations with fractions behave as expected. Stores all 
    /// fractions as mixed numbers with a proper fraction part without simplification.
    /// </summary>
    private void NormalizeInputs(ref int integer, ref int numerator, ref int denominator)
    {
        int improperNumerator = numerator + integer * denominator;
        numerator = improperNumerator % denominator;
        integer = improperNumerator / denominator;
    }

    // Methods
    public bool IsUnitFraction() => _integer == 0 && _numerator == 1;
    public bool IsProper() => _integer == 0 && _numerator < _denominator;
    public bool IsImproper() => _integer > 0;
    public bool IsMixedNumber() => _integer > 0;
    public int GetProperNumerator() => _numerator;
    public int GetImproperNumerator() => _numerator + _integer * _denominator;
    public int GetDenominator() => _denominator;
    public int GetInteger() => _integer;
    public Fraction GetFraction() => new(0, _numerator + _integer * _denominator, _denominator);
    public Fraction GetMixedNumber() => new(_integer, _numerator, _denominator);

    /// <summary>
    /// Returns the fraction in decimal form.
    /// </summary>
    public double GetDecimal() => _integer + (double)_numerator / (double)_denominator;

    /// <summary>
    /// Returns the fraction as as percentage.
    /// </summary>
    public double GetPercentage() => GetDecimal() * 100;
}

public static class FractionExtensions
{
    /// <summary>
    /// Simplifies a fraction by dividing the numerator and denominator by their highest common factor (HCF).
    /// </summary>
    public static void Simplify(this ref Fraction fraction)
    {
        int numerator = fraction.GetImproperNumerator();
        int denominator = fraction.GetDenominator();
        int hcf = HCF(numerator, denominator);
        numerator /= hcf;
        denominator /= hcf;
        fraction = new Fraction(numerator, denominator);
    }

    /// <summary>
    /// Simplifies a fraction by dividing the numerator and denominator by their highest common factor (HCF).
    /// </summary>
    public static Fraction SimplifyFraction(this Fraction fraction)
    {
        int numerator = fraction.GetImproperNumerator();
        int denominator = fraction.GetDenominator();
        int hcf = HCF(numerator, denominator);
        numerator /= hcf;
        denominator /= hcf;
        return new Fraction(numerator, denominator);
    }

    /// <summary>
    /// Increases the fraction by another fraction using cross multiplication.
    /// </summary>
    public static void Add(this ref Fraction a, Fraction b)
    {
        int newNumerator = a.GetImproperNumerator() * b.GetDenominator() + b.GetImproperNumerator() * a.GetDenominator();
        int newDenominator = a.GetDenominator() * b.GetDenominator();
        a = new Fraction(newNumerator, newDenominator).SimplifyFraction();
    }

    /// <summary>
    /// Reduces the fraction by another fraction. Returns a zero fraction if the result is negative.
    /// </summary>
    public static void Subtract(this ref Fraction a, Fraction b)
    {
        int newNumerator = a.GetImproperNumerator() * b.GetDenominator() - b.GetImproperNumerator() * a.GetDenominator();
        int newDenominator = a.GetDenominator() * b.GetDenominator();
        a = newNumerator > 0 
            ? new Fraction(newNumerator, newDenominator).SimplifyFraction()
            : new Fraction(1);
    }

    /// <summary>
    /// Multiplies the fraction by another fraction.
    /// </summary>
    public static void Multiply(this ref Fraction a, Fraction b)
    {
        int newNumerator = a.GetImproperNumerator() * b.GetImproperNumerator();
        int newDenominator = a.GetDenominator() * b.GetDenominator();
        a = new Fraction(newNumerator, newDenominator).SimplifyFraction();
    }

    /// <summary>
    /// Divides the fraction by another fraction.
    /// </summary>
    public static void Divide(this ref Fraction a, Fraction b)
    {
        int newNumerator = a.GetImproperNumerator() * b.GetDenominator();
        int newDenominator = a.GetDenominator() * b.GetImproperNumerator();
        a = new Fraction(newNumerator, newDenominator).SimplifyFraction();
    }

    // Helper methods

    /// <summary>
    /// Computes the highest common factor (HCF) of two integers using the Euclidean algorithm.
    /// </summary>
    private static int HCF(int a, int b)
    {
        while (b != 0)
        {
            int temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }

    /// <summary>
    /// Computes the least common multiple (LCM) of two integers using the HCF formula.
    /// </summary>
    private static int LCM(int a, int b)
    {
        return (a / HCF(a, b)) * b;
    }
}
