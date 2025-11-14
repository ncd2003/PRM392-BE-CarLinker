new logic are:
User table storing user, admin, garage owner. 
They will be itendify by role:
        CUSTOMER = 0,
        GARAGE = 1,
        ADMIN = 2.

1 garage owner can have many garrage. Garage will have column userId(refer to user table have role = 1) to know who is owner of garage.

about table chat_message:
public SenderType SenderType { get; set; } // CUSTOMER, STAFF, ADMIN
public int SenderId { get; set; } // References the sender's ID

SenderType:
public enum SenderType
    {
        CUSTOMER = 0, //sender id in user table
        STAFF = 1, // sender id in garage_staff table
        ADMIN = 2 // sender id in user table
    }

