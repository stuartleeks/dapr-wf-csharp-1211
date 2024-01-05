#!/bin/bash
set -e

# Start new workflow
echo "Starting new workflow"
resp=$(curl \
    --silent \
    --include \
    --request POST \
    --url http://localhost:8100/workflows \
    --header 'content-type: application/json' \
    --data "$(cat -)"
    )

# Get location header and body from response
head=true
location=""
while read -r line; do 
    if $head; then 
        if [[ $line = $'\r' ]]; then
            head=false
        else
            # if line starts with "Location:" extract remainder
            if [[ $line == Location:* ]]; then
                # extract Location header value and trim newlines
                location=$(echo "${line#Location: }"# | tr -d '\n\r')
            fi
        fi
    else
        body="$body"$'\n'"$line"
    fi
done < <(echo "$resp")

instance_id=$(echo "$body" | jq -r .instanceId)
if [[ -z $instance_id ]]; then
    echo "instanceId not set in result: $resp"
    exit 1
fi
if [[ -z $location ]]; then
    echo "location not set in result: $resp"
    exit 1
fi

echo "Waiting for workflow to complete..."
while :
do
    clear
    echo "Refreshing workflow status..."
    resp=$(curl \
        --silent \
        --request GET \
        --url "http://localhost:8100${location}" )
    status=$(echo "$resp" | jq -r .status)
    if [[ "$status" != "Running" ]]; then
        echo "Status: $status - done"
        break
    fi
    echo "$resp" | jq
    sleep 2
done

echo "Workflow complete"
echo "$resp" | jq
