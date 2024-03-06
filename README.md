# FGJTAM Directory API

## Overview
Api for manage people data

## Installation

### Ubuntu
1. Update package list:
> `sudo apt-get update`

2. Install .NET SDK and runtime .NET 8:
 > `sudo apt-get install -y dotnet-sdk-8.0`

 > `sudo apt-get install -y aspnetcore-runtime-8.0`


For full installation instructions on Ubuntu, refer to the [official .NET documentation](https://learn.microsoft.com/en-us/dotnet/core/install/linux-ubuntu-2204).

---
## Publish
### Ubuntu
1. Ensure .NET runtime is installed:
2. Navigate to the directory containing the *.csproj file.
3. Run the following command to publish the application:
> `dotnet publish --configuration Release`

This command generates the publish folder in the directory, e.g., `bin/Release/net8.0/publish`.

## Usage

Ensure you have the connection string and a secret key defined in the `appsettings.json` file.


- Set the connection string of the database replacing `{myConnectionstring}` :
    ```json
    {
      "ConnectionStrings": {
        "AuthApi": "{myConnectionstring}"
      },
      "Logging": {},    
      "Secret": "",
    }
    ```

- Set a new secret key used to encrypt data on the database and generate JWT Tokens by replacing `{my_secret_string}` :
    ```json
    {
      "ConnectionStrings": {},
      "Logging": {},    
      "Secret": "{my_secret_string}",
    }
    ```


To run the application, execute the following command:
> `dotnet bin/Release/net8.0/publish/{projectName}.dll`

Replace `{projectName}` with the actual name of your project.

## Hosting
### Ubuntu
For information on hosting a .NET API on Ubuntu, refer to the [official documentation](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx?view=aspnetcore-8.0&tabs=linux-ubuntu).

## Contributors

- [JuanRangel-FGJTam](https://github.com/JuanRangel-FGJTam)
