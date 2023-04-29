using System;

namespace Obshajka.Models
{
    public sealed record VerificationCodeWithEmail(string Email, string VerificationCode);
}