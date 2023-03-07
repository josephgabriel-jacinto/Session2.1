using ApiAutomationSession2._1.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ApiAutomationSession2._1.Tests
{
    [TestClass]
    public class PetClientTest
    {
        private static HttpClient httpClient;

        private static readonly string BaseUrl = "https://petstore.swagger.io/v2/";

        private static readonly string PetEndpoint = "pet";
        private static string GetUrl(string endpoint) => $"{BaseUrl}{endpoint}";
        private static Uri GetURI(string endpoint) => new Uri(GetUrl(endpoint));

        private readonly List<Pet> CleanUpList = new List<Pet>();

        [TestInitialize]
        public void TestInitialize()
        {
            httpClient = new HttpClient();
        }

        [TestCleanup]
        public async Task TestCleanup()
        {
            foreach(var data in CleanUpList)
            {
                var httpResponse = await SendAsyncFunction(HttpMethod.Delete, $"{PetEndpoint}/{data.Id}");
            }
        }

       
        [TestMethod]
        public async Task PutPetMethod()
        {
            #region CREATE PET OBJECT
            List<string> photoUrls = new List<string>();
            photoUrls.Add("sample.url.1");

            List<Tag> tags = new List<Tag>();
            tags.Add(new Tag(1, "DogTag1"));
            
            Category category = new Category(1, "Dog");

            Pet newPet = new Pet()
            {
                Id = 100021,
                Category = category,
                Name = "Rocky",
                PhotoUrls = photoUrls,
                Tags = tags,
                Status = "Available"

            };

            //send post request
            await SendAsyncFunction(HttpMethod.Post,PetEndpoint,newPet);
            #endregion

            #region GET NEWLY CREATED PET OBJECT
            var getResponse = await SendAsyncFunction(HttpMethod.Get, $"{PetEndpoint}/{newPet.Id}");
            var petRetrieved = JsonConvert.DeserializeObject<Pet>(getResponse.Content.ReadAsStringAsync().Result);
            #endregion

            #region UPDATE PET OBJECT
            category.Name = "Mammal";
            photoUrls.Add("sample.url.2");

            Tag newTag = new Tag(2, "DogTag2");
            tags.Add(newTag);

            Pet updatedPetData = new Pet()
            {
                Id = petRetrieved.Id,
                Category = category,
                Name = "Ceddy",
                PhotoUrls = photoUrls,
                Tags = tags,
                Status = "Unavailable"
            };

            var putResponse = await SendAsyncFunction(HttpMethod.Put, PetEndpoint, updatedPetData);
            var putStatusCode = putResponse.StatusCode;
            #endregion

            #region GET PET OBJECT AFTER UPDATED
            var getAfterUpdateResponse = await SendAsyncFunction(HttpMethod.Get, $"{PetEndpoint}/{petRetrieved.Id}");
            var getRetrievedAfterUpdate = JsonConvert.DeserializeObject<Pet>(getAfterUpdateResponse.Content.ReadAsStringAsync().Result);
            #endregion

            //add to clean up list
            CleanUpList.Add(getRetrievedAfterUpdate);

            //assertions
            Assert.AreEqual(HttpStatusCode.OK, putStatusCode, "Status Code is not equal to 200");
            Assert.IsTrue(getRetrievedAfterUpdate.Name.Equals(updatedPetData.Name), "Name is not updated successfully");
            Assert.IsTrue(getRetrievedAfterUpdate.Category.Name.Equals(updatedPetData.Category.Name), "Category Name is not updated successfully");
            Assert.AreEqual(getRetrievedAfterUpdate.PhotoUrls.ToList().Count(), updatedPetData.PhotoUrls.ToList().Count(), "PhotoUrls are not updated successfully");
            Assert.AreEqual(getRetrievedAfterUpdate.Tags.ToList().Count(), updatedPetData.Tags.ToList().Count(), "Tags are not updated successfully");
            Assert.IsTrue(getRetrievedAfterUpdate.Status.Equals(updatedPetData.Status), "Status is not updated successfully");
        }



        ///<summary>
        /// SendAsync
        /// reusable method 
        /// </summary>
        private async Task<HttpResponseMessage> SendAsyncFunction(HttpMethod method, string url, Pet petData = null)
        {
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage();

            httpRequestMessage.Method = method;
            httpRequestMessage.RequestUri = GetURI(url);
            httpRequestMessage.Headers.Add("Accept", "application/json");

            if(petData != null)
            {
                var request = JsonConvert.SerializeObject(petData);
                httpRequestMessage.Content = new StringContent(request, Encoding.UTF8, "application/json");
            }

            var httpResponse = await httpClient.SendAsync(httpRequestMessage);

            return httpResponse;
        }




    }
}
