using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using Entities = SampleBlog.Repositories.Entities;
using Model = SampleBlog.Models;

namespace SampleBlog
{
    /// <summary>
    /// Configuration of mappings performed by Automapper
    /// </summary>
    public static class AutoMapperConfig
    {
        public static void Init()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Entities.Post, Model.PostDetailsModel>();

                cfg.CreateMap<Entities.Post, Model.PostListItemModel>()
                    .ForMember(dest => dest.ShortDescription, opt => opt.MapFrom(src =>
                                string.IsNullOrEmpty(src.Content)
                                ? src.Content
                                : string.Format("{0}...", src.Content.Substring(0, System.Math.Min(src.Content.Length, 50)))
                                ));

                cfg.CreateMap<Model.PostDetailsModel, Entities.Post>();
            });
            
        }
    }
}