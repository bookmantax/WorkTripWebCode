using Braintree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Worktrip.Models
{
    public class BraintreeUtils
    {
        public static BraintreeGateway gateway;

        public static bool debug = false;

        public static void Init()
        {
            if (debug)
            {
                gateway = new BraintreeGateway
                {
                    Environment = Braintree.Environment.SANDBOX,
                    MerchantId = "5vbg675ndqb97t67",
                    PublicKey = "kxxmdybwgxpgr43j",
                    PrivateKey = "d814f47d963cd5d241da77f7791aedda"
                };
            }
            else
            {
                gateway = new BraintreeGateway
                {
                    Environment = Braintree.Environment.PRODUCTION,
                    MerchantId = "59yv2mxct52rnbxd",
                    PublicKey = "mcf22t45q97knsv6",
                    PrivateKey = "06e3c2118ca2a0074f25e0bed3978c4d"
                };
            }
        }

        public static string GenerateClientToken()
        {
            if (gateway == null)
            {
                Init();
            }

            var clientToken = gateway.ClientToken.generate();
            return clientToken;
        }

        public static bool ChargePayment(string userId, string paymentNonce, int year)
        {
            using (var db = new WorktripEntities())
            {
                var user = db.Users.FirstOrDefault(u => u.Id == userId);

                var link = db.UserToPreparers.FirstOrDefault(up => up.Year == year && up.UserId == userId);

                if (user == null || link == null || !link.Fee.HasValue)
                {
                    return false;
                }

                var request = new TransactionRequest
                {
                    Amount = link.Fee.Value * 1.07M,
                    PaymentMethodNonce = paymentNonce,
                    Customer = new CustomerRequest
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName
                    },
                    BillingAddress = new AddressRequest
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName
                    },
                    Options = new TransactionOptionsRequest
                    {
                        SubmitForSettlement = true
                    }
                };

                Result<Transaction> result = gateway.Transaction.Sale(request);

                if (result.IsSuccess())
                {
                    link.StatusId = db.Status.First(s => s.Name == "Finished").Id;
                    db.SaveChanges();
                    UserInfoViewModel.UpdateUserActionsLog(userId, "submitted payment");
                }

                return result.IsSuccess();
            }
        }

    }
}