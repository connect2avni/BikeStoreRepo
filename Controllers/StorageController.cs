using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using WebApiToBlobStorage.Models;

namespace WebApiToBlobStorage.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StorageController : Controller
    {
        // POST: Storage/Create

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orders"></param>
        /// <returns>The result is in string format </returns>
        [HttpPost]
        public string Create(Order orders)
        {

            try
            {
                string attendJsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(orders);
                
                //this we got from azure..
                string conStr = "DefaultEndpointsProtocol=https;AccountName=bikestoreproject;AccountKey=YRYPSE7M7gi+5aoBg1qM8G2nTOwLQqH0CDX0riKDw2wT1oxZfBqcjf9oiB9eX5j4eek2q8C7uU1f+AStMJayLg==;EndpointSuffix=core.windows.net";

                try
                {
                    UploadBlob(conStr, attendJsonStr, "bikestorecontainer", true);
                    ViewBag.MessageToScreent = "Details Updated to Blob :" + attendJsonStr;
                }
                catch (Exception ex)
                {
                    return "Failed to update blob " + ex.Message;
                }

                return "uploaded to blob";
            }
            catch
            {
                return "Created";
            }
        }

        private static string UploadBlob(string conStr, string fileContent, string containerName, bool isAppend = false)
        {
            //[{w.v.f.newdata,newdata}]

            ///
            string result = "Success";
            try
            {
                //string containerName = "sample1";
                string fileName, existingContent;
                BlobClient blobClient;
                SetVariables(conStr, containerName, out fileName, out existingContent, out blobClient);

                if (isAppend)
                {
                    string fillerStart = "";
                    string fillerEnd = "]";
                    existingContent = GetContentFromBlob(conStr, fileName, containerName);

                    if (string.IsNullOrEmpty(existingContent.Trim()))
                    {
                        fillerStart = "[";
                        fileContent = fillerStart + existingContent + fileContent + fillerEnd;

                    }
                    else
                    {
                        existingContent = existingContent.Substring(0, existingContent.Length - 3);
                        fileContent = fillerStart + existingContent + "," + fileContent + fillerEnd;
                    }
                }

                var ms = new MemoryStream();
                TextWriter tw = new StreamWriter(ms);
                tw.Write(fileContent);
                tw.Flush();
                ms.Position = 0;

                try
                {
                    blobClient.UploadAsync(ms, true);

                }
                catch (Exception ex)
                {

                    throw ex;
                }


            }
            catch (Exception ex)
            {

                result = "Failed";
                throw ex;
            }
            return result;
        }

        private static void SetVariables(string conStr, string containerName, out string fileName, out string existingContent, out BlobClient blobClient)
        {
            var serviceClient = new BlobServiceClient(conStr);
            var containerClient = serviceClient.GetBlobContainerClient(containerName);

            fileName = "data.txt";
            existingContent = "";
            blobClient = containerClient.GetBlobClient(fileName);
        }

        private static string GetContentFromBlob(string conStr, string fileName, string containerName)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(conStr);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            BlobClient blobClient = containerClient.GetBlobClient(fileName);
            string line = string.Empty;
            if (blobClient.Exists())
            {
                var response = blobClient.Download();
                using (var streamReader = new StreamReader(response.Value.Content))
                {
                    while (!streamReader.EndOfStream)
                    {
                        line += streamReader.ReadLine() + Environment.NewLine;
                    }
                }
            }
            return line;
        }
        // GET: Attendanc/Edit/5

    }
}

