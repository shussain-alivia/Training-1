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
            // ... (previous Lambda function and permissions setup)

            // Create CloudWatch Logs subscription filter for EventBridge
            var logGroup = new LogGroup(this, "MyLogGroup", new LogGroupProps
            {
                LogGroupName = "/eventbridge/rebates-etl-event-logs" // Replace with your log group name
            });

            var eventRuleForLogs = new Rule(this, "MyEventRuleForLogs", new RuleProps
            {
                EventPattern = new EventPattern
                {
                    Source = new[] { "aws.logs" },
                    DetailType = new[] { "Log Group" },
                    Detail = new Dictionary<string, object>
                    {
                        { "logGroupName", new[] { "/eventbridge/rebates-etl-event-logs" } } // Replace with your log group name
                    }
                }
            });

            eventRuleForLogs.AddTarget(new LambdaFunction(myLambda));
            AttachLogGroupSubscription(eventRuleForLogs.LogGroup);
        }

        private void AttachLogGroupSubscription(LogGroup logGroup)
        {
            // Set up a subscription filter for the log group
            logGroup.AddSubscriptionFilter("MySubscriptionFilter", new SubscriptionFilterOptions
            {
                FilterPattern = FilterPattern.StringLiteral("[timestamp=*Z, request_id=\"*\"]"), // Adjust the filter pattern as needed
                Destination = new LambdaDestination(myLambda)
            });
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
