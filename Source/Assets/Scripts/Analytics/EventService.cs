using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace EidolonTestTask.Analytics
{
    public class EventService : MonoBehaviour
    {
        private static readonly string EventsSaveKey = "EventService.Events";

        [SerializeField] private string _serverUrl;
        [SerializeField] private float _cooldownBeforeSend;

        private Queue<AnalyticsEvent> _queuedEvenets;
        private Coroutine _sendCoroutine;
        private WaitForSeconds _sendWait;
        private CancellationToken _destroyCt;

        private AnalyticsData PostData => new AnalyticsData(_queuedEvenets);

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

        public void TrackEvent(string type, string data)
        {
            TrackEvent(new AnalyticsEvent()
            {
                type = type,
                data = data
            });
        }

        public void TrackEvent(AnalyticsEvent analyticsEvent)
        {
            _queuedEvenets.Enqueue(analyticsEvent);
            ScheduleSendingEvents();
        }

        private void LoadEvents()
        {
            var queueJson = PlayerPrefs.GetString(EventsSaveKey);
            if (string.IsNullOrEmpty(queueJson) || queueJson == "null")
            {
                _queuedEvenets = new();
                return;
            };

            try
            {
                _queuedEvenets = JsonConvert.DeserializeObject<Queue<AnalyticsEvent>>(queueJson);
                SendEvents();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private void SaveCurrentEvents()
        {
            string data = _queuedEvenets.Count == 0 ? null : JsonConvert.SerializeObject(_queuedEvenets);
            PlayerPrefs.SetString(EventsSaveKey, data);
            PlayerPrefs.Save();
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

            string postData = JsonConvert.SerializeObject(PostData);

            var request = new UnityWebRequest(_serverUrl, UnityWebRequest.kHttpVerbPOST);
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(postData));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            try
            {
                await request.SendWebRequest().WithCancellation(_destroyCt);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return;
            }

            if (request.responseCode != 200)
            {
                throw new Exception($"Failed to post events. Code {request.responseCode}: {request.error}");
            }

            Debug.Log($"Events posted! Response: {request.downloadHandler.text}");

            _queuedEvenets.Clear();
        }
    }
}