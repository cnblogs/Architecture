using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using Cnblogs.Architecture.IntegrationTestProject.Application.Errors;
using Cnblogs.Architecture.IntegrationTestProject.Models;

namespace Cnblogs.Architecture.IntegrationTestProject.Application.Commands;

public class CreateArticleCommandHandler : ICommandHandler<CreateArticleCommand, ArticleDto, TestError>
{
    /// <inheritdoc />
    public async Task<CommandResponse<ArticleDto, TestError>> Handle(
        CreateArticleCommand request,
        CancellationToken cancellationToken)
    {
        if (request.ValidateOnly)
        {
            return CommandResponse<ArticleDto, TestError>.Success();
        }

        return CommandResponse<ArticleDto, TestError>.Success(new ArticleDto { Title = request.Title });
    }
}
