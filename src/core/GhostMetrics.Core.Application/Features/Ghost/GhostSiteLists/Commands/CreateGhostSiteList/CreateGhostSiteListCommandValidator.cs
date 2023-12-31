using GhostMetrics.Core.Application.Common.Interfaces;

namespace GhostMetrics.Core.Application.Features.GhostSiteLists.Commands.CreateGhostSiteList;

public class CreateGhostSiteListCommandValidator : AbstractValidator<CreateGhostSiteListCommand>
{
    private readonly IApplicationDbContext _context;

    public CreateGhostSiteListCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200)
            .MustAsync(BeAUniqueTitle)
                .WithMessage("'{PropertyName}' must be unique.")
                .WithErrorCode("Unique");
    }

    public async Task<bool> BeAUniqueTitle(string title, CancellationToken cancellationToken)
    {
		return await _context.SiteLists
			.AllAsync(x => x.Title != title, cancellationToken);
	}
}