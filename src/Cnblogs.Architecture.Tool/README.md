# dotnet-cnb

Tooling for [Cnblogs.Architecture](https://github.com/cnblogs/Architecture), installed as the `dotnet cnb` command.

Currently ships one module — **`serviceagent`** — which generates strongly-typed CQRS service agents
(`IXxxService` / `XxxService`) from an API project that registers endpoints with `MapQuery` / `MapCommand`. The
generated agents derive from `CqrsServiceAgent<TError>` and call its helpers (`GetItemAsync`,
`ListPagedItemsAsync`, `PostCommandAsync`, …), replacing what is otherwise hand-written, drift-prone client code.

This mirrors the EF Core design-time model: a **design package** installed in the API project exports the real
endpoint surface, and this **global tool** turns it into code.

## Install

```bash
dotnet tool install -g dotnet-cnb
```

This provides the `dotnet cnb` command.

## Prerequisites

The API project must reference the design package so the exporter can run at design time:

```bash
dotnet add <api-project> package Cnblogs.Architecture.ServiceAgent.Design
```

The exporter is a no-op unless a generation run is active, so adding the package has no effect on normal runs.

## Usage

```bash
dotnet cnb serviceagent generate \
  --api-project ./src/MyApi \
  --output ./src/MyApi.ServiceAgent \
  --namespace MyApi.ServiceAgent
```

Options:

| Option | Description |
| --- | --- |
| `--api-project` | Path to the API `.csproj` or its directory. |
| `--output` | Directory to write the generated `.cs` files into (the client project). |
| `--namespace` | Namespace for the generated types. |
| `--clean` | Remove previously generated files in `--output` before writing. |

The client project must reference `Cnblogs.Architecture.Ddd.Cqrs.ServiceAgent` (the base class + `AddServiceAgent`)
and the assemblies that hold the request/response DTO and error types.

## What it generates

For each group (by default, one per distinct error type):

- `I{Name}Service.cs` + `{Name}Service.cs` — a `partial` interface / class pair deriving from
  `CqrsServiceAgent<TError>`, with one method per endpoint.
- `ServiceAgentExtensions.cs` — `AddServiceAgents(IServiceCollection, string baseUri)` registering every agent as a
  typed `HttpClient`.

Endpoint shapes handled:

- Queries: single item (`GetItemAsync<T>`), list (`ListItemsAsync<List<T>>`), paged
  (`ListPagedItemsAsync<T>` with `pageIndex` / `pageSize` / `orderByString`), and `HEAD` companions
  (`Has{X}Async` via `HasItemAsync`).
- Commands: `POST` / `PUT` / `DELETE`, with or without a body payload and with or without a result.
- Delegate-form endpoints where the wire payload differs from the command.
- Mixed route-scalar + body signatures.
- Nullable-route expansion (`MapNullableRouteParameter.Enable`) collapsed into a single method that substitutes
  `"-"` for missing values.
- Route-group API-version tokens (`{version:apiVersion}`) substituted with the configured version (default `1`).

## Grouping

By default, commands are grouped by their `TError` (e.g. `VipError` → `VipService`, `StoreError` → `StoreService`),
and queries join the command group that shares their first route segment. Override with an explicit tag on a route
group:

```csharp
v1.MapGroup("/api/v1/store").WithServiceAgentGroup("Store");
```

A group with conflicting error types, or two groups resolving to the same name, are reported as errors.

## Limitations

- Endpoints whose route tokens have no matching parameter are skipped with a warning (e.g. route-bound paging
  like `articles/page:{pageIndex}-{pageSize}`).
- Two endpoints that produce identical method signatures (same name and parameter types) keep the first and skip
  the rest with a warning — disambiguate with `WithServiceAgentGroup` or a distinct route.
- `DELETE` commands with a request body are not supported (the base class has no such helper).
