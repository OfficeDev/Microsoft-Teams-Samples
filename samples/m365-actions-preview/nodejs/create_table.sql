CREATE TABLE Todo
(
    id INT IDENTITY PRIMARY KEY,
    description NVARCHAR(128) NOT NULL,
    objectId NVARCHAR(36),
    itemId NVARCHAR(128),
    channelOrChatId NVARCHAR(128),
    isCompleted TinyInt NOT NULL default 0,
)
