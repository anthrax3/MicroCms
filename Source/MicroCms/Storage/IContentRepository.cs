﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MicroCms.Storage
{
    public interface IContentRepository<TEntity>
        where TEntity : ContentEntity
    {
        TEntity Find(Guid id);
        void Save(TEntity entity);
        TEntity Delete(Guid id);
        IEnumerable<TEntity> GetAll();
    }
}