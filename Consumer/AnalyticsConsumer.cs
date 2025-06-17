using Common;
using Common.Models;
using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;

class AnalyticsConsumer
{
    static void Main(string[] args)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = "pkc-619z3.us-east1.gcp.confluent.cloud:9092",
            SaslUsername = "EPQFY3IVDZJ27ZUH",
            SaslPassword = "V+CQyESrHfHaaoCcIlwiwTEnkmUDFt+5EzwfHwYsdb2HDNrm/O0h3Nlcl6E06GY3",
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslMechanism = SaslMechanism.Plain,
            GroupId = "analytics-consumer",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        //sets up connection to cluster, authentication, and consumer group settings. Assigns to the group 
        //analytics-consumer, and starts reading from the earliest messages in the topic.

        var userPurchaseCount = new Dictionary<string, int>();
        var itemCount = new Dictionary<string, int>();
        //dictionaries to track how many purchases each user has made and how many times each item has been purchased.

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(KafkaTopics.Analytics);
        // Subscribes to the Analytics topic to consume messages.

        Console.CancelKeyPress += (_, e) => {
            e.Cancel = true;
            consumer.Close();
        };
        // Allows graceful shutdown when Ctrl+C is pressed.

        Console.WriteLine($"📊 Listening to topic: {KafkaTopics.Analytics}");  

        while (true)
        {
            try
            {
                var cr = consumer.Consume(CancellationToken.None);

                var evt = JsonSerializer.Deserialize<PurchaseEvent>(cr.Message.Value);
                //consumes a message from the Analytics topic, and tries to turn the JSON string into a PurchaseEvent object.
                if (evt is null)
                {
                    Console.WriteLine("⚠️ Skipped invalid JSON.");
                    continue;
                }
                //skips if bad

                if (!userPurchaseCount.ContainsKey(evt.UserId))
                    userPurchaseCount[evt.UserId] = 0;
                userPurchaseCount[evt.UserId]++;

                if (!itemCount.ContainsKey(evt.Item))
                    itemCount[evt.Item] = 0;
                itemCount[evt.Item]++;
                //these if statements check if the user or item already exists in the dictionaries, and if not, initializes them to 0.
                //Then it increments the count for that user/item.

                Console.WriteLine($"🧾 {evt.UserId} bought {evt.Item}");
                //shows each purchase event as it comes in.

                if ((userPurchaseCount[evt.UserId] + itemCount[evt.Item]) % 5 == 0)
                {
                    Console.WriteLine("\n📈 Current Analytics Snapshot:");
                    Console.WriteLine("👥 Purchases per user:");
                    foreach (var kv in userPurchaseCount)
                        Console.WriteLine($"  - {kv.Key}: {kv.Value}");

                    Console.WriteLine("📦 Items purchased:");
                    foreach (var kv in itemCount)
                        Console.WriteLine($"  - {kv.Key}: {kv.Value}");
                    Console.WriteLine();
                }
                //every 5th purchase, it prints a snapshot of the current analytics state, showing how many purchases each user has made and how many times each item has been purchased.
            }
            catch (ConsumeException ex)
            {
                Console.WriteLine($"💥 Kafka error: {ex.Error.Reason}");
            }
            catch (JsonException)
            {
                Console.WriteLine("⚠️ Failed to parse JSON.");
            }
            //error handling for kafka specific errors and JSON parsing errors.
        }
    }
}

/*

Deserializes the JSON into PurchaseEvent

    Tracks:

        Total purchases per user

        Total purchases per item

    Logs analytics snapshot every few messages for visibility
*/
//consumes messages from the Analytics Kafka topic,