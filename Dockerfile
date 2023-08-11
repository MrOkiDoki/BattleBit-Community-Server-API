# This is a multi-stage Dockerfile, in this case it means that
# we use a "heavy" SDK image with all compiler tools to build the application, but
# then only use the runtime and generated binaries when actually running the app.
# This allows us to make the resulting image much smaller in size.

# We start off by using the SDK to build the application
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# We first copy ONLY the project definition file, instead of all files at once.
# This prevents us from having to restore all dependencies (slow) every time
# we build, since every step in the Dockerfile build process is reusable. So
# if only our code changes, but our project file (incl. dependencies) does not,
# we can skip these two steps before recompiling, saving us quite some time.
COPY ["CommunityServerAPI.csproj", "./"]
RUN dotnet restore "CommunityServerAPI.csproj"

# Now we can copy all source files (except those listed in `.dockerignore`)
# and build the application
COPY [".", "./"]
RUN dotnet build "CommunityServerAPI.csproj" --no-restore -c Release -o /app


##########
# The next step is to take the built/compiled source code and package it
# into a single "publishable" binary
FROM build as publish
RUN dotnet publish "CommunityServerAPI.csproj" -c Release -o /app

#########
# Then, we start over with a completely new environment
# that just contains the .NET runtime. Here, we copy over the
# built and packaged binary files from the other environment to be able to run it.
FROM mcr.microsoft.com/dotnet/runtime:6.0 AS final
WORKDIR /app

# We expose port 29294 as the default port on which our app runs.
EXPOSE 29294

COPY --from=publish /app .

# Set the default action when the container is started, which in this case runs the API.
ENTRYPOINT [ "dotnet", "CommunityServerAPI.dll" ]