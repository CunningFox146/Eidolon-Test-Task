using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace EidolonTestTask.Analytics
{
    public class EventService : MonoBehaviour
    {
        [SerializeField] private string _serverUrl;
        [SerializeField] private float _cooldownBeforeSend;

        [SerializeField] private List<AnalyticsEvent> _queuedEvenets;
        private Coroutine _sendCoroutine;
        private WaitForSeconds _sendWait;
        private CancellationToken _destroyCt;

        private string EventsJson => JsonConvert.SerializeObject(_queuedEvenets);

        private void Awake()
        {
            _sendWait = new(_cooldownBeforeSend);
            _destroyCt = this.GetCancellationTokenOnDestroy();

            LoadEvents();
        }

        private void OnApplicationQuit()
        {
            SaveCurrentEvents();
        }

        private void LoadEvents()
        {
            var queueJson = PlayerPrefs.GetString("EventService.Queue");
            if (string.IsNullOrEmpty(queueJson) || queueJson == "null")
            {
                _queuedEvenets = new();
                return;
            };

            try
            {
                _queuedEvenets = JsonConvert.DeserializeObject<List<AnalyticsEvent>>(queueJson);
                SendEvents();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private void SaveCurrentEvents()
        {
            PlayerPrefs.SetString("EventService.Queue", _queuedEvenets.Count == 0 ? null : EventsJson);
            PlayerPrefs.Save();
        }

        public void TrackEvent(AnalyticsEvent analyticsEvent)
        {
            _queuedEvenets.Add(analyticsEvent);
            ScheduleSendingEvents();
        }

        private void ScheduleSendingEvents()
        {
            if (_sendCoroutine is not null)
            {
                StopCoroutine(_sendCoroutine);
                _sendCoroutine = null;
            }
            _sendCoroutine = StartCoroutine(SendEventsCoroutine());
        }

        private IEnumerator SendEventsCoroutine()
        {
            yield return _sendWait;
            SendEvents();
        }

        private async void SendEvents()
        {
            if (_queuedEvenets.Count == 0) return;

            var request = UnityWebRequest.Post(_serverUrl, _queuedEvenets.ToString());
            await request.SendWebRequest().WithCancellation(_destroyCt);
            if (request.responseCode != 200)
            {
                throw new Exception($"[EventService] Failed to post events. Code {request.responseCode}");
            }
            _queuedEvenets.Clear();
        }
    }
}