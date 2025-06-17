using Common.Models;
using Confluent.Kafka;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using static Confluent.Kafka.ConfigPropertyNames;

class ProducerProgram
{
    //Adjust the startup object in the csproj file depending on what you want to run.

    static async Task Main(string[] args)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = "pkc-619z3.us-east1.gcp.confluent.cloud:9092",
            SaslUsername = "EPQFY3IVDZJ27ZUH",
            SaslPassword = "V+CQyESrHfHaaoCcIlwiwTEnkmUDFt+5EzwfHwYsdb2HDNrm/O0h3Nlcl6E06GY3",
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslMechanism = SaslMechanism.Plain
        };

        var producer = new KafkaProducerService(config);
        var random = new Random();

        string[] users = { "eabara", "jsmith", "sgarcia", "jbernard", "htanaka" };
        string[] items = { "book", "alarm clock", "t-shirts", "gift card", "batteries" };

        for (int i = 0; i < 10; i++)
        {
            var user = users[random.Next(users.Length)];
            var item = items[random.Next(items.Length)];

            var purchase = new PurchaseEvent
            {
                UserId = user,
                Item = item,
                Timestamp = DateTime.UtcNow
            };

            // 1. Send to purchases topic (auto-partitioned)
            await producer.ProducePurchaseAsync(purchase);

            // 3. Send to analytics partition 0
            await producer.ProduceToAnalyticsPartitionAsync(user, $"[Partition 0] User {user} bought {item}", 0);

            // 4. Send to analytics partition 1
            await producer.ProduceToAnalyticsPartitionAsync(user, $"[Partition 1] User {user} bought {item}", 1);
        }

        producer.Flush();
    }
}
//creates the producer, creates events, and calls the methods on MultiTopicProducer to actually send those events.
/*     This class contains the Main method — the program’s starting point.

    It creates an instance of MultiTopicProducer with the Kafka config.

    It creates messages (PurchaseEvent objects), decides which users/items to use, and when/how many messages to send.

    It calls methods on MultiTopicProducer to actually send those messages.

    It controls the flow and timing of the message sending (like a manager telling the delivery service what to deliver and when).

Think of it like the manager that prepares all the parcels and tells the delivery service when and where to send them.
*/
//handles business logic: creating meaningful messages, looping, generating data, and deciding what to send.
// Deals with "What" messages to send and when to send them.

/*[Producer App]
     |
     ▼
  "purchases"(raw input messages)
     |
     ▼
[PurchaseValidator]
     ├── validates
     ├── skips if invalid
     └── forwards to ▼
                "Analytics" (only contains valid messages)
                        |
                        ▼
               [AnalyticsConsumer] → aggregates + logs
*/