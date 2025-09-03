using AutoMapper;
using MyPocket.Core.Models;
using MyPocket.Shared.DTOs;

namespace MyPocket.API.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Announcement mappings
            CreateMap<Announcement, AnnouncementDTO>()
                .ForMember(dest => dest.AdminName,
                    opt => opt.MapFrom(src => src.Admin.Nickname ?? src.Admin.Email));
            CreateMap<CreateAnnouncementDTO, Announcement>();
            CreateMap<UpdateAnnouncementDTO, Announcement>();

            // User mappings
            CreateMap<User, UserDTO>();
            CreateMap<CreateUserDTO, User>();
            CreateMap<UpdateUserDTO, User>();

            // Transaction mappings
            CreateMap<Transaction, TransactionDTO>()
                .ForMember(dest => dest.CategoryName,
                    opt => opt.MapFrom(src => src.Category.CategoryName));
            CreateMap<CreateTransactionDTO, Transaction>();
            CreateMap<UpdateTransactionDTO, Transaction>();

            // Category mappings
            CreateMap<Category, CategoryDTO>();
            CreateMap<CreateCategoryDTO, Category>();
            CreateMap<UpdateCategoryDTO, Category>();

            // Budget mappings
            CreateMap<Budget, BudgetDTO>()
                .ForMember(dest => dest.CategoryName,
                    opt => opt.MapFrom(src => src.Category.CategoryName));
            CreateMap<CreateBudgetDTO, Budget>();
            CreateMap<UpdateBudgetDTO, Budget>();

            // SavingGoal mappings
            CreateMap<SavingGoal, SavingGoalDTO>();
            CreateMap<CreateSavingGoalDTO, SavingGoal>();
            CreateMap<UpdateSavingGoalDTO, SavingGoal>();

            // SubscriptionPlan mappings
            CreateMap<SubscriptionPlan, SubscriptionPlanDTO>();
            CreateMap<CreateSubscriptionPlanDTO, SubscriptionPlan>();
            CreateMap<UpdateSubscriptionPlanDTO, SubscriptionPlan>();

            // UserSubscription mappings
            CreateMap<UserSubscription, UserSubscriptionDTO>()
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.User.Nickname ?? src.User.Email))
                .ForMember(dest => dest.PlanName,
                    opt => opt.MapFrom(src => src.Plan.PlanName));
            CreateMap<CreateUserSubscriptionDTO, UserSubscription>();
            CreateMap<UpdateUserSubscriptionDTO, UserSubscription>();

            // Payment mappings
            CreateMap<Payment, PaymentDTO>();
            CreateMap<CreatePaymentDTO, Payment>();
            CreateMap<UpdatePaymentDTO, Payment>();
        }
    }
}