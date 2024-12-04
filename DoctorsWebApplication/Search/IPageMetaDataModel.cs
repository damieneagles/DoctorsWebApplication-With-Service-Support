using DoctorsWebApplication.Search;
using System.Collections;
using X.PagedList;

namespace DoctorsWebApplication.Search
{
    public interface IPageMetaDataModel<T1, T2> where T1 : X.PagedList.IPagedList where T2 : IPageSearchData
    {
        public T1 PagedList { get; }
        public IPageSearchData PageSearchData { get; }
        
    }
}
