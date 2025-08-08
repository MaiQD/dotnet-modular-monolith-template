#!/usr/bin/env bash
set -euo pipefail

if [[ $# -lt 1 ]]; then
  echo "Usage: scripts/rename.sh <NewRootNamespace> [NewSolutionName]"
  exit 1
fi

NEW_NS="$1"
NEW_SOLUTION_NAME="${2:-$1}"

ROOT_DIR="$(pwd)"
OLD_SOLUTION_DIR=$(basename "$(ls -d src/*/ | head -n1)")

# Configure portable in-place sed args for Linux/macOS
SED_INPLACE=("-i")
if [[ "${OSTYPE:-}" == darwin* ]]; then
  SED_INPLACE=("-i" "")
fi

# Build a portable file list of likely text files to update
build_file_list() {
  find . \
    -path "./.git" -prune -o \
    -path "./**/bin" -prune -o \
    -path "./**/obj" -prune -o \
    -type f \
    \( -name "*.cs" -o -name "*.csproj" -o -name "*.sln" -o -name "*.json" -o -name "*.yml" -o -name "*.yaml" -o -name "*.http" -o -name "*.md" -o -name "*.sh" \) \
    -print0
}

# Replace solution identifier first (more specific), then root namespace
build_file_list | xargs -0 sed "${SED_INPLACE[@]}" -e "s/App.ModularMonolith/${NEW_SOLUTION_NAME}/g"
build_file_list | xargs -0 sed "${SED_INPLACE[@]}" -e "s/App/${NEW_NS}/g"

# Rename solution directory
if [[ -d "src/${OLD_SOLUTION_DIR}" && "${OLD_SOLUTION_DIR}" != "${NEW_SOLUTION_NAME}" ]]; then
  git mv "src/${OLD_SOLUTION_DIR}" "src/${NEW_SOLUTION_NAME}" || mv "src/${OLD_SOLUTION_DIR}" "src/${NEW_SOLUTION_NAME}"
fi

# Update Docker identifiers (container names, network, DB) in docker-compose.yml
# Derive a slug from the solution name for Docker resource names
SOLUTION_SLUG=$(echo "${NEW_SOLUTION_NAME}" | tr '[:upper:]' '[:lower:]' | sed -E 's/[^a-z0-9]+/-/g' | sed -E 's/^-+|-+$//g')
DB_NAME=$(echo "${SOLUTION_SLUG}" | tr '-' '_')
MONGO_DB_NAME="$(echo "${NEW_NS}" | tr '[:upper:]' '[:lower:]' | sed -E 's/[^a-z0-9]+//g')db"

# Try both old and new locations to handle rename order safely
for COMPOSE_PATH in "src/${OLD_SOLUTION_DIR}/docker-compose.yml" "src/${NEW_SOLUTION_NAME}/docker-compose.yml"; do
  if [[ -f "${COMPOSE_PATH}" ]]; then
    # container names
    sed "${SED_INPLACE[@]}" \
      -e "s/^\([[:space:]]*container_name:[[:space:]]*\).*-postgres$/\1${SOLUTION_SLUG}-postgres/" \
      -e "s/^\([[:space:]]*container_name:[[:space:]]*\).*-pgadmin$/\1${SOLUTION_SLUG}-pgadmin/" \
      -e "s/^\([[:space:]]*POSTGRES_DB:[[:space:]]*\).*/\1${DB_NAME}/" \
      -E -e "s/^([[:space:]]*-[[:space:]]*)[A-Za-z0-9_-]+-network(\s*)$/\1${SOLUTION_SLUG}-network\2/" \
      -E -e "s/^([[:space:]]+)[A-Za-z0-9_-]+-network:(\s*)$/\1${SOLUTION_SLUG}-network:\2/" \
      "${COMPOSE_PATH}"
  fi
done

echo "Renamed namespaces to ${NEW_NS} and solution to ${NEW_SOLUTION_NAME}. Review .sln and .csproj for any missed spots."

# Update application connection strings to use the new DB name (PostgreSQL)
while IFS= read -r -d '' APPSET_FILE; do
  sed "${SED_INPLACE[@]}" -E \
    -e "s/(Database=)[^;]*/\\1${DB_NAME}/g" \
    -e "s/AppDb/${MONGO_DB_NAME}/g" \
    "${APPSET_FILE}"
done < <(find src -type f -name "appsettings*.json" -print0)
