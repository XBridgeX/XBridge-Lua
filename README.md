# XBridge-Lua
基于Lua的XBridge

# 移动lua终端

由于lua找不到现成的AES库，索性自己写了一个终端
提供了websocket和AES等API

```lua

local json = require('./dkjson')
local ws = GetWebsocketClient('ws://127.0.0.1:8080','生存服务器','password')
k = ws.getK
iv = ws.getiv
ws:AddFunction('onMessage',function (s,m)
	print('['..s..'] '..m)
	local raw = json.decode(m).params.raw
	print(raw)
	print(AESDecrypt(raw,k,iv))
end)
ws:AddFunction('onOpen',function (s)
	print('服务器['..s..'] 连接成功')
end)
ws:AddFunction('onClose',function (s)
	print('服务器['..s..'] 断开连接')
end)
ws:AddFunction('onErrpr',function (s,m)
	print('服务器['..s..'] 连接异常：'..m)
end)
ws:Start()

```
