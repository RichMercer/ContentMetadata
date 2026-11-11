using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Telligent.Evolution.Extensibility;
using Telligent.Evolution.Extensibility.Rest.Version2;
using HttpMethod = Telligent.Evolution.Extensibility.Rest.Version2.HttpMethod;

namespace ContentMetadata.Api;

public class RestApi : IRestEndpoints, IRestScopeGroups
{
	// Verint Community 14 only lets callers reach endpoints whose scopes they were granted.
	// Without a scope assignment, endpoints fall into the uncategorised "noscope" group.
	private const string ScopeGroupId = "contentmetadata";
	private const string ScopeEntityId = "metadata";

	public void RegisterScopeGroups(IRestScopeGroupRegistrationController controller)
	{
		controller.Add(ScopeGroupId, () => "Content Metadata", () => "Read and change metadata stored against content.",
			new List<RestScopeEntity> { new(ScopeEntityId, () => "Metadata") });
	}

	private static RestRouteCreateOptions Scoped() => new()
	{
		ScopeEntityAssignments = new List<RestScopeEntityAssignment> { new(ScopeGroupId, ScopeEntityId) }
	};

	public string Description => "Allows access to IContent MetaData from widgets.";

	public string Name => "ContentMetadata REST API";

	public void Initialize()
	{
	}

	public void Register(IRestEndpointController controller)
	{
		#region GET

		controller.Add(2, "metadata/{ContentId}", HttpMethod.Get, req =>
		{
			var response = new RestResponse { Name = "ContentMetadata" };

			try
			{
				var param = req.PathParameters["ContentId"];
				if (param == null || !Guid.TryParse(param.ToString(), out var contentId))
					throw new ArgumentException("ContentId is required.");

				var key = GetParameter(req, "Key") ?? string.Empty;

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
		}, Scoped());

		controller.Add(2, "findmetadata/{Key}", HttpMethod.Get, req =>
		{
			var response = new RestResponse { Name = "ContentMetadata" };

			try
			{
				var dataKey = req.PathParameters["Key"].ToString() ?? string.Empty;
				if (string.IsNullOrEmpty(dataKey))
					throw new ArgumentException("Key is required");

				var dataValue = GetParameter(req, "Value") ?? string.Empty;
				if (string.IsNullOrEmpty(dataValue))
					throw new ArgumentException("Value is required");

				Guid.TryParse(GetParameter(req, "ContentTypeId") ?? string.Empty, out var contentTypeId);

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
		}, Scoped());


		#endregion

		#region POST

		controller.Add(2, "metadata", HttpMethod.Post, req =>
		{
			var response = new RestResponse { Name = "ContentMetadata" };

			try
			{
				if (!Guid.TryParse(GetParameter(req, "ContentId") ?? string.Empty, out var contentId))
					throw new ArgumentException("ContentId is required.");

				if (!Guid.TryParse(GetParameter(req, "ContentTypeId") ?? string.Empty, out var contentTypeId))
					throw new ArgumentException("ContentTypeId is required.");

				var key = GetParameter(req, "Key") ?? string.Empty;
				if (string.IsNullOrEmpty(key))
					throw new ArgumentException("Key is required.");

				var value = GetParameter(req, "Value") ?? string.Empty;
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
		}, Scoped());


		#endregion

		#region DELETE

		controller.Add(2, "metadata", HttpMethod.Delete, req =>
		{
			var response = new RestResponse { Name = "ContentMetadata" };

			try
			{
				if (!Guid.TryParse(GetParameter(req, "ContentId") ?? string.Empty, out var contentId))
					throw new ArgumentException("ContentId is required.");

				var key = GetParameter(req, "Key") ?? string.Empty;
				if (string.IsNullOrEmpty(key))
					throw new ArgumentException("Key is required.");

				Apis.Get<IContentMetadataApi>().Delete(contentId, key);
			}
			catch (Exception ex)
			{
				response.Errors = new[] { ex.Message };
			}

			return response;
		}, Scoped());

		#endregion
	}

	/// <summary>
	/// Reads a value from the query string, falling back to the form body. Replaces
	/// HttpRequest.Params, which no longer exists on Verint Community 14.
	/// </summary>
	private static string GetParameter(IRestRequest req, string name)
	{
		var value = req.Request.Query.GetFirst(name, null);
		if (value != null || string.Equals(req.Request.Method, "GET", StringComparison.OrdinalIgnoreCase))
			return value;

		return req.Form?.GetFirst(name, null);
	}
}

[XmlRoot(ElementName = "Response")]
public class RestResponse : IRestResponse
{
	public object Data { get; set; }
	public string[] Errors { get; set; }
	public string Name { get; set; }
}