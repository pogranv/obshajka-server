using System;
namespace Obshajka.VerificationCodeManager
{
	public sealed record EmailParams(string EmailSenderHeader, string EmailHeader, string MessageBody);
}

