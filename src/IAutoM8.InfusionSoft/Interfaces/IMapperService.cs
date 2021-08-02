using System.Collections.Generic;

namespace IAutoM8.InfusionSoft.Interfaces
{
    interface IMapperService
    {
        T Map<T>(string value) where T : new();
        T MapToStruct<T>(string value) where T : struct;
        List<T> MapToList<T>(string value) where T : new();
    }
}
