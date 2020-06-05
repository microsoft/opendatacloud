#!/bin/bash

echo Manual trigger for document id: $1
curl --request POST \
    -i \
    -H "Content-Type: application/json" \
    -H "Accept: application/json" \
    --data "{\"input\":\"{\\\"id\\\": \\\"$1\\\"}\"}" \
    http://localhost:7071/admin/functions/ManualEmailTrigger
