using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // store all information about all players in game
    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();

    public GameObject localPlayerPrefab;
    public GameObject playerPrefab;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            // We only allow one instance of this class to exist.
            // Destroy current instance if different from the current one.
            Debug.Log("Another instance already exists. Destroy this one.");
            Destroy(this);
        }
    }

    public void SpawnPlayer(int _id, string _username, Vector3 _position, Quaternion _rotation)
    {
        GameObject _player;

        
        if(_id == Client.instance.myId)
        {
            // create player for client
            _player = Instantiate(localPlayerPrefab, _position, _rotation);
        }
        else
        {
            // create player for another client
            _player = Instantiate(playerPrefab, _position, _rotation);
        }

        _player.GetComponent<PlayerManager>().id = _id;
        _player.GetComponent<PlayerManager>().username = _username;

        // record the player instance in the players dictionary
        players.Add(_id, _player.GetComponent<PlayerManager>());

    }
}
