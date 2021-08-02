using AutoMapper;
using IAutoM8.Domain.Models;
using IAutoM8.Domain.Models.Credits;
using IAutoM8.Service.Credits.Dto;
using IAutoM8.Service.CreditsService.Dto;
using IAutoM8.Service.Vendor.Dto;

namespace IAutoM8.Service.Infrastructure.MapperProfiles.Credits
{
    public class CreditsProfile : Profile
    {
        public CreditsProfile()
        {
            ToDto();
            FromDto();
        }

        private void FromDto()
        {
            CreateMap<CreditsDto, Domain.Models.Credits.Credits>()

                // Ignore
                .ForMember(dest => dest.UserId, src => src.Ignore())
                .ForMember(dest => dest.User, src => src.Ignore())
                .ForMember(dest => dest.BraintreeCustomerId, src => src.Ignore())
                .ForMember(dest => dest.TotalCredits, src => src.Ignore());

            CreateMap<VendorPaymentRequestDto, VendorPaymentRequest>(MemberList.None)
               .ForMember(dest => dest.Amount, opts => opts.Ignore());

        }

        private void ToDto()
        {
            CreateMap<Domain.Models.Credits.Credits, CreditsDto>()
                .ForMember(dest => dest.AvailableCredits, src => src.Ignore())
                .ForMember(dest => dest.PrepaidTasksCount, src => src.Ignore())
                .ForMember(dest => dest.Fee, src => src.Ignore())
                .ForMember(dest => dest.Percentage, src => src.Ignore())
                .ForMember(dest => dest.ReservedCredits, src => src.Ignore());

            CreateMap<CreditsTax, CreditsTaxDto>();
            CreateMap<TransferRequest, TransferRequestDto>();

            CreateMap<VendorPaymentRequest, VendorPaymentRequestDto>();
        }
    }
}
