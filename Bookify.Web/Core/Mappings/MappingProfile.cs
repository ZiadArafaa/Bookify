using AutoMapper;
using Bookify.Web.Core.Models;
using Bookify.Web.Core.ViewModels;

namespace Bookify.Web.Core.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //Categories
            CreateMap<Category, CategoryViewModel>();
            CreateMap<Category, CategoryFormViewModel>().ReverseMap();

            //Authors
            CreateMap<Author, AuthorFormViewModel>().ReverseMap();
            CreateMap<Author, AuthorViewModel>();


            //Books
            CreateMap<BookFormViewModel, Book>()
                .ForMember(dst => dst.Categories, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dst => dst.Categories, opt => opt.Ignore());

            CreateMap<Book, BookViewModel>()
                .ForMember(dst => dst.Author, opt => opt.MapFrom(src => src.Author!.Name))
                .ForMember(dst => dst.Categories, opt => opt.MapFrom(src => src.Categories.Select(c => c.Category!.Name).ToList()));

            CreateMap<BookCopy, BookCopyViewModel>()
                .ForMember(dst => dst.BookHall, opt => opt.MapFrom(b => b.Book!.Hall));

            //BookCopies
            CreateMap<BookCopyFormViewModel, BookCopy>().ReverseMap();

            //User
            CreateMap<ApplicationUser, ApplicationUserViewModel>()
                .ForMember(dst => dst.FullName, opt => opt.MapFrom(u => u.FirstName + " " + u.LastName))
                .ForMember(dst => dst.CreatedBy, opt => opt.MapFrom(u => u.CreatedById))
                .ForMember(dst => dst.UpdatedBy, opt => opt.MapFrom(u => u.UpdatedById));


            //Subscriper
            CreateMap<SubscriberFormViewModel, Subscriber>().ReverseMap();
            CreateMap<Subscriber, SubscribersearchResponseViewModel>()
                .ForMember(m => m.FullName, opt => opt.MapFrom(s => $"{s.FirstName} {s.LastName}"));
            CreateMap<Subscriber, SubscriberViewModel>()
                .ForMember(m => m.Governorate, opt => opt.MapFrom(s => s.Governorate!.Name))
                .ForMember(m => m.Area, opt => opt.MapFrom(s => s.Area!.Name))
                .ForMember(m => m.FullName, opt => opt.MapFrom(s => s.FirstName + " " + s.LastName));

        }
    }
}
