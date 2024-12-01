namespace Model
{
    public enum ErrorType : int
    {
        NEED_REGISTER = 0,
        NEED_LOGIN = 1,
        NEED_UPDATE_NICKNAME = 2,
        NEED_CONNECT_TO_MASTER = 3,
        NEED_CONNECT_TO_LOBBY = 4,
        ALREADY_CONNECTED = 5,
        NOT_CONNECTED = 6,
        NAME_EXISTS = 7,
        IN_ROOM = 8,
        NOT_IN_ROOM = 9,
        DOES_NOT_EXIST = 10,
        NO_FREE_SPACE = 11,
        BLOCKED = 12
    }
}