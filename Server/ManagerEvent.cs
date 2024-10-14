using Share;
using Photon.SocketServer;
using System.Collections.Generic;

namespace Mafia_Server
{   
    /// <summary>
    /// менеджер событий
    /// </summary>
    public class ManagerEvent
    {
        public static ManagerEvent Inst;

        public ManagerEvent()
        {
            Inst = this;
        }

        private void SendEvent(EventData eventData, List<Client> UserList)
        {
            SendParameters sendParameters = new SendParameters();
            eventData.SendTo(UserList, sendParameters);
        }

       
        //public void Event_Ray( List<Client> Event_Clients, Vector3 Start_Point, Vector3 End_Point, Item User_Item = null, int Item_ID = 0 )
        //{
        //    EventData Atack_Data = new EventData((byte)Events.Atack);
        //    SendParameters sendParameters = new SendParameters();

        //    Atack_Data.Parameters = new Dictionary<byte, object> { };

        //   

        //    Atack_Data.SendTo(Event_Clients, sendParameters);
        //}
    }
}
