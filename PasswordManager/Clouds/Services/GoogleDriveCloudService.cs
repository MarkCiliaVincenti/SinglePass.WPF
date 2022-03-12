﻿using PasswordManager.Authorization.Brokers;
using PasswordManager.Authorization.Interfaces;
using PasswordManager.Clouds.Interfaces;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

namespace PasswordManager.Clouds.Services
{
    public class GoogleDriveCloudService : ICloudService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IAuthorizationBroker AuthorizationBroker { get; }

        public GoogleDriveCloudService(
            GoogleAuthorizationBroker googleAuthorizationBroker,
            IHttpClientFactory httpClientFactory)
        {
            AuthorizationBroker = googleAuthorizationBroker;
            _httpClientFactory = httpClientFactory;
        }

        private async Task RefreshAccessTokenIfRequired(CancellationToken cancellationToken)
        {
            if (AuthorizationBroker.TokenHolder.Token.RefreshRequired)
            {
                await AuthorizationBroker.RefreshAccessToken(cancellationToken);
            }
        }

        public async Task Upload(Stream stream, string fileName, CancellationToken cancellationToken)
        {
            await RefreshAccessTokenIfRequired(cancellationToken);

            var accessToken = AuthorizationBroker.TokenHolder.Token.AccessToken;

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var metaContent = JsonContent.Create(new { name = fileName });
            var streamContent = new StreamContent(stream);
            var multipart = new MultipartContent { metaContent, streamContent };
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Octet);
            streamContent.Headers.ContentLength = stream.Length;
            var result = await client.PostAsync("https://www.googleapis.com/upload/drive/v3/files?uploadtype=multipart", multipart, cancellationToken);
        }
    }
}
