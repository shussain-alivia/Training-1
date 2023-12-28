using Amazon.CDK;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.Events.Targets;
using Amazon.CDK.AWS.S3;

namespace EventBridgeLambdaIntegration
{
    public class EventBridgeLambdaStack : Stack
    {
        public EventBridgeLambdaStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            // Create an S3 bucket
            var myBucket = new Bucket(this, "MyBucket", new BucketProps
            {
                // Your S3 bucket configuration
            });

            // Create a Lambda function
            var myLambda = new Function(this, "MyLambda", new FunctionProps
            {
                // Your Lambda function configuration
            });

            // Grant permissions to Lambda to access S3
            myBucket.GrantReadWrite(myLambda);

            // Create an EventBridge rule for S3 events
            var s3EventRule = new Rule(this, "S3EventRule", new RuleProps
            {
                EventPattern = new EventPattern
                {
                    Source = new[] { "aws.s3" },
                    DetailType = new[] { "AWS API Call via CloudTrail" },
                    Detail = new Dictionary<string, object>
                    {
                        { "eventSource", new[] { "s3.amazonaws.com" } },
                        { "eventName", new[] { "PutObject" } } // Adjust event name as needed
                        // Add more conditions as required to match your S3 event
                    }
                }
            });

            // Add Lambda as a target for S3 events
            s3EventRule.AddTarget(new LambdaFunction(myLambda));

            // Stream Lambda logs to EventBridge Logs
            AttachLambdaLogGroupSubscription(myLambda.LogGroup);
        }

        private void AttachLambdaLogGroupSubscription(LogGroup logGroup)
        {
            // Create an EventBridge rule for Lambda logs
            var lambdaLogRule = new Rule(this, "LambdaLogRule", new RuleProps
            {
                EventPattern = new EventPattern
                {
                    Source = new[] { "aws.logs" },
                    DetailType = new[] { "AWS Lambda Log Event" },
                    Detail = new Dictionary<string, object>
                    {
                        { "logGroup", new[] { logGroup.LogGroupName } }
                    }
                }
            });

            // Add the LogGroup subscription as a target for Lambda logs
            lambdaLogRule.AddTarget(new EventBridgePutEvents(new PutEventsProps
            {
                EventBus = EventBus.FromEventBusName(this, "MyEventBus", "YourEventBusName"), // Replace with your EventBridge bus name
            }));
        }
    }
}






myLambda.AddToRolePolicy(new PolicyStatement(new PolicyStatementProps
            {
                Actions = new[] { "events:PutEvents" },
                Resources = new[] { "*" } // Adjust the resource to a specific EventBridge bus ARN if possible
            }));

            // Check or modify the EventBridge rule configuration
            var eventRule = new Rule(this, "MyEventRule", new RuleProps
            {
                EventPattern = new EventPattern
                {
                    Source = new[] { "aws.lambda" },
                    DetailType = new[] { "Lambda Function Invocation Result" },
                    Detail = new Dictionary<string, object>
                    {
                        { "requestParameters.functionName", new[] { myLambda.FunctionName } }
                    }
                }
            });

            // Add the Lambda function as a target for the EventBridge rule
            eventRule.AddTarget(new LambdaFunction(myLambda));

            // Create CloudWatch Logs subscription filter for EventBridge
            var logGroup = new LogGroup(this, "MyLogGroup", new LogGroupProps
            {
                LogGroupName = "YourLogGroupName" // Replace with your log group name
            });

            var eventRuleForLogs = new Rule(this, "MyEventRuleForLogs", new RuleProps
            {
                EventPattern = new EventPattern
                {
                    Source = new[] { "aws.logs" },
                    DetailType = new[] { "Log Group" },
                    Detail = new Dictionary<string, object>
                    {
                        { "logGroupName", "YourLogGroupName" } // Replace with your log group name
                    }
                }
            });

            eventRuleForLogs.AddTarget(new LambdaFunction(myLambda));
        }
    }
}
