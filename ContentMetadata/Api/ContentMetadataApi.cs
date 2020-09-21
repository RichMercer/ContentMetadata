using System;
using System.Collections.Generic;
using System.Linq;
using ContentMetadata.Data;
using Telligent.Evolution.Extensibility;
using Telligent.Evolution.Extensibility.Api.Version1;

namespace ContentMetadata.Api
{
	public class ContentMetadataApi : IContentMetadataApi
	{
		private const string CacheKey = "ContentMetadata-ContentId:{0}";

		public IReadOnlyList<ContentMetadata> List(Guid contentId)
		{
			{
				return CacheHelper.Get(string.Format(CacheKey, contentId), () => DataService.List(contentId));
			}
		}

		public ContentMetadata Get(Guid contentId, string key)
		{
			var item = List(contentId).FirstOrDefault(x => x.Key == key);
			return item ?? new ContentMetadata();
		}


		public IReadOnlyList<ContentMetadata> List(string key, string value)
		{
			return  DataService.ListContent(key,value);
		}

		public ContentMetadata Get(Guid contentTypeId, string key, string value)
		{
			var item = List(key,value).FirstOrDefault(x => x.ContentTypeId == contentTypeId);
			return item ?? new ContentMetadata();
		}

		/// <summary>
		/// Deletes all metadata associated with the IContent.
		/// </summary>
		/// <param name="contentId">The contentId to delete metadata for.</param>
		public void Delete(Guid contentId)
		{
			DataService.Delete(contentId);
		}

		/// <summary>
		/// Deletes an item of metadata associated with an IContent with the relevant key.
		/// </summary>
		/// <param name="contentId">The IContent Id the metadata belongs to.</param>
		/// <param name="key">The key of the metadata to delete.</param>
		public void Delete(Guid contentId, string key)
		{
			DataService.Delete(contentId, key);

			CacheHelper.Remove(string.Format(CacheKey, contentId));
		}

		public ContentMetadata Set(Guid contentId, Guid contentTypeId, string key, string value)
		{
			var item = DataService.Set(contentId, contentTypeId, key, value);
			CacheHelper.Remove(string.Format(CacheKey, contentId));

			return item;

		}
	}
}
