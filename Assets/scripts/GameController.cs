using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class GameController : NetworkBehaviour
{
    [SyncVar]
    [SerializeField]
    bool isRoundActive = true;

    [SerializeField]
    GameObject[] enemysPrefabs;
    [SerializeField]
    Transform[] enemySpawnPoints;
    [SerializeField]
    Transform enemyParent;

    float spawnRateEnemy = 5;
    float nextEnemySpawn = 0;




    [Server]
    void Update()
    {
    
        if (isRoundActive)
        {
            if( Time.timeSinceLevelLoad > nextEnemySpawn )
            {
                spawnEnemy();
                nextEnemySpawn = Time.timeSinceLevelLoad + spawnRateEnemy;
            }
        }
       
    }


    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkIdentity networkIdentity = GetComponent<NetworkIdentity>();
        networkIdentity.AssignClientAuthority(NetworkServer.localConnection);
    }
    private void OnCollisionEnter(Collision collision)
    {
        CmdStartGame();
    }



    [Command]
    void CmdStartGame()
    {
        Debug.Log("[GameController] Start game ");
        isRoundActive = true;
    }

    void spawnEnemy()
    {
        Debug.Log("[GameController] spawn Enemy ");
        int randomEnemy = Random.Range(0, enemysPrefabs.Length - 1);
        int spawnPos = Random.Range(0, enemySpawnPoints.Length - 1);

        GameObject enemy = Instantiate(enemysPrefabs[randomEnemy], enemySpawnPoints[spawnPos].position, enemySpawnPoints[spawnPos].rotation, enemyParent);
        NetworkServer.Spawn(enemy);
       

    }
}
