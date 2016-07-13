# TelligentContentMetadata
ContentMetadata for Telligent Community adds support for Metadata on Telligent Community IContent entities. It was built as a replacement for ExtendedAttributes for developers who were using them to store their own custom data.
Although ExtendedAttributes have proved useful, they have many limitations. 
* They are not available on all entities (e.g. wikis, comments etc.).
* When too much data is stored against an object performance can be affected.
* Lookup based on ExtendedAttributes is not possible due to lack of API filtering.

This project seeks to solve these problem by saving metadata in its own table and exposes a full InProcess API, Widget API via Velocity extensions and a RESTful API.

# Installation
To add ContentMetadata to your project, you can add it via nuget.

* `Install-Package TelligentContentMetadata`

To access the REST API and Velocity extensions, you will need to enable the plugin in your Community. The plugin uses a custom SQL table to store the custom metadata. If your sites connection string has dbowner access to your database, the install script will be run automatically when the plugin is enabled. If not, or you would prefer to run the script yourself, you can execute the SQL file here.

* [SQL Install Script](https://raw.githubusercontent.com/RichMercer/ContentMetadata/master/ContentMetadata/Resources/Sql/Install.sql)
