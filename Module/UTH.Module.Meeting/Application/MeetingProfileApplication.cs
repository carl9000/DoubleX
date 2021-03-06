﻿namespace UTH.Module.Meeting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Text;
    using UTH.Infrastructure.Resource;
    using UTH.Infrastructure.Resource.Culture;
    using UTH.Infrastructure.Utility;
    using UTH.Framework;
    using UTH.Domain;
    using UTH.Plug;

    /// <summary>
    /// 会议账号配置信息业务
    /// </summary>
    public class MeetingProfileApplication :
        ApplicationCrudService<IMeetingProfileDomainService, MeetingProfileEntity, MeetingProfileDTO, MeetingProfileEditInput>,
        IMeetingProfileApplication
    {
        public MeetingProfileApplication(IMeetingProfileDomainService _service, IApplicationSession session, ICachingService caching) :
            base(_service, session, caching)
        {

        }

        /// <summary>
        /// 获取当前登录账号(Session)配置,如不存在，创建
        /// </summary>
        /// <returns></returns>
        public MeetingProfileDTO GetLoginAccountProfile()
        {
            Session.User.Id.CheckEmpty();
            return MapperToDto(service.GetOrInsertDefaultByAccount(Session.User.Id));
        }

        /// <summary>
        /// 保存当前登录账号(Session)配置
        /// </summary>
        /// <returns></returns>
        public MeetingProfileDTO SaveLoginAccountProfile(MeetingProfileEditInput input)
        {
            Session.User.Id.CheckEmpty();
            var entity = service.GetOrInsertDefaultByAccount(Session.User.Id);
            entity.CheckNull();
            input.Id = entity.Id;

            var output = Update(input);
            if (!output.IsEmpty() && !input.MeetingId.IsEmpty())
            {
                var meetingRep = EngineHelper.Resolve<IMeetingRepository>();
                var meeting = meetingRep.Get(input.MeetingId);
                meeting.CheckNull();
                meeting.Setting = JsonHelper.Serialize(EngineHelper.Map<MeetingSettingModel>(output));
                meetingRep.Update(meeting);
            }
            return output;
        }


    }
}
