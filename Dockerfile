# Build
FROM alpine:latest as build
RUN apk add dotnet7-sdk
WORKDIR /airroute
COPY . .
RUN dotnet publish ./Console -c Release -o /build

# Production
FROM alpine:latest as production

RUN apk add dotnet7-sdk
WORKDIR /app
COPY --from=build /build /app

CMD ["./AirRouteConsole"]