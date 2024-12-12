using Newtonsoft.Json.Linq;
using RecipeBook;
using RestSharp;
using System.Net;

namespace RecipeBook_CategoryManagement
{
    [TestFixture]
    public class Lifecycle
    {
        private RestClient client;
        private string token;
        private int randomNum;
        private string lastRandom;
        private string category;

        [SetUp]
        public void SetUp()
        {
            client = new RestClient(GlobalConstants.BaseUrl);
            token = GlobalConstants.AuthenticateUser("john.doe@example.com", "password123");

            var random = new Random();
            randomNum = random.Next(10, 50000);
        }

        [TearDown]
        public void TearDown()
        {
            client.Dispose();
        }

        [Test]
        public void Test_CategoryLifecycle()
        {
            //[1]: Create new Category
            var randomName = $"name_{randomNum}";
            var postReq = new RestRequest("/category", Method.Post)
                .AddHeader("Authorization", $"Bearer {token}")
                .AddJsonBody(new
                {
                    name = randomName
                });

            var postResponse = client.Execute(postReq);
            Assert.True(postResponse.IsSuccessful);

            var jsonResponse = JObject.Parse(postResponse.Content);
            Assert.That(jsonResponse, Is.Not.Empty);
            Assert.That(jsonResponse["_id"].ToString(), Is.Not.Null.Or.Empty);








            //[2]: Get all Categories
            var getReq = new RestRequest("/category", Method.Get);
            var getResponse = client.Execute(getReq);

            Assert.IsTrue(getResponse.IsSuccessful);
            Assert.That(getResponse.Content, Is.Not.Null.Or.Empty);
            Assert.That(JArray.Parse(getResponse.Content).Type, Is.EqualTo(JTokenType.Array));

            var categories = JArray.Parse(getResponse.Content);
            var myCreatedCat = categories.FirstOrDefault(c => c["name"].ToString() == randomName);

            Assert.That(myCreatedCat, Is.Not.Null);
            Assert.That(categories.Count, Is.GreaterThanOrEqualTo(1));
            var myCategoryId = myCreatedCat["_id"].ToString();
            var myCategoryName = myCreatedCat["name"].ToString();








            //[3]: Get category by ID
            var getByIDRequest = new RestRequest("/category/{id}", Method.Get)
                .AddUrlSegment("id", myCategoryId);

            var getByIDResponse = client.Execute(getByIDRequest);
            Assert.IsTrue(getByIDResponse.IsSuccessful);
            Assert.That(getByIDResponse.Content, Is.Not.Empty);

            var actualID = JObject.Parse(getByIDResponse.Content)["_id"].ToString();
            var expectedID = myCategoryId;
            Assert.AreEqual(expectedID, actualID);

            var expectedName = randomName;
            var actualName = JObject.Parse(getByIDResponse.Content)["name"].ToString();
            Assert.AreEqual(expectedName, actualName);








            //[4]: Edit the category 
            var updatedName = $"myNewName_${randomNum}";
            var putRequest = new RestRequest("/category/{id}", Method.Put)
                .AddUrlSegment("id", myCategoryId)
                .AddHeader("Authorization", $"Bearer {token}")
                .AddJsonBody(new
                {
                    name = updatedName
                });

            var putResponse = client.Execute(putRequest);
            Assert.IsTrue(putResponse.IsSuccessful);
            Assert.That(putResponse.Content, Is.Not.Empty.Or.Null);

            var jsonPutResponse = JObject.Parse(putResponse.Content);
            var actualUpdatedName = jsonPutResponse["name"].ToString();
            expectedName = updatedName;
            Assert.AreEqual(expectedName, actualUpdatedName);

            //get all categories again and assert firsly created name is not there
            getResponse = client.Execute(getReq);
            categories = JArray.Parse(getResponse.Content);
            var nullObject = categories.FirstOrDefault(c => c["name"].ToString() == randomName);
            Assert.Null(nullObject);
            var myUpdatedCategoryId = jsonPutResponse["_id"].ToString();








            //[5]: Verification after edit:
            getByIDRequest = new RestRequest("/category/{id}", Method.Get)
                .AddUrlSegment("id", myUpdatedCategoryId);

            var getByIDResponse2 = client.Execute(getByIDRequest);
            Assert.True(getByIDResponse2.IsSuccessful);
            Assert.That(getByIDResponse2.Content, Is.Not.Empty);
            Assert.That(JObject.Parse(getByIDResponse2.Content)["name"].ToString(), Is.EqualTo(expectedName));








            //[6]: Delete the category:
            var deleteRequest = new RestRequest("/category/{id}", Method.Delete)
                .AddUrlSegment("id", myUpdatedCategoryId)
                .AddHeader("Authorization", $"Bearer {token}");

            var delResponse = client.Execute(deleteRequest);
            Assert.True(delResponse.IsSuccessful);
            Assert.That(delResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));








            //[7]: Verify the deleted category cannot be found
            getByIDRequest = new RestRequest("/category/{id}", Method.Get)
                .AddUrlSegment("id", myUpdatedCategoryId);

            var getByIDResponse3 = client.Execute(getByIDRequest);
            Assert.That(getByIDResponse3.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(getByIDResponse3.Content, Is.EqualTo("null"));
        }

    }
}
