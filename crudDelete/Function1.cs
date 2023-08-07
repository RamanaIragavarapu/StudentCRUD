using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;
using System.Reflection.Metadata;
using Microsoft.Azure.Documents.Client;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using System.Xml.Linq;

namespace CrudStudent
{
    public class MyStudent
    {
        private const string DatabaseName = "Student";
        private const string CollectionName = "Info";
        private readonly CosmosClient _cosmosClient;
        private Microsoft.Azure.Cosmos.Container documentContainer;
        public MyStudent(CosmosClient cosmosClient)
        {
            _cosmosClient = cosmosClient;
            documentContainer = _cosmosClient.GetContainer("Student", "Info");
        }
        [FunctionName("GetStudent")]
        public async Task<IActionResult> GetStudent(
       [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetStudent")] HttpRequest req,
       [CosmosDB(
            DatabaseName,
                CollectionName,
                Connection ="CosmosDBConnectionStringSetting",
            SqlQuery ="SELECT * FROM c")]
               IEnumerable<student> stu,
            ILogger log)
        {
            log.LogInformation("Getting all student details");
            string mymessage = "Retrived student Details successfully";
            dynamic myData = new ExpandoObject();
            myData.message = mymessage;
            myData.Data = stu;
            var json = JsonConvert.SerializeObject(myData);
            return new OkObjectResult(json);
        }
        [FunctionName("GettingStudentById")]
        public async Task<IActionResult> GettingStudentById(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetStudentById/{id}")] HttpRequest req,
      [CosmosDB(
            DatabaseName,
                CollectionName,
                Connection ="CosmosDBConnectionStringSetting",
            SqlQuery ="SELECT * FROM c")]
               IEnumerable<student> stu,
           ILogger log,string Id)
        {
            log.LogInformation("Getting student details by Id");
            var item = stu.Where(p => p.Id == Id).FirstOrDefault();
            if(item==null)
            {
                return new NotFoundResult();
            }
            string mymessage = "Got Student Details By Id successfully";
            dynamic myData = new ExpandoObject();
            myData.message = mymessage;
            myData.Data = item;
            var json = JsonConvert.SerializeObject(myData);
            return new OkObjectResult(json);
        }


        [FunctionName("CreateStudent")]
        public static async Task<IActionResult> CreateStudent(
 [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "CreateStudent")] HttpRequest req,
        [CosmosDB(
            DatabaseName,
            CollectionName,
            Connection ="CosmosDBConnectionStringSetting")] IAsyncCollector<object> cmd, ILogger log)
        {
            log.LogInformation("Creating the details of the employee.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<Createstudent>(requestBody);

            var item = new student()
            {
                Id = input.Id,

                Name = input.Name,
                Age= input.Age,
                            };
            await cmd.AddAsync(new { id = item.Id,item.Name, item.Age });
            string mymessage = "Created student successfully";
            dynamic myData = new ExpandoObject();
            myData.message = mymessage;
            myData.Data = item;
            var json = JsonConvert.SerializeObject(myData);
            return new OkObjectResult(json);
        }
        [FunctionName("UpdatestuInfo")]
        public async Task<IActionResult> Updatestudent(
          [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "updatestudent/{id}")] HttpRequest req,
          ILogger log, string id)
        {
            log.LogInformation($"Updating student with ID: {id}");
            string requestData = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<Updatestudent>(requestData);
            var item = await documentContainer.ReadItemAsync<student>(id, new Microsoft.Azure.Cosmos.PartitionKey(id));
            if (item?.Resource == null)
            {
                return new NotFoundObjectResult("student record not found");
            }
            item.Resource.Name = data.Name;
            item.Resource.Age = data.Age;
            await documentContainer.UpsertItemAsync(item.Resource);
            string updatemessage = "Updating student data successfully";
            dynamic upmydata = new ExpandoObject();
            upmydata.message = updatemessage;
            upmydata.Data = item.Resource;
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(upmydata);
            return new OkObjectResult(json);
        }
        [FunctionName("DeleteStudentInfo")]
        public async Task<IActionResult> DeleteStudentInfo(
    [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "delstudent/{id}")] HttpRequest req,
       [CosmosDB(
            DatabaseName,
                CollectionName,
                Connection ="CosmosDBConnectionStringSetting")]
               System.Collections.Generic.IEnumerable<student> stu,
            ILogger log, string id)
        {
            log.LogInformation($"Deleting employee with ID: {id}");
            await documentContainer.DeleteItemAsync<student>(id, new Microsoft.Azure.Cosmos.PartitionKey(id));
            string responseMessage = "Deleted employee record sucessfully";
            return new OkObjectResult(responseMessage);
        }
    }
}
