using System;

namespace Apv.Models.Transaksi
{
    public class TransTracking
    {
        public int Id { get; set; }
        public Trans Trans { get; set; }
        public int TransId { get; set; }
        public DateTime ReceiveDate { get; set; }
        public virtual ApplicationUser Receiver { get; set; }
        public string ReceiverId { get; set; }
        public string ReceiverActivity { get; set; }
        public string ReceiverIcon { get; set; }
        public string ReceiverColorIcon { get; set; }
        public DateTime? SendDate { get; set; }
        public virtual ApplicationUser Sender { get; set; }
        public string SenderId { get; set; }
        public string SenderKeterangan { get; set; }
    }
}