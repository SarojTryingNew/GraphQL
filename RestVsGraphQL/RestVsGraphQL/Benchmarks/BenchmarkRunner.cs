using BenchmarkDotNet.Running;
using RestVsGraphQL.Benchmarks;

namespace RestVsGraphQL.BenchmarkRunner;

public class BenchmarkProgram
{
    public static void Main(string[] args)
    {
        Console.WriteLine("REST vs GraphQL Performance Benchmarks");
        Console.WriteLine("========================================");
        Console.WriteLine();
        Console.WriteLine("Make sure the API is running on http://localhost:5000 before starting benchmarks!");
        Console.WriteLine("Press any key to continue or Ctrl+C to cancel...");
        Console.ReadKey();
        Console.WriteLine();

        var summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<RestVsGraphQLBenchmark>();

        Console.WriteLine();
        Console.WriteLine("Benchmarks completed! Check the BenchmarkDotNet.Artifacts folder for detailed results.");
    }
}
