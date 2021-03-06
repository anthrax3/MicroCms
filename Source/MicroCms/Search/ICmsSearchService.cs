﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroCms.Search
{
    public interface ICmsSearchService
    {
        void AddOrUpdateDocuments(params CmsDocument[] documents);
        void DeleteDocuments(params CmsDocument[] documents);
        IEnumerable<CmsTitle> SearchDocuments(string queryText);
        IEnumerable<CmsTitle> SearchDocuments(CmsDocumentField field, string queryText);
        IEnumerable<CmsTitle> GetAll(); 
    }
}
