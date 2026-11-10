using System;
using System.Collections;
using System.Globalization;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class buttonmill : MonoBehaviour
{
    private class EmbeddedBypassCertificate : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        }
    }

    [Header("Режим работы")]
    [Tooltip("Включите, если server недоступен. Станок начнет двигаться автономно.")]
    public bool useFakeData = true;

    [Header("Управление из VR")]
    [Tooltip("Флаг запуска станка кнопкой VR. Если выключен — станок ждет нажатия.")]
    private bool isVRStartTriggered = false;

    [Header("Настройки тестового режима")]
    [Range(1f, 15f)] public float speedMultiplier = 5f;
    [Range(50f, 500f)] public float movementRange = 300f;
    [Tooltip("На сколько сместить шпиндель вниз относительно стартовой позиции")]
    [Range(0f, 400f)] public float spindleDownOffset = 150f;

    [Header("Настройки сети")]
    public string url = "https://kovalev.team";
    public int channel = 1;
    public float interval = 0.5f;

    [Header("Узлы станка (Transform)")]
    [Tooltip("Ось X (заполнится автоматически)")]
    public Transform nodeX;
    [Tooltip("Ось Y (заполнится автоматически)")]
    public Transform nodeY;
    [Tooltip("Ось Z (заполнится автоматически)")]
    public Transform nodeZ;

    [Header("Настройка масштаба движений")]
    [Tooltip("Если модель станка большая и движений не видно, увеличьте это число (например, до 0.01 или 0.1)")]
    public float modelScaleMultiplier = 0.001f;

    private Vector3 startX;
    private Vector3 startY;
    private Vector3 startZ;

    // ПУБЛИЧНЫЙ МЕТОД ДЛЯ ВАШЕЙ VR-КНОПКИ
    public void ActivateCncMovement()
    {
        isVRStartTriggered = true;
        Debug.Log("[CNC] VR Кнопка нажата! Станок запущен. Начинаем прием координат.");
    }

    private IEnumerator Start()
    {
        // ПОИСК ВНУТРИ ОБЪЕКТА (Ищет среди дочерних элементов)
        if (nodeX == null || nodeY == null)
        {
            // transform.Find ищет объект «внутри» того GameObject, на котором висит скрипт
            Transform foundTable = transform.Find("Куб.019");

            // Если не нашел напрямую, пробуем найти во всем дереве дочерних объектов
            if (foundTable == null)
            {
                foundTable = FindChildHelper(transform, "Куб.019");
            }

            if (foundTable != null)
            {
                if (nodeX == null) nodeX = foundTable;
                if (nodeY == null) nodeY = foundTable;
            }
        }

        if (nodeZ == null)
        {
            Transform foundSpindle = transform.Find("Куб.016");

            if (foundSpindle == null)
            {
                foundSpindle = FindChildHelper(transform, "Куб.016");
            }

            if (foundSpindle != null) nodeZ = foundSpindle;
        }

        // Проверка: если после всех поисков детали так и не нашлись
        if (nodeX == null || nodeY == null || nodeZ == null)
        {
            Debug.LogError("[CNC] Ошибка: Не удалось найти детали 'Куб.019' или 'Куб.016' внутри станка! Проверьте имена объектов в Hierarchy.");
            yield break;
        }

        // Запоминаем стартовые позиции
        startX = nodeX.localPosition;
        startY = nodeY.localPosition;
        startZ = nodeZ.localPosition;

        while (true)
        {
            if (!isVRStartTriggered)
            {
                yield return new WaitForSecondsRealtime(0.1f);
                continue;
            }

            if (useFakeData)
            {
                float time = Time.time * speedMultiplier;
                float xOffset = Mathf.Sin(time) * movementRange;
                float yOffset = Mathf.Cos(time * 0.75f) * (movementRange * 0.7f);
                float zOffset = (Mathf.Sin(time) * (movementRange * 0.5f)) - spindleDownOffset;

                nodeX.localPosition = startX + Vector3.right * (xOffset * modelScaleMultiplier);
                nodeY.localPosition = startY + Vector3.forward * (yOffset * modelScaleMultiplier);
                nodeZ.localPosition = startZ + Vector3.up * (zOffset * modelScaleMultiplier);
            }
            else
            {
                using (UnityWebRequest request = UnityWebRequest.Get(url))
                {
                    request.timeout = 10;
                    request.certificateHandler = new EmbeddedBypassCertificate();
                    yield return request.SendWebRequest();

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        try
                        {
                            MoveNodes(request.downloadHandler.text);
                        }
                        catch (Exception error)
                        {
                            Debug.LogWarning("[CNC] Ошибка парсинга данных: " + error.Message);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[CNC] Ошибка сети: " + request.error);
                    }
                }
            }

            yield return new WaitForSecondsRealtime(useFakeData ? 0.02f : Mathf.Max(0.02f, interval));
        }
    }

    // Вспомогательный метод для глубокого поиска объекта по имени во всех вложенных папках
    private Transform FindChildHelper(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform result = FindChildHelper(child, name);
            if (result != null) return result;
        }
        return null;
    }

    private float GetCoordinate(JObject data, string axis)
    {
        var dataArray = data["data"] as JArray;
        if (dataArray == null)
            throw new FormatException("В ответе сервера отсутствует массив 'data'.");

        foreach (JToken section in dataArray)
        {
            var sectionArray = section as JArray;
            if (sectionArray == null || sectionArray.Count < 2) continue;
            string sectionName = sectionArray?.ToString();
            if (sectionName != "Оси Канала " + channel) continue;
            var rows = sectionArray as JArray;
            if (rows == null) continue;

            foreach (JToken row in rows)
            {
                var rowArray = row as JArray;
                if (rowArray == null || rowArray.Count < 2) continue;
                string rowName = rowArray?.ToString();

                if (rowName == "Перемещение по оси " + axis)
                {
                    string text = rowArray?.ToString().Replace("мм", "").Trim();
                    if (string.IsNullOrEmpty(text))
                        throw new FormatException($"Пустое значение для оси {axis}");
                    text = text.Replace(',', '.');
                    if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
                    {
                        if (float.IsNaN(value) || float.IsInfinity(value))
                            throw new FormatException($"Некорректное числовое значение для оси {axis}");
                        return value;
                    }
                    throw new FormatException($"Не удалось преобразовать '{text}' в число для оси {axis}");
                }
            }
        }
        throw new FormatException($"Координата для оси {axis} в 'Оси Канала {channel}' не найдена.");
    }

    private void MoveNodes(string json)
    {
        if (string.IsNullOrEmpty(json)) return;
        if (json.Trim().StartsWith("<")) return;

        JObject data = JObject.Parse(json);

        float x = GetCoordinate(data, "X") * modelScaleMultiplier;
        float y = GetCoordinate(data, "Y") * modelScaleMultiplier;
        float z = GetCoordinate(data, "Z") * modelScaleMultiplier;

        nodeX.localPosition = startX + Vector3.right * x;
        nodeY.localPosition = startY + Vector3.forward * y;
        nodeZ.localPosition = startZ + Vector3.up * z;
    }
}