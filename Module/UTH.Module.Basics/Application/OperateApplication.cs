﻿namespace UTH.Module.Basics
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

    /// <summary>
    /// 操作权限应用服务
    /// </summary>
    public class OperateApplication :
        ApplicationCrudService<OperateEntity, OperateDTO, OperateEditInput>,
        IOperateApplication
    {
        public OperateApplication(IDomainDefaultService<OperateEntity> _service, IApplicationSession session, ICachingService caching) :
            base(_service, session, caching)
        {

        }
    }
}
