# Login
Login-AzureRmAccount

# Set the Subscriptions
Get-AzureRmSubscription  

# Set the Subscription to Solliance Development-M
Select-AzureRmSubscription -SubscriptionId "2a9e48ea-0698-4222-8414-7b917880a2da"

# Set the Subscription to Solliance Client
#Select-AzureRmSubscription -SubscriptionId "e433f371-e5e9-4238-abc2-7c38aa596a18"

# Create an application in Azure AD
$pwd = convertto-securestring "S!TE_100_client" -asplaintext -force
$app = New-AzureRmADApplication  -DisplayName "RideSharePublisher"  -HomePage "http://rideshare" -IdentifierUris "http://rideshare" -Password $pwd

# Create a service principal
New-AzureRmADServicePrincipal -ApplicationId $app.ApplicationId

# Assign role
New-AzureRmRoleAssignment -RoleDefinitionName Contributor -ServicePrincipalName $app.ApplicationId.Guid
