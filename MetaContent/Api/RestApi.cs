using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telligent.Evolution.Extensibility.Rest.Infrastructure.Version1;
using Telligent.Evolution.Extensibility.Rest.Version2;
using HttpMethod = Telligent.Evolution.Extensibility.Rest.Version2.HttpMethod;

namespace ContentMetadata.Api
{
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
                var response = new RestResponse { Name = "MetaData" };

                try
                {
                    Guid contentId;
                    var param = req.PathParameters["ContentId"];
                    if (param == null || !Guid.TryParse(param.ToString(), out contentId))
                        throw new ArgumentException("ContentId is required.");

                    response.Data = PublicApi.Instance.List(contentId);
                }
                catch (Exception ex)
                {
                    response.Errors = new[] { ex.Message };
                }
                return response;
            });

            controller.Add(2, "metadata/{contentid}/{key}", HttpMethod.Get, req =>
            {
                var response = new RestResponse { Name = "MetaData" };

                try
                {
                    Guid contentId;
                    var param = req.PathParameters["contentid"];
                    if (param == null || !Guid.TryParse(param.ToString(), out contentId))
                        throw new ArgumentException("ContentId is required.");

                    var key = req.PathParameters["key"]?.ToString();
                    if (key == null)
                        throw new ArgumentException("Key is required.");

                    var item = PublicApi.Instance.Get(contentId, key);
                    if (string.IsNullOrEmpty(item?.Value))
                    {
                        response.Errors = new[] { "Metadata Not Found." };
                    }
                    else
                    {
                        response.Data = item;
                    }
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
}
