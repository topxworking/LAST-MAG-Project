// ============================================================
// SceneSetupHelper.cs
// Drop this on an empty GameObject called "_SETUP_HELPER"
// Run the game once — it prints the required scene hierarchy.
// ============================================================
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class SceneSetupHelper : MonoBehaviour
{
    [ContextMenu("Print Setup Guide")]
    public void PrintSetupGuide()
    {
        Debug.Log(@"
=== SCENE HIERARCHY GUIDE ===

[Managers]             (Empty GO)
  ├── GameManager      (GameManager.cs)
  ├── WaveManager      (WaveManager.cs)
  ├── EnemyFactory     (EnemyFactory.cs)
  ├── UIManager        (UIManager.cs)
  ├── PoolManager      (PoolManager.cs)   ← assign Bullet prefab
  └── EventManager     (no component — static)

[Player]               (Empty GO, position 0,1,0)
  ├── CharacterController  (radius 0.35, height 1.8)
  ├── PlayerHealth.cs
  ├── PlayerController.cs  ← assign InputReader SO
  ├── PlayerShooter.cs
  └── CameraRoot          (child empty, pos 0, 0.7, 0)
        └── Main Camera   (assigned to PlayerController._playerCamera)
        └── Muzzle        (assigned to PlayerShooter._muzzle, pos 0, 0, 0.8)

[Environment]
  ├── Ground Plane    (with NavMesh baked)
  └── SpawnPoints[]   (assign in WaveManager._spawnPoints)

[UI Canvas]           (Screen Space Overlay)
  ├── HUD
  │   ├── HealthBar   (Slider)
  │   ├── WaveText    (TMP)
  │   ├── ScoreText   (TMP)
  │   ├── EnemyCount  (TMP)
  │   └── Announcement (TMP, center screen, large)
  ├── BossBar
  │   ├── BossHealthBar (Slider)
  │   └── BossNameText
  ├── UpgradePanel    (UpgradePanel.cs)
  │   ├── PointsText
  │   ├── WaveCompleteText
  │   ├── StatLabels (5x TMP)
  │   ├── UpgradeButtons (5x Button)
  │   └── ContinueButton
  └── GameOverPanel
      ├── FinalScore  (TMP)
      ├── FinalWave   (TMP)
      └── RestartButton

=== INPUT ACTIONS ===
1. Window → Package Manager → Input System (install)
2. Import GameInputActions.inputactions into project
3. Double-click → Generate C# class
4. Create → Game → InputReader (SO asset)
5. Assign SO to PlayerController

=== NAVMESH ===
1. Select ground / walkable surfaces
2. Window → AI → Navigation → Bake

=== LAYERS & TAGS ===
- Tag 'Player' on player GO
- Tag 'Enemy'  on enemy prefabs
- Layer 'Ground' on floor surfaces
- Assign Ground layer to PlayerController._groundLayer
");
    }
}
#endif
