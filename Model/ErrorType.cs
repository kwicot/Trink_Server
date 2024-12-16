namespace Model
{
    public enum ErrorType : int
    {
        NONE = 0,
        NEED_REGISTER = 1,
        NEED_LOGIN = 2,
        NEED_UPDATE_NICKNAME = 3,
        NEED_CONNECT_TO_MASTER = 4,
        NEED_CONNECT_TO_LOBBY = 5,
        ALREADY_CONNECTED = 6,
        NOT_CONNECTED = 7,
        NAME_EXISTS = 8,
        IN_ROOM = 9,
        NOT_IN_ROOM = 10,
        DOES_NOT_EXIST = 11,
        NO_FREE_SPACE = 12,
        BLOCKED = 13,
        NOT_ENOUGH_MONEY = 14,
        INCORRECT_RANGE = 15,
        PASSWORD_INCORECT = 16,
    }
}