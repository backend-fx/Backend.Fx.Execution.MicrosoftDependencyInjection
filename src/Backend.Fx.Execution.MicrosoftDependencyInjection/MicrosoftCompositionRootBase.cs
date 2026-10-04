using System;
using System.Collections.Generic;
using System.Linq;
using Backend.Fx.Execution.DependencyInjection;
using Backend.Fx.Logging;
using Backend.Fx.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Execution.MicrosoftDependencyInjection;

public abstract class MicrosoftCompositionRootBase : CompositionRoot
{
    private readonly ILogger _logger = Log.Create<MicrosoftCompositionRootBase>();

    protected MicrosoftCompositionRootBase(IServiceCollection serviceCollection = null)
    {
        ServiceCollection = serviceCollection ?? new ServiceCollection();
    }

    protected IServiceCollection ServiceCollection { get; }

    public override void Register(ServiceDescriptor serviceDescriptor)
    {
        var existingRegistrations = ServiceCollection
            .Where(sd => sd.ServiceType == serviceDescriptor.ServiceType)
            .ToArray();

        if (existingRegistrations.Length == 0)
        {
            ServiceCollection.Add(serviceDescriptor);
        }
        else
        {
            _logger.LogDebug("{Verb} {Count} {Lifetime} {RegistrationType} for {ServiceType}: {ImplementationType}",
                "Replacing",
                existingRegistrations.Length,
                serviceDescriptor.Lifetime.ToString().ToLowerInvariant(),
                "registration(s)",
                serviceDescriptor.ServiceType.GetDetailedTypeName(),
                serviceDescriptor.GetImplementationTypeDescription());

            foreach (var existingRegistration in existingRegistrations)
            {
                ServiceCollection.Remove(existingRegistration);
            }

            ServiceCollection.Add(serviceDescriptor);
        }
    }

    public override void RegisterDecorator(ServiceDescriptor serviceDescriptor)
    {
        if (serviceDescriptor.ServiceType.IsOpenGeneric())
        {
            throw new NotSupportedException("Microsoft's DI does not support decoration of open generic types. " +
                                            "See https://github.com/khellang/Scrutor/issues/39 for more info");
        }

        if (ServiceCollection.Any(sd => sd.ServiceType == serviceDescriptor.ServiceType))
        {
            ServiceCollection.Decorate(
                serviceDescriptor.ServiceType,
                serviceDescriptor.ImplementationType
                ?? throw new ArgumentException("You must provide an implementationType when registering a decorator",
                    nameof(serviceDescriptor)));
        }
        else
        {
            _logger.LogWarning(
                "Skipping registration of decorator {DecoratorTypeName} because the service type to decorate ({ServiceType}) is not registered",
                serviceDescriptor.GetImplementationTypeDescription(),
                serviceDescriptor.ServiceType.Name);
        }
    }

    public override void RegisterCollection(IEnumerable<ServiceDescriptor> serviceDescriptors)
    {
        var serviceDescriptorArray = serviceDescriptors as ServiceDescriptor[] ?? serviceDescriptors.ToArray();

        if (serviceDescriptorArray.Length == 0)
        {
            _logger.LogWarning("Skipping registration of empty collection");
            return;
        }

        foreach (var serviceDescriptor in serviceDescriptorArray)
        {
            ServiceCollection.Add(serviceDescriptor);
        }
    }

    public override IServiceScope BeginScope()
    {
        return ServiceProvider.CreateScope();
    }
}