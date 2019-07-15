---
languages:
- csharp
- javascript
products:
- azure
- azure-functions
- azure-logic-apps
- azure-event-grid
- azure-cosmos-db
- azure-sql-database
- azure-storage
- azure-app-service
page_type: sample
description: "This architecture walks you through the process involved in developing the Rideshare by Relecloud application (a fictitious company)."
---

# Serverless Microservices reference architecture

## The reference architecture

![RideShare Macro Architecture](documentation/media/macro-architecture.png)

This reference architecture walks you through the decision-making process involved in designing, developing, and delivering the Rideshare by Relecloud application (a fictitious company). You will be given hands-on instructions for configuring and deploying all of the architecture's components, with helpful information about each component along the way. The goal is to provide you with practical hands-on experience in working with several integral Azure services and the technologies that effectively use them in a cohesive and unified way to build a serverless-based microservices architecture. We hope you will leave with a newfound appreciation for how serverless can be leveraged for both large-scale and small solutions, and the confidence to apply what you learned in your own projects.

## Customer scenario

Relecloud is a new kind of company, not unlike many cloud services-focused organizations popping up around the world today. Born of the modern digital age that is focused on capitalizing on the fast-paced growth of the booming cloud industry, Relecloud has a track record of building lean startups with little operational overhead, owing to its distributed and remote workforce. Their latest endeavor is a ride share application that seeks to carve its place in the popular industry with competitive rates and improved communication between passengers and drivers.

The company's technical leadership thrives on keeping tabs on the rapidly growing technology industry and new innovations built on top of fully-managed services provided by cloud providers. They are not interested in maintaining infrastructure, as they feel as though their company's time is best spent on their core strength: rapidly developing new and innovative applications that can scale to meet global demand. Like many other technical leaders, they are keen on learning new buzzwords and leaving it up to their developers to do something useful with them. The hottest trend these days seems to be serverless. The promise of consumption-based pricing, where you only pay for what you use and nothing more, is enticing. Furthermore, they have heard many good things about how serverless platforms help you prototype and develop faster by reducing the amount of code and configuration required by more traditional web platforms. The cherry on top is the fact that serverless platforms also tend to manage scaling resources to meet demand automatically, giving them dreams of releasing a wildly popular new ride share app and enjoying near-instantaneous customer growth.

During their initial research phase consisting of comparing serverless offerings and creating rapid prototypes, Relecloud's team has decided to build their ride share application on Azure's serverless components, given the breadth of options and unique capabilities for orchestrating serverless activities, such as [Durable Functions](https://docs.microsoft.com/azure/azure-functions/durable-functions-overview). They also want to investigate using the [microservices](https://aka.ms/azure-microservices) pattern in their solution design, as it seems like a good fit alongside [Azure functions](https://docs.microsoft.com/azure/azure-functions/functions-overview), [API Management](https://docs.microsoft.com/azure/api-management/api-management-key-concepts), [Event Grid](https://docs.microsoft.com/azure/event-grid/overview), and other key components and services. Being able to monitor the solution as a whole is an important capability they want to put in place from the start, especially since they are relying on so many components. Finally, they wish to simplify the lifecycle management of all these pieces of the puzzle by applying [DevOps](https://docs.microsoft.com/azure/devops/learn/what-is-devops) practices to automate continuous integration and deployment, end-to-end.

## Explore Relecloud's solution using serverless and microservices

[Read about Relecloud's solution](https://github.com/Azure-Samples/Serverless-microservices-reference-architecture/blob/master/documentation/introduction.md) and overall architecture design and decisions. The article will briefly explain the concepts around both serverless and microservices, and how they can be used together to build solutions with little to no infrastructure overhead. It will then walk you through the sample solution you will deploy in the lab, broken down into its architectural components.

## Deploy Relecloud's solution today with a hands-on lab

After learning about Relecloud's [serverless microservices architecture](https://github.com/Azure-Samples/Serverless-microservices-reference-architecture/blob/master/documentation/introduction.md), deploy the companion solution by following the step-by-step [hands-on lab](https://github.com/Azure-Samples/Serverless-microservices-reference-architecture/blob/master/documentation/setup.md), or take the shortcut and deploy with a few clicks [using our templates](https://github.com/Azure-Samples/Serverless-microservices-reference-architecture/blob/master/documentation/setup.md#cake-provision).

Each section of the lab will briefly explain what you are trying to accomplish and why. It will also link you to the relative portion of the [architecture document](https://github.com/Azure-Samples/Serverless-microservices-reference-architecture/blob/master/documentation/introduction.md).

## Detailed documentation

Use the table of contents below for detailed documentation of each component of the reference architecture.

- [Introduction to serverless microservices](https://github.com/Azure-Samples/Serverless-microservices-reference-architecture/blob/master/documentation/introduction.md)
  - [What are microservices?](https://github.com/Azure-Samples/Serverless-microservices-reference-architecture/blob/master/documentation/introduction.md#what-are-microservices)
  - [What is serverless?](https://github.com/Azure-Samples/Serverless-microservices-reference-architecture/blob/master/documentation/introduction.md#what-is-serverless)
- [Architecture overview](https://github.com/Azure-Samples/Serverless-microservices-reference-architecture/blob/master/documentation/architecture-overview.md)
  - [Macro architecture](https://github.com/Azure-Samples/Serverless-microservices-reference-architecture/blob/master/documentation/architecture-overview.md#macro-architecture)
  - [Data flow](https://github.com/Azure-Samples/Serverless-microservices-reference-architecture/blob/master/documentation/architecture-overview.md#data-flow)
- [Initial setup](https://github.com/Azure-Samples/Serverless-microservices-reference-architecture/blob/master/documentation/setup.md)
- [API endpoints using Azure Functions](https://github.com/Azure-Samples/Serverless-microservices-reference-architecture/blob/master/documentation/api-endpoints.md)
  - [RideShare APIs](https://github.com/Azure-Samples/Serverless-microservices-reference-architecture/blob/master/documentation/api-endpoints.md#rideshare-apis)
  - [Durable Orchestrators](https://github.com/Azure-Samples/Serverless-microservices-reference-architecture/blob/master/documentation/api-endpoints.md#durable-orchestrators)
- [Services intercommunication using Event Grid](https://github.com/Azure-Samples/Serverless-microservices-reference-architecture/blob/master/documentation/services-intercommunication.md)
  - [Logic App handler](https://github.com/Azure-Samples/Serverless-microservices-reference-architecture/blob/master/documentation/services-intercommunication.md#logic-app-handler)
  - [SignalR handler](https://github.com/Azure-Samples/Serverless-microservices-reference-architecture/blob/master/documentation/services-intercommunication.md#signalr-handler)
    - [.NET SignalR client](https://github.com/Azure-Samples/Serverless-microservices-reference-architecture/blob/master/documentation/services-intercommunication.md#dotnet-signalr-client)
    - [JavaScript SignalR client](https://github.com/Azure-Samples/Serverless-microservices-reference-architecture/blob/master/documentation/services-intercommunication.md#javascript-signalr-client)
  - [Power BI handler](https://github.com/Azure-Samples/Serverless-microservices-reference-architecture/blob/master/documentation/services-intercommunication.md#power-bi-handler)
  - [Trip Archiver handler](https://github.com/Azure-Samples/Serverless-microservices-reference-architecture/blob/master/documentation/services-intercommunication.md#trip-archiver-handler)
- [Gateway with API Management](https://github.com/Azure-Samples/Serverless-microservices-reference-architecture/blob/master/documentation/api-management.md)
- [Data storage](https://github.com/Azure-Samples/Serverless-microservices-reference-architecture/blob/master/documentation/data-storage.md)
- [Client application](https://github.com/Azure-Samples/Serverless-microservices-reference-architecture/blob/master/documentation/client-application.md)
  - [Passengers page](https://github.com/Azure-Samples/Serverless-microservices-reference-architecture/blob/master/documentation/client-application.md#passengers-page)
  - [Drivers page](https://github.com/Azure-Samples/Serverless-microservices-reference-architecture/blob/master/documentation/client-application.md#drivers-page)
  - [Authentication](https://github.com/Azure-Samples/Serverless-microservices-reference-architecture/blob/master/documentation/client-application.md#authentication)
  - [Wrapping HTTP calls with authentication token](https://github.com/Azure-Samples/Serverless-microservices-reference-architecture/blob/master/documentation/client-application.md#wrapping-http-calls-with-authentication-token)
- [Monitoring and testing](https://github.com/Azure-Samples/Serverless-microservices-reference-architecture/blob/master/documentation/monitoring-testing.md)
  - [Integration testing](https://github.com/Azure-Samples/Serverless-microservices-reference-architecture/blob/master/documentation/monitoring-testing.md#integration-testing)
  - [Monitoring](https://github.com/Azure-Samples/Serverless-microservices-reference-architecture/blob/master/documentation/monitoring-testing.md#monitoring)
    - [Telemetry correlation](https://github.com/Azure-Samples/Serverless-microservices-reference-architecture/blob/master/documentation/monitoring-testing.md#telemetry-correlation)
    - [Monitoring for different audiences](https://github.com/Azure-Samples/Serverless-microservices-reference-architecture/blob/master/documentation/monitoring-testing.md#monitoring-for-different-audiences)
- [Source code structure](https://github.com/Azure-Samples/Serverless-microservices-reference-architecture/blob/master/documentation/source-code-structure.md)
  - [.NET](https://github.com/Azure-Samples/Serverless-microservices-reference-architecture/blob/master/documentation/source-code-structure.md#net)
  - [Node.js](https://github.com/Azure-Samples/Serverless-microservices-reference-architecture/blob/master/documentation/source-code-structure.md#nodejs)
  - [Web](https://github.com/Azure-Samples/Serverless-microservices-reference-architecture/blob/master/documentation/source-code-structure.md#web)
