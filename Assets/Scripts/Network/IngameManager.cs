using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

[System.Serializable]
struct PosRotate {
    public Vector3 pos;
    public Vector3 rotate;
}

public class IngameManager : MonoBehaviour
{
    private static IngameManager instance;
    public static IngameManager Instance
    {
        get
        {
            if (instance != null) return instance;
            instance = FindObjectOfType<IngameManager>();

            if (instance == null)
            {
                Debug.LogError("No ingame singleton");
            }
            return instance;
        }
    }

    [SerializeField] GameObject playerPrefab;
    
    // clientID: player 객체
    Dictionary<ulong, GameObject> players;

    bool isStart = false;
    [SerializeField] PosRotate[] spawnPoint;
    [SerializeField] List<PosRotate> spawnWeaponPoint;
    [SerializeField] NetworkObject[] spawnWeapons;
    Dictionary<WeaponType, NetworkObject> weaponDecors;
    Coroutine timeoutHandler;

    public ResultTextUI resultTextUI;

    private void Awake() {
        players = new();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = !Cursor.visible;
        }
    }
    
    public void PlayerLeft(ulong clientID) {
        players.Remove(clientID);
    }

    public void PlayerAdd(ulong clientID) {
        var player = Instantiate(playerPrefab);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientID);

        players[clientID] = player;

        if (isStart || players.Count <= 1) return;
        
        GameStart();
    }

    ////////////////// 게임 시작 관련
    void GameStart() {
        isStart = true;

        // 모든 사람 텔포
        int i = 0;
        foreach (var player in players)
        {
            var movement = player.Value.GetComponent<PlayerMovement>();
            var weapon = player.Value.GetComponent<PlayerWeapon>();

            movement.SetPlayerCoordsClientRpc(spawnPoint[i].pos);
            movement.SetPlayerHeadingClientRpc(spawnPoint[i].rotate);
            
            weapon.SetWeapon(WeaponType.None);
            i++;
        }

        print("게임이 시작됨니다..");
        print("준비하시고...");
    
        WaitSpawnWeapon();
    }

    void GameStop(bool timerStop = true) {
        print("GameStop");
        if (timerStop && timeoutHandler != null) {
            StopCoroutine(timeoutHandler);
            timeoutHandler = null;
        }
        
        isStart = false;
        
        foreach (var item in players)
        {
            var weapon = item.Value.GetComponent<PlayerWeapon>();
            weapon.SetWeapon(WeaponType.None);
            weapon.RestorePlayerClientRpc();
        }

        foreach (var item in weaponDecors)
            item.Value.Despawn();
        
        weaponDecors = null;
    }

    async void WaitSpawnWeapon() {
        await Task.Delay(Random.Range(3000, 15000));
        AllWeaponSpawn();
    }

    void AllWeaponSpawn() {
        print("소환!!!!!");
        ShuffleSpawnWeaponPoint(); // 스폰 포인트 섞고
        
        weaponDecors = new();
        for (int i = 0; i < spawnWeapons.Length; i++)
        {
            var spawn = spawnWeaponPoint[i];
            var weapon = Instantiate(spawnWeapons[i], spawn.pos, spawnWeapons[i].transform.rotation);
            
            WeaponType type = weapon.GetComponent<DecorObject>().GetWeaponType();
            weaponDecors[type] = weapon;

            weapon.Spawn();
        }

        timeoutHandler = StartCoroutine(TimeOutWait());
    }

    private IEnumerator TimeOutWait()
    {
        yield return new WaitForSeconds(10);
        GameStop(false);
        
        foreach (var item in players)
        {
            item.Value.GetComponent<PlayerWeapon>().ShowResultClientRpc(true, 0);
            break;
        }

        yield return new WaitForSeconds(3);
        GameStart();
    }

    private IEnumerator RetryWait()
    {
        isStart = false; // 일단 끄고
        if (timeoutHandler != null) { // 타이머 없애고
            StopCoroutine(timeoutHandler);
            timeoutHandler = null;
        }

        yield return new WaitForSeconds(3);

        GameStop();
        GameStart(); // 다시 시작
    }

    void ShuffleSpawnWeaponPoint() {
        for (int i = 0; i < spawnWeaponPoint.Count; i++)
        {
            int rand = Random.Range(0, spawnWeaponPoint.Count);
            var temp = spawnWeaponPoint[i];
            spawnWeaponPoint[i] = spawnWeaponPoint[rand];
            spawnWeaponPoint[rand] = temp;
        }
    }

    // [ServerRpc(RequireOwnership = false)]
    // public void GettingWeaponServerRpc(WeaponType type, ulong clientID) {
    //     print("GettingWeaponServerRpc isHost: " + NetworkManager.Singleton.IsHost);
    //     if (weaponDecors == null) return;

    //     print("weaponDecors OK");
    //     print("weaponDecors data " + weaponDecors.TryGetValue(type, out var _));
    //     print("players data " + players.TryGetValue(clientID, out var _));
    //     if (!weaponDecors.TryGetValue(type, out var entity) || !players.TryGetValue(clientID, out var player)) return;

    //     var weapon = player.GetComponent<PlayerWeapon>();
    //     print("player weapon " + weapon);
    //     print("player weapon2 " + weapon?.GetCurrentWeapon());
    //     if (weapon.GetCurrentWeapon() != WeaponType.None) return;
        
    //     weaponDecors.Remove(type);
        
    //     entity.Despawn();
    //     player.GetComponent<PlayerWeapon>().SetWeapon(type);
    // }

    public void GettingWeapon(WeaponType type, PlayerWeapon targetWeapon) {
        if (weaponDecors == null) return;

        if (!weaponDecors.TryGetValue(type, out var entity) || targetWeapon.GetCurrentWeapon() != WeaponType.None) return;

        weaponDecors.Remove(type);
        entity.Despawn();

        targetWeapon.SetWeapon(type);
    }

    public void PlayerAttack(ulong targetID) {
        if (!isStart) return;
        
        print(targetID + " 공격 당함!!");
        StartCoroutine(RetryWait());
        players[targetID].GetComponent<PlayerWeapon>().ShowResultClientRpc(false, targetID);
    }

    public Dictionary<ulong, GameObject> GetPlayers() => players;
}
