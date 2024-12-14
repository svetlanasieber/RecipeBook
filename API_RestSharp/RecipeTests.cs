using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Net;

namespace ApiTests
{
    [TestFixture]
    public class RecipeTests : IDisposable
    {
        private RestClient client;
        private string token;
        private Random random;
        private string recepyTitle;

        [SetUp]
        public void Setup()
        {
            client = new RestClient(GlobalConstants.BaseUrl);
            token = GlobalConstants.AuthenticateUser("john.doe@example.com", "password123");

            Assert.That(token, Is.Not.Null.Or.Empty, "Authentication token should not be null or empty");
            random = new Random();
            
        }

        [Test, Order(1)]
        public void Test_GetAllRecipes()
        {
            var request = new RestRequest("recipe", Method.Get);

            var response = client.Execute(request);

            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code OK (200)");
                Assert.That(response.Content, Is.Not.Empty, "Response content should not be empty");

                var recipes = JArray.Parse(response.Content);

                Assert.That(recipes.Type, Is.EqualTo(JTokenType.Array), "Expected response content to be a JSON array");
                Assert.That(recipes.Count, Is.GreaterThan(0), "Expected at least one recipe in the response");

                foreach (var recipe in recipes)
                {
                    Assert.That(recipe["title"]?.ToString(), Is.Not.Null.And.Not.Empty, "Recipe title should not be null or empty");
                    Assert.That(recipe["cookingTime"]?.ToString(), Is.Not.Null.And.Not.Empty, "Recipe cookingTime should not be null or empty");
                    Assert.That(recipe["servings"]?.ToString(), Is.Not.Null.And.Not.Empty, "Recipe servings should not be null or empty");

                    Assert.That(recipe["category"], Is.Not.Null.And.Not.Empty, "Recipe category should not be null or empty");

                    Assert.That(recipe["ingredients"]?.Type, Is.EqualTo(JTokenType.Array), "Recipe ingreients should be a JSON Array");
                    Assert.That(recipe["instructions"]?.Type, Is.EqualTo(JTokenType.Array), "Recipe instructions should be a JSON Array");
                }
            });
        }

        [Test, Order(2)]
        public void Test_GetRecipeByTitle()
        {
            var request = new RestRequest("recipe", Method.Get);

            var response = client.Execute(request);
            var expectedCookingTime = 25;
            var expectedServings = 24;
            var expectedIngredientsCount = 9;
            var expectedInstructionsCount = 7;

            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code OK (200)");
                Assert.That(response.Content, Is.Not.Empty, "Response content should not be empty");

                var recipes = JArray.Parse(response.Content);
                var recipe = recipes.FirstOrDefault(b => b["title"]?.ToString() == "Chocolate Chip Cookies");

                Assert.That(recipe, Is.Not.Null, "Recipe with title 'Chocolate Chip Cookies' not found");

                Assert.That(recipe["cookingTime"].Value<int>(), Is.EqualTo(expectedCookingTime), "Recipe cookingTime should match the expected value");
                Assert.That(recipe["servings"].Value<int>(), Is.EqualTo(expectedServings), "Recipe cookingTime should match the expected value");

                Assert.That(recipe["ingredients"].Count(), Is.EqualTo(expectedIngredientsCount), $"Recipe ingredients did not have the expected count of {expectedIngredientsCount}");
                Assert.That(recipe["instructions"].Count(), Is.EqualTo(expectedInstructionsCount), $"Recipe instructions did not have the expected count of {expectedInstructionsCount}");
            });
        }

        [Test, Order(3)]
        public void Test_AddRecipe()
        {
            //Get category and exctract id.
            var getCategoriesRequest = new RestRequest($"category", Method.Get);
            var getCategoriesResponse = client.Execute(getCategoriesRequest);

            Assert.Multiple(() =>
            {
                Assert.That(getCategoriesResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Get Categories Expected status code OK (200)");
                Assert.That(getCategoriesResponse.Content, Is.Not.Empty, "Categories response content should not be empty");
            });

            var categories = JArray.Parse(getCategoriesResponse.Content);

            Assert.Multiple(() =>
            {
                Assert.That(categories.Type, Is.EqualTo(JTokenType.Array), "Expected response content to be a JSON array");
                Assert.That(categories.Count, Is.GreaterThan(0), "Expected at least one category in the response");
            });

            var category = categories.First();
            var categoryId = category["_id"]?.ToString();

            //Create recepi
            var request = new RestRequest("recipe", Method.Post);
            request.AddHeader("Authorization", $"Bearer {token}");

            recepyTitle = $"New Recipe Title{random.Next(999, 9999)}";
            var description = "A captivating new recipe description.";
            var cookingTime = 50;
            var servings = 4;
            var ingredients = new[] { new { name = "Test Ingredient", quantity = "10g" } };
            var instructions = new[] { new { step = "Test Step" } };
            request.AddJsonBody(new
            {
                title = recepyTitle,
                description,
                cookingTime,
                servings,
                ingredients,
                instructions,
                category = categoryId
            });

            var response = client.Execute(request);

            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code OK (200)");
                Assert.That(response.Content, Is.Not.Empty, "Response content should not be empty");
            });

            var createdRecipe = JObject.Parse(response.Content);
            Assert.That(createdRecipe["_id"]?.ToString(), Is.Not.Empty, "Created recipe didn't have an Id.");

            var createdRecipeId = createdRecipe["_id"].ToString();

            var getRecipeRequest = new RestRequest("recipe/{id}", Method.Get);
            getRecipeRequest.AddUrlSegment("id", createdRecipeId);
            var getRecipeResponse = client.Execute(getRecipeRequest);

            Assert.Multiple(() =>
            {
                Assert.That(getRecipeResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code OK (200)");
                Assert.That(getRecipeResponse.Content, Is.Not.Empty, "Response content should not be empty");

                var content = JObject.Parse(getRecipeResponse.Content);

                Assert.That(content["title"]?.ToString(), Is.EqualTo(recepyTitle), "Recipe title should match the input");
                Assert.That(content["cookingTime"]?.Value<int>(), Is.EqualTo(cookingTime), "Recipe cookingTime should match the input");
                Assert.That(content["servings"]?.Value<int>(), Is.EqualTo(servings), "Recipe servings should match the input");

                Assert.That(content["category"], Is.Not.Empty, $"Recipe category should does not exist");
                Assert.That(content["category"]?["_id"]?.ToString(), Is.EqualTo(categoryId), $"Recipe category should be '{categoryId}'");

                Assert.That(content["ingredients"]?.Type, Is.EqualTo(JTokenType.Array), "Recipe ingredients should be a JSON Array");
                Assert.That(content["ingredients"]?.Count(), Is.EqualTo(ingredients.Count()), "Recipe ingredients should have the correct number of elements");
                Assert.That(content["ingredients"]?[0]?["name"]?.ToString(), Is.EqualTo(ingredients[0].name), "Ingredient name did not match");
                Assert.That(content["ingredients"]?[0]?["quantity"]?.ToString(), Is.EqualTo(ingredients[0].quantity), "Ingredient quantity did not match");

                Assert.That(content["instructions"]?.Type, Is.EqualTo(JTokenType.Array), "Recipe instructions should be a JSON Array");
                Assert.That(content["instructions"]?.Count(), Is.EqualTo(ingredients.Count()), "Recipe instructions should have the correct number of elements");
                Assert.That(content["instructions"]?[0]?["step"]?.ToString(), Is.EqualTo(instructions[0].step), "Instruction step did not match");
            });
        }

        [Test, Order(4)]
        public void Test_UpdateRecipe()
        {
            var getRequest = new RestRequest("recipe", Method.Get);

            var getResponse = client.Execute(getRequest);

            Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Failed to retrieve recipes");
            Assert.That(getResponse.Content, Is.Not.Empty, "Get recipes response content is empty");

            var recipes = JArray.Parse(getResponse.Content);
            var recipeToUpdate = recipes.FirstOrDefault(b => b["title"]?.ToString() == recepyTitle);

            Assert.That(recipeToUpdate, Is.Not.Null, "Recipe with title 'Spaghetti Carbonara' not found");

            var recipeId = recipeToUpdate["_id"].ToString();

            var updateRequest = new RestRequest("recipe/{id}", Method.Put);
            updateRequest.AddHeader("Authorization", $"Bearer {token}");
            updateRequest.AddUrlSegment("id", recipeId);
            recepyTitle = recepyTitle + "udpated";
            var servings = 30;
            updateRequest.AddJsonBody(new
            {
                title = recepyTitle,
                servings
            });

            var updateResponse = client.Execute(updateRequest);

            Assert.Multiple(() =>
            {
                Assert.That(updateResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code OK (200)");
                Assert.That(updateResponse.Content, Is.Not.Empty, "Update response content should not be empty");

                var content = JObject.Parse(updateResponse.Content);

                Assert.That(content["title"]?.ToString(), Is.EqualTo(recepyTitle), "Recipe title should match the updated value");
                Assert.That(content["servings"]?.Value<int>(), Is.EqualTo(servings), "Recipe servings should match the updated value");
            });
        }

        [Test, Order(5)]
        public void Test_DeleteRecipe()
        {
            var getRequest = new RestRequest("recipe", Method.Get);
            var getResponse = client.Execute(getRequest);

            Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Failed to retrieve recipes");
            Assert.That(getResponse.Content, Is.Not.Empty, "Get recipes response content is empty");

            var recipes = JArray.Parse(getResponse.Content);
            var recipeToDelete = recipes.FirstOrDefault(b => b["title"]?.ToString() == recepyTitle);

            Assert.That(recipeToDelete, Is.Not.Null, "Recipe with title 'Chicken Curry' not found");

            var recipeId = recipeToDelete["_id"].ToString();

            var deleteRequest = new RestRequest("recipe/{id}", Method.Delete);
            deleteRequest.AddHeader("Authorization", $"Bearer {token}");
            deleteRequest.AddUrlSegment("id", recipeId);

            var deleteResponse = client.Execute(deleteRequest);

            Assert.Multiple(() =>
            {
                Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code OK");

                var verifyGetRequest = new RestRequest("recipe/{id}", Method.Get);
                verifyGetRequest.AddUrlSegment("id", recipeId);

                var verifyGetResponse = client.Execute(verifyGetRequest);

                Assert.That(verifyGetResponse.Content, Is.Null.Or.EqualTo("null"), "Verify get response content should be empty");
            });
        }

        public void Dispose()
        {
            client?.Dispose();
        }
    }
}
