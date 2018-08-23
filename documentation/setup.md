# Serverless Microservices reference architecture

Brief introduction

## Step 1: Create the Azure function apps

**--FORMAT-- Have intro under each step explaining the concepts, what they're doing, and why. Link to associated section in [Introduction](./introduction.md)**

**-- Show code snippets within a section if appropriate. Like when provisioning Event Grid and creating topics, maybe show code snippet or two from functions where the topics are being used --**

In this step, you will be creating six new function apps in the Azure portal. There are many ways this can be accomplished, such as [publishing from Visual Studio](), [Visual Studio Code](), the [Azure CLI](), Azure [Cloud Shell](), an [Azure Resource Manager (ARM) template](), and through the Azure portal.

Each of these function apps act as a hosting platform for one or more functions. In our solution, they double as microservices with each function serving as an endpoint or method. Having functions distributed amongst multiple function apps enables isolation, providing physical boundaries between the microservices, as well as independent release schedules, administration, and scaling.

1.  Log in to the [Azure portal](https://portal.azure.com).

1.  Type **Function App** into the Search box at the top of the page, then select **Function App** within the Marketplace section.

    ![Type Function App into the Search box](media/function-app-search-box.png 'Function App search')

1.  Complete the function app creation form with the following:

    a. **App name**: Enter a unique value for the **Drivers** function app.
    b. **Subscription**: Select your Azure subscription.
    c. **Resource Group**: Either select an existing Resource Group or create a new one such as "serverless-microservices".
    d. **OS**: Select Windows.
    e. **Hosting Plan**: Select Consumption Plan.
    f. **Location**: Select a region closest to you. Make sure you select the same region for the rest of your resources.
    g. **Storage**: Select Create new and supply a unique name. You will use this storage account for the remaining function apps.
    h. **Application Insights**: Set to Off. We will create an Application Insights instance later that will be associated with all of the Function Apps and other services.

    ![Screenshot of the Function App creation form](media/new-function-app-form.png 'Create Function App form')

1.  Repeat the steps above to create the **Trips** function app.

    a. Enter a unique value for the App name, ensuring it has the word **Trips** within the name so you can easily identify it.
    b. Make sure you enter the same remaining settings and select the storage account you created in the previous step.

1.  Repeat the steps above to create the **Orchestrators** function app.

1.  Repeat the steps above to create the **Passengers** function app.

1.  Repeat the steps above to create the **ActiveDrivers** function app.

1.  Repeat the steps above to create the **ActiveTrips** function app.

At this point, your Resource Group should have a list of resources similar to the following:

![List of resources in the Resource Group after creating function apps](media/resource-group-function-apps.png 'Resource Group resource list')
