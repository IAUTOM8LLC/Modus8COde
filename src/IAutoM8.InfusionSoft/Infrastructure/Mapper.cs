using IAutoM8.InfusionSoft.Dto;
using IAutoM8.InfusionSoft.Extentions;
using IAutoM8.InfusionSoft.Responces;

namespace IAutoM8.InfusionSoft.Infrastructure
{
    internal class Mapper
    {
        internal static void Register()
        {
            CreateStructMapper<int>()
                .Prop(string.Empty, (t,n) =>t.Property = n.ToInt());

            CreateStructMapper<bool>()
                .Prop(string.Empty, (t, n) => t.Property = n.ToBool());

            CreateMapper<InvoiceDto>()
                .Prop("ContactId", (t, n) => t.ContactId = n.ToInt())
                .Prop("PayStatus", (t, n) => t.IsPayed = n.ToBool());

            CreateMapper<OrderDetailResponce>()
                .Prop("OrderId", (t, n) => t.OrderId = n.ToInt())
                .Prop("InvoiceId", (t, n) => t.InvoiceId = n.ToInt());

            CreateMapper<ActionItemResponce>()
                .Prop("Action", (t, n) => t.Action = n.ToStringValue())
                .Prop("Message", (t, n) => t.Message = n.ToStringValue())
                .Prop("IsError", (t, n) => t.IsError = n.ToBool());

            CreateMapper<AffiliateDto>()
                .Prop("AffCode", (t, n) => t.AffCode = n.ToStringValue())
                .Prop("Password", (t, n) => t.Password = n.ToStringValue())
                .Prop("Id", (t, n) => t.Id = n.ToInt());

        }

        private static Mapper<T> CreateMapper<T>()
            where T : new()
        {
            return new Mapper<T>();
        }

        private static Mapper<StructMapper<T>> CreateStructMapper<T>()
            where T : struct
        {
            return new Mapper<StructMapper<T>>();
        }
    }
}
