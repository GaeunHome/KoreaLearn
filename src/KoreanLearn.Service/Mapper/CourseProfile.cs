using AutoMapper;
using KoreanLearn.Data.Entities;
using KoreanLearn.Service.ViewModels.Course;
using KoreanLearn.Service.ViewModels.Home;

namespace KoreanLearn.Service.Mapper;

public class CourseProfile : Profile
{
    public CourseProfile()
    {
        CreateMap<Course, CourseListViewModel>()
            .ForMember(d => d.SectionCount, o => o.MapFrom(s => s.Sections.Count))
            .ForMember(d => d.LessonCount, o => o.MapFrom(s => s.Sections.SelectMany(sec => sec.Lessons).Count()));

        CreateMap<Course, CourseDetailViewModel>()
            .ForMember(d => d.Sections, o => o.MapFrom(s => s.Sections.OrderBy(sec => sec.SortOrder)));

        CreateMap<Section, SectionViewModel>()
            .ForMember(d => d.Lessons, o => o.MapFrom(s => s.Lessons.OrderBy(l => l.SortOrder)));

        CreateMap<Lesson, LessonSummaryViewModel>();

        CreateMap<Announcement, AnnouncementViewModel>();
    }
}
