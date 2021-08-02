using IAutoM8.InfusionSoft.Dto;
using System.Xml;

namespace IAutoM8.InfusionSoft.Extentions
{
    static class XmlWriterInfusionsoftDtoExtentions
    {
        internal static void Param(this XmlWriter xmlWriter, ContactDto contactDto)
        {
            xmlWriter.Param(s => s
                        .Member("FirstName", contactDto.FirstName)
                        .Member("Email", contactDto.Email));
        }

        internal static void Param(this XmlWriter xmlWriter, AffiliateDto affiliateDto)
        {
            xmlWriter.Param(s => s
                        .Member("AffCode", affiliateDto.AffCode)
                        .Member("Password", affiliateDto.Password));
        }

        
    }
}
