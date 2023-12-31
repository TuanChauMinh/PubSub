﻿using System.Net;
using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Contracts;

namespace Consumer;

public class SqsConsumerService : BackgroundService
{
    private readonly IAmazonSQS _sqs;
    private readonly MessageDispatcher _dispatcher;
    private readonly string _queueName = Environment.GetEnvironmentVariable("QUEUE_NAME")!;
    private readonly List<string> _messageAttributeNames = new() { "All" };

    public SqsConsumerService(IAmazonSQS sqs, MessageDispatcher dispatcher)
    {
        _sqs = sqs;
        _dispatcher = dispatcher;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var queueUrl = await _sqs.GetQueueUrlAsync(_queueName, ct);
        var receiveRequest = new ReceiveMessageRequest
        {
            QueueUrl = queueUrl.QueueUrl,
            MessageAttributeNames = _messageAttributeNames,
            AttributeNames = _messageAttributeNames
        };

        while (!ct.IsCancellationRequested)
        {
            var messageResponse = await _sqs.ReceiveMessageAsync(receiveRequest, ct);
            if (messageResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                continue;
            }

            foreach (var message in messageResponse.Messages)
            {
                var messageTypeName = message.MessageAttributes
                    .GetValueOrDefault(nameof(IMessage.MessageTypeName))?.StringValue;

                if (messageTypeName is null)
                {
                    await _sqs.DeleteMessageAsync(queueUrl.QueueUrl, message.ReceiptHandle, ct);
                    continue;
                }

                if (!_dispatcher.CanHandleMessageType(messageTypeName))
                {
                    continue;
                }

                var messageType = _dispatcher.GetMessageTypeByName(messageTypeName)!;
                var messageAsType = (IMessage)JsonSerializer.Deserialize(message.Body, messageType)!;

                await _dispatcher.DispatchAsync(messageAsType);
                await Task.Delay(3000);

                await _sqs.DeleteMessageAsync(queueUrl.QueueUrl, message.ReceiptHandle, ct);
            }
        }
    }
}
