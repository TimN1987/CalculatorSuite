using CalculatorSuite.Struct;

namespace CalculatorSuite.Tools;

public static class FractionTools
{
    // Standard calculations and operations for fractions

    /// <summary>
    /// Simplifies the given fraction and returns the simplified version.
    /// </summary>
    public static Fraction Simplify(Fraction fraction)
    {
        int numerator = fraction.GetImproperNumerator();
        int denominator = fraction.GetDenominator();
        int hcf = HCF(numerator, denominator);

        numerator /= hcf;
        denominator /= hcf;

        return new Fraction(numerator, denominator);
    }

    /// <summary>
    /// Adds a pair of fractions using cross multiplication and returns the result as a simplified fraction.
    /// </summary>
    public static Fraction Add(Fraction a, Fraction b)
    {
        int numerator = a.GetImproperNumerator() * b.GetDenominator() + b.GetImproperNumerator() * a.GetDenominator();
        int denominator = a.GetDenominator() * b.GetDenominator();

        return Simplify(new Fraction(numerator, denominator));
    }

    /// <summary>
    /// Subtracts a pair of fractions using cross multiplication and returns the result as a simplified fraction.
    /// </summary>
    public static Fraction Subtract(Fraction a, Fraction b)
    {
        int numerator = a.GetImproperNumerator() * b.GetDenominator() - b.GetImproperNumerator() * a.GetDenominator();
        int denominator = a.GetDenominator() * b.GetDenominator();

        return Simplify(new Fraction(Math.Abs(numerator), denominator));
    }

    /// <summary>
    /// Multiplies a pair of fractions and returns the result as a simplified fraction.
    /// </summary>
    public static Fraction Multiply(Fraction a, Fraction b)
    {
        int numerator = a.GetImproperNumerator() * b.GetImproperNumerator();
        int denominator = a.GetDenominator() * b.GetDenominator();
        return Simplify(new Fraction(numerator, denominator));
    }

    /// <summary>
    /// Divides a pair of fractions and returns the result as a simplified fraction.
    /// </summary>
    public static Fraction Divide(Fraction a, Fraction b)
    {
        int numerator = a.GetImproperNumerator() * b.GetDenominator();
        int denominator = a.GetDenominator() * b.GetImproperNumerator();
        return Simplify(new Fraction(numerator, denominator));
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
