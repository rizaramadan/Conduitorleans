# ![RealWorld Example App](logo.png)

> ### [Microsoft Orleans](https://dotnet.github.io/orleans/) codebase containing real world examples (CRUD, auth, advanced patterns, etc) that adheres to the [RealWorld](https://github.com/gothinkster/realworld) spec and API.


### [Demo](https://github.com/gothinkster/realworld)&nbsp;&nbsp;&nbsp;&nbsp;[RealWorld](https://github.com/gothinkster/realworld)

This **Work-In-Progress** codebase was created to demonstrate a fully fledged fullstack application built with [Microsoft Orleans](https://dotnet.github.io/orleans/) including CRUD operations, authentication, routing, pagination, and more.

We've gone to great lengths to adhere to the [Microsoft Orleans](https://dotnet.github.io/orleans/) community styleguides & best practices.

For more information on how to this works with other frontends/backends, head over to the [RealWorld](https://github.com/gothinkster/realworld) repo.


# How it works
This Web app is combining ASP.NET Core Web API and [Microsoft Orleans](https://dotnet.github.io/orleans/). 
ASP.NET Core is used as the Orleans Clients, so we need an Orleans Server and that is what [/src/SiloHost](https://github.com/rizaramadan/Conduitorleans/tree/main/src/SiloHost)
is for. 

For the ASP.NET Core Web API, some notable packages used (beside Orleans):
1. [Fluent validation](https://github.com/FluentValidation/FluentValidation)
2. [Microsoft JWT Bearer](https://github.com/aspnet/Security/tree/master/src/Microsoft.AspNetCore.Authentication.JwtBearer)
3. [Npgsql](https://www.npgsql.org) since we are using PostgreSQL as the database
4. [Swashbuckle for ASP.NET Core](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)
5. [Feature folder](https://github.com/OdeToCode/AddFeatureFolders)

For The Orleans implementation, some notes:
1. Postgresql is used for both AdoNet Clustering and Grains Persistent provider
2. Related to grains persistent provider, the **payloadjson** column of **Orleansstorage** table is changed from default TEXT to [JSONB](https://github.com/rizaramadan/Conduitorleans/blob/31d0abe5243349a402ece63acc9f8cf61a7dc69d/scripts/conduitorleans_all.sql#L491)
3. [Npgsql](https://www.npgsql.org) for direct query to database

In the code base, we also try to adopt practice from Golang language regarding method return value being Error
is one of it. See https://golang.org/doc/tutorial/handle-errors. In C#, tuple is used to replicate similar practice.
This might be changed in the future, like removed altogether, but currently we are still unsure to continue or stop.
Leave it like this for the time being.

For more in-depth and complete example of Orleans sample, please look at [OneBoxDeployment](https://github.com/dotnet/orleans/tree/master/Samples/OneBoxDeployment)
or [road-to-orleans](https://github.com/PiotrJustyna/road-to-orleans).

# Getting Started
to have this up and running, steps are as follow:
1. clone this repository
2. prepare the database, as instructed in [/scripts/README.md](https://github.com/rizaramadan/Conduitorleans/blob/main/scripts/README.md)
3. build the solution
4. start [/src/SiloHost](https://github.com/rizaramadan/Conduitorleans/tree/main/src/SiloHost) project first, after its up and running
5. start the [/src/Conduit](https://github.com/rizaramadan/Conduitorleans/tree/main/src/Conduit)

### Footnote
Copy many codes from [an awesome conduit implementation using ASP.NET Core](https://github.com/gothinkster/aspnetcore-realworld-example-app).
Please check that repository, many things done right and much to learn from it.