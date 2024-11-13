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

## Configuration

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
    
### Email Settings
The application sends emails for three main purposes: validating a user's email during registration, resetting passwords, and sending welcome messages upon successful registration.
Configuration Example

Include the following settings in your configuration file (appsettings.json):
```json
{
 "ResetPasswordSettings":{
     "TokenLifeTimeSeconds": 3600,
     "DestinationUrl": "https://auth.fgjtam.gob.mx/api/person/password-reset"
   },
   "EmailSettings":{
     "ApiUri": "https://api-email.fgjtam.gob.mx/api/v1/send-email",
     "From": "from@fgjtam.gob.mx",
     "Token": "authToken"
   },
   "WelcomeEmailSources": {
     "ImageNameSrc": "url-image-banner",
     "ImageProfileSrc": "url-image-profile"
   }
}
```

### Minio Settings
The API uses MinIO to store files, so ensure MinIO is configured properly in your application’s settings. The MinioSettings configuration includes essential properties for connecting to your MinIO instance.
Configuration Example:

Add the MinioSettings section to your configuration file (appsettings.json):
```json
{
 "MinioSettings": {
    "Endpoint": "host-port-without-http",
    "AccessKey": "key",
    "SecretKey": "secret",
    "BucketName": "directory-api"
  },
}
```
Properties:
 - **Endpoint**: The URL of your MinIO server, without the http or https schema. The URL should include the port, like "minio.example.com:9000".
 - **BucketName**: The name of the bucket where files will be stored.  If the bucket doesn’t exist, the application will create it automatically.


## Usage

To run the application, execute the following command:
> `dotnet bin/Release/net8.0/publish/{projectName}.dll`

Replace `{projectName}` with the actual name of your project.

## Hosting

### Ubuntu
For information on hosting a .NET API on Ubuntu, refer to the [official documentation](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx?view=aspnetcore-8.0&tabs=linux-ubuntu).

- Create the service file

`sudo vim /etc/systemd/system/api-fd.service`

```bash
[Unit]
Description=API Fiscalia Digital

[Service]
WorkingDirectory=/var/www/DirectoryApi
ExecStart=/usr/bin/dotnet /var/www/DirectoryApi/bin/Release/net8.0/AuthApi.dll
Restart=always
# Restart service after 10 seconds if the dotnet service crashes:
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=dotnet-example
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_NOLOGO=true

[Install]
WantedBy=multi-user.target
```

- Save the file and enable the service.

`sudo systemctl enable api-fd.service`

- Show the logs

`sudo journalctl -fu api-fd.service`

## Contributors

- [JuanRangel-FGJTam](https://github.com/JuanRangel-FGJTam)
