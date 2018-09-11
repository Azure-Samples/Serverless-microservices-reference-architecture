# Login to Azure - the client-id, the client-password and the tenant password are the same as setup in the Cake provision section
az login --service-principal -u <your-client-id> -p <your-client-password> --tenant <your-tenant-id>

# Display all accounts
az account list --output table

# Make sure you are using the proper subs
az account set --subscription "your-subs"

# Create a resource group
az group create --name serverless-microservices-k8s --location eastus

# Create a 1-node AKS cluster 
az aks create --resource-group serverless-microservices-k8s --name rideshareAKSCluster --service-principal <your-client-id> --client-secret <your-password>  --node-count 1 --enable-addons monitoring --generate-ssh-keys

# Make sure Kubectl is installed
az aks install-cli

# Allow Kubectrl use the cluster by getting the cluster credentials
az aks get-credentials --resource-group serverless-microservices-k8s --name rideshareAKSCluster 

# Check the cluster nodes
kubectl get nodes

# Deploy the rideshare app
kubectl apply -f rideshare-app.yaml

# Wait until the services expose drivers, passengers and trips to the Internet 
kubectl get service rideshare-drivers --watch
kubectl get service rideshare-passengers --watch
kubectl get service rideshare-trips --watch

