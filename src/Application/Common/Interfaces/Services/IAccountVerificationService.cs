﻿using Defender.IdentityService.Domain.Entities;

namespace Defender.IdentityService.Application.Common.Interfaces;

public interface IAccountVerificationService
{
    Task<string> SendVerificationEmailAsync(Guid userId);
    Task<AccountInfo> VerifyEmailAsync(int hash, int code);
}