#! /bin/bash
source /etc/profile
cd /home/ubuntu/identityserver
pm2 start "dotnet settl.identityserver.API.dll" --name identityserver --time