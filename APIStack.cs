using Amazon.CDK;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.Events.Targets;
using Amazon;

namespace EventBridgeLambdaIntegration
{
    public class EventBridgeLambdaStack : Stack
    {
        public EventBridgeLambdaStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            // Create the Lambda function
            var myLambda = new Function(this, "MyLambda", new FunctionProps
            {
                Runtime = Runtime.DOTNET_CORE_3_1,
                Handler = "EventBridgeLambdaIntegration::EventBridgeLambdaIntegration.Functions::FunctionHandler", // Replace with your handler method
                Code = Code.FromAsset("./MyLambdaFunction"), // Replace with your Lambda function's code path
                // ... other Lambda function configuration
            });

            // Grant permissions to the Lambda function to put events to EventBridge
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
