using UnityEngine;

namespace ClashOfClans.Cores
{
    public class DataManager : MonoSingleton<DataManager>
    {
        [SerializeField] private EventData eventData;
        
        public EventData EventData => eventData;
    }
}
