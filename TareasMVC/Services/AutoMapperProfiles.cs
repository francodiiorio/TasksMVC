using AutoMapper;
using TareasMVC.Models;

namespace TareasMVC.Services
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles() 
        {
            CreateMap<Tarea, TaskDTO>()
                .ForMember(dto => dto.TotalSteps, ent => ent.MapFrom(x => x.Steps.Count()))
                .ForMember(dto => dto.CompleteSteps, ent => ent.MapFrom(x => x.Steps.Where(s => s.Complete).Count()));
        }
    }
}
