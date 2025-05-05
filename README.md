# Event Sourcing POC with CQRS

This project is a Proof of Concept (POC) demonstrating **Event Sourcing** and **Command Query Responsibility Segregation (CQRS)** using separate **Reader** and **Writer** services. The architecture showcases how to decouple the write and read sides of an application, leveraging different databases for each side.

## Overview

The project consists of two main services:

1. **Writer Service**:
   - Handles commands (write operations) and persists events in a relational database (MS SQL).
   - Implements event sourcing by storing domain events that represent state changes.
   - Publishes events to a message broker (e.g., RabbitMQ) for the Reader service to consume.

2. **Reader Service**:
   - Handles queries (read operations) and maintains the view of the data.
   - Uses MongoDB as the database for fast and flexible querying.
   - Subscribes to events published by the Writer service to update its state.

## Business Domain

The system models a simple **Account** with the following properties:
- **Email (AccountId)**: A unique identifier for the account, typically the user's email address.
- **Balance**: The current monetary balance of the account.

### Writer Service
The Writer service is triggered by external services, such as:
- **User Service**: Registers a new account when a user is created.
- **Payment Service**: Adds or removes money from an account based on payment transactions.

The Writer service processes these commands and generates domain events, such as:
- `AccountCreatedEvent`: Represents the creation of a new account.
- `AccountCreditedEvent`: Represents a credit operation (adding money) to an account.
- `AccountDebitedEvent`: Represents a debit operation (removing money) from an account.

### Basic Validation
Before processing commands, the Writer service performs basic validation to ensure data integrity:
1. **Email Validation**: Ensures the `AccountId` is a valid email address format.
2. **Balance Validation**: Ensures that the account balance does not drop below zero during a debit operation.
3. **Duplicate Account Check**: Prevents the creation of duplicate accounts with the same `AccountId`.

If any validation fails, the command is rejected, and an appropriate error is logged or returned to the caller.

### Message Reply
The system supports a **message reply mechanism** to handle scenarios where the Reader service detects inconsistencies in its materialized view. This process involves the following steps:

1. **AccountStateCorruptedEvent**:
   - It triggers the Reader to rebuild the state of the given account.
   - This event includes the `AccountId` of the affected account.

2. **Reader Cleanup**:
   - Upon detecting the corrupted state, the Reader deletes the account's data from its materialized view to prepare for a full rebuild.

3. **AccountReplyRequestedEvent**:
   - The Reader triggers the Writer by publishing an `AccountReplyRequestedEvent`.
   - This event requests the Writer to resend all events related to the specified `AccountId`.

4. **Writer Response**:
   - The Writer processes the `AccountReplyRequestedEvent` and retrieves all events for the specified account from its event store.
   - These events are republished to RabbitMQ in the correct order.

5. **Reader Replay**:
   - The Reader consumes the republished events and replays them to rebuild the materialized view for the account.
   - It can handle unordered events as well.

This mechanism ensures that the Reader can recover from inconsistencies and maintain an accurate and up-to-date materialized view of the data.

## Key Concepts

### Event Sourcing
- Instead of persisting the current state of an entity, the system stores a sequence of events that represent state changes.
- The current state can be reconstructed by replaying these events.

### CQRS
- Separates the responsibilities of reading and writing data into distinct services.
- The **Writer** service is responsible for handling commands and persisting events.
- The **Reader** service is responsible for handling queries and maintaining a read-optimized view of the data.

## How to Run Locally

To run the project locally, follow these steps:

1. Ensure **Docker Desktop** is installed and running.
2. Set the "docker-compose" project as the default startup project and run it.
3. Once the SQL Server container is running, connect to it using the following settings:
   - Host: `localhost:1433`
   - User: `sa`
   - Password: `sql,s!Passw0rd`
4. Execute the `SqlScripts\WriterDbSchemaCreation.sql` script to create the database and tables.

## How to Test/Trigger the Writer

To test the Writer service, you can manually publish a message to RabbitMQ:

1. Open the RabbitMQ admin UI at [http://localhost:15672/](http://localhost:15672/) (username: `Administrator`, password: `admin`).
2. Navigate to the "Exchanges" tab and select the `RebusTopic` exchange.
3. In the "Publish message" section:
   - Set the routing key to `UserCreatedEvent`.
   - Add the following headers:
     - `"rbs2-content-type"`: `application/json`
     - `"rbs2-msg-type"`: `UserCreatedEvent`
   - Set the payload to: { "AccountId": "alice@example.com" }    
4. Click the "Publish message" button.

## Technologies Used
- **.NET 8**: The project is built using the latest version of .NET.
- **MS SQL**: Relational database for the Writer service.
- **MongoDB**: NoSQL database for the Reader service.
- **RabbitMQ**: Message broker for event communication between services.

## Future Improvements
- Add more robust error handling and retries for event processing.
- Implement snapshots to optimize event replay for large datasets.
- Ensure that events consumed by the Writer contain a unique ID to allow the Writer to ignore duplicate messages. This is already handled in the Reader, as messages sent by the Writer are unique (based on `AccountId` and `Version`).

## Conclusion
This POC demonstrates the power of event sourcing and CQRS in building scalable and maintainable systems. By separating the write and read sides, the architecture ensures better performance, scalability, and flexibility for future enhancements.


