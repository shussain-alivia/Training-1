def lambda_handler(event, context):
    try:
        # Check if the event is related to a PutObject operation
        if event['detail']['eventName'] == 'PutObject':
            # Get the bucket name and object key
            bucket_name = event['detail']['requestParameters']['bucketName']
            object_key = event['detail']['requestParameters']['key']

            # Process the S3 PutObject event (example: print the details)
            print(f"S3 PutObject event detected in bucket '{bucket_name}' for object '{object_key}'")
        else:
            # Handle other types of events
            print("Received event not related to S3 PutObject")
    except KeyError as e:
        print(f"KeyError: {e} - Event structure doesn't match the expected format")

namespace RebatesEtlInfrastructure
{
    public class RebatesEtlInfrastructureStack : Stack
    {
        internal RebatesEtlInfrastructureStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            // Define S3 bucket names
            string etlBucketName = "your-etl-bucket-name";
            string dataBucketName = "your-data-bucket-name";

            // Lambda Function
            var rbtLambda = new Function(this, "RbtLambdaFunction", new FunctionProps
            {
                Runtime = Runtime.PYTHON_3_9,
                Handler = "mdrpHandler.lambda_handler",
                Code = Code.FromAsset("lambda/mdrpHandler.zip"),
                // Add necessary Lambda configurations
            });

            // Glue Role
            var etlRole = new Role(this, "EtlRole", new RoleProps
            {
                AssumedBy = new ServicePrincipal("glue.amazonaws.com")
            });
            etlRole.AddManagedPolicy(ManagedPolicy.FromAwsManagedPolicyName("service-role/AWSGlueServiceRole"));
            // Add more permissions or policies as required

            // Glue Job
            var rbtGlueJob = new CfnJob(this, "RbtGlueJob", new CfnJobProps
            {
                Role = etlRole.RoleArn,
                Command = new Dictionary<string, object>
                {
                    { "Name", "glueetl" },
                    { "ScriptLocation", $"s3://{etlBucketName}/glue/glue_starter_job.py" }
                    // Add more Glue job configurations as needed
                }
            });

            // EventBridge Rule for S3 PutObject event
            var eventRule = new Rule(this, "RebatesFileEventRule", new RuleProps
            {
                EventPattern = new EventPattern
                {
                    Source = new[] { "aws.s3" },
                    DetailType = new[] { "AWS API Call via CloudTrail" },
                    Detail = new Dictionary<string, object>
                    {
                        { "eventSource", new[] { "s3.amazonaws.com" } },
                        { "eventName", new[] { "PutObject" } },
                        { "requestParameters", new Dictionary<string, object>
                            {
                                { "bucketName", etlBucketName },
                                // Add more event pattern configurations based on your requirements
                            }
                        }
                    }
                }
            });

            // CloudWatch Log Group for EventBridge events
            var eventLogGroup = new LogGroup(this, "EventBridgeLogGroup", new LogGroupProps
            {
                LogGroupName = "/your/eventbridge/log/group/name" // Replace with your desired log group name
            });

            // Add EventBridge target to log events to CloudWatch Logs
            eventRule.AddTarget(new CloudWatchLogGroup(eventLogGroup));

            // Output the Glue Role ARN for reference
            new CfnOutput(this, "EtlRoleOutput", new CfnOutputProps
            {
                Value = etlRole.RoleArn
            });
        }
    }
}
