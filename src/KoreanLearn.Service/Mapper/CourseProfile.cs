using AutoMapper;
using KoreanLearn.Data.Entities;
using KoreanLearn.Service.ViewModels.Course;
using KoreanLearn.Service.ViewModels.Home;

namespace KoreanLearn.Service.Mapper;

/// <summary>前台課程 AutoMapper 設定檔（Course / Section / Lesson / Announcement 的 Entity → ViewModel 映射）</summary>
public class CourseProfile : Profile
{
    public CourseProfile()
    {
        // 課程列表：自動計算章節與單元數量
        CreateMap<Course, CourseListViewModel>()
            .ForMember(d => d.SectionCount, o => o.MapFrom(s => s.Sections.Count))
            .ForMember(d => d.LessonCount, o => o.MapFrom(s => s.Sections.SelectMany(sec => sec.Lessons).Count()));

        // 課程詳情：章節依排序欄位排列
        CreateMap<Course, CourseDetailViewModel>()
            .ForMember(d => d.Sections, o => o.MapFrom(s => s.Sections.OrderBy(sec => sec.SortOrder)));

        // 章節：單元依排序欄位排列
        CreateMap<Section, SectionViewModel>()
            .ForMember(d => d.Lessons, o => o.MapFrom(s => s.Lessons.OrderBy(l => l.SortOrder)));

        CreateMap<Lesson, LessonSummaryViewModel>();

        CreateMap<Announcement, AnnouncementViewModel>();
    }
}
