using AutoMapper;
using KoreanLearn.Data.Entities;
using KoreanLearn.Service.ViewModels.Admin.Course;
using KoreanLearn.Service.ViewModels.Admin.Lesson;
using KoreanLearn.Service.ViewModels.Admin.Section;

namespace KoreanLearn.Service.Mapper;

public class CourseAdminProfile : Profile
{
    public CourseAdminProfile()
    {
        // Course
        CreateMap<Course, CourseAdminListViewModel>()
            .ForMember(d => d.SectionCount, o => o.MapFrom(s => s.Sections.Count))
            .ForMember(d => d.LessonCount, o => o.MapFrom(s => s.Sections.SelectMany(sec => sec.Lessons).Count()));

        CreateMap<Course, CourseDetailAdminViewModel>()
            .ForMember(d => d.Sections, o => o.MapFrom(s => s.Sections.OrderBy(sec => sec.SortOrder)));

        CreateMap<Course, EditCourseViewModel>()
            .ForMember(d => d.ExistingCoverImageUrl, o => o.MapFrom(s => s.CoverImageUrl))
            .ForMember(d => d.CoverImage, o => o.Ignore());

        CreateMap<CreateCourseViewModel, Course>()
            .ForMember(d => d.CoverImageUrl, o => o.Ignore());

        // Section
        CreateMap<Section, SectionAdminViewModel>()
            .ForMember(d => d.Lessons, o => o.MapFrom(s => s.Lessons.OrderBy(l => l.SortOrder)));

        CreateMap<Section, SectionFormViewModel>()
            .ForMember(d => d.CourseTitle, o => o.Ignore());

        CreateMap<SectionFormViewModel, Section>()
            .ForMember(d => d.Course, o => o.Ignore())
            .ForMember(d => d.Lessons, o => o.Ignore());

        // Lesson
        CreateMap<Lesson, LessonAdminViewModel>();

        CreateMap<Lesson, LessonFormViewModel>()
            .ForMember(d => d.SectionTitle, o => o.Ignore())
            .ForMember(d => d.CourseTitle, o => o.Ignore())
            .ForMember(d => d.CourseId, o => o.Ignore())
            .ForMember(d => d.VideoFile, o => o.Ignore())
            .ForMember(d => d.PdfFile, o => o.Ignore())
            .ForMember(d => d.ExistingVideoUrl, o => o.Ignore())
            .ForMember(d => d.ExistingPdfUrl, o => o.Ignore())
            .ForMember(d => d.ExistingPdfFileName, o => o.Ignore());

        CreateMap<LessonFormViewModel, Lesson>()
            .ForMember(d => d.Section, o => o.Ignore())
            .ForMember(d => d.Progresses, o => o.Ignore())
            .ForMember(d => d.Quiz, o => o.Ignore())
            .ForMember(d => d.VideoUrl, o => o.Ignore())
            .ForMember(d => d.PdfUrl, o => o.Ignore())
            .ForMember(d => d.PdfFileName, o => o.Ignore());
    }
}
