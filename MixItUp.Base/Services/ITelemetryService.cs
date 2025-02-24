﻿using MixItUp.Base.Actions;
using MixItUp.Base.Commands;
using MixItUp.Base.Services.External;
using System;

namespace MixItUp.Base.Services
{
    public interface ITelemetryService : IExternalService
    {
        void TrackException(Exception ex);
        void TrackPageView(string pageName);
        void TrackLogin(string userID, string userType);
        void TrackCommand(CommandTypeEnum type, string details = null);
        void TrackAction(ActionTypeEnum type);
        void TrackService(string type);
        void TrackChannelMetrics(string type, long viewerCount, long chatterCount, string game, long viewCount, long followCount);

        void TrackRemoteAuthentication(Guid clientID);
        void TrackRemoteSendProfiles(Guid clientID);
        void TrackRemoteSendBoard(Guid clientID, Guid profileID, Guid boardID);

        void SetUserID(string userID);
    }
}