# Introduction to serverless microservices

## What are microservices?

Explain in one or two paragraphs what microservices are, and the concepts. Link out to MS docs for full explanation.

## What is serverless?

Like the previous section, one or two paragraphs explaining serverless concepts, components in Azure, then links to docs.

## How do they work together?

One or two paragraphs describing how microservices can really be serverless components, and how you have them communicate.

## Technology used

Relecloud decided to use the following components in their ride share solution. (Here's what they picked and why)

Show architecture diagram and lable/explain its components.

**Different sections of the step-by-step lab instructions will point to the related technology below for more context**

Some explanation of their reasoning for the architecture as a whole, like which components are considered to be "microservices" and why those boundaries were chosen, like for independent scalability, different technology or language choices, etc.

Explain how messaging works in the big picture.

Touch on monitoring, how it's done and what it allows you to do. Perhaps mention Aplication Insight's live metrics view.

### API Management

Why was this selected? What does it do for the solution, and in particular Relecloud's requirements?

How could this be expanded in the future beyond the scope of the sample solution? Things like rate-limiting when sharing with external customers, etc.

### Azure Functions

Why was this selected? What does it do for the solution, and in particular Relecloud's requirements?

How could this be expanded in the future beyond the scope of the sample solution?

What are some potential challenges?

#### Durable Functions

What makes Durable Functions different than standard ones? What do they solve for Relecloud? Remember, they said this was one of the things that set Azure's FaaS offering apart from the competition.

Explain how Durable Functions are being used in the solution for orchestration. How could what is shown in the sample solution be expanded on? What are some other possible scenarios (briefly touch on fan out/fan in, etc.)

### Events and messaging

Why was this selected? What does it do for the solution, and in particular Relecloud's requirements?

How could this be expanded in the future beyond the scope of the sample solution?

What are some potential challenges?

What are the differences between events and messaging, and how do you choose?

How do these help "glue" the microservices together in the solution?

### Service Bus

...

### Event Grid

...

### SignalR Service

...

### Data storage

What storage options were selected and why? What do these choices do for the solution, and in particular Relecloud's requirements?

How could this be expanded in the future beyond the scope of the sample solution?

What are some potential challenges?
