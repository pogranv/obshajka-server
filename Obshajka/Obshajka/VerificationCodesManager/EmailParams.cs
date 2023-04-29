using System;

namespace Obshajka.VerificationCodesManager
{
	public sealed record EmailParams(string EmailSenderHeader, string EmailHeader, string MessageBody);
}

