using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
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
            //if (string.IsNullOrEmpty(queueJson) || queueJson == "null")
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

            var request = new UnityWebRequest(_serverUrl, UnityWebRequest.kHttpVerbPOST);
            request.uploadHandler = new UploadHandlerRaw(new System.Text.UTF8Encoding().GetBytes(@"[{""type"":""123"",""data"":""123""}]"));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("cache-control", "no-cache");
            try
            {
                await request.SendWebRequest().WithCancellation(_destroyCt);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return;
            }
            Debug.Log(request.downloadHandler.text);
            if (request.responseCode != 200)
            {
                throw new Exception($"[EventService] Failed to post events. Code {request.responseCode}");
            }

            _queuedEvenets.Clear();
        }
    }
}