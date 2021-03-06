﻿namespace UTH.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using System.Reflection;
    using Castle.DynamicProxy;
    using FluentValidation;
    using UTH.Infrastructure.Resource;
    using UTH.Infrastructure.Resource.Culture;
    using UTH.Infrastructure.Utility;

    /// <summary>
    /// Web控制器拦截器选择
    /// </summary>
    public class ControllerInterceptorSelector : IInterceptorSelector
    {
        public IInterceptor[] SelectInterceptors(Type type, MethodInfo method, IInterceptor[] interceptors)
        {
            return FrameworkHelper.GetServiceInterceptors(type, method, interceptors);
        }
    }
}
