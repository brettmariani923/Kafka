using Confluent.Kafka;
using System;
using System.Threading;
using System.Threading.Tasks;
using Common;

class PurchaseProcessor
{
    static async Task Main(string[] args)
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = "pkc-619z3.us-east1.gcp.confluent.cloud:9092",
            SaslUsername = "EPQFY3IVDZJ27ZUH",
            SaslPassword = "V+CQyESrHfHaaoCcIlwiwTEnkmUDFt+5EzwfHwYsdb2HDNrm/O0h3Nlcl6E06GY3",
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslMechanism = SaslMechanism.Plain,
            GroupId = "purchase-validator",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = consumerConfig.BootstrapServers,
            SaslUsername = consumerConfig.SaslUsername,
            SaslPassword = consumerConfig.SaslPassword,
            SecurityProtocol = consumerConfig.SecurityProtocol,
            SaslMechanism = consumerConfig.SaslMechanism,
            Acks = Acks.All
        };

        using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        using var producer = new ProducerBuilder<string, string>(producerConfig).Build();

        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        consumer.Subscribe(KafkaTopics.Purchases);
        Console.WriteLine($"🔍 Listening to topic: {KafkaTopics.Purchases}");

        var validator = new Validator();

        try
        {
            while (!cts.IsCancellationRequested)
            {
                try
                {
                    var cr = consumer.Consume(cts.Token);
                    Console.WriteLine($"📥 Received message: {cr.Message.Value}");

                    if (!validator.TryValidate(cr.Message.Value, out var evt))
                    {
                        Console.WriteLine($"❌ Invalid purchase message: {cr.Message.Value}");
                        continue;
                    }

                    await producer.ProduceAsync(KafkaTopics.Analytics, new Message<string, string>
                    {
                        Key = evt.UserId,
                        Value = cr.Message.Value
                    });

                    Console.WriteLine($"✅ Valid purchase forwarded to {KafkaTopics.Analytics}: {evt.UserId} bought {evt.Item}");
                }
                catch (ConsumeException ex)
                {
                    Console.WriteLine($"💥 Kafka error: {ex.Error.Reason}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("🛑 Graceful shutdown triggered.");
        }
        finally
        {
            consumer.Close();
        }
    }
}

//consumes purchase events, validates them (checks for well-formed JSON and required fields), and then forwards valid messages to another topic.
//validates purchase events and forwards valid ones