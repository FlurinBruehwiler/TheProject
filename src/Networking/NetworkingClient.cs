using System.Net.WebSockets;
using System.Reflection;
using System.Runtime.InteropServices;
using MemoryPack;

namespace Networking;

public static class NetworkingClient
{
    public static async Task ProcessMessagesForWebSocket(WebSocket webSocket, object messageHandler)
    {
        while (webSocket.State == WebSocketState.Open)
        {
            var messageContent = (await Networking.GetNextMessage(webSocket)).Span;

            var binaryReader = new BinaryReader
            {
                Data = messageContent,
            };

            var messageType = (MessageType)binaryReader.ReadByte();

            if (messageType == MessageType.Request || messageType == MessageType.Notification)
            {
                var requestId = binaryReader.ReadGuid();
                var procedureName = binaryReader.ReadUtf16String();
                var argCount = binaryReader.ReadByte();

                //this is just prototype code, in the future we want to do this without reflection...
                var method = messageHandler.GetType().GetMethod(procedureName.ToString(), BindingFlags.Public | BindingFlags.Instance);
                if (method != null)
                {
                    var parameters = method.GetParameters();
                    if (parameters.Length == argCount)
                    {
                        var parameterObjects = new object?[argCount];

                        for (int i = 0; i < argCount; i++)
                        {
                            var offset = binaryReader.ReadInt32();
                            var length = binaryReader.ReadInt32();
                            var parameterData = messageContent.Slice(offset, length);

                            var paramType = parameters[i];
                            var paraObj = MemoryPackSerializer.Deserialize(paramType.ParameterType, parameterData, MemoryPackSerializerOptions.Default);
                            parameterObjects[i] = paraObj;
                        }

                        var returnValue = method.Invoke(messageHandler, parameterObjects);

                        //if it is a notification, we don't send a response back
                        if (messageType == MessageType.Request)
                        {
                            var res = MemoryPackSerializer.Serialize(returnValue, MemoryPackSerializerOptions.Default);
                            byte[] response = new byte[res.Length + 1 + 4];
                            response[0] = (byte)MessageType.Response;

                            MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref requestId, 1)).CopyTo(response.AsSpan(1));
                            res.AsSpan().CopyTo(response.AsSpan(5));

                            Networking.SendMessage(webSocket, response);
                        }
                    }
                }
            }
            else if (messageType == MessageType.Response)
            {
                // var requestId = binaryReader.ReadGuid();
                // binaryReader.Data.AsSlice();
                //
                // MemoryPackSerializer.Deserialize();
            }
            else
            {
                Console.WriteLine($"Invalid message type {messageType}");
            }

            //get message header (
        }
    }
}