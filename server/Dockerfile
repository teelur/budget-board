FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
COPY . ./app

WORKDIR /app
RUN dotnet restore "BudgetBoard.WebAPI/BudgetBoard.WebAPI.csproj"
ARG configuration=Release
RUN dotnet publish "BudgetBoard.WebAPI/BudgetBoard.WebAPI.csproj" -c $configuration -o /app/publish /p:UseAppHost=false --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
RUN for i in 1 2 3; do \
		apt-get update -o Acquire::ForceIPv4=true -o Acquire::Retries=3 && break || sleep 2; \
	done \
	&& apt-get install -y --no-install-recommends -o Acquire::ForceIPv4=true -o Acquire::Retries=3 curl \
	&& rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BudgetBoard.WebAPI.dll"]
