using System.Threading.Tasks;
using Amazon.Backup;
using Amazon.Backup.Model;
using Testcontainers.Floci;
using Xunit;

namespace Testcontainers.Floci.Tests;

public sealed class BackupServiceTest : IAsyncLifetime
{
    private readonly FlociContainer _floci = new FlociBuilder(TestImages.Floci).Build();

    public Task InitializeAsync() => _floci.StartAsync();

    public Task DisposeAsync() => _floci.DisposeAsync().AsTask();

    private AmazonBackupClient CreateClient()
    {
        return new AmazonBackupClient(
            _floci.AccessKey,
            _floci.SecretKey,
            new AmazonBackupConfig
            {
                ServiceURL = _floci.GetEndpoint(),
                AuthenticationRegion = _floci.Region,
            });
    }

    [Fact]
    public async Task CreatesAndListsBackupVault()
    {
        using var backup = CreateClient();
        const string vaultName = "test-vault";

        await backup.CreateBackupVaultAsync(new CreateBackupVaultRequest { BackupVaultName = vaultName });

        var vaults = await backup.ListBackupVaultsAsync(new ListBackupVaultsRequest());

        Assert.Contains(vaults.BackupVaultList, v => v.BackupVaultName == vaultName);
    }
}
