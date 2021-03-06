﻿namespace UTH.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using UTH.Infrastructure.Resource;
    using UTH.Infrastructure.Resource.Culture;
    using UTH.Infrastructure.Utility;

    /// <summary>
    /// 分页结果
    /// </summary>
    public class PagingModel<TOutput>
    {
        /// <summary>
        /// 分页数据
        /// </summary>
        public List<TOutput> Rows { get; set; }

        /// <summary>
        /// 分页总数
        /// </summary>
        public long Total { get; set; }
    }
}
