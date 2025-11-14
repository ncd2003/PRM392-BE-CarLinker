
### <a name="_36i8ahh2p18g"></a>1.1 UC-01\_Send Message

#### <a name="_l22fl9ccqg1"></a>*a. Functionalities*

|UC ID and Name:|UC-01\_Send Message|||
| -: | :- | :- | :- |
|Created By:|HuyMT|Date Created:|13/10/2025|
|Primary Actor:|**Customer / Garage Staff**|Secondary Actors:|System Backend, Cloudflare Storage (for media), Admin / Garage Manager|
|Trigger:|A user (Customer or Staff) opens a chat room and sends a text or media message.|||
|Description:|As a user (Customer or Staff), I want to send messages (text or media) within a chat room so that I can communicate effectively with the other party regarding garage services.|||
|Preconditions:|<p>PRE-1: The user is logged in and authenticated.<br>PRE-2: A chat room exists between the customer and the garage.</p><p>PRE-3: The user has permission to send messages in this chat room.</p><p>PRE-4 (if media): The file to be sent is successfully uploaded to Cloudflare Storage.</p>|||
|Postconditions:|<p>POST-1: The message is saved in the ChatMessage table with correct sender\_type, sender\_id, message\_type, and status = active.</p><p>POST-2: If it is a media message, file\_url is stored in the database.</p><p>POST-3: ChatRoom.last\_message\_at is updated.</p><p>POST-4: The message is pushed in real-time to all participants in the chat room.</p><p>POST-5: The system logs the message for audit purposes.</p>|||
|Normal Flow|<p>NF1: The user opens a chat room in the mobile app.</p><p>NF2: The user types a text message or selects a file to send.</p><p>NF3: (For media) The app uploads the file to Cloudflare Storage and receives a file\_url.</p><p>NF4: The app calls the backend API /chat/messages with the message text or file\_url.</p><p>NF5: The backend validates the sender’s permission and message content.</p><p>NF6: The backend saves the message in the ChatMessage table.</p><p>NF7: The backend updates ChatRoom.last\_message\_at.</p><p>NF8: The backend pushes the message via real-time service (SignalR/WebSocket or Cloudflare Realtime Database) to all participants.</p><p>NF9: Recipients receive and display the message in their chat interface.</p>|||
|<p>Alternative Flo</p><p>ws:</p>|<p>AF1: Media Upload Fails</p><p>- Scenario: File upload to Cloudflare fails.</p><p>- Action: The system returns an error to the sender, and the message is not saved.</p><p>AF2: Invalid Message Content</p><p>- Scenario: The message is empty or contains prohibited content.</p><p>- Action: The system rejects the message and prompts the user to correct it.</p>|||
|Exceptions:|<p>- E1: Network/connection failure → the message is not sent; the system retries or prompts the user.</p><p>- E2: Backend database error → the message is not saved; the system logs the error and alerts the user.</p><p>- E3: User session timeout → the system requests the user to re-login.</p>|||
|Priority:|High — Essential for customer-staff communication in garage operations.|||
|Frequency of Use:|Multiple times daily — every chat session between customers and garage staff involves sending messages.|||
|Business Rules:|<p>BR1: Only participants of the chat room can send messages.</p><p>BR2: Media messages must have a valid file\_url before saving.</p><p>BR3: The message status is initially set to active.</p>|||
|Other Information:|<p>Supports both text and media messages.</p><p>Real-time updates ensure smooth and immediate communication.</p>|||
|Assumptions:|<p>A1: The backend supports concurrent writes and real-time push notifications.</p><p>A2: The mobile app handles offline retries for failed message sends.</p>|||

####

### <a name="_ixqel78qtc0n"></a><a name="_d70nx39cy9c3"></a>1.2 UC-02\_Receive & Read Messages Realtime

#### <a name="_213mxbgtw5jc"></a>*a. Functionalities*

|UC ID and Name:|UC-02\_Receive & Read Messages Realtime|||
| -: | :- | :- | :- |
|Created By:|HuyMT|Date Created:|13/10/2025|
|Primary Actor:|Customer / Garage Staff|Secondary Actors:|System Backend, SignalR/WebSocket or Cloudflare Realtime Database, Admin / Garage Manager|
|Trigger:|A new message is sent in a chat room by another participant.|||
|Description:|As a user (Customer or Staff), I want to receive messages in real-time and have them marked as read automatically or manually so that I can stay up-to-date with ongoing conversations without missing any messages.|||
|Preconditions:|<p>PRE-1: The user is logged in and authenticated.</p><p>PRE-2: The user is a participant of the chat room.</p><p>PRE-3: A message has been sent by another participant in the same chat room.</p>|||
|Postconditions:|<p>POST-1: The message is displayed in the chat interface in real-time.</p><p>POST-2: The message’s is\_read status is updated when the user views it.</p><p>POST-3: Realtime read status is propagated to other participants if applicable.</p><p>POST-4: System logs the read event for audit purposes.</p>|||
|Normal Flow|<p>NF1: The user has the chat room open in the app or receives a push notification.</p><p>NF2: Another participant sends a new message in the chat room.</p><p>NF3: The backend pushes the new message in real-time to all connected participants via SignalR/WebSocket or Cloudflare Realtime Database.</p><p>NF4: The user’s app receives the message and displays it in the chat interface.</p><p>NF5: If the user views the message, the app sends a “read” acknowledgment to the backend.</p><p>NF6: The backend updates ChatMessage.is\_read = true for this user and optionally notifies other participants.</p>|||
|<p>Alternative Flo</p><p>ws:</p>|<p>AF1: User Offline</p><p>- Scenario: User is offline when the message is sent.</p><p>- Action: The message is stored in the backend; upon reconnection, the app retrieves unread messages.</p><p>AF2: Notification Failure</p><p>- Scenario: Push notification cannot be delivered.</p><p>- Action: The app syncs messages upon next app launch or reconnection.</p>|||
|Exceptions:|<p>E1: Network/connection failure → messages may be delayed; system retries delivery.</p><p>E2: Backend database error → read status not updated; system logs the error.</p><p>E3: Session timeout → user must re-login to receive messages.</p>|||
|Priority:|High — Ensures timely communication between customers and garage staff.|||
|Frequency of Use:|Continuous — every active chat session involves receiving messages in real-time.|||
|Business Rules:|<p>BR1: Only participants of a chat room receive messages for that room.</p><p>BR2: is\_read status must be accurate for each participant.</p><p>BR3: Messages must be delivered in the order they were sent.</p>|||
|Other Information:|<p>Supports real-time display on multiple devices per user.</p><p>Works for both text and media messages.</p>|||
|Assumptions:|<p>A1: The backend supports real-time push mechanisms.</p><p>A2: Mobile app handles reconnection and sync of missed messages.</p>|||

### <a name="_pem7nstp6cyl"></a>1.3 UC-03\_Edit / Hide Message

#### <a name="_98riaot9issj"></a>*a. Functionalities*

|UC ID and Name:|UC-03\_View My Bookings|||
| -: | :- | :- | :- |
|Created By:|HuyMT|Date Created:|13/10/2025|
|Primary Actor:|Customer|Secondary Actors:|System Backend, Admin / Garage Manager|
|Trigger:|A user decides to edit or hide a message they have previously sent in a chat room.|||
|Description:|As a user (Customer or Staff), I want to edit or hide my previously sent messages so that I can correct mistakes, update content, or remove messages from the conversation without losing data for audit purposes.|||
|Preconditions:|<p>PRE-1: The user is logged in and authenticated.</p><p>PRE-2: The user is the original sender of the message.</p><p>PRE-3: The message exists in the chat room and is not permanently deleted.</p><p>PRE-4: User has permission to edit/hide messages.</p>|||
|Postconditions:|<p>POST-1: The message content is updated if edited.</p><p>POST-2: The message status is updated (edited if edited, hidden if hidden).</p><p>POST-3: Recipients see the updated message content or a placeholder indicating the message is hidden.</p><p>POST-4: System logs the edit or hide action for auditing purposes.</p>|||
|Normal Flow|<p>NF1: User opens the chat room and identifies the message to edit or hide.</p><p>NF2: User selects “Edit” or “Hide” option.</p><p>NF3: If “Edit”, user modifies the message content.</p><p>NF4: App calls backend API /chat/messages/{id} with updated content or hide request.</p><p>NF5: Backend validates the user’s permission and message existence.</p><p>NF6: Backend updates the ChatMessage record:</p><p>- If edited → update message field and set status = edited.</p><p>- If hidden → set status = hidden.</p><p>NF7: Backend pushes the update in real-time to all participants in the chat room.</p><p>NF8: Participants’ apps refresh the message display to reflect the edit or hidden status.</p>|||
|<p>Alternative Flo</p><p>ws:</p>|<p>AF1: Attempt to Edit Expired or Hidden Message</p><p>- Scenario: User tries to edit a message that is already hidden or past an allowed edit time.</p><p>- Action: System rejects the request and displays an error.</p><p>AF2: Invalid Edit Content</p><p>- Scenario: User submits empty content or forbidden words.</p><p>- Action: System rejects the edit and prompts the user to correct it.</p>|||
|Exceptions:|<p>E1: Network failure → edit/hide request fails; system retries or prompts user.</p><p>E2: Database error → message not updated; system logs error and alerts user.</p><p>E3: Session timeout → user must re-login to perform edit/hide.</p>|||
|Priority:|High — Supports accurate communication and user control over sent messages.|||
|Frequency of Use:|Moderate — Users occasionally edit or hide messages during active chat sessions.|||
|Business Rules:|<p>BR1: Only the original sender can edit or hide their message.</p><p>BR2: Hidden messages remain in the database for audit but are not visible to participants.</p><p>BR3: Message edits must be timestamped (updated\_at) for tracking.</p>|||
|Other Information:|<p>Supports both text and media messages (media cannot be partially edited; hiding applies to the whole message).</p><p>Real-time updates ensure all participants see the change immediately.</p>|||
|Assumptions:|<p>A1: Backend supports real-time updates for message edits/hides.</p><p>A2: Mobile app handles UI updates correctly when messages are edited or hidden.</p>|||

####

### <a name="_w5cchg95wbdr"></a><a name="_w6p4pkk0eru7"></a>1.4 UC-04\_Manage Chat Room & Participants

#### <a name="_nvnf0fxqcza4"></a>***a. Functionalities***

|UC ID and Name:|UC-04\_Manage Chat Room & Participants|||
| -: | :- | :- | :- |
|Created By:|HuyMT|Date Created:|13/10/2025|
|Primary Actor:|Garage Staff / Garage Manager / Admin|Secondary Actors:|System Backend, Customer, ChatRoomMember Table|
|Trigger:|A staff member, manager, or admin wants to manage chat room participants or monitor chat rooms.|||
|Description:|As a staff member, garage manager, or admin, I want to manage chat rooms and participants so that I can ensure the right staff have access, monitor conversations, and maintain smooth communication between customers and garage staff.|||
|Preconditions:|<p>PRE-1: The user is logged in and authenticated.</p><p>PRE-2: The user has the necessary role (STAFF, MANAGER, or ADMIN) to manage chat rooms.</p><p>PRE-3: Chat rooms exist for the garage and/or customers.</p><p>PRE-4: Staff has permission to add/remove participants from chat rooms or monitor messages.</p>|||
|Postconditions:|<p>POST-1: ChatRoomMember table is updated with added or removed staff participants.</p><p>POST-2: Staff added to a room can view and send messages; staff removed lose access.</p><p>POST-3: Admin/Manager can view chat history and monitor ongoing conversations.</p><p>POST-4: System logs all participant changes for auditing.</p>|||
|Normal Flow|<p>NF1: The user opens the chat management module in the garage dashboard.</p><p>NF2: The system displays a list of chat rooms for the garage.</p><p>NF3: User selects a chat room to manage participants.</p><p>NF4: User adds or removes staff members to/from the chat room.</p><p>NF5: Backend validates the user’s permission to modify participants.</p><p>NF6: Backend updates the ChatRoomMember table accordingly.</p><p>NF7: Backend pushes realtime updates to all participants about membership changes.</p><p>NF8: Admin/Manager can view all messages in the chat room for monitoring purposes.</p>|||
|Alternative <br>Flows:|N/A|||
|Exceptions:|<p>E1: Network failure → changes not applied; system retries or prompts user.</p><p>E2: Database error → participant update fails; system logs error and alerts user</p><p>E3: Session timeout → user must re-login to manage chat room.</p>|||
|Priority:|High — Ensures proper access control and monitoring for multi-staff garage operations.|||
|Frequency of Use:|Daily / As needed — Staff and managers manage participants when team changes or monitoring is required.|||
|Business Rules:|<p>BR1: Only staff assigned to the garage can be added to chat rooms.</p><p>BR2: Admin/Manager can monitor all chat rooms of all garages.</p><p>BR3: All changes to participants are logged with timestamps.</p><p>BR4: Customers cannot be manually added/removed; each chat room is tied to a single customer.</p>|||
|Other Information:|<p>Supports multi-staff collaboration in a single chat room.</p><p>Real-time updates ensure all participants are aware of changes immediately.</p>|||
|Assumptions:|<p>A1: Backend supports real-time updates for participant changes.</p><p>A2: Staff UI displays membership and message access clearly.</p>|||

####

###

|<a name="_b1qu1t54ugyr"></a><a name="_693ajlkewmdl"></a>ProjectCode – Final Report|Page  / 10|
| :- | -: |
