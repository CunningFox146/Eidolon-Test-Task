using EidolonTestTask.Analytics;
using TMPro;
using UnityEngine;

namespace EidolonTestTask.UI
{
    public class MessageForm : MonoBehaviour
    {
        [SerializeField] private EventService _eventService;
        [SerializeField] private TMP_InputField _data;
        [SerializeField] private TMP_InputField _type;

        public void Send()
        {
            _eventService.TrackEvent(_type.text, _data.text);
        }
    }
}