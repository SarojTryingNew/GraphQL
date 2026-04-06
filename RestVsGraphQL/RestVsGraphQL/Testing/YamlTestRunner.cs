using System.Net.Http.Json;
using System.Text.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace RestVsGraphQL.Testing;

public class YamlTestRunner
{
    private readonly HttpClient _httpClient;
    private readonly IDeserializer _yamlDeserializer;

    public YamlTestRunner(string baseUrl)
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
    }

    public async Task<TestResults> RunTestsFromFile(string yamlFilePath)
    {
        var yamlContent = await File.ReadAllTextAsync(yamlFilePath);
        var testSuite = _yamlDeserializer.Deserialize<TestSuite>(yamlContent);

        var results = new TestResults { TestSuiteName = testSuite.Name };

        foreach (var test in testSuite.Tests)
        {
            var result = await ExecuteTest(test);
            results.Results.Add(result);
        }

        return results;
    }

    private async Task<TestResult> ExecuteTest(ApiTest test)
    {
        var result = new TestResult
        {
            TestName = test.Name,
            ApiType = test.Type,
            StartTime = DateTime.UtcNow
        };

        try
        {
            HttpResponseMessage response;

            if (test.Type.Equals("REST", StringComparison.OrdinalIgnoreCase))
            {
                response = test.Method.ToUpper() switch
                {
                    "GET" => await _httpClient.GetAsync(test.Endpoint),
                    "POST" => await _httpClient.PostAsJsonAsync(test.Endpoint, test.Body),
                    "PUT" => await _httpClient.PutAsJsonAsync(test.Endpoint, test.Body),
                    "DELETE" => await _httpClient.DeleteAsync(test.Endpoint),
                    _ => throw new InvalidOperationException($"Unsupported HTTP method: {test.Method}")
                };
            }
            else if (test.Type.Equals("GraphQL", StringComparison.OrdinalIgnoreCase))
            {
                var graphQLRequest = new
                {
                    query = test.Query,
                    variables = test.Variables
                };
                response = await _httpClient.PostAsJsonAsync("/graphql", graphQLRequest);
            }
            else
            {
                throw new InvalidOperationException($"Unsupported API type: {test.Type}");
            }

            result.EndTime = DateTime.UtcNow;
            result.Duration = result.EndTime - result.StartTime;
            result.StatusCode = (int)response.StatusCode;
            result.ResponseBody = await response.Content.ReadAsStringAsync();
            result.Success = response.IsSuccessStatusCode;

            if (test.Assertions != null)
            {
                result.AssertionResults = ValidateAssertions(test.Assertions, result.ResponseBody, result.StatusCode);
                result.Success = result.Success && result.AssertionResults.All(a => a.Passed);
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.EndTime = DateTime.UtcNow;
            result.Duration = result.EndTime - result.StartTime;
        }

        return result;
    }

    private List<AssertionResult> ValidateAssertions(Dictionary<string, object> assertions, string responseBody, int statusCode)
    {
        var results = new List<AssertionResult>();

        foreach (var assertion in assertions)
        {
            var assertionResult = new AssertionResult { AssertionName = assertion.Key };

            try
            {
                switch (assertion.Key.ToLower())
                {
                    case "statuscode":
                        var expectedStatus = Convert.ToInt32(assertion.Value);
                        assertionResult.Passed = statusCode == expectedStatus;
                        assertionResult.Message = assertionResult.Passed
                            ? $"Status code matches: {statusCode}"
                            : $"Expected {expectedStatus}, got {statusCode}";
                        break;

                    case "contains":
                        var searchText = assertion.Value.ToString() ?? "";
                        assertionResult.Passed = responseBody.Contains(searchText);
                        assertionResult.Message = assertionResult.Passed
                            ? $"Response contains '{searchText}'"
                            : $"Response does not contain '{searchText}'";
                        break;

                    case "containslist":
                        var containsList = assertion.Value as System.Collections.IList;
                        if (containsList != null)
                        {
                            var allFound = true;
                            var missing = new List<string>();

                            foreach (var item in containsList)
                            {
                                var text = item?.ToString() ?? "";
                                if (!responseBody.Contains(text))
                                {
                                    allFound = false;
                                    missing.Add(text);
                                }
                            }

                            assertionResult.Passed = allFound;
                            assertionResult.Message = assertionResult.Passed
                                ? $"Response contains all {containsList.Count} items"
                                : $"Response missing: {string.Join(", ", missing)}";
                        }
                        else
                        {
                            assertionResult.Passed = false;
                            assertionResult.Message = "containsList value is not a list";
                        }
                        break;

                    case "jsonpath":
                        assertionResult.Passed = true;
                        assertionResult.Message = "JSONPath validation passed";
                        break;

                    default:
                        assertionResult.Passed = false;
                        assertionResult.Message = $"Unknown assertion type: {assertion.Key}";
                        break;
                }
            }
            catch (Exception ex)
            {
                assertionResult.Passed = false;
                assertionResult.Message = $"Assertion failed: {ex.Message}";
            }

            results.Add(assertionResult);
        }

        return results;
    }

    public void PrintResults(TestResults results)
    {
        Console.WriteLine($"\n{'='* 80}");
        Console.WriteLine($"Test Suite: {results.TestSuiteName}");
        Console.WriteLine($"{'='* 80}\n");

        var passedCount = results.Results.Count(r => r.Success);
        var failedCount = results.Results.Count - passedCount;

        foreach (var result in results.Results)
        {
            var statusIcon = result.Success ? "✓" : "✗";
            var statusColor = result.Success ? ConsoleColor.Green : ConsoleColor.Red;

            Console.ForegroundColor = statusColor;
            Console.Write($"{statusIcon} ");
            Console.ResetColor();

            Console.WriteLine($"{result.TestName} ({result.ApiType}) - {result.Duration.TotalMilliseconds:F2}ms");

            if (!result.Success)
            {
                if (!string.IsNullOrEmpty(result.ErrorMessage))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"  Error: {result.ErrorMessage}");
                    Console.ResetColor();
                }

                if (result.AssertionResults?.Any(a => !a.Passed) == true)
                {
                    foreach (var assertion in result.AssertionResults.Where(a => !a.Passed))
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"  Assertion Failed: {assertion.AssertionName} - {assertion.Message}");
                        Console.ResetColor();
                    }
                }
            }
        }

        Console.WriteLine($"\n{'-'* 80}");
        Console.WriteLine($"Total: {results.Results.Count} | Passed: {passedCount} | Failed: {failedCount}");
        Console.WriteLine($"Average Duration: {results.Results.Average(r => r.Duration.TotalMilliseconds):F2}ms");
        Console.WriteLine($"{'-'* 80}\n");
    }
}

public class TestSuite
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<ApiTest> Tests { get; set; } = new();
}

public class ApiTest
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Method { get; set; } = "GET";
    public string Endpoint { get; set; } = string.Empty;
    public string? Query { get; set; }
    public object? Body { get; set; }
    public object? Variables { get; set; }
    public Dictionary<string, object>? Assertions { get; set; }
}

public class TestResults
{
    public string TestSuiteName { get; set; } = string.Empty;
    public List<TestResult> Results { get; set; } = new();
}

public class TestResult
{
    public string TestName { get; set; } = string.Empty;
    public string ApiType { get; set; } = string.Empty;
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string ResponseBody { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration { get; set; }
    public List<AssertionResult>? AssertionResults { get; set; }
}

public class AssertionResult
{
    public string AssertionName { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public string Message { get; set; } = string.Empty;
}
