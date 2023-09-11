#!/usr/bin/env sh

# fail if a command fails
set -e
set -o pipefail

# set rx to all directories
find "$APP_DIR" -type d -exec chmod 500 {} +

if [ -z "$1" ]
then
  # set r to all files
  find "$APP_DIR" -type f -exec chmod 400 {} +
else
  # set r to all files excluding executable binary
  find "$APP_DIR" ! -name $1 -type f -exec chmod 400 {} +
fi
