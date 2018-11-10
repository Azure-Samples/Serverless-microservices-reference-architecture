# Data storage

- [Next steps](#next-steps)

Relecloud decided to use [Azure Cosmos DB](https://docs.microsoft.com/en-us/azure/cosmos-db/introduction) as the main data storage for the solution entities. Since Relecloud targets a globally distributed audience accessing its services from different parts of the world, Cosmos DB provides key advantages:

- A global distribution capability replicates data in different Azure Data centers around the world making the data closer to consumers thereby reducing the response time.
- Independent storage and throughput scale capability allows for great granularity and flexibility that can be used to adjust for unpredictable usage patterns.
- Being the main centric entities in the solution, `Trip` entities capture the trip state such as the associated driver, the associated passenger, the available drivers and many other metrics. It is more convenient to query and store `Trip` entities as a whole without requiring transformation or complex object to relational mapping layers.
- Trip schema can change without having to go through database schema changes. Only the application code will have to adjust to the schema changes.

**Please note** that the Cosmos DB `Main` and `Archive` collections used in the reference implementation use a fixed data size and the minimum 400 RUs without a partition key. This will have to be addressed in a real solution.

In addition to Azure Cosmos DB, Relecloud decided to use [Azure SQL Database](https://azure.microsoft.com/en-us/services/sql-database/) to persist trip summaries so they can be reported on in Power BI, for example. Please refer to [Power BI Handler](services-intercommunication.md#power-bi-handler) section of the [services intercommunication](services-intercommunication.md) document for more details.

## Next steps

Set up the storage components for the reference architecture:

- [Create the Azure Cosmos DB assets](setup.md#create-the-azure-cosmos-db-assets)
- [Create the Storage Account](setup.md#create-the-storage-account)
- [Create the Azure SQL Database assets](setup.md#create-the-azure-sql-database-assets)

Read about the single page application (SPA) used as a user-facing static website for this reference architecture:

- [Client application](client-application.md)
  - [Passengers page](client-application.md#passengers-page)
  - [Drivers page](client-application.md#drivers-page)
  - [Authentication](client-application.md#authentication)
  - [Wrapping HTTP calls with authentication token](client-application.md#wrapping-http-calls-with-authentication-token)
