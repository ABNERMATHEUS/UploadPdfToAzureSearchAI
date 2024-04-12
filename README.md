
Replace `<Your Azure Storage Connection String>`, `<Your Container Name>`, `<Your Azure Search Service Name>`, and `<Your Azure Search Service API Key>`, with your actual values.

## Running the Application

1. Build the solution in Visual Studio.
2. Run the application. This will start the application and listen for incoming HTTP requests.
3. Use a tool like Postman to send a POST request to `http://localhost:<port>/upload/uploadpdfs` with your PDF files in the form data. Replace `<port>` with the port number your application is listening on.

## Understanding the Code

The `UploadController` class handles the PDF uploads. It has a constructor that takes an `IConfiguration` parameter, which is automatically provided by ASP.NET Core's dependency injection system. The constructor uses the `IConfiguration` parameter to get the Azure Storage and Azure Search details from the `appsettings.json` file.

The `UploadPdfs` method handles the PDF uploads. It creates a `CloudBlobClient` and a `SearchServiceClient`, uploads the PDFs to Azure Blob Storage, and then creates an index and an indexer in Azure Search. The index schema is inferred from the `MyModel` class. The indexer is configured to extract both content and metadata from the PDFs and to use the default parsing mode.


{
  "AzureStorage": {
    "ConnectionString": "<Your Azure Storage Connection String>",
    "ContainerName": "<Your Container Name>"
  },
  "AzureSearch": {
    "ServiceName": "<Your Azure Search Service Name>",
    "ApiKey": "<Your Azure Search Service API Key>"
  }
}
