# Introduction to serverless microservices

- [What are microservices?](#what-are-microservices)
- [What is serverless?](#what-is-serverless)
- [Next steps](#next-steps)

## What are microservices?

Microservices are independent modules that are small enough to take care of a single action, and can be independently built, verified, deployed, and monitored. Applications that are based on microservices combine these independent modules into a highly decoupled collection, providing the following additional benefits over traditional "monolithic" applications:

- **Autonomous scalability**: The independent microservices modules and their related services can be individually and automatically scaled based on their respective demands without impacting the application's overall performance. The ability to independently scale removes the need to scale the entire app up or down, potentially saving costs and reducing downtime.
- **Isolated points of failure**: Each of the services can be managed independently, isolating potential problem areas to individual services, and replacing or retiring services when deprecated or unused without affecting the overall structure and functionality.
- **Pick what's best**: Microservices solutions let development teams use the best deployment approach, language, platform and programming model for each service, providing flexibility in choosing technologies and tools.
- **Faster value delivery**: Microservices increase agility putting new features in production and adding business value to solutions, as the deployment of small and independent modules requires much less time and several teams can be working on different services at the same time, reducing development time and simplifying deployment.

Relecloud chose to capitalize on these benefits of a microservices architecture to help them build their Rideshare application in a way that enables their teams of developers to independently focus on portions of the solution based on their strengths without too many dependencies on what other teams are working on.

[Read more information](https://aka.ms/azure-microservices) on the benefits of building microservice-based applications.

## What is serverless?

The term "serverless" can be confusing to the uninitiated. It can be read as "no servers", in the way that "hopeless" means "no hope", or "cloudless sky" means "no clouds". In the case of serverless architecture, it simply means you focus "less on servers", and more on the functionality and features of your solution. This is because serverless abstracts away servers so you do not need to worry about server configuration, scaling of underlying resources is usually automatically handled for you based on load, number of messages, and other heuristics, and your deployments are done at the service and application-level rather than at the infrastructure-level. The end result is increased productivity, ease of development, simplified interoperability with other services through event-driven triggers and preconfigured service hooks, and increased choice of languages and tooling for the solution as a whole through mixing and matching various serverless components.

Relecloud recognized great value in combining the flexibility and service isolation of a microservices architecture, with the consumption-based (pay based on usage, like a utility bill), independent distributed nature of serverless technologies on Azure to rapidly build and grow their new Rideshare application. The combination of these architectures, deemed "serverless microservices", is ideal for helping them reach their goals for this application:

- Rapid development by their teams of developers who can focus on specific components of the solution without the usual dependency-riddled challenges of developing monolithic applications
- Global distribution of their architecture, with automatic scaling of individual components based on demand
- Consumption-based billing that saves them money during off-peak hours
- The ability to deploy updates to portions of the solution without affecting the application as a whole

The following sections detail Relecloud's architectural decisions, based on these goals. You can also [read more about serverless on Azure](https://aka.ms/serverless-azure), for more information on the serverless components used in this solution.

## Next steps

Read an overview of the reference architecture:

- [Architecture overview](architecture-overview.md)
  - [Macro architecture](architecture-overview.md#macro-architecture)
  - [Data flow](architecture-overview.md#data-flow)
