using Confluent.Kafka;
using System;
using System.Threading;

class Analytics
{
    //Adjust the startup object in the csproj file depending on what you want to run.
    static void Main(string[] args)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = "pkc-619z3.us-east1.gcp.confluent.cloud:9092",
            SaslUsername = "EPQFY3IVDZJ27ZUH",
            SaslPassword = "V+CQyESrHfHaaoCcIlwiwTEnkmUDFt+5EzwfHwYsdb2HDNrm/O0h3Nlcl6E06GY3",
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslMechanism = SaslMechanism.Plain,
            GroupId = "Kafka",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using (var consumer = new ConsumerBuilder<string, string>(config).Build())
        {
            consumer.Subscribe(new[] { "Analytics" });

            CancellationTokenSource cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) => {
                e.Cancel = true;
                cts.Cancel();
            };

            try
            {
                while (true)
                {
                    var cr = consumer.Consume(cts.Token);
                    Console.WriteLine($"[{cr.Topic}] Key: {cr.Message.Key}, Value: {cr.Message.Value}");
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                consumer.Close();
            }
        }
    }
}
