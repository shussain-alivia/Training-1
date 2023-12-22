using Amazon.CDK;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.S3;
using Amazon;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using System;
using System.Threading.Tasks;

namespace PythonLambda
{
    public class CdkPythonLambdaStack : Stack
    {
        public Function MyPythonLambdaHandler { get; private set; }

        internal CdkPythonLambdaStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            // Fetch the PythonFiles directory path from the environment variable
            var pythonFilesPath = Environment.GetEnvironmentVariable("PYTHON_FILES_PATH");

            if (pythonFilesPath == null)
            {
                throw new InvalidOperationException("PYTHON_FILES_PATH environment variable is not set.");
            }

            // Create an Asset from the specified path
            var pythonFilesAsset = new Asset(this, "PythonFilesAsset", new AssetProps
            {
                Path = pythonFilesPath
            });

            // Create a Lambda function with a custom name
            MyPythonLambdaHandler = new Function(this, "CustomLambdaName", new FunctionProps
            {
                Runtime = Runtime.PYTHON_3_8,
                Handler = "python.my_python_function",
                Code = Code.FromAsset(pythonFilesAsset.AssetPath),
                Timeout = Duration.Seconds(30),
                FunctionName = "MyCustomLambdaFunction" // Custom name for the Lambda function
            });

            // Check if the Lambda function exists
            Task.Run(async () =>
            {
                using (var lambdaClient = new AmazonLambdaClient(RegionEndpoint.USWest2)) // Replace with the appropriate region
                {
                    var functionName = "MyCustomLambdaFunction"; // Replace with your Lambda function's name
                    var exists = await FunctionExists(lambdaClient, functionName);

                    if (exists)
                    {
                        Console.WriteLine($"Lambda function '{functionName}' exists.");
                    }
                    else
                    {
                        Console.WriteLine($"Lambda function '{functionName}' does not exist.");
                    }
                }
            }).Wait(); // Blocking call to wait for the asynchronous task to complete
        }

        // Function to check if a Lambda function exists
        private static async Task<bool> FunctionExists(AmazonLambdaClient lambdaClient, string functionName)
        {
            try
            {
                var response = await lambdaClient.GetFunctionAsync(new GetFunctionRequest { FunctionName = functionName });
                return response != null;
            }
            catch (ResourceNotFoundException)
            {
                return false;
            }
        }
    }
}
