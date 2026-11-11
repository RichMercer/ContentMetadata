#!/usr/bin/env bash
set -eo pipefail

script_name="$(basename "$0")"
script_dir="$(dirname "$(readlink -f "$0")")"
eula_env_file="${script_dir}/../.devcontainer/mssql-eula.env"

show_help() {
  cat <<EOF
Usage: ${script_name} [--yes]

Creates the local SQL Server End-User License Agreement acceptance file required to start the Community stack.

Options:
  --yes    Accept without prompting
  -h       Show this help message
EOF
}

accept_without_prompt=false

while [[ $# -gt 0 ]]; do
  case "$1" in
    --yes|-y)
      accept_without_prompt=true
      shift
      ;;
    -h|--help)
      show_help
      exit 0
      ;;
    *)
      echo "Error: Unknown option '$1'." >&2
      show_help
      exit 1
      ;;
  esac
done

cat <<EOF
SQL Server for this stack is pulled from Microsoft's container registry.
Your use of SQL Server is governed by Microsoft's End-User License Agreement.

Accepting here sets a local environmental variable (ACCEPT_EULA=Y)
confirming you have a valid license for SQL Server. SQL Server runs in
the free Developer edition (MSSQL_PID=Developer) by default.

For more information:
https://hub.docker.com/r/microsoft/mssql-server#environment-variables
https://go.microsoft.com/fwlink/?linkid=857698
EOF

if [[ "${accept_without_prompt}" != "true" ]]; then
  if [[ ! -t 0 || ! -t 1 ]]; then
    echo "Error: '--yes' is required when no interactive terminal is available." >&2
    exit 1
  fi

  read -r -p "Type 'yes' to accept and continue: " response
  if [[ "${response}" != "yes" ]]; then
    echo "SQL Server End-User License Agreement acceptance was not recorded."
    exit 1
  fi
fi

mkdir -p "$(dirname "${eula_env_file}")"
printf 'ACCEPT_EULA=Y\n' > "${eula_env_file}"
echo "Created ${eula_env_file}"
