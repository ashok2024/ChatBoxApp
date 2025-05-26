using System.Collections.Concurrent;
using System.Security.Claims;
using ChatApp.Api.Services;
using ChatApp.API.Data;
using ChatApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IUserService _userService;
        private readonly IChatService _chatService;
        private readonly IGrouupService _groupService;
        private static readonly ConcurrentDictionary<string, string> OnlineUsers = new ConcurrentDictionary<string, string>();
        public ChatHub(IUserService userService, IChatService chatService, IGrouupService groupService)
        {
            _userService = userService;
            _chatService = chatService;
            _groupService = groupService;
        }

        public override async Task OnConnectedAsync()
        {
            var userEmail = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userEmail != null)
            {
                var user = await _userService.GetUserByEmailAsync(userEmail);
                if (user != null)
                {
                    // Store the connection ID for the user
                    OnlineUsers.AddOrUpdate(userEmail, Context.ConnectionId, (key, oldValue) => Context.ConnectionId);
                    await _userService.UpdateUserConnectionIdAsync(userEmail, Context.ConnectionId);

                    // Notify all clients about the newly connected user
                    await Clients.All.SendAsync("UserConnected", userEmail, user.DisplayName);

                    var groups = await _groupService.GetGroupsForUserAsync(userEmail);
                    foreach (var group in groups)
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, $"Group_{group.Id}");
                    }
                }
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userEmail = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userEmail != null)
            {
                // Remove the connection ID for the user
                OnlineUsers.TryRemove(userEmail, out string? removedConnectionId);
                await _userService.UpdateUserConnectionIdAsync(userEmail, null); // Clear connection ID

                // Notify all clients about the disconnected user
                await Clients.All.SendAsync("UserDisconnected", userEmail);
            }
            await base.OnDisconnectedAsync(exception);
        }

        // Public chat message (optional, you can remove if only direct messaging)
        public async Task SendMessage(string messageText)
        {
            var senderEmail = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var senderDisplayName = (await _userService.GetUserByEmailAsync(senderEmail))?.DisplayName;

            if (senderEmail != null && senderDisplayName != null)
            {
                var chatMessage = new Message
                {
                    SenderEmail = senderEmail,
                    SenderDisplayName = senderDisplayName,
                    Text = messageText,
                    ReceiverEmail = null // Public message
                };

                await _chatService.SaveMessageAsync(chatMessage);

                // Broadcast to all connected clients (including sender)
                await Clients.All.SendAsync("ReceiveMessage", senderEmail, senderDisplayName, messageText, chatMessage.SentAt, null);
            }
        }

        public async Task SendDirectMessage(string receiverEmail, string messageText)
        {
            var senderEmail = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var senderDisplayName = (await _userService.GetUserByEmailAsync(senderEmail))?.DisplayName;
            var receiverDisplayName = (await _userService.GetUserByEmailAsync(receiverEmail))?.DisplayName;

            if (senderEmail == null || senderDisplayName == null || receiverDisplayName == null)
            {
                // Optionally send an error message back to the client
                await Clients.Caller.SendAsync("ReceiveError", "Could not send message: Invalid sender or receiver.");
                return;
            }

            var chatMessage = new Message
            {
                SenderEmail = senderEmail,
                SenderDisplayName = senderDisplayName,
                ReceiverEmail = receiverEmail,
                Text = messageText,
                SentAt = DateTime.UtcNow
            };

            await _chatService.SaveMessageAsync(chatMessage);

            // Send to sender (to update their own chat)
            await Clients.Caller.SendAsync("ReceiveMessage", senderEmail, senderDisplayName, messageText, chatMessage.SentAt, receiverEmail);

            // Send to receiver if online
            if (OnlineUsers.TryGetValue(receiverEmail, out string? receiverConnectionId) && receiverConnectionId != null)
            {
                await Clients.Client(receiverConnectionId).SendAsync("ReceiveMessage", senderEmail, senderDisplayName, messageText, chatMessage.SentAt, receiverEmail);
            }
            else
            {
                // Receiver is offline, you might want to notify sender or log this
                Console.WriteLine($"User {receiverEmail} is offline. Message saved but not sent in real-time.");
            }
        }

        public async Task GetDirectMessagesHistory(string otherUserEmail)
        {
            var currentUserEmail = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserEmail == null) return;

            var messages = await _chatService.GetDirectMessagesAsync(currentUserEmail, otherUserEmail);
            await Clients.Caller.SendAsync("LoadMessages", messages);
        }

        // Optional: Method to get public chat history
        public async Task GetPublicMessagesHistory()
        {
            var messages = await _chatService.GetPublicMessagesAsync();
            await Clients.Caller.SendAsync("LoadMessages", messages);
        }
        public async Task SendGroupMessage(int groupId, string messageText)
        {
            var senderEmail = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (senderEmail == null) return;

            var senderDisplayName = (await _userService.GetUserByEmailAsync(senderEmail))?.DisplayName;
            if (senderDisplayName == null) return;

            var group = await _groupService.GetGroupByIdAsync(groupId);
            if (group == null)
            {
                await Clients.Caller.SendAsync("ReceiveError", "Group does not exist.");
                return;
            }

            var chatMessage = new Message
            {
                SenderEmail = senderEmail,
                SenderDisplayName = senderDisplayName,
                GroupId = groupId,
                Text = messageText,
                SentAt = DateTime.UtcNow
            };

            await _chatService.SaveMessageAsync(chatMessage);

            await Clients.Group($"Group_{groupId}")
                .SendAsync("ReceiveGroupMessage", chatMessage);
        }
        public async Task GetGroupMessagesHistory(int groupId)
        {
            var currentUserEmail = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserEmail == null) return;

            var isMember = await _groupService.IsUserInGroupAsync(currentUserEmail, groupId);
            if (!isMember)
            {
                await Clients.Caller.SendAsync("ReceiveError", "You are not a member of this group.");
                return;
            }

            var messages = await _chatService.GetGroupMessagesAsync(groupId);
            await Clients.Caller.SendAsync("ReceiveGroupMessage", messages);
        }
    }
}
