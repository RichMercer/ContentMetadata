﻿using Telligent.Evolution.Extensibility.Api;

namespace ContentMetadata.Api;

public class InProcessApi : IApiDefinition
{
    public string Name => "ContentMetadata API Loader";

    public string Description => "Exposes ContentMetadata APIs to the API service";

    public void Initialize()
    {
    }

    public void RegisterApi(IApiController controller)
    {
        controller.Add(new ContentMetadataApi());
    }
}