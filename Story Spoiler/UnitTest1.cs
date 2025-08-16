using System.Net;
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using Story_Spoiler.Models;

namespace StorySpoiler
{
    [TestFixture]
    public class StorySpoilerTests
    {
        private RestClient client;
        private static string createdStoryId;

        private const string JwtToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiJlNTU5N2QxMi1kMGJiLTQyNTEtOGU0Ni0yODZmMTE3NGY2NGUiLCJpYXQiOiIwOC8xNi8yMDI1IDA3OjI0OjE2IiwiVXNlcklkIjoiMmZiZGM2YWEtMTllZC00ZDBjLThlMzQtMDhkZGRiMWExM2YzIiwiRW1haWwiOiJOYWR5YTFAZ21haWwuY29tIiwiVXNlck5hbWUiOiJOYWR5YTEiLCJleHAiOjE3NTUzNTA2NTYsImlzcyI6IlN0b3J5U3BvaWxfQXBwX1NvZnRVbmkiLCJhdWQiOiJTdG9yeVNwb2lsX1dlYkFQSV9Tb2Z0VW5pIn0.QhX2ANBkTDmeBgFxx8FAMV31kqr8iqaIOOeCkhG8vrQ";

        private const string BaseUrl = "https://d3s5nxhwblsjbi.cloudfront.net";

        [OneTimeSetUp]
        public void Setup()
        {
            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(JwtToken)
            };

            client = new RestClient(options);
        }

        [Test, Order(1)]
        public void CreateStory_ShouldReturnCreated()
        {
            var request = new RestRequest("/api/Story/Create", Method.Post);

            var story = new StoryDTO
            {
                Title = "Test Story Title",
                Description = "This is a spoiler test description.",
                Url = "https://example.com/img.jpg"
            };

            request.AddJsonBody(story);
            var response = client.Execute<ApiResponseDTO>(request);

            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
                Assert.That(response.Data, Is.Not.Null);
                Assert.That(response.Data.StoryId, Is.Not.Null.And.Not.Empty);
                Assert.That(response.Data.Msg, Does.Contain("Successfully created"));
            });

            createdStoryId = response.Data.StoryId;
        }

        [Test, Order(2)]
        public void EditStory_ShouldReturnOk()
        {
            var request = new RestRequest($"/api/Story/Edit/{createdStoryId}", Method.Put);

            var updatedStory = new StoryDTO
            {
                Title = "Updated Story Title",
                Description = "Updated description of the spoiler.",
                Url = "https://example.com/updated.jpg"
            };

            request.AddJsonBody(updatedStory);
            var response = client.Execute<ApiResponseDTO>(request);

            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(response.Data, Is.Not.Null);
                Assert.That(response.Data.Msg, Does.Contain("Successfully edited"));
            });
        }

        [Test, Order(3)]
        public void GetAllStories_ShouldReturnList()
        {
            var request = new RestRequest("/api/Story/All", Method.Get);
            var response = client.Execute(request);

            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(response.Content, Is.Not.Null.And.Contains("title").IgnoreCase);
            });
        }

        [Test, Order(4)]
        public void DeleteStory_ShouldReturnOk()
        {
            var request = new RestRequest($"/api/Story/Delete/{createdStoryId}", Method.Delete);
            var response = client.Execute<ApiResponseDTO>(request);

            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(response.Data, Is.Not.Null);
                Assert.That(response.Data.Msg, Is.EqualTo("Deleted successfully!"));
            });
        }

        [Test, Order(5)]
        public void CreateStory_WithoutRequiredFields_ShouldReturnBadRequest()
        {
            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(new { url = "https://example.com/missing-fields.jpg" });

            var response = client.Execute<ApiResponseDTO>(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test, Order(6)]
        public void EditNonExistingStory_ShouldReturnNotFound()
        {
            var request = new RestRequest("/api/Story/Edit/invalid-id-123", Method.Put);

            var story = new StoryDTO
            {
                Title = "Fake Title",
                Description = "Fake Desc",
                Url = null
            };

            request.AddJsonBody(story);
            var response = client.Execute<ApiResponseDTO>(request);

            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
                Assert.That(response.Data, Is.Not.Null);
                Assert.That(response.Data.Msg, Does.Contain("No spoilers"));
            });
        }

        [Test, Order(7)]
        public void DeleteNonExistingStory_ShouldReturnBadRequest()
        {
            var request = new RestRequest("/api/Story/Delete/invalid-id-456", Method.Delete);
            var response = client.Execute<ApiResponseDTO>(request);

            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
                Assert.That(response.Data, Is.Not.Null);
                Assert.That(response.Data.Msg, Does.Contain("Unable to delete this story spoiler"));
            });
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            client?.Dispose();
        }
    }
}
