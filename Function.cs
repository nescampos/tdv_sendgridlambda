using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.Lambda.Serialization.SystemTextJson;
using SendGrid;
using SendGrid.Helpers.Mail;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace IncomingS3FilesMonitor;

public class Function
{
    private string SendGridApiKey = Environment.GetEnvironmentVariable("SendGridApiKey");
    private string FromEmailAddress = Environment.GetEnvironmentVariable("FromEmailAddress");
    private string ToEmailAddress = Environment.GetEnvironmentVariable("ToEmailAddress");

    public async Task FunctionHandler(S3Event s3Event, ILambdaContext context)
    {
        var client = new SendGridClient(SendGridApiKey);
        foreach (var s3EventRecord in s3Event.Records)
        {
            var s3Object = s3EventRecord.S3.Object;
            var objectKey = s3Object.Key;

            var message = MailHelper.CreateSingleEmail(
                from: new EmailAddress(FromEmailAddress), 
                to: new EmailAddress(ToEmailAddress), 
                subject: "New file uploaded to S3", 
                plainTextContent: $"New file uploaded to S3: {objectKey}\n\n" +
                    $"Size of the file (in bytes): {s3Object.Size}", 
                htmlContent: null
            );

            var response = await client.SendEmailAsync(message);
            if(!response.IsSuccessStatusCode)
            {
                var errorMessage = "Email failed to sent." +
                    $"Response status code: {response.StatusCode}, " +
                    $"body: {await response.Body.ReadAsStringAsync()}";
                Console.Error.WriteLine(errorMessage);
            }
        }
    }
}
