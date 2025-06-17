using Common;
using Common.Models;
using Confluent.Kafka;
using System;
using System.ComponentModel.Design;
using System.Text.Json;
using System.Threading.Tasks;

public class KafkaProducerService
{
    private readonly IProducer<string, string> _producer;

    public KafkaProducerService(ProducerConfig config)
    {
        _producer = new ProducerBuilder<string, string>(config).Build();
    }
    //ProducerConfig defines connection to Kafka, how to serialize messages, and how to send them.
    //setting up dependency injection for the producer so it can be reused
    public async Task ProducePurchaseAsync(PurchaseEvent e)
    {
        var json = JsonSerializer.Serialize(e);
        await _producer.ProduceAsync(
            KafkaTopics.Purchases,
            new Message<string, string> { Key = e.UserId, Value = json });
    }
    //sends purchase events to the "purchases" topic in Kafka.
    //json serializer converts PurchaseEvent objects to JSON strings.

    public async Task ProduceToAnalyticsPartitionAsync(string key, string message, int partition)
    {
        await _producer.ProduceAsync(
            new TopicPartition(KafkaTopics.Analytics, new Partition(partition)),
            new Message<string, string> { Key = key, Value = message });
    }
    //sends purchase events to the "Analytics" topic, specifically to a given partition.

    public void Flush() => _producer.Flush(TimeSpan.FromSeconds(5));
}


/*public async Task ProduceAlertAsync(string key, string message)
{
    await _producer.ProduceAsync(
        KafkaTopics.Alerts,
        new Message<string, string> { Key = key, Value = message });
}
*/

// helper class that encapsulates Kafka producing logic (sending messages to different topics/partitions).

/*This class is designed to wrap all the logic about how to send messages to Kafka.

    It has methods like ProducePurchaseAsync() and ProduceToAnalyticsPartitionAsync() which handle producing messages to specific Kafka topics or partitions.

    It knows Kafka config, how to serialize objects to JSON, and how to call Kafka client APIs.

    It does not create the messages — it only sends them.

Think of it like a delivery service: it knows how to deliver parcels, but it doesn’t decide what parcels to send or when.
*/

//only handles kafka messaing logic, not the actual messages or when to send them.

// can be reused by any program that wants to send Kafka messages without rewriting Kafka send code.

// Deals with "How" messeges are sent (Kafka client, serialization, etc.)