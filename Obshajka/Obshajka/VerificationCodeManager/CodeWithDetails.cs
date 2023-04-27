﻿using Obshajka.VerificationCodeSender.Interfaces;

namespace Obshajka.VerificationCodeSender
{

    internal class CodeWithDetails
    {
        public IUser User { get; }
        public string VerificationCode { get; }
        DateTime _timeOfCreation;
        public CodeWithDetails(IUser user, string verificationCode) 
        {
            User = user;
            VerificationCode = verificationCode;
            _timeOfCreation = DateTime.Now;
        }

        public bool IsEqualsVerificationCode(string code)
        {
            return VerificationCode == code;
        }

        public bool IsDurationOfExistsOverdue(int lifeTimeMinutes)
        {
            var now = DateTime.Now;
            return lifeTimeMinutes < now.Subtract(_timeOfCreation).Minutes;
        }
    }
}
