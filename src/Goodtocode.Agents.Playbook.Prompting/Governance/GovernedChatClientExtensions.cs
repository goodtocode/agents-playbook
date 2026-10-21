using Goodtocode.Agents.Governance.Application;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Goodtocode.Agents.Playbook.Prompting.Governance;

/// <summary>
/// Registration helpers that make governed Agentic Intent the default for a chat client.
/// </summary>
public static class GovernedChatClientExtensions
{
    /// <summary>
    /// Wraps the pipeline's chat client with <see cref="GovernedChatClient"/>. Resolves
    /// <see cref="IIntentPolicyProvider"/>, <see cref="IGovernedIntentScope"/> and, optionally,
    /// <see cref="IGovernedTurnObserver"/> from the service provider.
    /// </summary>
    public static ChatClientBuilder UseGovernedIntent(this ChatClientBuilder builder, GovernedChatClientOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.Use((inner, services) => new GovernedChatClient(
            inner,
            services.GetRequiredService<IIntentPolicyProvider>(),
            services.GetService<IGovernancePreambleComposer>() ?? new GovernancePreambleComposer(),
            services.GetRequiredService<IGovernedIntentScope>(),
            options,
            services.GetService<IGovernedTurnObserver>()));
    }

    /// <summary>
    /// Registers the governance preamble composer when the host has not supplied one.
    /// </summary>
    public static IServiceCollection AddGovernedIntentPreamble(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<IGovernancePreambleComposer, GovernancePreambleComposer>();
        return services;
    }
}
