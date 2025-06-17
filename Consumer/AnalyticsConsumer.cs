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

        var userPurchaseCount = new Dictionary<string, int>();
        var itemCount = new Dictionary<string, int>();

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(KafkaTopics.Analytics);  // Changed topic here

        Console.CancelKeyPress += (_, e) => {
            e.Cancel = true;
            consumer.Close();
        };

        Console.WriteLine($"📊 Listening to topic: {KafkaTopics.Analytics}");  // Changed topic here

        while (true)
        {
            try
            {
                var cr = consumer.Consume(CancellationToken.None);

                var evt = JsonSerializer.Deserialize<PurchaseEvent>(cr.Message.Value);
                if (evt is null)
                {
                    Console.WriteLine("⚠️ Skipped invalid JSON.");
                    continue;
                }

                // Update counts
                if (!userPurchaseCount.ContainsKey(evt.UserId))
                    userPurchaseCount[evt.UserId] = 0;
                userPurchaseCount[evt.UserId]++;

                if (!itemCount.ContainsKey(evt.Item))
                    itemCount[evt.Item] = 0;
                itemCount[evt.Item]++;

                Console.WriteLine($"🧾 {evt.UserId} bought {evt.Item}");

                // Display simple analytics every 5 messages
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
            }
            catch (ConsumeException ex)
            {
                Console.WriteLine($"💥 Kafka error: {ex.Error.Reason}");
            }
            catch (JsonException)
            {
                Console.WriteLine("⚠️ Failed to parse JSON.");
            }
        }
    }
}

/*🧠 Summary of What's Happening

Deserializes the JSON into PurchaseEvent

    Tracks:

        Total purchases per user

        Total purchases per item

    Logs analytics snapshot every few messages for visibility
*/
//consumes messages from the Analytics Kafka topic,