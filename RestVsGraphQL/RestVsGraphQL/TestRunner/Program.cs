using RestVsGraphQL.Testing;

namespace RestVsGraphQL.TestRunner;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║         REST vs GraphQL - YAML Test Runner                     ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        var baseUrl = "http://localhost:5072";

        if (args.Length > 0)
        {
            baseUrl = args[0];
        }

        Console.WriteLine($"API Base URL: {baseUrl}");
        Console.WriteLine("Make sure the API is running before continuing!");
        Console.WriteLine();
        Console.WriteLine("Available Test Suites:");
        Console.WriteLine("  1. REST Tests");
        Console.WriteLine("  2. GraphQL Tests");
        Console.WriteLine("  3. Comparison Tests (REST vs GraphQL)");
        Console.WriteLine("  4. Run All Tests");
        Console.WriteLine();
        Console.Write("Select test suite (1-4): ");

        var choice = Console.ReadLine();
        Console.WriteLine();

        var runner = new YamlTestRunner(baseUrl);

        switch (choice)
        {
            case "1":
                await RunTestSuite(runner, "TestSuites/rest-tests.yaml");
                break;
            case "2":
                await RunTestSuite(runner, "TestSuites/graphql-tests.yaml");
                break;
            case "3":
                await RunTestSuite(runner, "TestSuites/comparison-tests.yaml");
                break;
            case "4":
                await RunTestSuite(runner, "TestSuites/rest-tests.yaml");
                await RunTestSuite(runner, "TestSuites/graphql-tests.yaml");
                await RunTestSuite(runner, "TestSuites/comparison-tests.yaml");
                break;
            default:
                Console.WriteLine("Invalid choice. Exiting...");
                break;
        }

        Console.WriteLine();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    private static async Task RunTestSuite(YamlTestRunner runner, string yamlFile)
    {
        try
        {
            var results = await runner.RunTestsFromFile(yamlFile);
            runner.PrintResults(results);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error running test suite: {ex.Message}");
            Console.ResetColor();
        }
    }
}
