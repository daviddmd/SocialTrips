﻿namespace BackendAPI.Entities.Enums
{
    public enum EventType
    {
        GROUP_CREATE=0,
        GROUP_USER_ENTER=1,
        GROUP_USER_ENTER_INVITE=2,
        GROUP_USER_LEAVE=3,
        GROUP_INVITE_CREATE=4,
        GROUP_INVITE_REJECT=5,
        GROUP_DETAILS_UPDATE=6,
        GROUP_USER_ROLE_CHANGE=7,
        GROUP_DELETE=8,
        GROUP_USER_BAN=17,
        GROUP_USER_UNBAN=18,

        TRIP_CREATE=9,
        TRIP_DETAILS_UPDATE=10,
        TRIP_USER_ENTER=11,
        TRIP_USER_ENTER_INVITE=12,
        TRIP_USER_LEAVE=13,
        TRIP_INVITE_CREATE=14,
        TRIP_INVITE_REJECT=15,
        TRIP_DELETE=16
    }
}
