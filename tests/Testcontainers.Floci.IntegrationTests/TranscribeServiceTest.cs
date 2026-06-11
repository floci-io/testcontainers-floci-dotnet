using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.TranscribeService;
using Amazon.TranscribeService.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class TranscribeServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci)
        .WithTranscribe(new TranscribeConfig())
        .Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonTranscribeServiceClient CreateClient()
    {
        return new AmazonTranscribeServiceClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonTranscribeServiceConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task CreatesAndListsVocabulary()
    {
        using var transcribe = CreateClient();
        const string vocabularyName = "test-vocab";

        await transcribe.CreateVocabularyAsync(new CreateVocabularyRequest
        {
            VocabularyName = vocabularyName,
            LanguageCode = LanguageCode.EnUS,
            Phrases = new List<string> { "hello", "world" },
        });

        var response = await transcribe.ListVocabulariesAsync(new ListVocabulariesRequest());

        Assert.Contains(response.Vocabularies, v => v.VocabularyName == vocabularyName);
    }
}
