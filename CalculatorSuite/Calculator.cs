using CalculatorSuite.Struct;
using CalculatorSuite.Tools;

namespace CalculatorSuite;

public partial class Calculator : Form
{
    // Constant error messages
    private const string DivisionByZeroError = "Error: Division by zero";
    private const string MissingOperatorError = "Error: Missing operator";
    private const string InputTooLowError = "Error: Input too low";
    private const string InputAboveMillionError = "Error: Input max is 1,000,000";
    private const string ThinkingMessage = "Thinking...";

    // Constant values
    private const int BasicScreenLength = 16;
    private const int PrimeScreenLength = 12;
    private const int FractionScreenLength = 6;

    // UI fields
    private readonly Panel[] _panels;

    // Calculation fields
    private string _mainDisplay;
    private string _calculationString;
    private double _firstNumber;
    private double _secondNumber;
    private char _operator;
    private double _memoryValue;
    private bool _requiresOvertype;
    private bool _calculationCompleted;

    // Prime fields
    private ulong _primeValue;
    private List<int> _primesList;
    private int _index;

    // Fraction fields
    private Fraction _firstFraction;
    private Fraction _memoryFraction;
    private bool _isInteger;
    private bool _isNumerator;
    private bool _isDenominator;

    public Calculator()
    {
        InitializeComponent();

        // Set up panels array
        _panels = new Panel[3];
        _panels[0] = BasicCalculator;
        _panels[1] = PrimeCalculator;
        _panels[2] = FractionCalculator;
        ChangePanel(FractionCalculator);

        // Set up calculation fields
        _mainDisplay = "0";
        _calculationString = string.Empty;
        _firstNumber = 0;
        _secondNumber = 0;
        _operator = ' ';
        _memoryValue = 0;
        _requiresOvertype = false;
        _calculationCompleted = false;

        _primeValue = 0;
        _primesList = [];
        _index = 0;

        _firstFraction = new Fraction(0, 0, 1);
        _memoryFraction = new Fraction(0, 0, 1);
        _isInteger = true;
        _isNumerator = false;
        _isDenominator = false;

        BasicScreen.Text = _mainDisplay;
        PrimeScreen.Text = _mainDisplay;
    }

    private void Calculator_Load(object sender, EventArgs e)
    {
    }

    // UI methods

    /// <summary>
    /// Updates the visible panel to the specified panel. Allows the user to change the visible calculator panel 
    /// on a button click.
    /// </summary>
    private void ChangePanel(Panel panel)
    {
        foreach (var p in _panels)
        {
            p.Visible = false;
        }
        panel.Visible = true;
        Reset();
        ViewScreen.Text = panel.Name;
    }


    // Helper methods

    private void Reset()
    {
        _mainDisplay = "0";
        _calculationString = string.Empty;
        _firstNumber = 0;
        _secondNumber = 0;
        _operator = ' ';
        _memoryValue = 0;
        _requiresOvertype = false;
        _calculationCompleted = false;

        _primeValue = 0;
        _primesList = [];
        _index = 0;

        ResetFractionParts();
        _firstFraction = new Fraction(1);
        _memoryFraction = new Fraction(1);
        _isInteger = true;
        _isNumerator = false;
        _isDenominator = false;

        UpdateScreens();
    }

    private void UpdateScreens(string? message = null)
    {
        message ??= _mainDisplay;

        BasicScreen.Text = message;
        PrimeScreen.Text = message;
        if (_isInteger)
            IntegerScreen.Text = message;
        if (_isNumerator)
            NumeratorScreen.Text = message;
        if (_isDenominator)
            DenominatorScreen.Text = message;
    }

    /// <summary>
    /// Updates the main display when a number button is clicked.
    /// </summary>
    private void OnNumberButtonClick(string number)
    {
        // If number clicked after calculation completed, reset calculator to start new calculation.
        if (_calculationCompleted)
        {
            Reset();
            return;
        }

        if (_mainDisplay == "0" || _requiresOvertype)
        {
            _mainDisplay = number;
            _requiresOvertype = false;
        }
        else
            _mainDisplay += number;
        UpdateScreens();
    }

    /// <summary>
    /// Updates the first number and sets the operator when a basic operator button is clicked. Allows a second 
    /// number to be entered for calculation. If a calculation is already in progress, it will complete the previous 
    /// calculation and use the result as the first number for the new operation.
    /// </summary>
    private void OnOperatorButtonClick(char operatorChar)
    {
        // If operator clicked after calculation completed, continue calculation with result as first number.
        if (_calculationCompleted)
            _calculationCompleted = false;

        // If there is no first number or operator yet, set these.
        if (_firstNumber == 0 && _operator == ' ')
        {
            _firstNumber = double.Parse(_mainDisplay);
            _operator = operatorChar;
        }
        else
        {
            _secondNumber = double.Parse(_mainDisplay);

            double result = PerformCalculation();

            _firstNumber = result;
            _secondNumber = 0;
            _operator = operatorChar;

            _mainDisplay = result.ToString();
        }

        _requiresOvertype = true;
    }

    private void OnFractionOperatorButtonClick(char operatorChar)
    {
        // If operator clicked after calculation completed, continue calculation with result as first number.
        if (_calculationCompleted)
            _calculationCompleted = false;

        // If there is no first number or operator yet, set these.
        if (_operator == ' ')
        {
            _firstFraction = ReadFractionFromScreen();
            _operator = operatorChar;
        }
        else
        {
            Fraction secondFraction = ReadFractionFromScreen();

            switch (_operator)
            {
                case '+':
                    _firstFraction.Add(secondFraction);
                    break;
                case '-':
                    _firstFraction.Subtract(secondFraction);
                    break;
                case '*':
                    _firstFraction.Multiply(secondFraction);
                    break;
                case '/':
                    _firstFraction.Divide(secondFraction);
                    break;
                default:
                    throw new Exception("Unknown operator");
            }

            _operator = operatorChar;
            SetFractionParts("integer", _firstFraction.GetInteger().ToString());
            SetFractionParts("numerator", _firstFraction.GetProperNumerator().ToString());
            SetFractionParts("denominator", _firstFraction.GetDenominator().ToString());
        }

        _requiresOvertype = true;
        _isInteger = true;
        _isNumerator = false;
        _isDenominator = false;
        _mainDisplay = "0";
    }

    /// <summary>
    /// Performs a calculation based on the current operator and the two numbers stored.
    /// </summary>
    private double PerformCalculation()
    {
        return _operator switch
        {
            '+' => _firstNumber + _secondNumber,
            '-' => _firstNumber - _secondNumber,
            '*' => _firstNumber * _secondNumber,
            '/' => _secondNumber != 0
                ? _firstNumber / _secondNumber
                : throw new DivideByZeroException("Division by zero attempted!"),
            '%' => _secondNumber != 0
                ? _firstNumber / _secondNumber * 100
                : throw new DivideByZeroException("Division by zero attempted!"),
            _ => 0
        };
    }

    /// <summary>
    /// Performs a calculation based on the stored fraction value and the displayed fraction value.
    /// </summary>
    private void PerformFractionCalculation()
    {
        Fraction secondFraction = ReadFractionFromScreen();

        switch (_operator)
        {
            case '+':
                _firstFraction.Add(secondFraction);
                break;
            case '-':
                _firstFraction.Subtract(secondFraction);
                break;
            case '*':
                _firstFraction.Multiply(secondFraction);
                break;
            case '/':
                _firstFraction.Divide(secondFraction);
                break;
            default:
                throw new ArgumentException("Invalid operator entered.");
        }
    }

    /// <summary>
    /// Updates the screen with an error message if division by zero is attempted.
    /// </summary>
    private void HandleDivisionByZero()
    {
        Reset();
        UpdateScreens(DivisionByZeroError);
        _requiresOvertype = true;
    }

    /// <summary>
    /// Allows fraction screen updates by assigning the message to the given part.
    /// </summary>
    private void SetFractionParts(string part, string message)
    {
        if (!new string[] { "integer", "numerator", "denominator" }.Contains(part))
            throw new ArgumentException("Invalid part parameter. Must be 'integer', 'numerator' or 'denominator'.");

        _isInteger = part == "integer";
        _isNumerator = part == "numerator";
        _isDenominator = part == "denominator";
        UpdateScreens(message);
    }

    private void ResetFractionParts()
    {
        SetFractionParts("integer", "0");
        SetFractionParts("numerator", "0");
        SetFractionParts("denominator", "1");
    }

    private Fraction ReadFractionFromScreen()
    {
        if (int.TryParse(IntegerScreen.Text, out int integer)
            && int.TryParse(NumeratorScreen.Text, out int numerator)
            && int.TryParse(DenominatorScreen.Text, out int denominator))
        {
            return new Fraction(integer, numerator, denominator);
        }

        return new Fraction(1);
    }

    // Navigation button click methods

    private void BasicButton_Click(object sender, EventArgs e)
    {
        ChangePanel(BasicCalculator);
    }

    private void PrimeButton_Click(object sender, EventArgs e)
    {
        ChangePanel(PrimeCalculator);
    }

    private void FractionButton_Click(object sender, EventArgs e)
    {
        ChangePanel(FractionCalculator);
    }

    // Basic calculator button click methods

    /// <summary>
    /// Clears the screen and resets all calculation fields when the 'AC' button is clicked.
    /// </summary>
    private void ACButton_Click(object sender, EventArgs e)
    {
        Reset();
    }

    /// <summary>
    /// Clears the current entry on the screen when the 'C' button is clicked.
    /// </summary>
    private void CButton_Click(object sender, EventArgs e)
    {
        _mainDisplay = "0";
        UpdateScreens();
    }

    /// <summary>
    /// Adds a '0' to the current display when the '0' button is clicked if it is not already exactly '0'.
    /// </summary>
    private void ZeroButton_Click(object sender, EventArgs e)
    {
        OnNumberButtonClick("0");
    }

    /// <summary>
    /// Adds a '1' to the current display when the '1' button is clicked or replaces the display if it is '0'.
    /// </summary>
    private void OneButton_Click(object sender, EventArgs e)
    {
        OnNumberButtonClick("1");
    }

    /// <summary>
    /// Adds a '2' to the current display when the '2' button is clicked or replaces the display if it is '0'.
    /// </summary>
    private void TwoButton_Click(object sender, EventArgs e)
    {
        OnNumberButtonClick("2");
    }

    /// <summary>
    /// Adds a '3' to the current display when the '3' button is clicked or replaces the display if it is '0'.
    /// </summary>
    private void ThreeButton_Click(object sender, EventArgs e)
    {
        OnNumberButtonClick("3");
    }

    /// <summary>
    /// Adds a '4' to the current display when the '4' button is clicked or replaces the display if it is '0'.
    /// </summary>
    private void FourButton_Click(object sender, EventArgs e)
    {
        OnNumberButtonClick("4");
    }

    /// <summary>
    /// Adds a '5' to the current display when the '5' button is clicked or replaces the display if it is '0'.
    /// </summary>
    private void FiveButton_Click(object sender, EventArgs e)
    {
        OnNumberButtonClick("5");
    }

    /// <summary>
    /// Adds a '6' to the current display when the '6' button is clicked or replaces the display if it is '0'.
    /// </summary>
    private void SixButton_Click(object sender, EventArgs e)
    {
        OnNumberButtonClick("6");
    }

    /// <summary>
    /// Adds a '7' to the current display when the '7' button is clicked or replaces the display if it is '0'.
    /// </summary>
    private void SevenButton_Click(object sender, EventArgs e)
    {
        OnNumberButtonClick("7");
    }

    /// <summary>
    /// Adds a '8' to the current display when the '8' button is clicked or replaces the display if it is '0'.
    /// </summary>
    private void EightButton_Click(object sender, EventArgs e)
    {
        OnNumberButtonClick("8");
    }

    /// <summary>
    /// Adds a '9' to the current display when the '9' button is clicked or replaces the display if it is '0'.
    /// </summary>
    private void NineButton_Click(object sender, EventArgs e)
    {
        OnNumberButtonClick("9");
    }

    /// <summary>
    /// Updates the operator to addition when the '+' button is clicked and stores the first number for calculation.
    /// </summary>
    private void PlusButton_Click(object sender, EventArgs e)
    {
        try
        {
            OnOperatorButtonClick('+');
        }
        catch (DivideByZeroException)
        {
            HandleDivisionByZero();
        }
    }

    /// <summary>
    /// Updates the operator to addition when the '-' button is clicked and stores the first number for calculation.
    /// </summary>
    private void MinusButton_Click(object sender, EventArgs e)
    {
        try
        {
            OnOperatorButtonClick('-');
        }
        catch (DivideByZeroException)
        {
            HandleDivisionByZero();
        }
    }

    /// <summary>
    /// Updates the operator to addition when the '*' button is clicked and stores the first number for calculation.
    /// </summary>
    private void TimesButton_Click(object sender, EventArgs e)
    {
        try
        {
            OnOperatorButtonClick('*');
        }
        catch (DivideByZeroException)
        {
            HandleDivisionByZero();
        }
    }

    /// <summary>
    /// Updates the operator to addition when the '/' button is clicked and stores the first number for calculation.
    /// </summary>
    private void DivideButton_Click(object sender, EventArgs e)
    {
        try
        {
            OnOperatorButtonClick('/');
        }
        catch (DivideByZeroException)
        {
            HandleDivisionByZero();
        }
    }

    /// <summary>
    /// Peforms the calculation when the '=' button is clicked and updates the display with the result. Resets 
    /// the second number and operator for future calculations.
    /// </summary>
    private void EqualsButton_Click(object sender, EventArgs e)
    {
        if (_operator == ' ')
        {
            UpdateScreens(MissingOperatorError);
            return;
        }

        try
        {
            _secondNumber = double.Parse(_mainDisplay);
            bool isPercentage = _operator == '%';
            double result = PerformCalculation();
            _firstNumber = 0;
            _secondNumber = 0;
            _operator = ' ';
            _mainDisplay = result.ToString();

            // Add a percentage sign if the operation was a percentage calculation.
            // Do not store the percentage sign in the main display string to allow further calculations.
            UpdateScreens(isPercentage ? _mainDisplay + '%' : _mainDisplay);

        }
        catch (DivideByZeroException)
        {
            HandleDivisionByZero();
        }
        finally
        {
            _requiresOvertype = true;
        }
    }

    /// <summary>
    /// Adds a decimal point to the display number if it doesn't contain one already to allow decimals to be used.
    /// </summary>
    private void DecimalButton_Click(object sender, EventArgs e)
    {
        if (_requiresOvertype)
            _mainDisplay = "0";

        if (_mainDisplay.Contains('.'))
            return;
        _mainDisplay += ".";
        UpdateScreens();
    }

    /// <summary>
    /// Used to change the sign of the current number on the display when the '+/-' button is clicked.
    /// </summary>
    private void SignButton_Click(object sender, EventArgs e)
    {
        if (_requiresOvertype)
            _requiresOvertype = false;

        if (_mainDisplay.StartsWith('-'))
            _mainDisplay = _mainDisplay[1..];
        else if (_mainDisplay != "0")
            _mainDisplay = "-" + _mainDisplay;
        UpdateScreens();
    }

    /// <summary>
    /// Squares the displayed number when the 'x²' button is clicked.
    /// </summary>
    private void SquareButton_Click(object sender, EventArgs e)
    {
        double displayValue = double.Parse(_mainDisplay);
        double result = displayValue * displayValue;
        _mainDisplay = result.ToString();
        UpdateScreens();

        _requiresOvertype = true;
    }

    /// <summary>
    /// Square roots the displayed number when the square root button is clicked.
    /// </summary>
    private void SqrtButton_Click(object sender, EventArgs e)
    {
        double displayValue = double.Parse(_mainDisplay);
        double result = Math.Sqrt(displayValue);
        _mainDisplay = result.ToString("G16");
        UpdateScreens();

        _requiresOvertype = true;
    }

    /// <summary>
    /// Calculates the percentage of the displayed number when the '%' button is clicked.
    /// </summary>
    private void PercentageButton_Click(object sender, EventArgs e)
    {
        try
        {
            OnOperatorButtonClick('%');
        }
        catch (DivideByZeroException)
        {
            HandleDivisionByZero();
        }
    }

    /// <summary>
    /// Stores the current displayed number into memory when the 'MIn' button is clicked.
    /// </summary>
    private void MInButton_Click(object sender, EventArgs e)
    {
        _memoryValue = double.Parse(_mainDisplay);
    }

    /// <summary>
    /// Retrieves the stored memory number and displays it when the 'MR' button is clicked.
    /// </summary>
    private void MRButton_Click(object sender, EventArgs e)
    {
        _mainDisplay = _memoryValue.ToString();
        UpdateScreens();
    }

    // Prime calculator button click methods

    /// <summary>
    /// Determines if the displayed number is prime when the 'PPrime' button is clicked and updates the 
    /// display to indicate the result.
    /// </summary>
    private void PPrimeButton_Click(object sender, EventArgs e)
    {
        _primeValue = ulong.Parse(_mainDisplay);

        try
        {
            if (PrimeTools.IsPrime(_primeValue))
            {
                UpdateScreens($"{_primeValue} is prime.");
            }
            else
            {
                UpdateScreens($"{_primeValue} is not prime.");
            }
        }
        catch (ArgumentOutOfRangeException)
        {
            UpdateScreens(InputTooLowError);
        }

        _requiresOvertype = true;
    }

    /// <summary>
    /// Finds the last prime number less than or equal to the displayed number when the 'PLast' button is 
    /// clicked and updates the display with the result.
    /// </summary>
    private void PLastButton_Click(object sender, EventArgs e)
    {
        _primeValue = ulong.Parse(_mainDisplay);

        try
        {
            if (_primeValue > 1000000)
                throw new ArgumentOutOfRangeException(nameof(PrimeTools), "Input cannot be greater than 1,000,000.");
            _primesList = PrimeTools.ListPrimes((int)_primeValue);
            _mainDisplay = _primesList.Last().ToString();
            UpdateScreens();
        }
        catch (ArgumentOutOfRangeException ex)
        {
            string message = ex.Message.Contains(" low ") ? InputTooLowError : InputAboveMillionError;
            UpdateScreens(message);
        }

        _requiresOvertype = true;
    }

    /// <summary>
    /// Counts the number of prime numbers less than or equal to the displayed number when the 'PCount' button 
    /// is clicked and updates the display with the result.
    /// </summary>
    private async void PCountButton_Click(object sender, EventArgs e)
    {
        _primeValue = ulong.Parse(_mainDisplay);
        UpdateScreens(ThinkingMessage);

        ulong result = await Task.Run(() => PrimeTools.CountPrimes(_primeValue));

        _mainDisplay = result.ToString();
        UpdateScreens();

        _requiresOvertype = true;
    }

    /// <summary>
    /// Lists all prime numbers less than or equal to the displayed number when the 'PList' button is clicked. 
    /// Allows each result to be viewed one at a time by clicking the 'PNext' button.
    /// </summary>
    /// <remarks>Inputs must be below 1,000,000.</remarks>
    private void PListButton_Click(object sender, EventArgs e)
    {
        _primeValue = ulong.Parse(_mainDisplay);

        try
        {
            _primesList = PrimeTools.ListPrimes((int)_primeValue);
            _index = 0;
            _mainDisplay = _primesList[_index].ToString();
            UpdateScreens();
        }
        catch (ArgumentOutOfRangeException)
        {
            UpdateScreens(InputAboveMillionError);
        }
        finally
        {
            _requiresOvertype = true;
        }
    }

    /// <summary>
    /// Lists all the prime factors of the displayed number when the 'PFactors' button is clicked. Allows 
    /// each result to be viewed one at a time by clicking the 'PNext' button.
    /// </summary>
    /// <remarks>Inputs must be between 2 and 1,000,000.</remarks>
    private void PFactorsButton_Click(object sender, EventArgs e)
    {
        _primeValue = ulong.Parse(_mainDisplay);

        try
        {
            _primesList = PrimeTools.PrimeFactors((int)_primeValue);
            _index = 0;
            _mainDisplay = _primesList[_index].ToString();
            UpdateScreens();
        }
        catch (ArgumentOutOfRangeException ex)
        {
            UpdateScreens(ex.Message.Contains(" low ") ? InputTooLowError : InputAboveMillionError);
        }
        finally
        {
            _requiresOvertype = true;
        }
    }

    /// <summary>
    /// Displays the next prime number in the list when the 'PNext' button is clicked. Allows the user to 
    /// iterate through the list of primes or prime factors obtained from the 'PList' or 'PFactors' buttons.
    /// </summary>
    private void PNextButton_Click(object sender, EventArgs e)
    {
        if (_primesList.Count == 0)
            return;

        _index++;
        if (_index >= _primesList.Count)
            _index = 0;

        _mainDisplay = _primesList[_index].ToString();
        UpdateScreens();

        _requiresOvertype = true;
    }

    // Fraction calculator button click methods

    /// <summary>
    /// Sets the isInteger boolean to true, so that the input is displayed in the IntegerScreen.
    /// </summary>
    private void FIntButton_Click(object sender, EventArgs e)
    {
        _isInteger = true;
        _isNumerator = false;
        _isDenominator = false;
        _mainDisplay = "0";
    }

    /// <summary>
    /// Sets the isNumerator boolean to true, so that the input is displayed in the NumeratorScreen.
    /// </summary>
    private void FNumButton_Click(object sender, EventArgs e)
    {
        _isInteger = false;
        _isNumerator = true;
        _isDenominator = false;
        _mainDisplay = "0";
    }

    /// <summary>
    /// Sets the isDenominator boolean to true, so that the input is displayed in the DenominatorScreen.
    /// </summary>
    private void FDenomButton_Click(object sender, EventArgs e)
    {
        _isInteger = false;
        _isNumerator = false;
        _isDenominator = true;
        _mainDisplay = "0";
    }

    /// <summary>
    /// Displays the decimal form of the current fraction.
    /// </summary>
    private void FDecButton_Click(object sender, EventArgs e)
    {
        Fraction fraction = ReadFractionFromScreen();
        string dec = fraction.GetDecimal().ToString("G6");
        ResetFractionParts();
        SetFractionParts("integer", dec);
        _requiresOvertype = true;
    }

    /// <summary>
    /// Display the current fraction as a percentage.
    /// </summary>
    private void FPercButton_Click(object sender, EventArgs e)
    {
        Fraction fraction = ReadFractionFromScreen();
        string percentage = fraction.GetPercentage().ToString("G5") + '%';
        ResetFractionParts();
        SetFractionParts("integer", percentage);
        _requiresOvertype = true;
    }

    /// <summary>
    /// Displays the fraction stored in memory.
    /// </summary>
    private void FMRButton_Click(object sender, EventArgs e)
    {
        int integer = _memoryFraction.GetInteger();
        int numerator = _memoryFraction.GetProperNumerator();
        int denominator = _memoryFraction.GetDenominator();

        SetFractionParts("integer", integer.ToString());
        SetFractionParts("numerator", numerator.ToString());
        SetFractionParts("denominator", denominator.ToString());
    }

    /// <summary>
    /// Stores the displayed fraction in memory if it is valid, e.g. not an error message.
    /// </summary>
    private void FMInButton_Click(object sender, EventArgs e)
    {
        _memoryFraction = ReadFractionFromScreen();
    }

    /// <summary>
    /// Adds the currently displayed fraction to the fraction stored in memory.
    /// </summary>
    private void FMPlusButton_Click(object sender, EventArgs e)
    {
        Fraction displayedFraction = ReadFractionFromScreen();
        _memoryFraction.Add(displayedFraction);
    }

    /// <summary>
    /// Subtracts the currently displayed fraction from the in memory fraction. Fractions cannot be displayed 
    /// as negative, so if the fraction subtracted is larger then the result will be zero.
    /// </summary>
    private void FMMinusButton_Click(object sender, EventArgs e)
    {
        Fraction displayedFraction = ReadFractionFromScreen();
        _memoryFraction.Subtract(displayedFraction);
    }

    /// <summary>
    /// Simplifies the fraction and displays the result.
    /// </summary>
    private void FSFButton_Click(object sender, EventArgs e)
    {
        Fraction simplifiedFraction = ReadFractionFromScreen().SimplifyFraction();
        SetFractionParts("integer", simplifiedFraction.GetInteger().ToString());
        SetFractionParts("numerator", simplifiedFraction.GetProperNumerator().ToString());
        SetFractionParts("denominator", simplifiedFraction.GetDenominator().ToString());
    }

    private void FPlusButton_Click(object sender, EventArgs e)
    {
        OnFractionOperatorButtonClick('+');
    }

    private void FMinusButton_Click(object sender, EventArgs e)
    {
        OnFractionOperatorButtonClick('-');
    }

    private void FTimesButton_Click(object sender, EventArgs e)
    {
        OnFractionOperatorButtonClick('*');
    }

    private void FDivideButton_Click(object sender, EventArgs e)
    {
        OnFractionOperatorButtonClick('/');
    }

    private void FEqualButton_Click(object sender, EventArgs e)
    {
        PerformFractionCalculation();
        SetFractionParts("integer", _firstFraction.GetInteger().ToString());
        SetFractionParts("numerator", _firstFraction.GetProperNumerator().ToString());
        SetFractionParts("denominator", _firstFraction.GetDenominator().ToString());
        _operator = ' ';
        _requiresOvertype = true;
    }
}
