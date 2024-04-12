using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.WindowsAzure.Storage;

namespace UploadAzureSearch.Controllers
{
    /// <summary>
    /// Controller for handling PDF uploads and indexing in Azure Search.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class UploadController : ControllerBase
    {

        private const string FolderName = "Proccess1";//TODO: TO DO DYNAMIC
        private const string IndexName = "index_test";//TODO: TO DO DYNAMIC

        private readonly string _connectionString;
        private readonly string _containerName;
        private readonly string _searchServiceName;
        private readonly string _searchServiceApiKey;


        /// <summary>
        /// Initializes a new instance of the <see cref="UploadController"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration, injected by the ASP.NET runtime.</param>

        public UploadController(IConfiguration configuration)
        {
            _connectionString = configuration.GetSection("AzureStorage:ConnectionString").Value;
            _containerName = configuration.GetSection("AzureStorage:ContainerName").Value;
            _searchServiceName = configuration.GetSection("AzureSearch:ServiceName").Value;
            _searchServiceApiKey = configuration.GetSection("AzureSearch:ApiKey").Value;

        }

        /// <summary>
        /// Uploads PDFs, stores them in Azure Blob Storage, and indexes them in Azure Search.
        /// </summary>
        /// <param name="pdfs">The PDFs to upload.</param>
        /// <returns>An IActionResult that represents the result of the action.</returns>

        [HttpPost("uploadpdfs")]
        public async Task<IActionResult> UploadPdfs([FromForm] List<IFormFile> pdfs)
        {
            var storageAccount = CloudStorageAccount.Parse(_connectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(_containerName);

            // Create the container if it doesn't exist.
            await container.CreateIfNotExistsAsync();

            //add inside blob
            foreach (var pdf in pdfs)
            {
                // Put the PDFs inside a folder.
                var blob = container.GetBlockBlobReference($"{FolderName}/{pdf.FileName}");
                using (var stream = pdf.OpenReadStream())
                {
                    await blob.UploadFromStreamAsync(stream);
                }
            }



            var searchServiceClient = new SearchServiceClient(_searchServiceName, new SearchCredentials(_searchServiceApiKey));

            //Create a index in Azure Search
            await searchServiceClient.Indexes.CreateOrUpdateAsync(new Microsoft.Azure.Search.Models.Index()
            {
                Name = IndexName,
                Fields = new[]
                {
                    new Field("content", DataType.String) { IsSearchable = true, IsFilterable = true, IsSortable = true, IsFacetable = true, Analyzer = AnalyzerName.StandardLucene },
                    new Field("metadata_storage_path", DataType.String) { IsKey = true },
                    new Field("metadata_storage_name", DataType.String)
                },
            });

            // Create a Indexers in Azure Search
            searchServiceClient.Indexers.CreateOrUpdate(new Indexer()
            {
                Name = "azureblob-indexer-pdf-test",
                DataSourceName = "pdfs-source",
                TargetIndexName = IndexName,

                FieldMappings = new[]
                {
                    new FieldMapping("metadata_storage_path") { SourceFieldName = "metadata_storage_path" },
                    new FieldMapping("metadata_storage_name") { SourceFieldName = "metadata_storage_name" },
                },
                Parameters = new IndexingParameters()
                {

                    Configuration = new Dictionary<string, object>()
                    {
                        ["dataToExtract"] = "contentAndMetadata",
                        ["parsingMode"] = "default",
                    }
                },

            });



            return Ok();
        }
    }
}
