using Amazon;
using Amazon.SimpleNotificationService;
using Contracts;
using Publisher;

// Declare SnS client
var client = new AmazonSimpleNotificationServiceClient(RegionEndpoint.APSoutheast2);

// Declare a Publisher
var publisher = new SnsPublisher(client);

// Publis the massage the to sns topic

int i = 0;
while (i< 10)
{
    var customer = new CustomerCreated
    {
        Id = new Random().Next(),
        FullName = $"KMS Test {i}",
        //LifetimeSpent = 69
    };

    await publisher.PublishAsync("", customer);
    Console.WriteLine("Push");
    i++;
}




