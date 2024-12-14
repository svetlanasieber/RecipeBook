using Newtonsoft.Json.Linq;
using RestSharp;
using System.Net;

namespace ApiTests
{
    [TestFixture]
    public class CategoryTests : IDisposable
    {
        private RestClient client;
        private string token;
        private Random random;

        [SetUp]
        public void Setup()
        {
            client = new RestClient(GlobalConstants.BaseUrl);
            token = GlobalConstants.AuthenticateUser("john.doe@example.com", "password123");

            Assert.That(token, Is.Not.Null.Or.Empty, "Authentication token should not be null or empty");

            random = new Random();
        }

        [Test]
        public void Test_CategoryLifecycle()
        {
            // Step 1: Create a new category
            var categoryName = $"Vegan Recipes {random.Next(999, 9999)}";
            var createRequest = new RestRequest("category", Method.Post);
            createRequest.AddHeader("Authorization", $"Bearer {token}");
            createRequest.AddJsonBody(new
            {
                name = categoryName,
            });

            var createResponse = client.Execute(createRequest);
            Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code OK (200)");

            var createdCategory = JObject.Parse(createResponse.Content);
            string categoryId = createdCategory["_id"]?.ToString();
            Assert.That(categoryId, Is.Not.Null.And.Not.Empty, "Category ID should not be null or empty");

            // Step 2: Get all categories
            var getAllRequest = new RestRequest("category", Method.Get);
            var getAllResponse = client.Execute(getAllRequest);

            Assert.Multiple(() =>
            {
                Assert.That(getAllResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code OK (200)");
                Assert.That(getAllResponse.Content, Is.Not.Empty, "Response content should not be empty");

                var categories = JArray.Parse(getAllResponse.Content);
                Assert.That(categories.Type, Is.EqualTo(JTokenType.Array), "Expected response content to be a JSON array");
                Assert.That(categories.Count, Is.GreaterThan(0), "Expected at least one category in the response");
            });

            // Step 3: Get category by ID
            var getByIdRequest = new RestRequest($"category/{categoryId}", Method.Get);
            var getByIdResponse = client.Execute(getByIdRequest);

            Assert.Multiple(() =>
            {
                Assert.That(getByIdResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code OK (200)");
                Assert.That(getByIdResponse.Content, Is.Not.Empty, "Response content should not be empty");

                var category = JObject.Parse(getByIdResponse.Content);
                Assert.That(category["_id"]?.ToString(), Is.EqualTo(categoryId), "Expected the category ID to match");
                Assert.That(category["name"]?.ToString(), Is.EqualTo(categoryName), "Expected the category name to match");
            });

            // Step 4: Edit the category
            var editRequest = new RestRequest($"category/{categoryId}", Method.Put);
            var udpatedCategoryName = categoryName + "_updated";
            editRequest.AddHeader("Authorization", $"Bearer {token}");
            editRequest.AddJsonBody(new
            {
                name = udpatedCategoryName
            });

            var editResponse = client.Execute(editRequest);
            Assert.That(editResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code OK (200)");

            // Step 5: Verify the category is updated
            var getUpdatedCategoryRequest = new RestRequest($"category/{categoryId}", Method.Get);
            var getUpdatedCategoryResponse = client.Execute(getUpdatedCategoryRequest);

            Assert.Multiple(() =>
            {
                Assert.That(getUpdatedCategoryResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code OK (200)");
                Assert.That(getUpdatedCategoryResponse.Content, Is.Not.Empty, "Response content should not be empty");

                var updatedCategory = JObject.Parse(getUpdatedCategoryResponse.Content);
                Assert.That(updatedCategory["name"]?.ToString(), Is.EqualTo(udpatedCategoryName), "Expected the updated category name to match");
            });

            // Step 6: Delete the category
            var deleteRequest = new RestRequest($"category/{categoryId}", Method.Delete);
            deleteRequest.AddHeader("Authorization", $"Bearer {token}");

            var deleteResponse = client.Execute(deleteRequest);
            Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code OK (200)");

            // Step 7: Verify that the deleted category cannot be found
            var getDeletedCategoryRequest = new RestRequest($"category/{categoryId}", Method.Get);
            var getDeletedCategoryResponse = client.Execute(getDeletedCategoryRequest);

            Assert.That(getDeletedCategoryResponse.Content, Is.Empty.Or.EqualTo("null"), "Deleted category should not be found");
        }

        public void Dispose()
        {
            client?.Dispose();
        }
    }
}
