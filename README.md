Code = Code.FromInline(@"
def lambda_handler(event, context):
    import boto3

    # Initialize S3 client
    s3 = boto3.client('s3')

    # Specify the bucket name
    bucket_name = '" + bucket.BucketName + @"'

    try:
        # List objects in the bucket
        response = s3.list_objects_v2(Bucket=bucket_name)

        # Print object details
        if 'Contents' in response:
            for obj in response['Contents']:
                print('Object Key:', obj['Key'])
                print('Object Size:', obj['Size'])
                print('---')

        return 'Successfully listed objects in S3 bucket.'
    except Exception as e:
        print('Error:', str(e))
        return 'Error listing objects in S3 bucket.'
")
