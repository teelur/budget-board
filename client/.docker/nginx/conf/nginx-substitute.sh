#!/bin/sh
# Make sure your end of line is LF. Otherwise this will cause issues.
export DOLLAR="$"
envsubst < default.conf.template > /etc/nginx/conf.d/default.conf

# Replace environment variables in projectEnvVariables.js
projectEnvVariables=$(ls -t /usr/share/nginx/html/assets/projectEnvVariables*.js | head -n1)
if [ -z "$projectEnvVariables" ] || [ ! -f "$projectEnvVariables" ]; then
    echo "Error: No projectEnvVariables*.js file found in /usr/share/nginx/html/assets/"
    exit 1
fi
envsubst < "$projectEnvVariables" > /tmp/projectEnvVariables_temp
cp /tmp/projectEnvVariables_temp "$projectEnvVariables"
rm /tmp/projectEnvVariables_temp

nginx -g "daemon off;"