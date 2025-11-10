using System.Net;
using System.Net.WebSockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using MemoryPack;

namespace TheProject;

/*
 * Goals of this networking implementation:
 * RPC: A procedure can have n parameters and a return value.
 *      The parameter and return value are individually binary serialized/deserialized
 * Support large values, therefor we need some kind of message interleaving
 * Sending Objects to the client, and the server knowing that these objects where send, to that it can send updates once these objects change.
 * Do we want / need some kind of signals system that works across the network?
 * Do we always send the entire objects with all it's fields, or can we send single fields?
 */

//Messages Types: Request, Response, Notification

public enum MessageType : byte
{
    Request = 0,
    Response = 1,
    Notification = 2
}

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

                //this is just prototype code, int the future we want to do this without reflection...
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

public static class UnsafeAccessors<T>
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_items")]
    public static extern ref T[] GetBackingArray(List<T> list);
}

public class Networking
{
    public async Task ListenForConnections()
    {
        var listener = new HttpListener();
        listener.Prefixes.Add("http://localhost/connect");
        listener.Start();

        while (true)
        {
            var context = await listener.GetContextAsync();
            if (context.Request.IsWebSocketRequest)
            {
                var wsContext = await context.AcceptWebSocketAsync(subProtocol: null);

                //client connected


            }
            else
            {
                context.Response.StatusCode = 400;
                context.Response.Close();
            }
        }
    }

    public static void SendMessage(WebSocket webSocket, Memory<byte> input)
    {
         webSocket.SendAsync(input, WebSocketMessageType.Binary, true, CancellationToken.None);
    }

    public static async Task<ReadOnlyMemory<byte>> GetNextMessage(WebSocket webSocket)
    {
        if (webSocket.State != WebSocketState.Open)
            throw new Exception("This method should only be called as long as the websocket is open");

        var buffer = new byte[4096];

        var messageBuffer = new List<byte>();

        WebSocketReceiveResult result;
        do
        {
            result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
            messageBuffer.AddRange(buffer.AsSpan(result.Count));
        }
        while (!result.EndOfMessage);

        Memory<byte> arr = UnsafeAccessors<byte>.GetBackingArray(messageBuffer).AsMemory(0, messageBuffer.Count);

        return arr;
    }
}