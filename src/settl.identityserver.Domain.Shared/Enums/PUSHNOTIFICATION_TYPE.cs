using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace settl.identityserver.Domain.Shared.Enums
{
    public class PUSHNOTIFICATION_TYPE
    {
        public enum PushNotificationType
        {
            P2P_DEBIT = 1,
            P2P_CREDIT = 2,
            MONEY_REQUEST_ACCEPTANCE = 3,
            MONEY_REQUEST_DECLINE = 4,
            REFUND = 5,
            BILLPAYMENT = 6,
            FIRST_TIME_LOGIN = 7,
            REFERRAL_BONUS_REFERRER = 8,
            REFERRAL_BONUS_REFERRED = 9,
            REFERRAL_BONUS_TARGETTED_SAVINGS_WALLET = 10,
            PIN_RESET = 11,
            VIRTUAL_CARD_REQUEST = 12,
            PHYSICAL_CARD_REQUEST = 13,
            VIRTUAL_CARD_APPROVAL = 14,
            PHYSICAL_CARD_APPROVAL = 15,
            TRANSACTION_PIN_RESET = 16
        }
    }
}
