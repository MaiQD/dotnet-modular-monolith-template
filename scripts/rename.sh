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

# Replace namespaces and product name across files
rg -l "dotFitness" | xargs sed -i  -e "s/dotFitness/${NEW_NS}/g"
rg -l "dotFitness.WorkoutTracker" | xargs sed -i  -e "s/dotFitness.WorkoutTracker/${NEW_SOLUTION_NAME}/g"

# Rename solution directory
if [[ -d "src/${OLD_SOLUTION_DIR}" && "${OLD_SOLUTION_DIR}" != "${NEW_SOLUTION_NAME}" ]]; then
  git mv "src/${OLD_SOLUTION_DIR}" "src/${NEW_SOLUTION_NAME}" || mv "src/${OLD_SOLUTION_DIR}" "src/${NEW_SOLUTION_NAME}"
fi

echo "Renamed namespaces to ${NEW_NS} and solution to ${NEW_SOLUTION_NAME}. Review .sln and .csproj for any missed spots."
