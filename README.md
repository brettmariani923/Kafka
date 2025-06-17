✅ Step-by-Step Flow
1. Message Creation

File: ProducerProgram.cs

    A list of random users and items is defined.

    A loop generates fake PurchaseEvent objects:

    new PurchaseEvent { UserId = "eabara", Item = "book", Timestamp = DateTime.UtcNow }

    These messages simulate actual purchases.

2. Sending Messages to Kafka

File: KafkaProducerService.cs

    The PurchaseEvent is serialized to JSON and sent to the Kafka topic "purchases":

    await _producer.ProduceAsync(KafkaTopics.Purchases, message);

🔁 Extra logic: Also sends string summaries to "Analytics" partitions 0 and 1 — useful for load balancing or advanced analytics.

3. Consuming + Validating Purchases

File: PurchaseProcessor.cs

    Subscribes to the "purchases" topic.

    For each message:

        Deserializes it from JSON.

        Uses Validator.cs to ensure:

            The message is valid JSON.

            It contains a non-empty UserId and Item.

        If valid, it’s forwarded to the "Analytics" topic:

        await producer.ProduceAsync(KafkaTopics.Analytics, validMessage);

        If invalid, it’s skipped and logged.

✅ Purpose: This stage ensures only clean, meaningful data reaches the analytics system.
4. Analyzing Clean Data

File: AnalyticsConsumer.cs

    Subscribes to the "Analytics" topic.

    For each valid message:

        Deserializes it to a PurchaseEvent.

        Updates:

            userPurchaseCount → how many times each user purchased.

            itemCount → how often each item was bought.

        Logs each purchase.

        Every 5th purchase (based on a formula), prints a full snapshot:

        👥 Purchases per user:
          - eabara: 2
        📦 Items purchased:
          - book: 2

✅ Purpose: Real-time tracking of user behavior and product demand.
🧩 Core Components & Responsibilities
File/Class	Responsibility
PurchaseEvent.cs	Defines the structure of a purchase message.
KafkaTopics.cs	Centralized names for Kafka topics.
ProducerProgram.cs	Entry point for simulating and sending fake purchases.
KafkaProducerService.cs	Handles how messages are sent to Kafka.
PurchaseProcessor.cs	Validates raw purchase events and forwards clean data.
Validator.cs	Tries to parse and validate each JSON message.
AnalyticsConsumer.cs	Reads clean data and logs aggregate stats.
🧭 End-to-End Data Flow (Simplified)
```
[ProducerProgram] ──> "purchases" topic ──┬──> [PurchaseProcessor]
                                         │       └── valid messages ──> "Analytics" topic ──> [AnalyticsConsumer]
                                         │
                                         └── invalid messages ──> skipped/logged
```
