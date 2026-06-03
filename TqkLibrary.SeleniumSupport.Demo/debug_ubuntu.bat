dotnet publish -c Debug -r linux-x64 --self-contained false -o bin\publish_linux
scp -o StrictHostKeyChecking=accept-new -r .\bin\publish_linux\* tqk2811@192.168.42.130:~/demo/
pause