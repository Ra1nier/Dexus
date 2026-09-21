#!/usr/bin/env bash

set -Eeuo pipefail

readonly SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"

IMAGE_NAME="${IMAGE_NAME:-dexus}"
IMAGE_TAG="${IMAGE_TAG:-latest}"
CONTAINER_NAME="${CONTAINER_NAME:-dexus}"
HOST_PORT="${HOST_PORT:-5001}"
CONTAINER_PORT="${CONTAINER_PORT:-8080}"
BIND_ADDRESS="${BIND_ADDRESS:-192.168.86.36}"
ENV_FILE="${ENV_FILE:-.env}"
RESTART_POLICY="${RESTART_POLICY:-unless-stopped}"
PIDS_LIMIT="${PIDS_LIMIT:-256}"
MANIFEST_VOLUME="${MANIFEST_VOLUME:-${CONTAINER_NAME}-manifest-data}"
DATA_PROTECTION_VOLUME="${DATA_PROTECTION_VOLUME:-${CONTAINER_NAME}-data-protection}"

RUN_AFTER_BUILD=false
RUN_ONLY=false

usage() {
    cat <<'EOF'
Usage: ./build.sh [OPTION]

Build the Dexus Docker image using its containerized .NET SDK stage.

Options:
  -r, --run       Build, then run the image
      --run-only  Run the existing image without rebuilding it
  -h, --help      Show this help text

Run configuration can be overridden with environment variables:
  IMAGE_NAME, IMAGE_TAG, CONTAINER_NAME, HOST_PORT, CONTAINER_PORT,
  BIND_ADDRESS, ENV_FILE, RESTART_POLICY, PIDS_LIMIT,
  MANIFEST_VOLUME, and DATA_PROTECTION_VOLUME.

Examples:
  ./build.sh
  ./build.sh -r
  ./build.sh --run-only
  HOST_PORT=8088 IMAGE_TAG=production ./build.sh -r
EOF
}

fail() {
    printf 'Error: %s\n' "$*" >&2
    exit 1
}

require_command() {
    command -v "$1" >/dev/null 2>&1 || fail "Required command '$1' was not found."
}

absolute_path() {
    if [[ "$1" = /* ]]; then
        printf '%s\n' "$1"
    else
        printf '%s/%s\n' "$SCRIPT_DIR" "$1"
    fi
}

build_image() {
    require_command docker

    printf '\n==> Building Docker image %s:%s\n' "$IMAGE_NAME" "$IMAGE_TAG"
    docker build \
        --tag "${IMAGE_NAME}:${IMAGE_TAG}" \
        "$SCRIPT_DIR"
}

run_image() {
    require_command docker

    local env_path
    local env_mode
    env_path="$(absolute_path "$ENV_FILE")"

    [[ -f "$env_path" ]] || fail "Environment file not found at '$env_path'. Copy .env.example to .env and supply your deployment settings."
    if command -v stat >/dev/null 2>&1; then
        env_mode="$(stat -c '%a' "$env_path" 2>/dev/null || true)"
        if [[ "$env_mode" =~ ^[0-7]+$ ]] && (( (8#$env_mode & 077) != 0 )); then
            fail "'$env_path' is readable by other users. Protect it with: chmod 600 '$env_path'"
        fi
    fi
    if rg -q 'replace-with-|dexus\.example\.com' "$env_path" 2>/dev/null \
        || grep -Eq 'replace-with-|dexus\.example\.com' "$env_path"; then
        fail "'$env_path' still contains placeholder values. Supply real deployment settings first."
    fi
    for required_setting in \
        AllowedHosts \
        Dexus__KnownProxies__0; do
        grep -Eq "^[[:space:]]*${required_setting}=.+$" "$env_path" \
            || fail "Required setting '$required_setting' is missing from '$env_path'."
    done
    if grep -Eq '^[[:space:]]*AllowedHosts=.*\*' "$env_path"; then
        fail "AllowedHosts must contain explicit deployment hostnames, not '*'."
    fi
    docker image inspect "${IMAGE_NAME}:${IMAGE_TAG}" >/dev/null 2>&1 || \
        fail "Docker image '${IMAGE_NAME}:${IMAGE_TAG}' does not exist. Run ./build.sh first."

    if docker container inspect "$CONTAINER_NAME" >/dev/null 2>&1; then
        printf '\n==> Replacing existing container %s\n' "$CONTAINER_NAME"
        docker container rm --force "$CONTAINER_NAME" >/dev/null
    fi

    printf '\n==> Starting %s at http://%s:%s\n' "$CONTAINER_NAME" "$BIND_ADDRESS" "$HOST_PORT"
    docker run --detach \
        --name "$CONTAINER_NAME" \
        --restart "$RESTART_POLICY" \
        --read-only \
        --cap-drop ALL \
        --security-opt no-new-privileges=true \
        --pids-limit "$PIDS_LIMIT" \
        --tmpfs /tmp:rw,noexec,nosuid,nodev,size=64m \
        --env-file "$env_path" \
        --publish "${BIND_ADDRESS}:${HOST_PORT}:${CONTAINER_PORT}" \
        --volume "${MANIFEST_VOLUME}:/app/RequestManager/Manifest" \
        --volume "${DATA_PROTECTION_VOLUME}:/home/app/.aspnet/DataProtection-Keys" \
        "${IMAGE_NAME}:${IMAGE_TAG}"

    printf '\nDexus is running. Follow its logs with:\n  docker logs --follow %q\n' "$CONTAINER_NAME"
}

while (($# > 0)); do
    case "$1" in
        -r|--run)
            RUN_AFTER_BUILD=true
            ;;
        --run-only)
            RUN_ONLY=true
            ;;
        -h|--help)
            usage
            exit 0
            ;;
        *)
            usage >&2
            fail "Unknown option '$1'."
            ;;
    esac
    shift
done

if [[ "$RUN_AFTER_BUILD" == true && "$RUN_ONLY" == true ]]; then
    fail "--run and --run-only cannot be used together."
fi

cd -- "$SCRIPT_DIR"

if [[ "$RUN_ONLY" == true ]]; then
    run_image
else
    build_image

    if [[ "$RUN_AFTER_BUILD" == true ]]; then
        run_image
    else
        printf '\nBuild complete. Start the image later with:\n  ./build.sh --run-only\n'
    fi
fi
