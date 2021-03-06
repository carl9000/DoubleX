﻿namespace UTH.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Security.Claims;
    using UTH.Infrastructure.Resource.Culture;
    using UTH.Infrastructure.Utility;
    using UTH.Framework;

    /// <summary>
    /// 会议信息应用服务接口
    /// </summary>
    public interface IMeetingApplication :
        IApplicationCrudService<MeetingDTO, MeetingEditInput>, 
        IApplicationService
    {
        /// <summary>
        /// 根据Code获取会议信息
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        MeetingDTO GetByCode(MeetingEditInput input);

        /// <summary>
        /// 获取同步数据
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        MeetingSyncOutput SyncQuery(MeetingSyncInput input);
    }
}
