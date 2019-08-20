using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using insurance.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace insurance.Helpers
{
    public class StorageHelper
    {
        private CloudStorageAccount storageAccount;
        private CloudBlobClient blobClient;
        private CloudTableClient tableClient;
        private CloudQueueClient queueClient;

        public string ConnectionString
        {
            set {
                this.storageAccount = CloudStorageAccount.Parse(value);
                this.blobClient = storageAccount.CreateCloudBlobClient();
                this.tableClient = storageAccount.CreateCloudTableClient();
                this.queueClient = storageAccount.CreateCloudQueueClient();
            }
        }

        public async Task<string> uploadCustImgAsync(string containerName, string imagePath) {
            var container = blobClient.GetContainerReference(containerName);

            await container.CreateIfNotExistsAsync();

            var imgName = Path.GetFileName(imagePath);

            var blob = container.GetBlockBlobReference(imgName);

            await blob.DeleteIfExistsAsync();
            await blob.UploadFromFileAsync(imagePath);

            return blob.Uri.AbsoluteUri;
        }

        public async Task<Customer> InsertCustAsync(string tblName, Customer cust)
        {
            var tbl = tableClient.GetTableReference(tblName);

            await tbl.CreateIfNotExistsAsync();

            TableOperation insertOpr = TableOperation.Insert(cust);
            var res = await tbl.ExecuteAsync(insertOpr);

            return res.Result as Customer;
        }

        public async Task<string> AddMsgAsync(string queueName,Customer cust)
        {
            var que = queueClient.GetQueueReference(queueName);

            await que.CreateIfNotExistsAsync();

            var msgBdy = JsonConvert.SerializeObject(cust);
            CloudQueueMessage msg = new CloudQueueMessage(msgBdy);

            await que.AddMessageAsync(msg,TimeSpan.FromDays(3),TimeSpan.Zero,null,null);

            return que.Uri.AbsoluteUri;
        }
    }
}
