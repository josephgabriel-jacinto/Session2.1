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
                await httpClient.DeleteAsync(GetURI($"{PetEndpoint}/{data.Id}"));
            }
        }

       
        [TestMethod]
        public async Task PutPetMethod()
        {
            #region CREATE PET OBJECT
            //instantiate photoUrls and add to photoUrl list
            List<string> photoUrls = new List<string>();
            photoUrls.Add("sample.url.1");

            //instantiate tag and add to tags list
            List<Tag> tags = new List<Tag>();
            tags.Add(new Tag(1, "DogTag1"));
            
            //instantiate category
            Category category = new Category(1, "Dog");

            //instantiate Pet
            Pet newPet = new Pet()
            {
                Id = 100021,
                Category = category,
                Name = "Rocky",
                PhotoUrls = photoUrls,
                Tags = tags,
                Status = "Available"
            };

            //convert newPet to json for post request body
            var request = JsonConvert.SerializeObject(newPet);
            var postRequest = new StringContent(request, Encoding.UTF8, "application/json");

            //send post request
            await httpClient.PostAsync(GetUrl(PetEndpoint), postRequest);
            //await SendAsyncFunction(HttpMethod.Post,PetEndpoint,newPet);
            #endregion

            #region GET NEWLY CREATED PET OBJECT
            //send get request
            var getResponse = await httpClient.GetAsync(GetURI($"{PetEndpoint}/{newPet.Id}"));
            
            //set retrieved pet to petRetrived variable
            var petRetrieved = JsonConvert.DeserializeObject<Pet>(getResponse.Content.ReadAsStringAsync().Result);
            #endregion

            #region UPDATE PET OBJECT
            //instantiate Pet object for update
            //update category value
            petRetrieved.Category.Name = "Mammal";

            //add another photourls value
            petRetrieved.PhotoUrls.Add("sample.url.2");

            //update name value
            petRetrieved.Name = "Ceddy";

            //add another Tag value
            petRetrieved.Tags.Add(new Tag(2, "DogTag2"));            

            //update status value
            petRetrieved.Status = "Unavailable";

            //convert petRetrieved to json for put request body
            request = JsonConvert.SerializeObject(petRetrieved);
            var putRequest = new StringContent(request, Encoding.UTF8, "application/json");

            //send put request
            var putResponse = await httpClient.PutAsync(GetUrl(PetEndpoint), putRequest);
            var putStatusCode = putResponse.StatusCode;
            #endregion

            #region GET PET OBJECT AFTER UPDATED
            //send get request after update request
            var afterUpdateResponse = await httpClient.GetAsync(GetURI($"{PetEndpoint}/{petRetrieved.Id}"));
            var latestUpdatedPet = JsonConvert.DeserializeObject<Pet>(afterUpdateResponse.Content.ReadAsStringAsync().Result);
            #endregion

            //add to clean up list
            CleanUpList.Add(latestUpdatedPet);

            #region ASSERTIONS
            Assert.AreEqual(HttpStatusCode.OK, putStatusCode, "Status Code is not equal to 200");
            Assert.IsTrue(petRetrieved.Name.Equals(latestUpdatedPet.Name), "Name is not updated successfully");
            Assert.IsTrue(petRetrieved.Category.Name.Equals(latestUpdatedPet.Category.Name), "Category Name is not updated successfully");
            Assert.AreEqual(petRetrieved.PhotoUrls.ToList().Count(), latestUpdatedPet.PhotoUrls.ToList().Count(), "PhotoUrls are not updated successfully");
            Assert.AreEqual(petRetrieved.Tags.ToList().Count(), latestUpdatedPet.Tags.ToList().Count(), "Tags are not updated successfully");
            Assert.IsTrue(petRetrieved.Status.Equals(latestUpdatedPet.Status), "Status is not updated successfully");

            for (int ctr = 0; ctr < petRetrieved.PhotoUrls.Count; ctr++)
                Assert.IsTrue(petRetrieved.PhotoUrls[ctr].Equals(latestUpdatedPet.PhotoUrls[ctr]), "PhotoUrl value is not updated successfully");

            for (int ctr = 0; ctr < petRetrieved.Tags.Count; ctr++)
                Assert.IsTrue(petRetrieved.Tags[ctr].Name.Equals(latestUpdatedPet.Tags[ctr].Name), "Tag name is not updated successfully");
            #endregion
        }
    }
}
