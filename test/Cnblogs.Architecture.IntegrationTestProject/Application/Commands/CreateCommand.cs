using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.IntegrationTestProject.Application.Errors;

namespace Cnblogs.Architecture.IntegrationTestProject.Application.Commands;

public record CreateCommand(bool NeedError, bool ValidateOnly = false) : ICommand<TestError>;