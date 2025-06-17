🧠 Goal of the Project

Simulating a real-world event streaming system with Kafka using C#. The idea is:

    A Producer sends data (e.g., purchases).

    A Validator ensures data is well-formed and routes it forward.

    An Analytics service consumes validated data and analyzes it.

This mimics modern microservice event pipelines used in e-commerce, IoT, logging systems, etc.
📁 Project Structure (Organized)

/Producer/Producer.cs            🟢 Sends purchase events (JSON)
/Consumer/Validator.cs          🟡 Validates messages (soon)
/Consumer/Analytics.cs          🔵 Analyzes processed events

/Common/KafkaTopics.cs          📌 Shared topic names
/Common/Models/PurchaseEvent.cs📌 Shared data model

✅ STEP-BY-STEP BREAKDOWN
1. Producer.cs (in /Producer)

Purpose: Sends randomized purchase events to Kafka topic purchases.
How it works:

    Randomly picks a user and item.

    Builds a PurchaseEvent object.

    Serializes it to JSON using System.Text.Json.

    Sends the JSON string to the purchases topic in Kafka.

Example Message Sent:

{
  "UserId": "sgarcia",
  "Item": "t-shirts",
  "Timestamp": "2025-06-17T22:30:15Z"
}

Key concepts:

    Uses Confluent.Kafka for Kafka client.

    Topic name is managed via KafkaTopics.Purchases.

    Message key is the UserId, value is the JSON.

2. Common Project (in /Common)

Used to share constants and models across all services.
🔸 KafkaTopics.cs

A static class that holds the topic names:

public const string Purchases = "purchases";
public const string ProcessedPurchases = "processed-purchases";
public const string Analytics = "analytics";

Why? Centralizes configuration to avoid typos and hardcoding.
🔸 PurchaseEvent.cs

A shared data class:

public class PurchaseEvent
{
    public string UserId { get; set; }
    public string Item { get; set; }
    public DateTime Timestamp { get; set; }
}

Why? You’ll deserialize this in the validator and analytics services. Keeps type-safety and structure.
3. What’s Next: Validator (Validator.cs)

We'll build this next. It will:

    Consume from purchases topic.

    Deserialize JSON into PurchaseEvent.

    Check if data is valid (e.g. no nulls).

    Forward valid events to processed-purchases topic.

This step simulates a service that cleans or filters data — common in real-world pipelines.
4. Analytics (Analytics.cs)

Later, this service will:

    Read from processed-purchases

    Group or summarize data (e.g., top items, counts)

    Print results to console or save them

🧩 Built So Far (Summary)
Component	Role	Kafka Topic	Built? ✅
Producer	Sends random purchases	purchases	✅
Common	Shared models + topics	—	✅
Validator	Validates and re-publishes	processed-purchases	🛠️ Next
Analytics	Processes/aggregates data	processed-purchases
