using Confluent.Kafka;
using System;
using System.Threading.Tasks;

class AnalyticsProducer
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

        using (var producer = new ProducerBuilder<string, string>(config).Build())
        {
            try
            {
                var key0 = "key-part-0";
                var value0 = $"Message to partition 0 at {DateTime.UtcNow}";
                var result0 = await producer.ProduceAsync(
                    new TopicPartition("Analytics", 0),
                    new Message<string, string> { Key = key0, Value = value0 }
                );
                Console.WriteLine($"Produced to {result0.TopicPartitionOffset}: {key0} => {value0}");

                var key1 = "key-part-1";
                var value1 = $"Message to partition 1 at {DateTime.UtcNow}";
                var result1 = await producer.ProduceAsync(
                    new TopicPartition("Analytics", 1),
                    new Message<string, string> { Key = key1, Value = value1 }
                );
                Console.WriteLine($"Produced to {result1.TopicPartitionOffset}: {key1} => {value1}");
            }
            catch (ProduceException<string, string> e)
            {
                Console.WriteLine($"Delivery failed: {e.Error.Reason}");
            }

            producer.Flush(TimeSpan.FromSeconds(5));
        }
    }
}
