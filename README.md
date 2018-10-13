# Serverless Microservices reference architecture

View the [documentation](./documentation)

## Contents

- [Docs](./Docs): Contains project information such as architecture design documents, diagrams, and source images
- [documentation](./documentation): Contains the documentation that will be released with the solution, and included within Azure docs
- [Introduction](./documentation/introduction.md): Macro architecture, data storage, source code and monitoring
- [pipelines](./pipelines): Azure DevOps build and release pipeline definition files for running CI/CD operations on the static web app and all Azure Function Apps
- [Setup guide](./documentation/setup.md): Step-by-step instructions for provisioning Azure resources and deploying the solution
- [dotnet](./dotnet): Visual Studio 2017 solution and related projects (Function Apps, seeder, common libraries, etc.)
- [nodejs](./nodejs/serverless-microservices-functionapp-triparchiver): Node.js-based Function App for archiving trip data
- [web](./web/serverless-microservices-web): Static web app written in Vue.js
