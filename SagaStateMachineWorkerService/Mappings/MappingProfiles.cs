using AutoMapper;
using SagaStateMachineWorkerService.Models;
using Shared;
using Shared.Events;
using Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SagaStateMachineWorkerService.Mappings
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<IOrderCreatedRequestEvent, OrderStateInstance>()
                .IncludeMembers(s=>s.Payment)
                .ReverseMap();
            CreateMap<PaymentMessage, OrderStateInstance>().ReverseMap();

        }
    }
}
