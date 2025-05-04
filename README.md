How to run locally?

You need docker desktop installed and running. Select the "docker-compose" project as default startup project and run it.
	
1. When the sql server is running, you can connect to the sql server
2. use the following settings: localhost:1433 , User=sa , Pwd=sql,s!Passw0rd
3. run the SqlScripts\WriterDbSchemaCreation.sql script which will create the database and tables.


How to test/trigger the writer?

1. Open the RabbitMQ admin UI at http://localhost:15672/ (username: Administrator, password: admin).)
2. Go to the "Exchanges" tab and select the RebusTopic exchange.
3. Open "Publish message" section.
	- set the routing key to "AccountCreated"
	- set the following headers:
		- "rbs2-content-type" to "application/json"
		- "rbs2-msg-type" to "AccountCreated"
	- set the payload: 	{  "AccountId": "alice@example.com" }
4. Click "Publish message" button.
