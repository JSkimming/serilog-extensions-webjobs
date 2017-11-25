FROM microsoft/aspnetcore-build:2.0.0

ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true

WORKDIR /work

# Copy just the solution and proj files to make best use of docker image caching
COPY ./serilog-extensions-webjobs.sln .
COPY ./src/Serilog.Extensions.WebJobs/Serilog.Extensions.WebJobs.csproj ./src/Serilog.Extensions.WebJobs/Serilog.Extensions.WebJobs.csproj
COPY ./test/Serilog.Extensions.WebJobs.Tests/Serilog.Extensions.WebJobs.Tests.csproj ./test/Serilog.Extensions.WebJobs.Tests/Serilog.Extensions.WebJobs.Tests.csproj

# Run restore on just the project files, this should cache the image after restore.
RUN dotnet restore

COPY . .

# Build to ensure the tests are their own distinct step
RUN dotnet build -f netcoreapp2.0 -c Debug ./test/Serilog.Extensions.WebJobs.Tests/Serilog.Extensions.WebJobs.Tests.csproj

# Run unit tests
RUN dotnet test --no-restore --no-build -c Debug -f netcoreapp2.0 test/Serilog.Extensions.WebJobs.Tests/Serilog.Extensions.WebJobs.Tests.csproj
