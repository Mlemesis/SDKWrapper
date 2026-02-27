 using System;
 using System.Collections.Generic;
 using CentralTech.CTSystemsBase;

 namespace CentralTech.CTEventSystem
 {
     public interface IEvent
     {
     }

     public interface IEventSystem : IGenericSystem
     {
         void RegisterEvent<T>(EventSystem.EventCallBack eventCallBack) where T : IEvent;
         void UnregisterEvent<T>(EventSystem.EventCallBack eventCallBack) where T : IEvent;
         void TriggerEvent(IEvent eventObject);
     }

     public class EventSystem : IEventSystem
     {
         public delegate void EventCallBack(IEvent eventObject);

         Dictionary<Type, List<EventCallBack>> registeredEvents = new();

         public void RegisterEvent<T>(EventCallBack eventCallBack) where T : IEvent
         {
             Type type = typeof(T);
             if (!registeredEvents.ContainsKey(type))
             {
                 List<EventCallBack> eventCallBacks = new();
                 eventCallBacks.Add(eventCallBack);
                 registeredEvents.Add(type, eventCallBacks);
             }
             else
             {
                 registeredEvents[type].Add(eventCallBack);
             }
         }

         public void UnregisterEvent<T>(EventCallBack eventCallBack) where T : IEvent
         {
             Type type = typeof(T);
             if (registeredEvents.ContainsKey(type))
             {
                 registeredEvents[type].Remove(eventCallBack);
                 if (registeredEvents[type].Count == 0)
                 {
                     registeredEvents.Remove(type);
                 }
             }
         }

         public void TriggerEvent(IEvent eventObject)
         {
             Type type = eventObject.GetType();
             if (registeredEvents.TryGetValue(type, out List<EventCallBack> eventsList))
             {
                 for (int i = 0; i < eventsList.Count; i++)
                 {
                     eventsList[i].Invoke(eventObject);
                 }
             }
         }

         public Type Interface => typeof(IEventSystem);

         public void Destroy()
         {
             registeredEvents.Clear();
         }
     }
 }