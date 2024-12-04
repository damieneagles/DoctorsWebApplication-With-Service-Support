using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Caching.Distributed;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using X.PagedList;

namespace DoctorsWebApplication.Search
{
    public interface IPageSearchData
    {
        string? SearchString { get; set; }
        bool CheckCurrent { get; set; }
        string SortOrder { get; set; }
        string SortDirection { get; set; }
        string CurrentFilter { get; set; }
        int? PageIndex { get; set; }
        string PageCacheKey { get; set; }
        IDistributedCache Cache { get; }
    }

    public class PageSearchData : IPageSearchData
    {
        private readonly IDistributedCache _cache;
        private readonly string _containerPageName;

        public string? SearchString
        {
            get
            {
#pragma warning disable CS8604 // Possible null reference argument.
                return ConvertCacheBytes<string?>(_cache.Get("SearchString" + PageCacheKey));
#pragma warning restore CS8604 // Possible null reference argument.
            }
            set
            {
                _cache.Set("SearchString" + PageCacheKey, GetCacheBytes<string?>(value));
            }
        }

        public bool CheckCurrent
        {
            get
            {
#pragma warning disable CS8604 // Possible null reference argument.
                return (ConvertCacheBytes<bool>(_cache.Get("CheckCurrent" + PageCacheKey)));
#pragma warning restore CS8604 // Possible null reference argument.
            }
            set
            {
                _cache.Set("CheckCurrent" + PageCacheKey, GetCacheBytes<bool>(value));
            }
        }

        public string SortOrder
        {
            get
            {
#pragma warning disable CS8604 // Possible null reference argument.
                return ConvertCacheBytes<string>(_cache.Get("SortOrder" + PageCacheKey));
#pragma warning restore CS8604 // Possible null reference argument.
            }
            set
            {
                _cache.Set("SortOrder" + PageCacheKey, GetCacheBytes<string>(value));
            }
        }

        public string CurrentFilter
        {
            get
            {
#pragma warning disable CS8604 // Possible null reference argument.
                return ConvertCacheBytes<string>(_cache.Get("CurrentFilter" + PageCacheKey));
#pragma warning restore CS8604 // Possible null reference argument.
            }
            set
            {
                _cache.Set("CurrentFilter" + PageCacheKey, GetCacheBytes<string>(value));
            }
        }

        public IDistributedCache Cache
        {
            get
            { return this._cache; }
        }

        public int? PageIndex
        {
            get
            {
#pragma warning disable CS8604 // Possible null reference argument.
                return ConvertCacheBytes<int?>(_cache.Get("PageIndex" + PageCacheKey));
#pragma warning restore CS8604 // Possible null reference argument.
            }
            set
            {
                _cache.Set("PageIndex" + PageCacheKey, GetCacheBytes<int?>(value));
            }
        }

        public string SortDirection
        {
            get
            {
#pragma warning disable CS8604 // Possible null reference argument.
                return ConvertCacheBytes<string>(_cache.Get("SortDirection" + PageCacheKey));
#pragma warning restore CS8604 // Possible null reference argument.
            }
            set
            {
                _cache.Set("SortDirection" + PageCacheKey, GetCacheBytes<string>(value));
            }
        }

        public string PageCacheKey
        {
            get
            {
#pragma warning disable CS8604 // Possible null reference argument.
                return ConvertCacheBytes<string>(_cache.Get(_containerPageName));
#pragma warning restore CS8604 // Possible null reference argument.
            }
            set
            {
                //Only set it once
                _cache.Set(_containerPageName, GetCacheBytes<string>(value));
            }
        }

        private static byte[] GetCacheBytes<T>(T param)
        {
            byte[] paramBytes = Encoding.Default.GetBytes(JsonSerializer.Serialize<T>(param));
            return paramBytes;
        }

        private static T ConvertCacheBytes<T>(byte[] param)
        {
            if (param != null)
            {
#pragma warning disable CS8603 // Possible null reference return.
                return JsonSerializer.Deserialize<T>(param);
#pragma warning restore CS8603 // Possible null reference return.
            }
            else
#pragma warning disable CS8603 // Possible null reference return.
                return default(T);
#pragma warning restore CS8603 // Possible null reference return.

        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public PageSearchData(IDistributedCache cache, string containerPageName, Guid cacheKey)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            //Singleton
            if (_cache == null)
            {
                _cache = cache;
                _containerPageName = containerPageName;

                PageCacheKey ??= cacheKey.ToString();
            }
        }

    }

    public class PageMetaDataModel<T> : IPageMetaDataModel<X.PagedList.IPagedList<T>, IPageSearchData>
    {
        private readonly IPageSearchData _pageSearchData;
        private readonly IPagedList<T> _pagedList;

        public PageMetaDataModel(IPagedList<T> pagedList, IPageSearchData pageSearchData)
        {
            this._pagedList = pagedList;
            this._pageSearchData = pageSearchData;
        }

        public IPageSearchData PageSearchData { get { return this._pageSearchData; } }

        public IPagedList<T> PagedList { get { return this._pagedList; } }

    }
}
