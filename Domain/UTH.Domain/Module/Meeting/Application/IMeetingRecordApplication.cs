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
    /// 会议记录应用服务接口
    /// </summary>
    public interface IMeetingRecordApplication :
        IApplicationCrudService<MeetingRecordDTO, MeetingRecordEditInput>,
        IApplicationService
    {
        /// <summary>
        /// 创建会议记录
        /// </summary>
        /// <param name="input"></param>
        /// <returns>返回同步数所有(会议记录+翻译列表)</returns>
        List<MeetingSyncModel> Create(MeetingSyncModel input);

        /// <summary>
        /// 添加会议记录(含翻译记录)
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        MeetingRecordOutput Add(MeetingRecordEditInput input);
    }
}
