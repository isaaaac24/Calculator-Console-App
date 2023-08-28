using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;

class Operation
{
    //Declaring elements of object
    public double operand1;
    public double operand2;
    public string operationLetter;
    public string operationSymbol;
    public double result;

    // Constructor
    public Operation(double operand1, double operand2, string operationLetter)
    {
        this.operand1 = operand1;
        this.operand2 = operand2;
        this.operationLetter = operationLetter;
        this.operationSymbol = "";
        this.result = 0;

        switch (operationLetter)
        {
            case "a":
                this.operationSymbol = "+";
                break;
            case "s":
                this.operationSymbol = "-";
                break;
            case "m":
                this.operationSymbol = "*";
                break;
            case "d":
                this.operationSymbol = "/";
                break;
            default:
                break;
        }
    }
}

class Calculator
{
    public static double DoOperation(Operation operation)
    {
        double result = double.NaN; // Default value is "not-a-number" if an operation, such as division, could result in an error.

        // Use a switch statement to do the math.
        switch (operation.operationLetter)
        {
            case "a":
                result = operation.operand1 + operation.operand2;
                break;
            case "s":
                result = operation.operand1 - operation.operand2;
                break;
            case "m":
                result = operation.operand1 * operation.operand2;
                break;
            case "d":
                // Ask the user to enter a non-zero divisor.
                if (operation.operand2 != 0)
                {
                    result = operation.operand1 / operation.operand2;
                }
                break;
            // Return text for an incorrect option entry.
            default:
                break;
        }
        return result;
    }
}
class Program
{
    public static MySqlConnection GetMySqlConnection()
    {
        string connectionString = Environment.GetEnvironmentVariable("ConnectionString");
        MySqlConnection conn = new MySqlConnection(connectionString);
        return conn;
    }
    static void Main(string[] args)
    {
        try
        {
            
            using (MySqlConnection conn = GetMySqlConnection())
            {
                conn.Open();  // Opens the connection
                Console.WriteLine("Connection Opened");
                conn.Close(); // Closes the connection
                Console.WriteLine("Connection Closed");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        // Declare endApp boolean
        bool endApp = false;

        // Display title as the C# console calculator app.
        Console.WriteLine("Console Calculator in C#\r");
        Console.WriteLine("------------------------\n");

        // Main program loop
        while (!endApp)
        {
            // Declare operation variables and set to empty.
            double cleanNum1 = 0;
            double cleanNum2 = 0;
            string operationLetter = "";

            // Ask the user to type the first number then validates
            cleanNum1 = GetValidatedNumber("Type a number, and then press Enter: ");
            cleanNum2 = GetValidatedNumber("Type another number, and then press Enter: ");

            // Ask the user to choose an operator.
            operationLetter = GetOperationLetter();

            // Perform operation
            Operation operation = new Operation(cleanNum1, cleanNum2, operationLetter);
            operation = PerformOperation(operation);
            Console.WriteLine("------------------------\n");

            // Store operation in history and print operation history
            PrintOperationHistory();
            Console.WriteLine("\n------------------------\n");

            // Ask if user would like to do more calculations.
            Console.Write("If you would like to perform more operations, enter 'y', if not press any key to continue...");
            if (Console.ReadLine() == "y") endApp = true; else endApp = false;
            Console.WriteLine("\n------------------------\n");
        }
        return;
    }

    static double GetValidatedNumber(string Prompt) 
    {
        // Declare variables and set to empty.
        string numInput = "";
        double cleanNum = 0;

        // Obtain user input
        Console.Write(Prompt);
        numInput = Console.ReadLine();

        // Validate user input
        cleanNum = ValidateNum(numInput);

        return cleanNum;
    }

    // Validate user input is a number
    public static double ValidateNum(string numInput)
    {
        double cleanNum = 0;
        while (!double.TryParse(numInput, out cleanNum))
        {
            Console.Write("This is not valid input. Please enter an integer value: ");
            numInput = Console.ReadLine();
        }

        return cleanNum;
    }

    static string GetOperationLetter()
    {
        // Declare variables and set to empty.
        string operationInput = "";
        string operationLetter = "";

        // Obtain user input
        Console.WriteLine("\nChoose an operator from the following list:");
        Console.WriteLine("\ta - Add");
        Console.WriteLine("\ts - Subtract");
        Console.WriteLine("\tm - Multiply");
        Console.WriteLine("\td - Divide");
        Console.Write("Your option? ");
        operationInput = Console.ReadLine();
        operationInput = operationInput.ToLower();

        // Validate user input
        operationLetter = ValidateOperationLetter(operationInput);

        return operationLetter;
    }

    // Validate user input is a valid operation letter
    public static string ValidateOperationLetter(string operationInput)
    {
        if (operationInput == "a" || operationInput == "s" || operationInput == "m" || operationInput == "d")
        {
            return operationInput;
        }
        else
        {
            Console.Write("This is not a valid operation. Please enter a valid operation letter: ");
            operationInput = Console.ReadLine();
            operationInput = operationInput.ToLower();
            return ValidateOperationLetter(operationInput);
        }
    }

    static Operation PerformOperation(Operation operation)
    {
        double result = 0;
        try
        {
            result = Calculator.DoOperation(operation);
            if (double.IsNaN(result))
            {
                Console.WriteLine("This operation will result in a mathematical error.\n");
            }
            else
            {
                Console.WriteLine("\nYour result: {0:0.##}\n", result);
                operation.result = result;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Oh no! An exception occurred trying to do the math.\n - Details: " + e.Message);
        }
        InsertOperationHistory(operation);
        return operation;
    }

    public static void InsertOperationHistory(Operation operation)
    {
        string query = "INSERT INTO operationhistory (Operand1, Operand2, OperationLetter, OperationSymbol, Result) VALUES (@Operand1, @Operand2, @OperationLetter, @OperationSymbol, @Result)";

        using (MySqlConnection conn = GetMySqlConnection())
        {
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Operand1", operation.operand1);
                cmd.Parameters.AddWithValue("@Operand2", operation.operand2);
                cmd.Parameters.AddWithValue("@OperationLetter", operation.operationLetter);
                cmd.Parameters.AddWithValue("@OperationSymbol", operation.operationSymbol);
                cmd.Parameters.AddWithValue("@Result", operation.result);

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }
    }

    // Print operation history
    static public void PrintOperationHistory()
    {
        List<Operation> operations = new List<Operation>();
        operations = GetAllOperations(operations);
        Console.WriteLine("Operation History:");
        foreach (Operation operation in operations)
        {
            Console.WriteLine("{0} {1} {2} = {3}", operation.operand1, operation.operationSymbol, operation.operand2, operation.result);
        }
    }

    public static List<Operation> GetAllOperations(List<Operation> operations)
    {
        string query = "SELECT Operand1, Operand2, OperationLetter, OperationSymbol, Result FROM operationhistory";

        using (MySqlConnection conn = GetMySqlConnection())
        {
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                conn.Open();
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        double operand1 = Convert.ToDouble(reader["Operand1"]);
                        double operand2 = Convert.ToDouble(reader["Operand2"]);
                        string operationLetter = reader["OperationLetter"].ToString();
                        string operationSymbol = reader["OperationSymbol"].ToString();
                        double result = Convert.ToDouble(reader["Result"]);

                        Operation operation = new Operation(operand1, operand2, operationLetter);
                        operation.result = result;
                        operations.Add(operation);
                    }
                }
                conn.Close();
            }
        }
        return operations;
    }
}