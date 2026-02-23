using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SessionItems : MonoBehaviour
{
    [SerializeField] private TMP_Text _sessionNameText;
    [SerializeField] private TMP_Text _playerCountText;

    private Action<string> _onJoinCallback;
    private string _sessionName;

    public void SetInfo(string sessionName, int currentPlayers, int maxPlayers, Action<string> onJoin)
    {
        _sessionName = sessionName;
        _onJoinCallback = onJoin;

        if (_sessionNameText) _sessionNameText.text = $"Sala: {sessionName}";
        if (_playerCountText) _playerCountText.text = $"{currentPlayers}/{maxPlayers}";
    }

    public void OnClick()
    {
        // Al hacer click, ejecutamos la función que nos pasó el ConnectionManager
        _onJoinCallback?.Invoke(_sessionName);
    }
}
