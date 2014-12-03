start Agent\bin\Debug\Agent.exe
ping 192.0.2.2 -n 1 -w 3000 > nul
start FinalServer\bin\Debug\Cloud.exe
ping 192.0.2.3 -n 1 -w 4000 > nul
start FinalClient\bin\Debug\Router.exe 127.0.1.1 false
ping 192.0.2.4 -n 1 -w 4000 > nul
start FinalClient\bin\Debug\Router.exe 127.0.1.2 false
ping 192.0.2.5 -n 1 -w 4000 > nul
start FinalClient\bin\Debug\Router.exe 127.0.1.3 false
ping 192.0.2.6 -n 1 -w 4000 > nul
start FinalClient\bin\Debug\Router.exe 127.0.1.4 false
ping 192.0.2.7 -n 1 -w 4000 > nul
start FinalClient\bin\Debug\Router.exe 127.0.1.5 false
ping 192.0.2.8 -n 1 -w 7000 > nul
start User\bin\Debug\Client.exe 127.0.0.5
ping 192.0.2.9 -n 1 -w 7000 > nul
start User\bin\Debug\Client.exe 127.0.0.7
ping 192.0.2.8 -n 1 -w 7000 > nul
start User\bin\Debug\Client.exe 127.0.0.4
ping 192.0.2.9 -n 1 -w 7000 > nul
start User\bin\Debug\Client.exe 127.0.0.8
pause