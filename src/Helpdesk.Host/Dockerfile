# Build context is the repository root (see docker-compose.yaml).
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first, from project files only, so the layer is cached until a .csproj changes.
COPY Directory.Build.props ./
COPY src/Helpdesk.SharedKernel/Helpdesk.SharedKernel.csproj src/Helpdesk.SharedKernel/
COPY src/Modules/Tickets/Helpdesk.Tickets.Domain/Helpdesk.Tickets.Domain.csproj src/Modules/Tickets/Helpdesk.Tickets.Domain/
COPY src/Modules/Tickets/Helpdesk.Tickets.Application/Helpdesk.Tickets.Application.csproj src/Modules/Tickets/Helpdesk.Tickets.Application/
COPY src/Modules/Tickets/Helpdesk.Tickets.Infrastructure/Helpdesk.Tickets.Infrastructure.csproj src/Modules/Tickets/Helpdesk.Tickets.Infrastructure/
COPY src/Modules/Tickets/Helpdesk.Tickets.Presentation/Helpdesk.Tickets.Presentation.csproj src/Modules/Tickets/Helpdesk.Tickets.Presentation/
COPY src/Helpdesk.Host/Helpdesk.Host.csproj src/Helpdesk.Host/
RUN dotnet restore src/Helpdesk.Host/Helpdesk.Host.csproj

COPY src/ src/
RUN dotnet publish src/Helpdesk.Host/Helpdesk.Host.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Helpdesk.Host.dll"]
