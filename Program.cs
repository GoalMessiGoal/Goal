using Npgsql;
using AWSSDK.Runtime;
using Amazon.Lambda.Model;
using Amazon.Lambda;
using Amazon.Runtime;
using System.Text.Json;



internal class Program
{

    private readonly IAmazonLambda _lambdaService;


    public async Task<string> InvokeLambdaFunction()
    {
        // Create Lambda client
        var lambdaClient = new AmazonLambdaClient();

        // Prepare the request
        var request = new InvokeRequest
        {
            FunctionName = "your-lambda-function-name",
            InvocationType = InvocationType.RequestResponse, // Use Event for async invocation
            Payload = "{\"key1\":\"value1\", \"key2\":\"value2\"}" // JSON string input
        };

        // Invoke the function
        var response = await lambdaClient.InvokeAsync(request);

        // Read the response
        using (var streamReader = new StreamReader(response.Payload))
        {
            return await streamReader.ReadToEndAsync();
        }



    }

    /// <summary>
    /// Constructor for the LambdaWrapper class.
    /// </summary>
    /// <param name="lambdaService">An initialized Lambda service client.</param>
    public Program(IAmazonLambda lambdaService)
    {
        _lambdaService = lambdaService;
    }
    /// <summary>
    /// Invoke a Lambda function.
    /// </summary>
    /// <param name="functionName">The name of the Lambda function to
    /// invoke.</param
    /// <param name="parameters">The parameter values that will be passed to the function.</param>
    /// <returns>A System Threading Task.</returns>
    public async Task<string> InvokeFunctionAsync(
        string functionName,
        string parameters)
    {
        var payload = parameters;
        var request = new InvokeRequest
        {
            FunctionName = functionName,
            Payload = payload,
        };

        var response = await _lambdaService.InvokeAsync(request);
        MemoryStream stream = response.Payload;
        string returnValue = System.Text.Encoding.UTF8.GetString(stream.ToArray());
        return returnValue;
    }
    // snippet-start:[Lambda.dotnetv3.LambdaActions.ListFunctions]
    /// <summary>
    /// Get a list of Lambda functions.
    /// </summary>
    /// <returns>A list of FunctionConfiguration objects.</returns>
    public async Task<List<FunctionConfiguration>> ListFunctionsAsync()
    {
        var functionList = new List<FunctionConfiguration>();

        var functionPaginator =
            _lambdaService.Paginators.ListFunctions(new ListFunctionsRequest());
        await foreach (var function in functionPaginator.Functions)
        {
            functionList.Add(function);
        }

        return functionList;
    }
    private static void Main(string[] args)
    {

        var credentials = new BasicAWSCredentials("accessKey", "secretKey");
        var config = new AmazonLambdaConfig
        {
            RegionEndpoint = Amazon.RegionEndpoint.USWest2 // Set your region
        };

        var lambdaClient = new AmazonLambdaClient(credentials, config);

        var input = new MyInput { Key1 = "value", Key2 = 123 };
        var inputJson = JsonSerializer.Serialize(input);

        var request = new InvokeRequest
        {
            FunctionName = "your-lambda-function-name",
            Payload = inputJson
        };

        var response = lambdaClient.InvokeAsync(request);

        var responseJson = new StreamReader(response.Payload).ReadToEnd();
        string s =  JsonSerializer.Deserialize<MyOutput>(responseJson);

        /*
        string connectionString = "Host=localhost;Port=5433;Database=postgres;User Id=postgres;Password=admin;";

        using NpgsqlConnection connection = new NpgsqlConnection(connectionString);
        connection.Open();

        using NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM customers", connection);

        using NpgsqlDataReader reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            Console.WriteLine(reader[1]);
            // Use the fetched results
        }
        */
    }

    public class MyInput
    {
        public string Key1 { get; set; }
        public int Key2 { get; set; }
    }

    public class MyOutput
    {
        public string Result { get; set; }
        public bool Success { get; set; }
    }

}