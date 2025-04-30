How to run locally?

You need docker desktop installed and running. Select the "docker-compose" project as default startup project and run it.


How to test/trigger the writer?

1. Open the RabbitMQ admin UI at http://localhost:15672/ (username: Administrator, password: admin).)
2. Go to the "Exchanges" tab and select the RebusTopic exchange.
3. Open "Publish message" section.
	- set the routing key to "AccountCreatedEvent"
	- set the following headers:
		- "rbs2-content-type" to "application/json"
		- "rbs2-msg-type" to "AccountCreatedEvent"
	- set the payload: 	{  "AccountId": "alice@example.com",  "OccurredAt": "2025-04-30T08:00:00Z" }
4. Click "Publish message" button.
