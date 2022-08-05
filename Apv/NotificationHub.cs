using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;

namespace Apv
{
    public class NotificationHub : Hub
    {

        public void KickUser(string IdUser, string SessionID)
        {
            Clients.All.Kickuser(IdUser, SessionID);
        }
        public void SendNotif(string userId, int data)
        {
            Clients.User(userId).send(data);
        }

    }
}