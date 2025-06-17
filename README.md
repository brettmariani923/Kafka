🧾 Kafka Purchase Event Pipeline

This project simulates a full Kafka event-driven pipeline using .NET 8. It showcases real-time data streaming with message validation, multi-topic routing, and analytics aggregation.
📦 Project Structure

KafkaPipeline/
│
├── Common/              # Shared models and constants
│   ├── Models/
│   │   └── PurchaseEvent.cs
│   └── KafkaTopics.cs
│
├── Producer/            # Publishes purchase events to Kafka
│   ├── KafkaProducerService.cs
│   └── ProducerProgram.cs
│
├── Consumer/            # Consumes, validates, and analyzes events
│   ├── Validator.cs           # Validates raw messages
│   ├── PurchaseProcessor.cs  # Validates and forwards to Analytics
│   └── AnalyticsConsumer.cs  # Consumes valid events and generates analytics


🛠 Technologies Used

    .NET 8

    Confluent Kafka .NET Client

    Kafka (Cloud-hosted via Confluent Cloud)

    JSON serialization via System.Text.Json

## 🔄 Data Flow Overview

ProducerApp

   |
   v
Kafka Topic: purchases

   |
   v
PurchaseProcessor (Validator)

   |
   v
Kafka Topic: Analytics

   |
   v
AnalyticsConsumer




📤 Producer Logic

📄 ProducerProgram.cs

    Simulates purchases from random users and items.

    Sends:

        Raw purchase data to purchases topic.

        Formatted analytics messages to specific partitions of the Analytics topic.

🧱 KafkaProducerService.cs

    Encapsulates Kafka producing logic:

        Sends raw purchase messages.

        Sends to specific topic partitions.

        Flushes Kafka buffers after message production.

✅ Validation Pipeline

📄 PurchaseProcessor.cs

    Subscribes to the purchases topic.

    Validates messages using Validator.cs.

    Forwards valid purchases to the Analytics topic.

📄 Validator.cs

    Deserializes JSON.

    Checks:

        JSON format is correct.

        UserId and Item are present.

📊 Analytics Aggregation

📄 AnalyticsConsumer.cs

    Subscribes to the Analytics topic.

    Deserializes and tracks:

        Purchase counts per user.

        Item popularity.

    Logs analytics snapshot every few messages.

🧪 Sample Output

🔍 Listening to topic: purchases
📥 Received message: {"UserId":"jsmith","Item":"book","Timestamp":"..."}
✅ Valid purchase forwarded to Analytics: jsmith bought book

📊 Listening to topic: Analytics
🧾 jsmith bought book

📈 Current Analytics Snapshot:
👥 Purchases per user:
  - jsmith: 3
📦 Items purchased:
  - book: 2
  - gift card: 1

📚 Learning Objectives

✅ How to:

    Use Kafka producers/consumers in .NET.

    Route messages between Kafka topics.

    Validate and deserialize JSON messages.

    Design reusable Kafka services.

    Build end-to-end data pipelines with clean architecture.

🧠 Concepts Demonstrated

    Clean Separation of Concerns

        Producer = what and when to send

        Processor = whether it should be sent

        Consumer = what to do with messages

    Stream Processing Pipeline

        Raw ➜ Validated ➜ Aggregated

    Partitioned Topics

        Simulates load-balancing or message routing by partition
