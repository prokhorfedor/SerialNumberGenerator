#!/bin/bash
dotnet run --project SerialNumberGeneratorAPI/SerialNumberGeneratorAPI.csproj --launch-profile 'SerialNumberGenerator' &
PID=$!
wait $PID
open 'http://localhost:53348/swagger/index.html'