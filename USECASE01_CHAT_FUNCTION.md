
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
