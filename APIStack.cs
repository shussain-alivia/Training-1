using Amazon.CDK;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.Lambda.EventSources;

namespace S3EventLambdaStack
{
    public class S3EventLambdaStack : Stack
    {
        public S3EventLambdaStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            // Create an S3 bucket
            var bucket = new Bucket(this, "MyS3Bucket", new BucketProps
            {
                BucketName = "my-unique-bucket-name", // Replace with your unique bucket name
                RemovalPolicy = RemovalPolicy.DESTROY // Just an example, adjust according to your needs
            });

            // Create a Lambda function
            var lambdaFn = new Function(this, "MyS3EventLambda", new FunctionProps
            {
                Runtime = Runtime.DOTNET_CORE_3_1, // Change to your preferred runtime
                Handler = "MyAssembly::MyNamespace.MyClass::MyHandler", // Update with your handler method
                Code = Code.FromAsset("path/to/your/lambda/code"), // Path to your lambda function code
                Timeout = Duration.Seconds(30) // Set timeout as needed
            });

            // Grant permission for Lambda to access S3 bucket
            bucket.GrantRead(lambdaFn);

            // Add S3 trigger to Lambda function
            lambdaFn.AddEventSource(new S3EventSource(bucket, new S3EventSourceProps
            {
                Events = new[] { S3EventType.OBJECT_CREATED } // Trigger Lambda on object creation
            }));
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var app = new App();
            new S3EventLambdaStack(app, "S3EventLambdaStack");
            app.Synth();
        }
    }
}
