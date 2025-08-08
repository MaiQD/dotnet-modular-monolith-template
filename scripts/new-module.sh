#!/usr/bin/env bash
set -euo pipefail

if [[ $# -lt 1 ]]; then
  echo "Usage: scripts/new-module.sh <ModuleName> [RootNamespace]"
  exit 1
fi

MODULE="$1"
ROOT_NS="${2:-App}"
SOLUTION_DIR=$(basename "$(ls -d src/*/ | head -n1)")
BASE="src/${SOLUTION_DIR}/Modules/${MODULE}"

mkdir -p "${BASE}/${ROOT_NS}.Modules.${MODULE}.Domain/Entities" \
         "${BASE}/${ROOT_NS}.Modules.${MODULE}.Domain/Events" \
         "${BASE}/${ROOT_NS}.Modules.${MODULE}.Domain/Repositories" \
         "${BASE}/${ROOT_NS}.Modules.${MODULE}.Application/Commands" \
         "${BASE}/${ROOT_NS}.Modules.${MODULE}.Application/Queries" \
         "${BASE}/${ROOT_NS}.Modules.${MODULE}.Application/DTOs" \
         "${BASE}/${ROOT_NS}.Modules.${MODULE}.Application/Mappers" \
         "${BASE}/${ROOT_NS}.Modules.${MODULE}.Application/Validators" \
         "${BASE}/${ROOT_NS}.Modules.${MODULE}.Infrastructure/Handlers" \
         "${BASE}/${ROOT_NS}.Modules.${MODULE}.Infrastructure/Repositories" \
         "${BASE}/${ROOT_NS}.Modules.${MODULE}.Infrastructure/Configuration"

cat > "${BASE}/${ROOT_NS}.Modules.${MODULE}.Domain/${ROOT_NS}.Modules.${MODULE}.Domain.csproj" <<CSproj
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
CSproj

cat > "${BASE}/${ROOT_NS}.Modules.${MODULE}.Application/${ROOT_NS}.Modules.${MODULE}.Application.csproj" <<CSproj
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="../../${ROOT_NS}.Modules.${MODULE}.Domain/${ROOT_NS}.Modules.${MODULE}.Domain.csproj" />
  </ItemGroup>
</Project>
CSproj

cat > "${BASE}/${ROOT_NS}.Modules.${MODULE}.Infrastructure/${ROOT_NS}.Modules.${MODULE}.Infrastructure.csproj" <<CSproj
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="../../${ROOT_NS}.Modules.${MODULE}.Application/${ROOT_NS}.Modules.${MODULE}.Application.csproj" />
  </ItemGroup>
</Project>
CSproj

echo "Created module skeleton at ${BASE}" 
