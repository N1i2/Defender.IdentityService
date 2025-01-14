﻿using Defender.Common.Errors;
using Defender.Common.Exceptions;
using Defender.Common.Interfaces;
using Defender.IdentityService.Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace Defender.IdentityService.Application.Modules.Account.Commands;

public record BlockUserCommand : IRequest<Unit>
{
    public Guid AccountId { get; set; }
    public bool DoBlockUser { get; set; } = true;
};

public sealed class BlockUserCommandValidator : AbstractValidator<BlockUserCommand>
{
    public BlockUserCommandValidator()
    {
        RuleFor(s => s.AccountId)
                  .NotEmpty().WithMessage(ErrorCodeHelper.GetErrorCode(ErrorCode.VL_InvalidRequest));
    }
}

public sealed class BlockUserCommandHandler : IRequestHandler<BlockUserCommand, Unit>
{
    private readonly IAccountAccessor _accountAccessor;
    private readonly IAccountManagementService _accountManagementService;

    public BlockUserCommandHandler(
        IAccountAccessor accountAccessor,
        IAccountManagementService accountManagementService
        )
    {
        _accountAccessor = accountAccessor;
        _accountManagementService = accountManagementService;
    }

    public async Task<Unit> Handle(BlockUserCommand request, CancellationToken cancellationToken)
    {
        var userAccount = await _accountManagementService.GetOrCreateAccountAsync(request.AccountId);

        var currentUser = _accountAccessor.AccountInfo;

        if (currentUser.IsSuperAdmin)
        {
            if (!userAccount.IsSuperAdmin)
            {
                await _accountManagementService.BlockAsync(request.AccountId, request.DoBlockUser);
            }
            else
            {
                throw new ForbiddenAccessException(ErrorCode.BR_ACC_SuperAdminCannotBeBlocked);
            }
        }
        else if (currentUser.IsAdmin)
        {
            if (!userAccount.IsAdmin)
            {
                await _accountManagementService.BlockAsync(request.AccountId, request.DoBlockUser);
            }
            else
            {
                throw new ForbiddenAccessException(ErrorCode.BR_ACC_AdminCannotBlockAdmins);
            }
        }

        return Unit.Value;
    }
}
