# ContentMetadata
ContentMetadata for Telligent Community adds support for Metadata on Telligent Community IContent entities. It was built as a replacement for ExtendedAttributes for developers who were using them to store their own custom data.
Although ExtendedAttributes have proved useful, they have many limitations. 
* They are not available on all entities (e.g. wikis, comments etc.).
* When too much data is stored against an object performance can be affected.
* Lookup based on ExtendedAttributes is not possible due to lack of API filtering.

This project seeks to solve these problem by saving metadata in its own table and exposes a full InProcess API, Widget API via Velocity extensions and a RESTful API.
