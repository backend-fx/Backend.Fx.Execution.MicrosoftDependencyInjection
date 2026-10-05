using System;
using System.Threading.Tasks;
using Backend.Fx.Execution.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Execution.MicrosoftDependencyInjection;

/// <summary>
/// This is a shim to use an existing <see cref="IServiceProvider"/> instance as <see cref="ICompositionRoot"/>
/// </summary>
[PublicAPI]
public class SharedMicrosoftCompositionRoot : MicrosoftCompositionRootBase
{
    private IServiceProvider _serviceProvider;

    public SharedMicrosoftCompositionRoot(IServiceCollection serviceCollection)
        : base(serviceCollection) { }

    public override IServiceProvider ServiceProvider =>
        _serviceProvider
        ?? throw new InvalidOperationException(
            "ServiceProvider not in use. Call UseServiceProvider(app.ServiceProvider) in your entry point"
        );

    public override void Verify()
    {
        // out of our control
    }

    public void UseServiceProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override void Dispose(bool disposing)
    {
        // don't dispose the service provider, we don't own it
    }

    protected override ValueTask DisposeAsyncCore()
    {
        // don't dispose the service provider, we don't own it
        return new ValueTask(Task.CompletedTask);
    }
}
