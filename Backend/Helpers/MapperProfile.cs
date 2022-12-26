using AutoMapper;
using BackendAPI.Entities;
using BackendAPI.Models.Group;
using BackendAPI.Models.User;
using BackendAPI.Models.Trip;
using BackendAPI.Models.Ranking;
using BackendAPI.Models.Activity;
using BackendAPI.Models.Attachment;
using BackendAPI.Models.Post;
using BackendAPI.Models.Information;
using System.Collections.Generic;

namespace BackendAPI.Data
{
    /// <summary>
    /// AutoMapper Profile
    /// </summary>
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<User, UserModel>().ForMember(d => d.Photo, opt => opt.MapFrom(o => o.Photo.Url));
            CreateMap<User, UserModelSimple>().ForMember(d => d.Photo, opt => opt.MapFrom(o => o.Photo.Url));
            CreateMap<User, UserModelSelf>().ForMember(d => d.Photo, opt => opt.MapFrom(o => o.Photo.Url));
            CreateMap<UserGroup, UserGroupModel>();
            CreateMap<UserGroup, GroupUserModel>();
            CreateMap<GroupCreateModel, Group>();
            CreateMap<Group, GroupModel>().ForMember(d => d.Image, opt => opt.MapFrom(o => o.Image.Url));
            CreateMap<Group, GroupModelSimple>().ForMember(d => d.Image, opt => opt.MapFrom(o => o.Image.Url));
            CreateMap<Group, GroupModelAdmin>().ForMember(d => d.Image, opt => opt.MapFrom(o => o.Image.Url));
            CreateMap<Group, GroupModelTrip>().ForMember(d => d.Image, opt => opt.MapFrom(o => o.Image.Url));
            CreateMap<GroupInviteModel, GroupInvite>();
            CreateMap<UserGroupInviteModel, GroupInvite>();
            CreateMap<GroupInvite, UserGroupInviteModel>();
            CreateMap<GroupInvite, GroupInviteModel>();
            CreateMap<GroupDetailsUpdateModel, Group>();
            CreateMap<TripCreateModel, Trip>();
            CreateMap<Trip, TripModel>().ForMember(d => d.Image, opt => opt.MapFrom(o => o.Image.Url));
            CreateMap<Trip, TripModelSimple>().ForMember(d => d.Image, opt => opt.MapFrom(o => o.Image.Url));
            CreateMap<Trip, TripModelGroup>().ForMember(d => d.Image, opt => opt.MapFrom(o => o.Image.Url));
            CreateMap<Trip, TripModelAdmin>().ForMember(d => d.Image, opt => opt.MapFrom(o => o.Image.Url));
            CreateMap<UserTrip, TripUserModel>();
            CreateMap<UserTrip, UserTripModel>();
            CreateMap<TripCreateModel, Trip>();
            CreateMap<TripInvite, TripInviteModel>();
            CreateMap<TripInvite, UserTripInviteModel>();
            CreateMap<UserTripInviteModel,TripInvite>();
            CreateMap<Ranking, RankingModel>();
            CreateMap<Ranking, RankingModelAdmin>();
            CreateMap<RankingCreateModel, Ranking>();
            CreateMap<ActivityCreateModelInvidual, Activity>();
            CreateMap<Activity,ActivityModelSimple>();
            CreateMap<Attachment, AttachmentModel>();
            CreateMap<Post, PostModel>();
            CreateMap<PostCreateModel, Post>();
            CreateMap<Post, PostModel>();
            CreateMap<Post, PostModelTrip>();
            CreateMap<Post, PostModelUser>();
            CreateMap<GroupEvent,GroupEventModel>();
            CreateMap<TripEvent, TripEventModel>();
            CreateMap<GroupBan, GroupBanModel>();
            CreateMap<Recommendation, RecommendationModel>();
            CreateMap<Statistic, StatisticsModel>();
        }
    }
}
