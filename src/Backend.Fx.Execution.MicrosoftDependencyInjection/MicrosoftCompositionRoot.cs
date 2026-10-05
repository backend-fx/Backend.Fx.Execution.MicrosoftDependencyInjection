using System;
using System.Threading.Tasks;
using Backend.Fx.Execution.DependencyInjection;
using Backend.Fx.Logging;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Execution.MicrosoftDependencyInjection;

/// <summary>
/// Exclusively owns a Microsoft ServiceProvider and uses it as <see cref="ICompositionRoot"/>.
/// </summary>
[PublicAPI]
public class MicrosoftCompositionRoot : MicrosoftCompositionRootBase
{
    private readonly ILogger _logger = Log.Create<MicrosoftCompositionRoot>();
    private readonly Lazy<ServiceProvider> _serviceProvider;

    public MicrosoftCompositionRoot()
    {
        _serviceProvider = new Lazy<ServiceProvider>(() =>
        {
            _logger.LogInformation("Building Microsoft ServiceProvider");
            return ServiceCollection.BuildServiceProvider(
                new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true }
            );
        });
    }

    public override IServiceProvider ServiceProvider => _serviceProvider.Value;

    public override void Verify()
    {
        // ensure creation of lazy service provider, this will trigger the validation
        _ = _serviceProvider.Value;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && _serviceProvider.IsValueCreated)
        {
            _serviceProvider.Value.Dispose();
        }
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        if (_serviceProvider.IsValueCreated)
        {
            await _serviceProvider.Value.DisposeAsync().ConfigureAwait(false);
        }
    }
}
