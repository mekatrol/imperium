### Package checks

`dotnet list package --vulnerable --include-transitive --source https://api.nuget.org/v3/index.json`
`dotnet list package --deprecated --include-transitive --source https://api.nuget.org/v3/index.json`
`dotnet list package --outdated   --include-transitive --source https://api.nuget.org/v3/index.json`

## Code format checks

> Check but don't fix  
> `dotnet format --verify-no-changes --verbosity diagnostic`

> Check and fix  
> `dotnet format --verbosity diagnostic`
