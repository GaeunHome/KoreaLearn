using KoreanLearn.Data.Entities;
using KoreanLearn.Service.ViewModels.Course;
using KoreanLearn.Service.ViewModels.Home;
using Mapster;

namespace KoreanLearn.Service.Mapper;

/// <summary>前台課程 Mapster 映射設定（Course / Section / Lesson）</summary>
public class CourseMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // 課程列表：自動計算章節與單元數量
        config.NewConfig<Course, CourseListViewModel>()
            .Map(d => d.SectionCount, s => s.Sections.Count)
            .Map(d => d.LessonCount, s => s.Sections.SelectMany(sec => sec.Lessons).Count());

        // 課程詳情：章節依排序欄位排列
        config.NewConfig<Course, CourseDetailViewModel>()
            .Map(d => d.Sections, s => s.Sections.OrderBy(sec => sec.SortOrder));

        // 章節：單元依排序欄位排列
        config.NewConfig<Section, SectionViewModel>()
            .Map(d => d.Lessons, s => s.Lessons.OrderBy(l => l.SortOrder));

        config.NewConfig<Lesson, LessonSummaryViewModel>();
    }
}
