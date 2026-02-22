using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTheme", menuName = "ScriptableObjects/ThemeData")]
public class ThemeStageData : ScriptableObject
{
    public string themeName;
    [Header("필드 설정")]
    public GameObject fieldPrefab; // 필드로 쓸 큰 맵 하나

    [Header("던전 설정")]
    public MapPiece startMap;      // 던전 시작 방
    public List<MapPiece> battleMaps; // 던전 중간 방들
    public MapPiece endMap;        // 던전 끝 방

    [Header("필드 설정")]
    public List<EnemyData> fieldEnemies; // 필드에서 스폰될 약한 몬스터들
}