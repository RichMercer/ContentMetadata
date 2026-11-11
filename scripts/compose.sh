#!/usr/bin/env bash
set -eo pipefail

script_name="$(basename "$0")"
script_dir="$(dirname "$(readlink -f "$0")")"
eula_script="${script_dir}/accept-mssql-eula.sh"
eula_env_file="${script_dir}/../.devcontainer/mssql-eula.env"

show_help() {
  cat <<EOF
Usage: ${script_name} <command> [options]

Commands:
  up       Start the containers (passes -d to detach)
  down     Stop and remove the containers
  ps       List containers
  logs     View container logs
  exec     Execute a command in a running container
  Any other docker compose command

Options:
  -h       Show this help message

Examples:
  ${script_name} up
  ${script_name} down
EOF
}

ensure_mssql_eula() {
  if [[ -f "${eula_env_file}" ]]; then
    return 0
  fi

  if [[ -t 0 && -t 1 ]]; then
		echo "SQL Server for this stack is pulled from Microsoft's container registry."
		echo "Your use of SQL Server is governed by Microsoft's End-User License Agreement."
    echo
    echo "For more information:"
    echo "https://hub.docker.com/r/microsoft/mssql-server#environment-variables"
    echo "https://go.microsoft.com/fwlink/?linkid=857698"
    echo
    "${eula_script}"
    return $?
  fi

  cat >&2 <<EOF
Error: SQL Server End-User License Agreement acceptance is required before starting the stack.
Run '${eula_script}' once to create '${eula_env_file}' and confirm your acceptance.
EOF
  return 1
}

if [[ $# -eq 0 ]]; then
  echo "Error: No command specified." >&2
  show_help
  exit 1
fi

cmd=$1
if [[ "${cmd}" == "-h" ]]; then
  show_help
  exit 0
fi
shift

extra_flags=()
if [[ "${cmd}" == "up" ]]; then
  extra_flags=(-d)
fi

if [[ "${cmd}" == "up" || "${cmd}" == "start" || "${cmd}" == "restart" ]]; then
  ensure_mssql_eula
fi

if command -v docker >/dev/null 2>&1; then
    container_cmd="docker compose"
elif command -v podman >/dev/null 2>&1; then
    container_cmd="podman compose"
else
    echo "Error: Neither Docker nor Podman is available." >&2
    exit 1
fi

read -r -a container_cmd_parts <<< "${container_cmd}"

full_cmd=(
  "${container_cmd_parts[@]}"
  -f "${script_dir}/../.devcontainer/compose.yml"
  "${cmd}"
  "${extra_flags[@]}"
)

# add extra parameters if defined
if [ "$#" -gt 0 ] && [[ $* =~ [^[:space:]] ]]; then
  for arg in "$@"; do
    if [ -n "$arg" ]; then
      full_cmd+=( "$arg" )
    fi
  done
fi

echo "Executing: ${full_cmd[*]}"
exec "${full_cmd[@]}"
