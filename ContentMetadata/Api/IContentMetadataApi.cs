using System;
using System.Collections.Generic;
using Telligent.Evolution.Extensibility.Api;

namespace ContentMetadata.Api;

public interface IContentMetadataApi : IApi
{
	IReadOnlyList<ContentMetadata> List(Guid contentId);

	IReadOnlyList<ContentMetadata> List(string key, string value);

	ContentMetadata Get(Guid contentId, string key);

	ContentMetadata Get(Guid contentTypeId, string key, string value);

	/// <summary>
	/// Deletes all metadata associated with the IContent.
	/// </summary>
	/// <param name="contentId">The contentId to delete metadata for.</param>
	void Delete(Guid contentId);

	/// <summary>
	/// Deletes an item of metadata associated with an IContent with the relevant key.
	/// </summary>
	/// <param name="contentId">The IContent Id the metadata belongs to.</param>
	/// <param name="key">The key of the metadata to delete.</param>
	void Delete(Guid contentId, string key);

	ContentMetadata Set(Guid contentId, Guid contentTypeId, string key, string value);
}