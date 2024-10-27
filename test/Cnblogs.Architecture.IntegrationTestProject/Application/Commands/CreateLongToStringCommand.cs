using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.IntegrationTestProject.Application.Errors;
using Cnblogs.Architecture.IntegrationTestProject.Models;

namespace Cnblogs.Architecture.IntegrationTestProject.Application.Commands;

public record CreateLongToStringCommand(long Id, bool ValidateOnly = false) : ICommand<LongToStringModel, TestError>;
