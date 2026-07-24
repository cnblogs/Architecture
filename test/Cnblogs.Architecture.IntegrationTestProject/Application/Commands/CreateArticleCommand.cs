using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.Ddd.Cqrs.AgentFramework;
using Cnblogs.Architecture.IntegrationTestProject.Application.Errors;
using Cnblogs.Architecture.IntegrationTestProject.Models;

namespace Cnblogs.Architecture.IntegrationTestProject.Application.Commands;

[AgentTool(Description = "Create an article with the given title.")]
public record CreateArticleCommand(string Title, bool ValidateOnly = false) : ICommand<ArticleDto, TestError>;
