namespace WPF_Payment_Project.Models
{
    using System;

    public partial class Payment
    {
        public int ID { get; set; }
        public int CategoryID { get; set; }
        public int UserID { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public int Num { get; set; }
        public decimal Price { get; set; }

        public virtual Category Category { get; set; }
        public virtual Users Users { get; set; }
    }
}
