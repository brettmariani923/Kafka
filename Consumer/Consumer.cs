using Confluent.Kafka;
using System.Threading;
using System;

class Consumer
{
    static void Main(string[] args)
    {
        //Adjust the startup object in the csproj file depending on what you want to run.

        var config = new ConsumerConfig
        {
            // User-specific properties that you must set
            BootstrapServers = "pkc-619z3.us-east1.gcp.confluent.cloud:9092",
            SaslUsername = "EPQFY3IVDZJ27ZUH",
            SaslPassword = "V+CQyESrHfHaaoCcIlwiwTEnkmUDFt+5EzwfHwYsdb2HDNrm/O0h3Nlcl6E06GY3",

            // Fixed properties
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslMechanism = SaslMechanism.Plain,
            GroupId = "Kafka",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        const string topic = "purchases";

        CancellationTokenSource cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) => {
            e.Cancel = true; // prevent the process from terminating.
            cts.Cancel();
        };

        using (var consumer = new ConsumerBuilder<string, string>(config).Build())
        {
            consumer.Subscribe(topic);
            try
            {
                while (true)
                {
                    var cr = consumer.Consume(cts.Token);
                    Console.WriteLine($"Consumed event from topic {topic}: key = {cr.Message.Key,-10} value = {cr.Message.Value}");
                }
            }
            catch (OperationCanceledException)
            {
                // Ctrl-C was pressed.
            }
            finally
            {
                consumer.Close();
            }
        }
    }
}