using Amazon.CDK;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.Events.Targets;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.S3.Events;
using Amazon.CDK.AWS.CloudWatch;
using Amazon.CDK.AWS.CloudWatch.Logs;

namespace YourNamespace
{
    public class S3LambdaEventBridgeStack : Stack
    {
        public S3LambdaEventBridgeStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            // Create Lambda function
            var lambdaFunction = new Function(this, "MyLambdaFunction", new FunctionProps
            {
                Runtime = Runtime.PYTHON_3_8,
                Handler = "index.lambda_handler",
                Code = Code.FromAsset("./lambda"), // Path to your Python Lambda code
                Timeout = Duration.Seconds(30) // Adjust timeout as needed
            });

            // Create an S3 bucket
            var s3Bucket = new Bucket(this, "MyS3Bucket", new BucketProps
            {
                RemovalPolicy = RemovalPolicy.DESTROY // Adjust as needed
            });

            // Grant permissions to the Lambda function to access the S3 bucket
            s3Bucket.GrantReadWrite(lambdaFunction);

            // Configure S3 event notification to trigger the Lambda for PutObject events
            s3Bucket.AddEventNotification(EventType.OBJECT_CREATED, new LambdaDestination(lambdaFunction));

            // Create an EventBridge event bus
            var eventBus = new EventBus(this, "MyEventBus");

            // Create an EventBridge rule to match the S3 event and trigger the Lambda
            var eventRule = new Rule(this, "S3LambdaEventRule", new RuleProps
            {
                EventPattern = new EventPattern
                {
                    Source = new[] { "aws.s3" },
                    DetailType = new[] { "AWS API Call via CloudTrail" },
                    Detail = new
                    {
                        eventSource = new[] { "s3.amazonaws.com" },
                        eventName = new[] { "PutObject" }
                    }
                },
                EventBus = eventBus
            });

            // Add Lambda function as a target for the EventBridge rule
            eventRule.AddTarget(new LambdaFunction(lambdaFunction));

            // Create a CloudWatch Log Group
            var logGroup = new LogGroup(this, "MyLogGroup", new LogGroupProps
            {
                LogGroupName = "/your/log/group/name" // Replace with your desired log group name
            });

            // Create a CloudWatch Log Stream for the Lambda logs
            var logStream = new LogStream(this, "MyLogStream", new LogStreamProps
            {
                LogGroup = logGroup
            });

            // Add Lambda function as a target for streaming logs to the CloudWatch Log Group
            lambdaFunction.LogGroup.AddSubscriptionFilter("LambdaLogsSubscription", new SubscriptionFilterOptions
            {
                Destination = logStream
            });

            // Create an EventBridge Log Group
            var eventBridgeLogGroup = new LogGroup(this, "MyEventBridgeLogGroup", new LogGroupProps
            {
                LogGroupName = "/your/eventbridge/log/group/name" // Replace with your EventBridge log group name
            });

            // Create an EventBridge Log Stream for Lambda logs
            var eventBridgeLogStream = new LogStream(this, "MyEventBridgeLogStream", new LogStreamProps
            {
                LogGroup = eventBridgeLogGroup
            });

            // Connect the EventBridge Log Stream to the Lambda logs
            logStream.AddSubscriptionFilter("EventBridgeLogsSubscription", new SubscriptionFilterOptions
            {
                Destination = eventBridgeLogStream
            });
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var app = new App();
            new S3LambdaEventBridgeStack(app, "S3LambdaEventBridgeStack");
            app.Synth();
        }
    }
}
