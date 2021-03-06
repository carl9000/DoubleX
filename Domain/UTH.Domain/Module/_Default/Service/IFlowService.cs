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
    /// 流程领域服务接口
    /// </summary>
    public interface IFlowService : IDomainService
    {
        /// <summary>
        /// 创建流程
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        FlowModel Create(FlowEditInput input);

        /// <summary>
        /// 获取进度
        /// </summary>
        /// <param name="flowId"></param>
        /// <returns></returns>
        FlowModel Progress(Guid flowId);

        /// <summary>
        /// 设置进度
        /// </summary>
        /// <param name="model"></param>
        void SetProgress(FlowModel model);
    }
}
