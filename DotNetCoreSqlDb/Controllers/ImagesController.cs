using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotNetCoreSqlDb.Controllers
{
    public class ImagesController : Controller
    {
        const string blobContainerName = "images";
        static BlobContainerClient blobContainer;
        public IConfiguration Configuration { get; }
        public ImagesController(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public async Task<ActionResult> Index()
        {
            try
            {                
                BlobServiceClient blobServiceClient = new(Configuration["STORAGE-CONNECTION-STRING"]);

                blobContainer = blobServiceClient.GetBlobContainerClient(blobContainerName);
                await blobContainer.CreateIfNotExistsAsync(PublicAccessType.Blob);

                List<string> allBlobs = new();
                foreach (BlobItem blob in blobContainer.GetBlobs())
                {
                    if (blob.Properties.BlobType == BlobType.Block)
                        allBlobs.Add(blob.Name);
                }

                return View(allBlobs);
            }
            catch (Exception ex)
            {
                ViewData["message"] = ex.Message;
                ViewData["trace"] = ex.StackTrace;
                return View("Error");
            }
        }

    }
}
