# Serverless Microservices reference architecture

## The reference architecture

This reference architecture walks you through the decision-making process involved in designing, developing, and delivering the Rideshare by Relecloud application. You will be given hands-on instructions for configuring and deploying all of the architecture's components, with helpful information about each component along the way. The goal is to provide you with practical hands-on experience in working with several integral Azure services and the technologies that effectively use them in a cohesive and unified way to build a serverless-based microservices architecture. We hope you will leave with a newfound appreciation for how serverless can be leveraged for both large-scale and small solutions, and the confidence to apply what you learned in your own projects.

## Customer scenario

Relecloud is a new kind of company, not unlike many cloud services-focused organizations popping up around the world today. Born of the modern digital age that is focused on capitalizing on the fast-paced growth of the booming cloud industry, Relecloud has a track record of building lean startups with little operational overhead, owing to its distributed and remote workforce. Their latest endeavor is a ride share application that seeks to carve its place in the popular industry with competitive rates and improved communication between passengers and drivers.

The company's technical leadership thrives on keeping tabs on the rapidly growing technology industry and new innovations built on top of fully-managed services provided by cloud providers. They are not interested in maintaining infrastructure, as they feel as though their company's time is best spent on their core strength: rapidly developing new and innovative applications that can scale to meet global demand. Like many other technical leaders, they are keen on learning new buzzwords and leaving it up to their developers to do something useful with them. The hottest trend these days seems to be serverless. The promise of consumption-based pricing, where you only pay for what you use and nothing more, is enticing. Furthermore, they have heard many good things about how serverless platforms help you prototype and develop faster by reducing the amount of code and configuration required by more traditional web platforms. The cherry on top is the fact that serverless platforms also tend to manage scaling resources to meet demand automatically, giving them dreams of releasing a wildly popular new ride share app and enjoying near-instantaneous customer growth.

During their initial research phase consisting of comparing serverless offerings and creating rapid prototypes, Relecloud's team has decided to build their ride share application on Azure's serverless components, given the breadth of options and unique capabilities for orchestrating serverless activities, such as [Durable Functions](). They also want to investigate using the [microservices]() pattern in their solution design, as it seems like a good fit alongside [Azure functions](), [API Management](), [Service Bus](), [Event Grid](), and other key components and services. Being able to monitor the solution as a whole is an important capability they want to put in place from the start, especially since they are relying on so many components. Finally, they wish to simplify the lifecycle management of all these pieces of the puzzle by applying [DevOps]() practices to automate continuous integration and deployment, end-to-end.

## Explore Relecloud's solution using serverless and microservices

[Read about Relecloud's solution](./introduction.md) and overall architecture design and decisions. The article will briefly explain the concepts around both serverless and microservics, and how they can be used together to build solutions with little to no infrastructure overhead. It will then walk you through the sample solution you will deploy in the lab, broken down into its architectural components.

## Deploy Relecloud's solution today with a hands-on lab

After learning about Relecloud's [serverless microservices architecture](./introduction.md), deploy the companion solution by following the step-by-step [hands-on lab](./setup.md), or take the shortcut and deploy with a few clicks [using our templates]().

Each section of the lab will briefly explain what you are trying to accomplish and why. It will  also link you to the relative portion of the [architecture document](./introduction.md).
