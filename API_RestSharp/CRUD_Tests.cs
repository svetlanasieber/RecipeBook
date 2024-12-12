using Newtonsoft.Json.Linq;
using RecipeBook;
using RestSharp;

namespace RecipeBook_CRUD
{
    [TestFixture]
    public class CRUD_Tests
    {
        private RestClient client;
        private string token;
        private int randomNum;
        private string firstCategory;
        private string myRecipeID;
        private string myTitle;

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


        [Test, Order(1)]
        public void Test_GetAllRecipes()
        {
            var getRequest = new RestRequest("/recipe", Method.Get);
            var getResponse = client.Execute(getRequest);

            Assert.IsTrue(getResponse.IsSuccessful);
            Assert.That(getResponse.Content, Is.Not.Empty);

            var typeOfResponse = JArray.Parse(getResponse.Content).Type;
            Assert.That(typeOfResponse, Is.EqualTo(JTokenType.Array));

            var recipes = JArray.Parse(getResponse.Content);
            Assert.That(recipes.Count, Is.GreaterThanOrEqualTo(1));

            foreach (var r in recipes)
            {
                Assert.That(r["title"].ToString(), Is.Not.Null.Or.Empty);
                Assert.That(r["ingredients"].ToString(), Is.Not.Null.Or.Empty);
                Assert.That(r["instructions"].ToString(), Is.Not.Null.Or.Empty);
                Assert.That(r["cookingTime"].ToString(), Is.Not.Null.Or.Empty);
                Assert.That(r["category"].ToString(), Is.Not.Null.Or.Empty);
                Assert.That(r["servings"].ToString(), Is.Not.Null.Or.Empty);

                Assert.That(r["ingredients"].Type, Is.EqualTo(JTokenType.Array));
                Assert.That(r["instructions"].Type, Is.EqualTo(JTokenType.Array));
            }

            firstCategory = recipes[0]["category"]["_id"].ToString();
        }

        [Test, Order(2)]
        public void Test_GetRecipeByTitle()
        {
            var getRequest = new RestRequest("/recipe", Method.Get);
            var getResponse = client.Execute(getRequest);

            var recipes = JArray.Parse(getResponse.Content);

            var wantedRecipe = recipes.FirstOrDefault(r => r["title"].ToString() == "Chocolate Chip Cookies");
            Assert.That(wantedRecipe, Is.Not.Null);

            Assert.Multiple(() =>
            {
                Assert.That(wantedRecipe["description"].ToString(), Is.EqualTo("Crispy on the outside, chewy on the inside, these cookies are a timeless classic."));
                Assert.That(int.Parse(wantedRecipe["cookingTime"].ToString()), Is.EqualTo(25));
                Assert.That(int.Parse(wantedRecipe["servings"].ToString()), Is.EqualTo(24));
                Assert.That(wantedRecipe["ingredients"].Count, Is.EqualTo(9));
                Assert.That(wantedRecipe["instructions"].Count, Is.EqualTo(7));
            });
        }

        [Test, Order(3)]
        public void Test_AddRecipe()
        {
            var myTitle = $"tikvenik_{randomNum}";
            var postRequest = new RestRequest("/recipe", Method.Post)
                .AddHeader("Authorization", $"Bearer {token}")
                .AddJsonBody(new
                {
                    title = myTitle,
                    description = "mega dobriq",
                    ingredients = new[]
                    {
                         new { name = "kori", quantity = "20" },
                    },
                    instructions = new[]
                    {
                        new { step = "call ur grandma"}
                    },
                    cookingTime = 20,
                    servings = 2,
                    category = firstCategory
                });



            var postResponse = client.Execute(postRequest);
            Assert.True(postResponse.IsSuccessful);
            Assert.That(postResponse.Content, Is.Not.Empty);

            var jsonResponse = JObject.Parse(postResponse.Content);
            Assert.That(jsonResponse["_id"].ToString(), Is.Not.Null.Or.Empty);

            Assert.Multiple(() =>
            {

                Assert.That(jsonResponse["title"].ToString(), Is.EqualTo(myTitle));
                Assert.That(jsonResponse["description"].ToString(), Is.EqualTo("mega dobriq"));

                Assert.That(jsonResponse["ingredients"][0]["name"].ToString(), Is.EqualTo("kori"));
                Assert.That(jsonResponse["ingredients"][0]["quantity"].ToString(), Is.EqualTo("20"));
                Assert.That(jsonResponse["instructions"][0]["step"].ToString(), Is.EqualTo("call ur grandma"));

                Assert.That(int.Parse(jsonResponse["cookingTime"].ToString()), Is.EqualTo(20));
                Assert.That(int.Parse(jsonResponse["servings"].ToString()), Is.EqualTo(2));

                Assert.That(jsonResponse["category"].ToString(), Is.Not.Empty);
                Assert.That(jsonResponse["category"]["_id"].ToString(), Is.EqualTo(firstCategory));

                Assert.That(jsonResponse["ingredients"].Type, Is.EqualTo(JTokenType.Array));        
                Assert.That(jsonResponse["ingredients"].Count, Is.EqualTo(1)); 
                
                Assert.That(jsonResponse["instructions"].Type, Is.EqualTo(JTokenType.Array));
                Assert.That(jsonResponse["instructions"].Count, Is.EqualTo(1));

            });


            myRecipeID = jsonResponse["_id"].ToString();
        }

        [Test,Order(4)]
        public void Test_UpdateRecipe()
        {
            var myUpdatedTitle = $"updatedTitle_{randomNum}";
            var putRequest = new RestRequest("/recipe/{id}", Method.Put)
                .AddUrlSegment("id", myRecipeID)
                .AddHeader("Authorization", $"Bearer {token}")
                .AddJsonBody(new
                {
                    title = myUpdatedTitle,
                    description = "mega dobriq2",
                    ingredients = new[]
                    {
                         new { name = "eggs", quantity = "2" },
                    },
                    instructions = new[]
                    {
                        new { step = "call ur mom"}
                    },
                    cookingTime = 30,
                    servings = 5,
                    category = firstCategory
                });

            var putResponse = client.Execute(putRequest);
            Console.WriteLine(putResponse.Content);
            Assert.True(putResponse.IsSuccessful);
            Assert.That(putResponse.Content, Is.Not.Empty);

            var jsonResponse = JObject.Parse(putResponse.Content);
            Assert.That(jsonResponse["_id"].ToString(), Is.Not.Null.Or.Empty);



            Assert.Multiple(() =>
            {

                Assert.That(jsonResponse["title"].ToString(), Is.EqualTo(myUpdatedTitle));
                Assert.That(jsonResponse["description"].ToString(), Is.EqualTo("mega dobriq2"));

                Assert.That(jsonResponse["ingredients"][0]["name"].ToString(), Is.EqualTo("eggs"));
                Assert.That(jsonResponse["ingredients"][0]["quantity"].ToString(), Is.EqualTo("2"));
                Assert.That(jsonResponse["instructions"][0]["step"].ToString(), Is.EqualTo("call ur mom"));

                Assert.That(int.Parse(jsonResponse["cookingTime"].ToString()), Is.EqualTo(30));
                Assert.That(int.Parse(jsonResponse["servings"].ToString()), Is.EqualTo(5));

                Assert.That(jsonResponse["category"].ToString(), Is.Not.Empty);
                Assert.That(jsonResponse["category"]["_id"].ToString(), Is.EqualTo(firstCategory));

                Assert.That(jsonResponse["ingredients"].Type, Is.EqualTo(JTokenType.Array));
                Assert.That(jsonResponse["ingredients"].Count, Is.EqualTo(1));

                Assert.That(jsonResponse["instructions"].Type, Is.EqualTo(JTokenType.Array));
                Assert.That(jsonResponse["instructions"].Count, Is.EqualTo(1));

            });

            myTitle = myUpdatedTitle;
        }

        [Test, Order(5)]
        public void Test_DeleteRecipe()
        {
            var deleteReq = new RestRequest("/recipe/{id}", Method.Delete)
                .AddUrlSegment("id", myRecipeID)
                .AddHeader("Authorization", $"Bearer {token}");

            var deleteResp = client.Execute(deleteReq);
            Assert.True(deleteResp.IsSuccessful);

            var jsonResponse = JObject.Parse(deleteResp.Content);
            Assert.That(jsonResponse["title"].ToString(), Is.EqualTo(myTitle));
        }

        [Test, Order(6)]
        public void Test_VerifyRecipeDeletion()
        {
            var getReq = new RestRequest("/recipe/{id}", Method.Get)
                .AddUrlSegment("id", myRecipeID);

            var getResponse = client.Execute(getReq);

            Assert.Multiple(() =>
            {
                Assert.That(getResponse.IsSuccessful, Is.True, $"response is not successful and failed with content: {getResponse.Content}");
                Assert.That(getResponse.Content, Is.EqualTo("null"), "The response content should be 'null'");
            });
        }
    }
}
