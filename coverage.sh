#!/usr/bin/env bash
# Runs all tests with code coverage and generates an HTML report.
# Usage: ./coverage.sh [--open]
set -euo pipefail
cd "$(dirname "$0")"

rm -rf TestResults

dotnet tool restore
dotnet test --collect:"XPlat Code Coverage" --results-directory TestResults
dotnet reportgenerator \
  -reports:"TestResults/**/coverage.cobertura.xml" \
  -targetdir:TestResults/CoverageReport \
  -reporttypes:Html

echo "Report: TestResults/CoverageReport/index.html"
if [[ "${1:-}" == "--open" ]]; then
  openCmd=$([ -n "${WINDIR:-}" ] && echo "start" || echo "open")
  $openCmd TestResults/CoverageReport/index.html
fi
