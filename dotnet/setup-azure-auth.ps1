# Login
Login-AzureRmAccount

# Set the Subscriptions
Get-AzureRmSubscription  

# Set the Subscription to your preferred subscription
Select-AzureRmSubscription -SubscriptionId "<your_subs_id>"

# Create an application in Azure AD
$pwd = convertto-securestring "<your_pwd>" -asplaintext -force
$app = New-AzureRmADApplication  -DisplayName "RideSharePublisher"  -HomePage "http://rideshare" -IdentifierUris "http://rideshare" -Password $pwd

# Create a service principal
New-AzureRmADServicePrincipal -ApplicationId $app.ApplicationId

# Assign role
New-AzureRmRoleAssignment -RoleDefinitionName Contributor -ServicePrincipalName $app.ApplicationId.Guid
