#pragma warning disable CS8603, CS8600
using KoreanLearn.Data.Entities;
using KoreanLearn.Service.ViewModels.Admin.Course;
using KoreanLearn.Service.ViewModels.Admin.Lesson;
using KoreanLearn.Service.ViewModels.Admin.Section;
using Mapster;

namespace KoreanLearn.Service.Mapper;

/// <summary>後台課程管理 Mapster 映射設定（Course / Section / Lesson 的 Entity ↔ ViewModel）</summary>
public class CourseAdminMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // ── 課程映射 ──
        config.NewConfig<Course, CourseAdminListViewModel>()
            .Map(d => d.SectionCount, s => s.Sections.Count)
            .Map(d => d.LessonCount, s => s.Sections.SelectMany(sec => sec.Lessons).Count());

        config.NewConfig<Course, CourseDetailAdminViewModel>()
            .Map(d => d.Sections, s => s.Sections.OrderBy(sec => sec.SortOrder));

        config.NewConfig<Course, CourseFormViewModel>()
            .Map(d => d.ExistingCoverImageUrl, s => s.CoverImageUrl)
            .Ignore(d => d.CoverImage!);

        config.NewConfig<CourseFormViewModel, Course>()
            .Ignore(d => d.CoverImageUrl!);

        // ── 章節映射 ──
        config.NewConfig<Section, SectionAdminViewModel>()
            .Map(d => d.Lessons, s => s.Lessons.OrderBy(l => l.SortOrder));

        config.NewConfig<Section, SectionFormViewModel>()
            .Ignore(d => d.CourseTitle!);

        config.NewConfig<SectionFormViewModel, Section>()
            .Ignore(d => d.Course!)
            .Ignore(d => d.Lessons!);

        // ── 單元映射 ──
        config.NewConfig<Lesson, LessonAdminViewModel>();

        config.NewConfig<Lesson, LessonFormViewModel>()
            .Ignore(d => d.SectionTitle!)
            .Ignore(d => d.CourseTitle!)
            .Ignore(d => (object)d.CourseId)
            .Ignore(d => d.VideoFile!)
            .Ignore(d => d.PdfFile!)
            .Ignore(d => d.ExistingVideoUrl!)
            .Ignore(d => d.ExistingPdfUrl!)
            .Ignore(d => d.ExistingPdfFileName!);

        config.NewConfig<LessonFormViewModel, Lesson>()
            .Ignore(d => d.Section!)
            .Ignore(d => d.Progresses!)
            .Ignore(d => d.Quiz!)
            .Ignore(d => d.VideoUrl!)
            .Ignore(d => d.PdfUrl!)
            .Ignore(d => d.PdfFileName!);
    }
}
