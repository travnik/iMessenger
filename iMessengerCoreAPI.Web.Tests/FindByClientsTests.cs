using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using iMessengerCoreAPI.Web.Core;
using iMessengerCoreAPI.Web.Tests.Common;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace iMessengerCoreAPI.Web.Tests
{
    public class FindByClientsTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _applicationFactory;
        private readonly HttpClient _client;
        private readonly IDialogsRepository _dialogsRepository;

        public FindByClientsTests(WebApplicationFactory<Startup> applicationFactory)
        {
            _applicationFactory = applicationFactory;
            _client = _applicationFactory.CreateClient();
            _dialogsRepository = GetService<IDialogsRepository>();
        }

        [Fact]
        public async Task FindByClients_WhenOneClient()
        {
            // Arrange
            var dialog = _dialogsRepository
                .Get()
                .First();

            // Act
            var factDialogId = await _client.FindByClientsAsync($"?clientIds={dialog.IDClient}");

            // Assert
            Assert.Equal(dialog.IDRGDialog, factDialogId);
        }

        [Fact]
        public async Task FindByClients_WhenDialogNotFound()
        {
            // Act
            var factDialogId = await _client.FindByClientsAsync($"?clientIds={Guid.NewGuid()}");

            // Assert
            Assert.Equal(Guid.Empty, factDialogId);
        }

        [Fact]
        public async Task FindByClients_WhenSeveralClients()
        {
            // Arrange
            var dialogClients = _dialogsRepository
                .Get()
                .GroupBy(o => o.IDRGDialog)
                .First();

            var query = dialogClients
                .Select(o => $"clientIds={o.IDClient}")
                .ToList();

            // Act
            var factDialogId = await _client.FindByClientsAsync($"?{string.Join("&", query)}");

            // Assert
            Assert.Equal(dialogClients.Key, factDialogId);
        }

        private T GetService<T>()
        {
            return _applicationFactory.Services.GetRequiredService<T>();
        }
    }
}
