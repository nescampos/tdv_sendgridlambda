using System.Text;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using SendGrid;
using SendGrid.Helpers.Mail;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace IncomingS3FilesMonitor;

public class Function
{
    
    private string SendGridApiKey = Environment.GetEnvironmentVariable("SendGridApiKey");
    private string FromEmailAddress = Environment.GetEnvironmentVariable("FromEmailAddress");
    private string ToEmailAddress = Environment.GetEnvironmentVariable("ToEmailAddress");


    public async Task FunctionHandler(S3Event s3Event, ILambdaContext context)
    {
        foreach (var s3EventRecord in s3Event.Records)
        {
            var s3Object = s3EventRecord.S3.Object;
            var objectKey = s3Object.Key;

            //var objectMetadata = await GetObjectMetadataAsync(objectKey);

            var emailContent = new StringBuilder();
            emailContent.Append($"New file uploaded to S3: {objectKey}\n\n");
            emailContent.Append($"Size of the file (in bytes): {s3Object.Size}\n\n");

            await SendEmailAsync(emailContent.ToString());
        }
    }


    private async Task SendEmailAsync(string content)
    {
        var client = new SendGridClient(SendGridApiKey);
        var from = new EmailAddress(FromEmailAddress);
        var to = new EmailAddress(ToEmailAddress);
        var subject = "New file uploaded to S3";
        var plainTextContent = content;
        var htmlContent = $"<pre>{content}</pre>";
        var message = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
        var response = await client.SendEmailAsync(message);
    }
}
