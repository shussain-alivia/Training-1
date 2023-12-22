[4:54 AM] Sadaf Hussain
using Amazon.CDK;
using Amazon.CDK.AWS.CloudWatch;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.Events.Targets;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.S3;
 
namespace YourNamespace
{
    public class S3EventBridgeStack : Stack
    {
        public S3EventBridgeStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            // Create a new EventBridge EventBus
            EventBus eventBus = new EventBus(this, "NewEventBus", new EventBusProps
            {
                EventSourceName = "MyCustomEventSource"
            });
 
            // S3 bucket creation or retrieval if it already exists
            Bucket existingBucket = Bucket.FromBucketName(this, "ExistingBucket", "YourExistingBucketName") ??
                                    new Bucket(this, "NewBucket", new BucketProps
                                    {
                                        BucketName = "NewBucketName"
                                    });
 
            // Lambda function
            Function pythonLambda = new Function(this, "PythonLambda", new FunctionProps
            {
                Runtime = Runtime.PYTHON_3_8,
                Handler = "index.handler",
                Code = Code.FromAsset("path/to/your/python/function/code"),
            });
 
            // Granting necessary permissions for the lambda function to access S3
            existingBucket.GrantReadWrite(pythonLambda);
 
            // EventBridge Rule under the newly created EventBus for S3 put event
            Rule s3PutRule = new Rule(this, "S3PutRule", new RuleProps
            {
                EventBus = eventBus,
                EventPattern = new EventPattern
                {
                    Source = new[] { "aws.s3" },
                    DetailType = new[] { "AWS API Call via CloudTrail" },
                    Detail = new
                    {
                        eventName = new[] { "PutObject" },
                        requestParameters = new
                        {
                            bucketName = new[] { existingBucket.BucketName },
                        }
                    }
                }
            });
 
            // Connecting S3 put event to Lambda function through EventBridge
            s3PutRule.AddTarget(new LambdaFunction(pythonLambda, new LambdaFunctionProps
            {
                Event = RuleTargetInput.FromObject(new
                {
                    detailType = "CustomEvent",
                    source = "MyCustomEventSource",
                    resources = new[] { existingBucket.BucketArn }
                })
            }));
 
            // Pushing EventBridge logs to CloudWatch Logs for processing
            LogGroup logGroup = new LogGroup(this, "EventBridgeLogGroup", new LogGroupProps
            {
                LogGroupName = $"/aws/events/{eventBus.EventBusName}",
                RemovalPolicy = RemovalPolicy.DESTROY
            });
 
            eventBus.AddPermission("PushToCloudWatch", new EventBusProps
            {
                PolicyStatements = new[] {
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Actions = new[] { "logs:CreateLogStream", "logs:PutLogEvents" },
                        Resources = new[] { logGroup.LogGroupArn }
                    })
                }
            });
 
            // CDK Diff and Deploy
            var diffOutput = new DiffOptions();
            var diff = this.Diff(diffOutput);
 
            var deployOutput = new DeployOptions();
            this.Deploy(deployOutput);
        }
    }
}
