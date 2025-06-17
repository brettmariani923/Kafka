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
        //tells which broker to connect to, how to authenticate, and which group this consumer belongs to, and to read from the beginning of the topic if no offsets are stored.

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = consumerConfig.BootstrapServers,
            SaslUsername = consumerConfig.SaslUsername,
            SaslPassword = consumerConfig.SaslPassword,
            SecurityProtocol = consumerConfig.SecurityProtocol,
            SaslMechanism = consumerConfig.SaslMechanism,
            Acks = Acks.All
        };
        //This mirrors the consumer settings and ensures messages are reliably acknowledged (Acks.All)

        using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        //reads messages from the "purchases" topic, deserializes them, and validates their structure.
        using var producer = new ProducerBuilder<string, string>(producerConfig).Build();
        //writes messages to the "Analytics" topic, specifically to partition 0 and 1.

        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };
        //allows graceful shutdown when Ctrl+C is pressed.

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
                    //uses the Validator class to check if the message is well-formed JSON and contains the required fields.
                    //invalid messages are skipped and logged.

                    await producer.ProduceAsync(KafkaTopics.Analytics, new Message<string, string>
                    {
                        Key = evt.UserId,
                        Value = cr.Message.Value
                    });
                    //Sends the valid message to the analytics topic, using the same message value and UserId as the key.

                    Console.WriteLine($"✅ Valid purchase forwarded to {KafkaTopics.Analytics}: {evt.UserId} bought {evt.Item}");
                }
                catch (ConsumeException ex)
                {
                    Console.WriteLine($"💥 Kafka error: {ex.Error.Reason}");
                }
                //catches any errors without crashing
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

//this section is quality control: it listens for purchase events, checks if they are real and correct, and then sends them to the analytics topic if they are valid. 
//if not, throws them away and logs the error.