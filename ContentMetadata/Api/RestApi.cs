﻿using System;
using System.Linq;
using System.Xml.Serialization;
using Telligent.Evolution.Extensibility;
using Telligent.Evolution.Extensibility.Rest.Version2;
using HttpMethod = Telligent.Evolution.Extensibility.Rest.Version2.HttpMethod;

namespace ContentMetadata.Api;

public class RestApi : IRestEndpoints
{
	public string Description => "Allows access to IContent MetaData from widgets.";

	public string Name => "ContentMetadata REST API";

	public void Initialize()
	{
	}

	public void Register(IRestEndpointController controller)
	{
		#region GET

		controller.Add(2, "metadata/{contentid}", HttpMethod.Get, req =>
		{
			var response = new RestResponse { Name = "ContentMetadata" };

			try
			{
				var param = req.PathParameters["ContentId"];
				if (param == null || !Guid.TryParse(param.ToString(), out var contentId))
					throw new ArgumentException("ContentId is required.");

				var key = req.Request.Params["Key"] ?? string.Empty;

				if (!string.IsNullOrEmpty(key))
				{
					var item = Apis.Get<IContentMetadataApi>().Get(contentId, key);
					if (string.IsNullOrEmpty(item.Value))
					{
						response.Errors = new[] { "Content Metadata Not Found." };
					}
					else
					{
						response.Data = new RestContentMetadata(item);
					}
				}
				else
				{
					var items = Apis.Get<IContentMetadataApi>().List(contentId);
					response.Data = items.Select(x => new RestContentMetadata(x)).ToList();
				}
			}
			catch (Exception ex)
			{
				response.Errors = new[] { ex.Message };
			}
			return response;
		});

		controller.Add(2, "findmetadata/{Key}", HttpMethod.Get, req =>
		{
			var response = new RestResponse { Name = "ContentMetadata" };

			try
			{
				var dataKey = req.PathParameters["Key"].ToString() ?? string.Empty;
				if (string.IsNullOrEmpty(dataKey))
					throw new ArgumentException("Key is required");

				var dataValue = req.Request.Params["Value"] ?? string.Empty;
				if (string.IsNullOrEmpty(dataValue))
					throw new ArgumentException("Value is required");

				Guid.TryParse(req.Request.Params["ContentTypeId"] ?? string.Empty, out var contentTypeId);

				if (!string.IsNullOrEmpty(dataKey) && !string.IsNullOrEmpty(dataValue) && contentTypeId != Guid.Empty)
				{
					var item = Apis.Get<IContentMetadataApi>().Get(contentTypeId, dataKey, dataValue);
					if (string.IsNullOrEmpty(item.Value))
					{
						response.Errors = new[] { "Content Metadata Not Found." };
					}
					else
					{
						response.Data = new RestContentMetadata(item);
					}
				}
				else 
				{
					var items = Apis.Get<IContentMetadataApi>().List(dataKey, dataValue);
					response.Data = items.Select(x => new RestContentMetadata(x)).ToList(); 
				}
					

			}
			catch (Exception ex)
			{
				response.Errors = new[] { ex.Message };
			}

			return response;
		});


		#endregion

		#region POST

		controller.Add(2, "metadata", HttpMethod.Post, req =>
		{
			var response = new RestResponse { Name = "ContentMetadata" };

			try
			{
				if (!Guid.TryParse(req.Request.Params["ContentId"] ?? string.Empty, out var contentId))
					throw new ArgumentException("ContentId is required.");

				if (!Guid.TryParse(req.Request.Params["ContentTypeId"] ?? string.Empty, out var contentTypeId))
					throw new ArgumentException("ContentTypeId is required.");

				var key = req.Request.Params["Key"] ?? string.Empty;
				if (string.IsNullOrEmpty(key))
					throw new ArgumentException("Key is required.");

				var value = req.Request.Params["Value"] ?? string.Empty;
				if (string.IsNullOrEmpty(value))
					throw new ArgumentException("Value is required.");


				var item = Apis.Get<IContentMetadataApi>().Set(contentId, contentTypeId, key, value);

				response.Data = new RestContentMetadata(item);
			}
			catch (Exception ex)
			{
				response.Errors = new[] { ex.Message };
			}

			return response;
		});


		#endregion

		#region DELETE

		controller.Add(2, "metadata", HttpMethod.Delete, req =>
		{
			var response = new RestResponse { Name = "ContentMetadata" };

			try
			{
				if (!Guid.TryParse(req.Request.Params["ContentId"] ?? string.Empty, out var contentId))
					throw new ArgumentException("ContentId is required.");

				var key = req.Request.Params["Key"] ?? string.Empty;
				if (string.IsNullOrEmpty(key))
					throw new ArgumentException("Key is required.");

				Apis.Get<IContentMetadataApi>().Delete(contentId, key);
			}
			catch (Exception ex)
			{
				response.Errors = new[] { ex.Message };
			}

			return response;
		});

		#endregion
	}
}

[XmlRoot(ElementName = "Response")]
public class RestResponse : IRestResponse
{
	public object Data { get; set; }
	public string[] Errors { get; set; }
	public string Name { get; set; }
}