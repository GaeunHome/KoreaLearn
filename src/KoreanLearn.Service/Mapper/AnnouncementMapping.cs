using KoreanLearn.Data.Entities;
using KoreanLearn.Service.ViewModels.Home;
using Mapster;

namespace KoreanLearn.Service.Mapper;

/// <summary>公告 Mapster 映射設定</summary>
public class AnnouncementMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Announcement, AnnouncementViewModel>();
    }
}
