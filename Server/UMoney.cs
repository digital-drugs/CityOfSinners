using ExitGames.Concurrency.Fibers;
using Mafia_Server;
using System;
using System.Net;
using Yandex.Checkout.V3;

///TO DO
///сделать менеджер, который будет отслеживать состояние платежей
///при оплате платежа найти клиента (логин или id) и передать ему товар

namespace Mafia_Server
{
    public class UMoney
    {
        public static readonly Yandex.Checkout.V3.Client client = new Yandex.Checkout.V3.Client(
          shopId: "967272",
          secretKey: "test_HnRzFdSKK8Di6hz007vry4c1FHpB8A35GtlRDECfm_Q");

        public UMoney()
        {
            
        }       

        public static string CreateRequest(int shopItemId, Mafia_Server.Client webClient)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
          

            //AsyncClient asyncClient = client.MakeAsync();

            var newPaymernt = new NewPayment
            {
                Amount = new Amount
                {
                    Value = 100.00m,
                    Currency = "RUB"
                },
                Confirmation = new Confirmation
                {
                    Type = ConfirmationType.Redirect,
                    ReturnUrl = "https://95.220.115.2"
                },
                Description = $"test order {shopItemId}"
            };

            Payment payment = client.CreatePayment(newPaymernt);

            string confirmURL = payment.Confirmation.ConfirmationUrl;

            var check = new CheckPayment(payment, webClient);

            return confirmURL;
        } 


         
    }        
}

public class CheckPayment
{
    public Payment payment;
    public Mafia_Server.Client webClient;

    private long checkInterval = 5000;

    PoolFiber fiber;
    IDisposable timer;

    public CheckPayment(Payment payment, Mafia_Server.Client webClient)
    {
        this.payment = payment;

        this.webClient = webClient;

        fiber = new PoolFiber();
        fiber.Start();

        timer = fiber.ScheduleOnInterval(() => { Check(); }, 0, checkInterval);

        //Mafia_Server.Logger.Log.Debug($"confirmURL => ");
    }

    private void Check()
    {
        var paymentStatus = UMoney.client.GetPayment(payment.Id);

        Mafia_Server.Logger.Log.Debug($"paymentStatus => {paymentStatus.Status} ");

        if (paymentStatus.Status == PaymentStatus.WaitingForCapture)
        {
            UMoney.client.CapturePayment(payment);

            Mafia_Server.Logger.Log.Debug($"payment captured ");
        }

        if (paymentStatus.Status == PaymentStatus.Succeeded)
        {            
            timer.Dispose();

            //добавить покупку клиенту //БД

            webClient.PaymentSucceeded(payment.Description);

            Mafia_Server.Logger.Log.Debug($"payment Succeeded ");
        }
    }
}