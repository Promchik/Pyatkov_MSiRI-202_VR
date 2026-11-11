using System;
using System.Collections;
using System.Globalization;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class BypassCertificate : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData) => true;
}

public class MillControl : MonoBehaviour
{
    [Header("Панель управления (UI)")]
    public MachinePanel panel;

    [Header("Управление")]
    public bool isWorking = false;

    [Header("Режим работы")]
    public bool useFakeData = true;

    [Header("Настройки тестового режима")]
    [Range(1f, 15f)] public float speedMultiplier = 5f;
    [Range(50f, 500f)] public float movementRange = 300f;
    [Range(0f, 400f)] public float spindleDownOffset = 150f;

    [Header("Настройки сети")]
    public string url = "https://kovalev.team";
    public int channel = 1;
    public float interval = 0.5f;

    [Header("Узлы станка (Transform)")]
    public Transform nodeX;
    public Transform nodeY;
    public Transform nodeZ;

    [Header("Настройка масштаба движений")]
    public float modelScaleMultiplier = 0.001f;

    private Vector3 startX, startY, startZ;
    private float fakeTime = 0f;
    private float currentX, currentY, currentZ;

    // МЕТОД ДЛЯ КНОПКИ "ПУСК"
    public void StartMachine()
    {
        isWorking = true;
        if (panel != null) panel.ShowMessage("Команда Пуск принята");
        Debug.Log("Пуск нажат");
    }

    // МЕТОД ДЛЯ КНОПКИ "СТОП"
    public void StopMachine()
    {
        isWorking = false;
        if (panel != null) panel.ShowMessage("Команда Стоп принята");
        Debug.Log("Стоп нажат");
    }

    private IEnumerator Start()
    {
        if (nodeX == null || nodeY == null || nodeZ == null)
        {
            Debug.LogError("[CNC] Перетащите узлы станка!");
            yield break;
        }

        startX = nodeX.localPosition;
        startY = nodeY.localPosition;
        startZ = nodeZ.localPosition;

        if (panel != null) panel.ShowMessage("Система готова к работе");

        while (true)
        {
            if (!isWorking)
            {
                if (panel != null) panel.UpdateUI(false, currentX, currentY, currentZ);
                yield return new WaitForSecondsRealtime(0.1f);
                continue;
            }

            if (useFakeData)
            {
                fakeTime += Time.deltaTime * speedMultiplier;

                currentX = Mathf.Sin(fakeTime) * movementRange;
                currentY = Mathf.Cos(fakeTime * 0.75f) * (movementRange * 0.7f);
                currentZ = (Mathf.Sin(fakeTime) * (movementRange * 0.5f)) - spindleDownOffset;

                ApplyMovement();
                if (panel != null)
                {
                    panel.UpdateUI(true, currentX, currentY, currentZ);
                    panel.ShowMessage("Автономный режим (OK)");
                }
            }
            else
            {
                if (panel != null) panel.ShowMessage("Запрос данных...");
                using (UnityWebRequest request = UnityWebRequest.Get(url))
                {
                    request.timeout = 10;
                    request.certificateHandler = new BypassCertificate();
                    yield return request.SendWebRequest();

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        try
                        {
                            ParseAndMove(request.downloadHandler.text);
                            if (panel != null)
                            {
                                panel.UpdateUI(isWorking, currentX, currentY, currentZ);
                                panel.ShowMessage("Связь с сервером: OK");
                            }
                        }
                        catch (Exception error)
                        {
                            if (panel != null) panel.ShowMessage("Ошибка данных: " + error.Message);
                        }
                    }
                    else
                    {
                        if (panel != null) panel.ShowMessage("Нет связи: " + request.error);
                    }
                }
            }

            yield return new WaitForSecondsRealtime(useFakeData ? 0.02f : Mathf.Max(0.02f, interval));
        }
    }

    private void ApplyMovement()
    {
        nodeX.localPosition = startX + Vector3.right * (currentX * modelScaleMultiplier);
        nodeY.localPosition = startY + Vector3.forward * (currentY * modelScaleMultiplier);
        nodeZ.localPosition = startZ + Vector3.up * (currentZ * modelScaleMultiplier);
    }

    private float GetCoordinate(JObject data, string axis)
    {
        var dataArray = data["data"] as JArray;
        if (dataArray == null) throw new FormatException("Нет массива 'data'");

        foreach (JToken section in dataArray)
        {
            var sectionArray = section as JArray;
            if (sectionArray == null || sectionArray.Count < 2) continue;
            if (sectionArray[0].ToString() != "Оси Канала " + channel) continue;

            var rows = sectionArray[1] as JArray;
            if (rows == null) continue;

            foreach (JToken row in rows)
            {
                var rowArray = row as JArray;
                if (rowArray == null || rowArray.Count < 2) continue;
                if (rowArray[0].ToString() == "Перемещение по оси " + axis)
                {
                    string text = rowArray[1].ToString().Replace("мм", "").Trim().Replace(',', '.');
                    if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
                        return value;
                }
            }
        }
        throw new FormatException($"Ось {axis} не найдена");
    }

    private void ParseAndMove(string json)
    {
        if (string.IsNullOrEmpty(json) || json.Trim().StartsWith("<")) return;
        JObject data = JObject.Parse(json);
        currentX = GetCoordinate(data, "X");
        currentY = GetCoordinate(data, "Y");
        currentZ = GetCoordinate(data, "Z");
        ApplyMovement();
    }
}