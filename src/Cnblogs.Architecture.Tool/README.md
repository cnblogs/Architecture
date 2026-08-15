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
| `--base-url` | Bake this base URL into the generated `AddXxxService` extensions (otherwise each takes a `baseUri` argument). |
| `--api-version` | Emit only endpoints declared for this API version (e.g. `--api-version 2`), as one un-suffixed `IXxxService` per group. |
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
- Command-as-body payloads: when the generic `MapPostCommand<T>` / `MapPutCommand<T>` form is used (so the command
  itself is the request body), the generator emits a payload POCO (e.g. `CreateBlogCommand` → `CreateBlogPayload`)
  mirroring the command's settable properties, and uses it as the body type. This keeps the generated client from
  referencing the command's (Application-layer) assembly. Delegate-form bodies (a separate DTO) are referenced as-is.
- Mixed route-scalar + body signatures.
- Nullable-route expansion (`MapNullableRouteParameter.Enable`) collapsed into a single method that substitutes
  `"-"` for missing values.
- Route-group API-version tokens (`{version:apiVersion}`) substituted with the endpoint's declared API version
  (falling back to `1` for endpoints without version metadata).

## Grouping

By default, commands are grouped by their `TError` (e.g. `VipError` → `VipService`, `StoreError` → `StoreService`),
and queries join the command group that shares their first route segment. Override with an explicit tag on a route
group:

```csharp
v1.MapGroup("/api/v1/store").WithServiceAgentGroup("Store");
```

A group with conflicting error types, or two groups resolving to the same name, are reported as errors.

## Multiple API versions

When the API registers versioned endpoints (`.HasApiVersion(...)` / `[ApiVersion]`), each endpoint's declared
versions are exported to the manifest, and the `{version:apiVersion}` route token is stamped with the endpoint's
own version instead of a hard-coded one.

- **Default (no option):** all endpoints are emitted. A group whose endpoints span several API versions is split
  into one agent per version — e.g. `IAccusationV1Service` + `IAccusationV2Service` — each calling its own
  `/api/v1/...` / `/api/v2/...` routes. A group within a single version keeps the un-suffixed name.
- **`--api-version 2`:** only endpoints declaring version 2 (or carrying no version metadata) are emitted, as one
  un-suffixed `IAccusationService`; endpoints of other versions are dropped with a warning.

## Limitations

- Endpoints whose route tokens have no matching parameter are skipped with a warning (e.g. route-bound paging
  like `articles/page:{pageIndex}-{pageSize}`).
- Two endpoints that produce identical method signatures (same name and parameter types) keep the first and skip
  the rest with a warning — disambiguate with `WithServiceAgentGroup` or a distinct route.
- `DELETE` commands with a request body are not supported (the base class has no such helper).
- A generated payload POCO mirrors only the command's own settable properties. A property whose type lives in the
  command's assembly is still referenced (full decoupling would require recursive POCO generation). `[JsonPropertyName]`
  is not carried through, so property names rely on the default (case-insensitive) serialization matching.
